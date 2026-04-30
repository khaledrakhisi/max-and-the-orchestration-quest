using System;
using Newtonsoft.Json;
using UnityEngine;

public class Factory : MonoBehaviour
{
    public enum States
    {
        Off = 0,
        Input,
        Process,
        Output,
    }
    private const string imageTag = "Docker-Image";
    public States state = States.Off;
    private States prevState = States.Off;
    public Rotate2D gear1;
    public Rotate2D gear2;
    public Smoke smoke;
    public AutoMovingwalkway conveyBelt;
    [SerializeField] private LayerMask imageLayer;
    public GameObject imageObject;
    private string dockerImageName = "";
    [SerializeField] private Vector3 collisionCubePosition = Vector3.zero;
    [SerializeField] private Vector3 collisionCubeSize;
    [SerializeField] private InfoBoard infoBoard;
    [SerializeField] private ResourcesIndicators resourcesIndicators;
    [SerializeField] private ObjectSpawner spawner;
    [SerializeField] private MoveToPoint fence;
    private bool isStatusPosted = false;

    public class Response
    {
        public string container_name;
        public string container_id;
        public string container_status;
    }
    public class Results
    {
        public string type;
        public string status;
        public string message;
        // public Response response;
        public Response response;
        public Response[] list;
    }
    private Results results;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // --- SUBSCRIBE TO GLOBAL MESSAGES ---
        // Tell the global manager: "Whenever you receive a message, trigger my ReceiveDataFromWebsocket function"
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnMessageReceived += ReceiveDataFromWebsocket;
        }
        else
        {
            Debug.LogError("WebSocketManager Singleton is missing from the scene!");
        }
    }

    private void OnDestroy()
    {
        // --- UNSUBSCRIBE TO PREVENT MEMORY LEAKS ---        
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnMessageReceived -= ReceiveDataFromWebsocket;
        }
    }

    /// <summary>
    /// Triggered automatically by WebSocketManager when data arrives.
    /// </summary>
    void ReceiveDataFromWebsocket(string message)
    {
        try
        {
            // Attempt to parse the message. 
            results = JsonConvert.DeserializeObject<Results>(message);

            if (results != null && results.type == "create_container")
            {
                if (spawner && results.status == "ok")
                {
                    imageObject.GetComponent<DirectorWorker>().DoDistroyObject();

                    state = States.Output;
                    GameObject dockerContainer = spawner.DoSpawn();
                    if (dockerContainer && resourcesIndicators)
                    {
                        // dockerContainer.GetComponent<Container>().containerName = results.response.container_name;
                        dockerContainer.GetComponent<Container>().cpu = resourcesIndicators.cpu;
                        dockerContainer.GetComponent<Container>().ram = resourcesIndicators.ram;

                        infoBoard.DoShowOneMessage(">>> Image Converted to Container Successfully", "Success");
                        isStatusPosted = true;
                        prevState = state;

                        resourcesIndicators.cpu = 0;
                        resourcesIndicators.ram = 0;
                    }
                }
                else if (results.status == "failed")
                {
                    infoBoard.DoShowOneMessage(">>> " + results.message, "Danger");
                    isStatusPosted = true;
                    prevState = state;
                }
            }
            else if (results != null && results.type == "container_list")
            {
                foreach (Response res in results.list)
                {
                    if (spawner)
                    {
                        GameObject dockerContainer = spawner.DoSpawn();
                        if (dockerContainer)
                        {
                            dockerContainer.GetComponent<Container>().containerName = res.container_name;
                            Debug.Log(res.container_status);
                            state = States.Output;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Ignore messages that aren't DockerImage lists (they might be meant for a ChatBox or other object)
            Debug.Log($"Message was not a DockerImage list: {e.Message}");
        }
    }
    // Update is called once per frame
    void Update()
    {
        var pos = collisionCubePosition;
        pos.x += transform.position.x;
        pos.y += transform.position.y;

        Collider2D collidedObject = Physics2D.OverlapBox(pos, collisionCubeSize, 1, imageLayer);
        if (collidedObject != null)
        {
            if (collidedObject.gameObject.CompareTag(imageTag))
            {
                if (state == States.Input)
                {
                    state = States.Process;

                    imageObject = collidedObject.gameObject;
                    dockerImageName = collidedObject.gameObject.GetComponent<DockerImage>().imageName;
                }
            }
        }

        if (state == States.Off)
        {
            if (prevState == States.Input)
            {
                if (isStatusPosted)
                {
                    isStatusPosted = false;
                }
                if (!isStatusPosted && infoBoard)
                {
                    infoBoard.DoShowOneMessage(">>> Error: No Image Found!", "Danger");
                    prevState = state;
                }
            }

            if (gear1)
                gear1.isOn = false;
            if (gear2)
                gear2.isOn = false;
            if (smoke)
                smoke.isOn = false;
            if (conveyBelt)
                conveyBelt.isOn = false;
        }
        else if (state == States.Input)
        {
            if (prevState == States.Off)
            {
                if (resourcesIndicators)
                {
                    if (resourcesIndicators.cpu >= 1 && resourcesIndicators.ram >= 128)
                    {
                        prevState = States.Input;
                    }
                    else
                    {
                        infoBoard.DoShowOneMessage(">>> Error: Not enough resources collected to create container !\n\nRAM required: 128+\nCPU required: 1+", "Danger");
                        DoResetFactory();
                        return;
                    }
                }
                else
                {
                    infoBoard.DoShowOneMessage(">>> Error: No resources attached!", "Danger");
                    DoResetFactory();
                    return;
                }
            }

            if (!isStatusPosted && infoBoard)
            {
                infoBoard.DoShowOneMessage(">>> Now trying to convert image to container . . . . . . . . . . . . . .", "Warning");
                isStatusPosted = true;
                prevState = state;
            }

            // visuals
            if (gear1)
            {
                gear1.isOn = true;
                gear1.RPM = -Math.Abs(gear1.RPM);
            }
            if (gear2)
            {
                gear2.isOn = true;
                gear2.RPM = Math.Abs(gear2.RPM);
            }
            if (smoke)
                smoke.isOn = true;

            if (conveyBelt)
            {
                conveyBelt.isOn = true;
                conveyBelt.targetSpeed = new Vector2(-1f, 0f);
            }
        }

        else if (state == States.Process)
        {
            if (prevState == States.Input)
            {
                prevState = States.Process;

                if (dockerImageName != "")
                {
                    // 1. Get the original image name (e.g., "uzyexe/tetris:latest")
                    string safeImageName = dockerImageName;
                    int colonIndex = safeImageName.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        safeImageName = safeImageName.Substring(0, colonIndex);
                    }
                    Debug.Log(dockerImageName + safeImageName);

                    // --- SEND COMMAND USING SINGLETON ---
                    WebSocketManager.Instance.SendMessageToServer($"create_container:{safeImageName}:{resourcesIndicators.cpu}:{resourcesIndicators.ram}");
                }
            }

            // if (conveyBelt)
            // {
            //     conveyBelt.isOn = false;
            // }
        }

        // output state decided using Director
        else if (state == States.Output)
        {
            if (prevState == States.Input)
            {
                if (isStatusPosted)
                {
                    isStatusPosted = false;

                }
            }
            if (gear1)
            {
                gear1.isOn = true;
                gear1.RPM = Math.Abs(gear1.RPM);
            }
            if (gear2)
            {
                gear2.isOn = true;
                gear2.RPM = -Math.Abs(gear2.RPM);
            }
            if (smoke)
                smoke.isOn = true;

            if (conveyBelt)
            {
                conveyBelt.isOn = true;
                conveyBelt.targetSpeed = new Vector2(1f, 0f);
            }
        }
    }

    public void DoResetFactory()
    {
        state = States.Off;
        prevState = States.Off;
        isStatusPosted = false;

        if (fence)
        {
            fence.DoMove();
        }
    }

    void OnDrawGizmos()
    {
        Color color = Color.softRed;
        color.a = .7f;
        Gizmos.color = color;
        //draw hit area
        var bpos = collisionCubePosition;
        bpos.x += transform.position.x;
        bpos.y += transform.position.y;
        Gizmos.DrawWireCube(bpos, collisionCubeSize);
    }

    public void DoSpawnCreatedContainers()
    {
        // --- SEND COMMAND USING SINGLETON ---
        WebSocketManager.Instance.SendMessageToServer($"list_containers");
    }
}
