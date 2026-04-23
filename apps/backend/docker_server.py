import asyncio
import websockets
import json
import random
import docker
import threading
import os

from dotenv import load_dotenv
from logger import setup_logging

load_dotenv()

# ---- INIT ---- #
log = setup_logging(mongo_uri=os.environ["MONGO_URI"])
client = docker.DockerClient(base_url='tcp://localhost:2375')
MAIN_LOOP = None


# ---- STATS ---- #

def get_container_stats(container_name):
    container = client.containers.get(container_name)
    stats = container.stats(stream=False)

    cpu_delta = stats["cpu_stats"]["cpu_usage"]["total_usage"] - stats["precpu_stats"]["cpu_usage"]["total_usage"]
    system_delta = stats["cpu_stats"]["system_cpu_usage"] - stats["precpu_stats"]["system_cpu_usage"]
    percpu = stats["cpu_stats"]["cpu_usage"].get("percpu_usage")
    cpu_percent = (cpu_delta / system_delta) * len(percpu) * 100 if (system_delta > 0 and percpu) else 0

    mem_usage = stats["memory_stats"]["usage"]
    mem_limit = stats["memory_stats"]["limit"]
    mem_percent = (mem_usage / mem_limit) * 100

    return {
        "type": "container_stats",
        "container": container_name,
        "status": container.status,
        "cpu_percent": cpu_percent,
        "memory_percent": round(mem_percent, 2),
        "memory_bytes": mem_usage
    }


# ---- CONTAINER OPERATIONS ---- #

async def create_container(ws, client_id, image_name, cpus, memory):
    try:
        container_name = image_name.split("/")[1]
        obj = client.containers.create(
            name=container_name + "_container",
            image=image_name,
            detach=True,
            ports={'80/tcp': 8080},
            cpu_count=cpus,
            mem_limit=str(memory) + "m"
        )
        log.info("Container created", extra={"client_id": client_id, "container": obj.name})
        await ws.send(json.dumps({"type": "create_container", "response": {
            "container_name": obj.name,
            "container_id": obj.id,
            "container_status": obj.status
        }}))
    except Exception as e:
        log.error("Container creation failed", extra={"client_id": client_id, "image": image_name, "error": str(e)})
        await ws.send(json.dumps({"type": "create_container", "status": "cannot create the container", "message": str(e)}))


async def start_container(ws, client_id, container_name):
    try:
        obj = client.containers.get(container_name)
        obj.start()
        log.info("Container started", extra={"client_id": client_id, "container": container_name})
        await ws.send(json.dumps({"type": "start_container", "response": {
            "container_name": obj.name,
            "container_id": obj.id,
            "container_status": "started"
        }}))
    except Exception as e:
        log.error("Container start failed", extra={"client_id": client_id, "container": container_name, "error": str(e)})
        await ws.send(json.dumps({"type": "start_container", "status": "cannot start the container", "message": str(e)}))


async def stop_container(ws, client_id, container_name):
    try:
        obj = client.containers.get(container_name)
        obj.stop()
        log.info("Container stopped", extra={"client_id": client_id, "container": container_name})
        await ws.send(json.dumps({"type": "stop_container", "response": {
            "container_name": obj.name,
            "container_id": obj.id,
            "container_status": "stopped"
        }}))
    except Exception as e:
        log.error("Container stop failed", extra={"client_id": client_id, "container": container_name, "error": str(e)})
        await ws.send(json.dumps({"type": "stop_container", "status": "cannot stop the container", "message": str(e)}))


async def remove_container(ws, client_id, container_name):
    try:
        obj = client.containers.get(container_name)
        obj.remove()
        log.info("Container removed", extra={"client_id": client_id, "container": container_name})
        await ws.send(json.dumps({"type": "remove_container", "message": "container " + container_name + " is removed"}))
    except Exception as e:
        log.error("Container removal failed", extra={"client_id": client_id, "container": container_name, "error": str(e)})
        await ws.send(json.dumps({"type": "remove_container", "status": "cannot remove the container", "message": str(e)}))


async def remove_image(ws, client_id, image_name):
    try:
        obj = client.images.get(image_name)
        obj.remove()
        log.info("Image removed", extra={"client_id": client_id, "image": image_name})
        await ws.send(json.dumps({"type": "remove_image", "message": "image " + image_name + " is removed"}))
    except Exception as e:
        log.error("Image removal failed", extra={"client_id": client_id, "image": image_name, "error": str(e)})
        await ws.send(json.dumps({"type": "remove_image", "status": "cannot remove the image", "message": str(e)}))


async def get_container_list(ws, client_id):
    containers = client.containers.list(all=True)
    container_list = [{"container_id": c.id, "container_name": c.name, "container_status": c.status} for c in containers]
    await ws.send(json.dumps({"type": "container_list", "response": container_list}))


async def get_image_list(ws, client_id):
    images = client.images.list(all=True)
    image_list = [{"image_id": i.id, "image_name": i.tags[0]} for i in images]
    await ws.send(json.dumps({"type": "image_list", "response": image_list}))


