import asyncio
import websockets
import time
import random
import json
import docker
import threading

client = docker.DockerClient(base_url='tcp://localhost:2375')

# Will be assigned once event loop starts
MAIN_LOOP = None


def get_container_stats(container_name):
    container = client.containers.get(container_name)
    stats = container.stats(stream=False)

    # CPU calculation
    cpu_delta = stats["cpu_stats"]["cpu_usage"]["total_usage"] - stats["precpu_stats"]["cpu_usage"]["total_usage"]
    system_delta = stats["cpu_stats"]["system_cpu_usage"] - stats["precpu_stats"]["system_cpu_usage"]
    percpu = stats["cpu_stats"]["cpu_usage"].get("percpu_usage")

    cpu_percent = (cpu_delta / system_delta) * len(percpu) * 100 if (system_delta > 0 and percpu) else 0

    # Memory usage
    mem_usage = stats["memory_stats"]["usage"]
    mem_limit = stats["memory_stats"]["limit"]
    mem_percent = (mem_usage / mem_limit) * 100

    return {
        "container": container_name,
        "status": container.status,
        "cpu_percent": cpu_percent,
        "memory_percent": round(mem_percent, 2),
        "memory_bytes": mem_usage
    }

async def create_container(ws,image_name):
    try:
        container_object=client.containers.create(name= image_name[-6:]+"_container",image=image_name,detach=True,ports= {'80/tcp': 8080})
        result_object = {
            "container_name":container_object.name,
            "container_id":container_object.id,
            "container_status": container_object.status
        }
        await ws.send(json.dumps(result_object))
    except Exception as e:
        await ws.send(json.dumps({"status": "cannot create the container", "message": str(e)}))


async def start_container(ws,container_name):
    try:
        container_object=client.containers.get(container_name)
        container_object.start()
        result_object = {
            "container_name":container_object.name,
            "container_id":container_object.id,
            "container_status": container_object.status
        }
        print(result_object)
        await ws.send(json.dumps(result_object))
    except Exception as e:
        await ws.send(json.dumps({"status": "cannot start the container", "message": str(e)}))

async def stop_container(ws,container_name):
    try:
        container_object=client.containers.get(container_name)
        container_object.stop()
        result_object = {
            "container_name":container_object.name,
            "container_id":container_object.id,
            "container_status": container_object.status
        }
        print(result_object)
        await ws.send(json.dumps(result_object))
    except Exception as e:
        await ws.send(json.dumps({"status": "cannot stop the container", "message": str(e)}))

async def remove_container(ws,container_name):
    try:
        container_object=client.containers.get(container_name)
        container_object.remove()
        result_object = {
            "container_name":container_object.name,
            "container_id":container_object.id,
            "container_status": container_object.status
        }
        await ws.send(json.dumps("container "+container_name+" is removed"))
    except Exception as e:
        await ws.send(json.dumps({"status": "cannot remove the container", "message": str(e)}))


async def remove_image(ws,image_name):
    try:
        image_object=client.images.get(image_name)
        image_object.remove()
        await ws.send(json.dumps("image "+image_name+" is removed"))
    except Exception as e:
        await ws.send(json.dumps({"status": "cannot remove the image", "message": str(e)}))

async def get_container_list(ws):
    container_list = []
    containers = client.containers.list(all=True)
    for container in containers:
        container_object =  {} 
        container_object["container_id"] = container.id
        container_object["container_name"] = container.name
        container_object["container_status"] = container.status
        container_list.append(container_object)
    # print(container_list)
    await ws.send(json.dumps(container_list))

async def get_image_list(ws):
    image_list = []
    images = client.images.list(all=True)
    print(images)
    for image in images:
        image_object =  {} 
        image_object["image_id"] = image.id
        image_object["image_name"] = image.tags[0]
        # image_object["image_status"] = image.status
        image_list.append(image_object)
    # print(container_list)
    await ws.send(json.dumps(image_list))

# ---- IMAGE PULL THREAD SAFE ---- #
def pull_image_thread(websocket, image_name, done_event):
    global MAIN_LOOP
    try:
        for line in client.api.pull(image_name, stream=True, decode=True):
            if "error" in line or "errorDetail" in line:
                msg = line.get("error") or line["errorDetail"]["message"]
                asyncio.run_coroutine_threadsafe(
                    websocket.send(json.dumps({"status": "failed", "message": msg})),
                    MAIN_LOOP
                )
                return
        
        asyncio.run_coroutine_threadsafe(
            websocket.send(json.dumps({"status": "done_"+image_name+"_pull"})),
            MAIN_LOOP
        )

    except Exception as e:
        asyncio.run_coroutine_threadsafe(
            websocket.send(json.dumps({"status": "failed", "message": str(e)})),
            MAIN_LOOP
        )
    finally:
        done_event.set()


# ---- STREAM TASKS ---- #
async def stream_time(ws):
    while not ws.closed:
        await ws.send(json.dumps({"time": time.time()}))
        await asyncio.sleep(1)


async def stream_random(ws):
    while not ws.closed:
        await ws.send(json.dumps({"random": random.randint(1, 100)}))
        await asyncio.sleep(1)


async def stream_stats(ws, container):
    while not ws.closed:
        try:
            data = get_container_stats(container)
            await ws.send(json.dumps(data))
        except Exception as e:
            await ws.send(json.dumps({"error": str(e)}))
            return
        await asyncio.sleep(1)

# async def send_threadsafe(ws, data):
#     """Ensures WebSocket send works from thread context."""
#     if ws.closed:
#         return
#     await ws.send(json.dumps(data))

# ---- MAIN WEBSOCKET HANDLER ---- #
async def handler(websocket):
    tasks = []

    async for message in websocket:
        print("Client requested:", message)


        if message == "get_time":
            tasks.append(asyncio.create_task(stream_time(websocket)))

        elif message == "get_random":
            tasks.append(asyncio.create_task(stream_random(websocket)))

        # function for getting docker containers
        elif message == "list_containers":
            # print(message)
            tasks.append(asyncio.create_task(get_container_list(websocket)))

         # function for getting docker containers
        elif message == "list_images":
            # print(message)
            tasks.append(asyncio.create_task(get_image_list(websocket)))

        # function for creating docker containers.
        elif message.startswith("create_container:"):
            image_name = message.split(":")[1]
            print(message)
            tasks.append(asyncio.create_task(create_container(websocket,image_name)))

        elif message.startswith("start_container:"):
            container_name = message.split(":")[1]
            print(message)
            tasks.append(asyncio.create_task(start_container(websocket,container_name)))

        elif message.startswith("stop_container:"):
            container_name = message.split(":")[1]
            print(message)
            tasks.append(asyncio.create_task(stop_container(websocket,container_name)))

        elif message.startswith("remove_container:"):
            container_name = message.split(":")[1]
            print(message)
            tasks.append(asyncio.create_task(remove_container(websocket,container_name)))

        elif message.startswith("remove_image:"):
            image_name = message.split(":")[1]
            print(message)
            tasks.append(asyncio.create_task(remove_image(websocket,image_name)))

        elif message.startswith("stats:"):
            container = message.split(":")[1]
            tasks.append(asyncio.create_task(stream_stats(websocket, container)))


        elif message.startswith("pull_image:"):
            # image = message.split(":")[1]
            # thread = threading.Thread(target=pull_image_thread, args=(websocket, image))
            # thread.start()
            image = message.split(":")[1]

            # event to track completion
            done_event = threading.Event()

            thread = threading.Thread(
                target=pull_image_thread,
                args=(websocket, image, done_event),
                daemon=True
            )
            thread.start()

            # Keep websocket open until pull finishes
            await asyncio.to_thread(done_event.wait)

            # Now return to listening without closing
            continue

    # cleanup tasks once client disconnects
    for task in tasks:
        task.cancel()


async def main():
    global MAIN_LOOP
    MAIN_LOOP = asyncio.get_running_loop()

    async with websockets.serve(handler, "localhost", 8765, ping_interval=None):
        print("Server running...")
        await asyncio.Future()  # keep running


asyncio.run(main())