# ---- IMAGE PULL ---- #

def pull_image_thread(websocket, client_id, image_name, done_event):
    global MAIN_LOOP
    try:
        for image in client.images.list(all=True):
            if image_name == image.tags[0].split(":")[0]:
                print(image_name)
                print(image.tags[0].split(":")[0])
                asyncio.run_coroutine_threadsafe(
                    websocket.send(json.dumps({"status": "failed", "message": f"{image_name} is already present"})),
                    MAIN_LOOP
                )
                return

        ram = random.choice([1200, 700, 800, 600, 1100])
        cpu = random.randint(1, 5)
        last_status = ""

        for line in client.api.pull(image_name, stream=True, decode=True):
            if "error" in line or "errorDetail" in line:
                msg = line.get("error") or line["errorDetail"]["message"]
                log.error("Image pull failed", extra={"client_id": client_id, "image": image_name, "error": msg})
                asyncio.run_coroutine_threadsafe(
                    websocket.send(json.dumps({"status": "failed", "message": msg})),
                    MAIN_LOOP
                )
                return

            status_text = line.get("status", "")
            progress_text = line.get("progress", "")
            current_message = f"{status_text} {progress_text}".strip()

            if current_message != last_status:
                asyncio.run_coroutine_threadsafe(
                    websocket.send(json.dumps({"type": "image_pull","status": "pulling", "image": image_name, "detail": current_message})),
                    MAIN_LOOP
                )
                last_status = current_message

        log.info("Image pulled", extra={"client_id": client_id, "image": image_name})
        asyncio.run_coroutine_threadsafe(
            websocket.send(json.dumps({"status": "done", "image": image_name, "rss": [ram, cpu]})),
            MAIN_LOOP
        )
    except Exception as e:
        log.error("Image pull exception", extra={"client_id": client_id, "image": image_name, "error": str(e)})
        asyncio.run_coroutine_threadsafe(
            websocket.send(json.dumps({"status": "failed", "message": str(e)})),
            MAIN_LOOP
        )
    finally:
        done_event.set()


# ---- STREAM TASKS ---- #

async def stream_stats(ws, client_id, container):
    while not ws.closed:
        try:
            data = get_container_stats(container)
            await ws.send(json.dumps(data))
        except Exception as e:
            log.error("Stats stream error", extra={"client_id": client_id, "container": container, "error": str(e)})
            await ws.send(json.dumps({"error": str(e)}))
            return
        await asyncio.sleep(1)


# ---- HANDLER ---- #

async def handler(websocket):
    client_id = id(websocket)
    log.info("Player connected", extra={"client_id": client_id})
    tasks = []

    try:
        async for message in websocket:
            log.info("Command", extra={"client_id": client_id, "cmd": message})

            if message == "list_containers":
                tasks.append(asyncio.create_task(get_container_list(websocket, client_id)))

            elif message == "list_images":
                tasks.append(asyncio.create_task(get_image_list(websocket, client_id)))

            elif message.startswith("create_container:"):
                chunks = message.split(":")
                tasks.append(asyncio.create_task(create_container(websocket, client_id, chunks[1], int(chunks[2]), chunks[3])))

            elif message.startswith("start_container:"):
                tasks.append(asyncio.create_task(start_container(websocket, client_id, message.split(":")[1])))

            elif message.startswith("stop_container:"):
                tasks.append(asyncio.create_task(stop_container(websocket, client_id, message.split(":")[1])))

            elif message.startswith("remove_container:"):
                tasks.append(asyncio.create_task(remove_container(websocket, client_id, message.split(":")[1])))

            elif message.startswith("remove_image:"):
                tasks.append(asyncio.create_task(remove_image(websocket, client_id, message.split(":")[1])))

            elif message.startswith("stats:"):
                tasks.append(asyncio.create_task(stream_stats(websocket, client_id, message.split(":")[1])))

            elif message.startswith("pull_image:"):
                chunks = message.split(":")
                image_name = chunks[1] + ":" + chunks[2] if len(chunks) > 2 else chunks[1]
                done_event = threading.Event()
                thread = threading.Thread(target=pull_image_thread, args=(websocket, client_id, image_name, done_event), daemon=True)
                thread.start()
                await asyncio.to_thread(done_event.wait)
                continue

    except websockets.exceptions.ConnectionClosedError:
        pass
    finally:
        log.info("Player disconnected", extra={"client_id": client_id})
        for task in tasks:
            task.cancel()


# ---- MAIN ---- #

async def main():
    global MAIN_LOOP
    MAIN_LOOP = asyncio.get_running_loop()
    log.info("Server starting on ws://localhost:8765")

    async with websockets.serve(handler, "localhost", 8765, ping_interval=None):
        log.info("Server running")
        try:
            await asyncio.Future()
        except (asyncio.CancelledError, KeyboardInterrupt):
            log.info("Server shutting down")


asyncio.run(main())