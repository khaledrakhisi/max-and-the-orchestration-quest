using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ServerMachine : MonoBehaviour
{
    enum Statuses { Idle = 0, Checking, Accepted, Running, Rejected }
    [SerializeField] private LayerMask containerLayer;
    public GameObject containerObject;
    private const string containerTag = "Docker-Container";
    private readonly List<Container> containers = new();
    [SerializeField] private Vector3 collisionCubePosition = Vector3.zero;
    [SerializeField] private Vector3 collisionCubeSize;
    [SerializeField] private InfoBoard infoBoard;
    [SerializeField] private MoveToPoint serverDoor;
    [SerializeField] private Rotate2D fan1, fan2;
    private float timeElapsed = 0f;
    private Statuses status = Statuses.Idle;

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
        public Response response;
    }

    public Results results;

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

            if (results != null && results.type == "start_container")
            {
                if (results.status == "ok")
                {
                    DoDockContainer();
                    DirectorWorker dw = containerObject.GetComponent<DirectorWorker>();
                    if (dw) dw.DoDistroyObject();
                }
                else if (results.status == "failed")
                {

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

        Collider2D collidedObject = Physics2D.OverlapBox(pos, collisionCubeSize, 1, containerLayer);
        if (collidedObject != null)
        {
            containerObject = collidedObject.gameObject;
            if (status == Statuses.Idle) status = Statuses.Checking;

            if (timeElapsed > 3f && status == Statuses.Checking)
            {
                // if some object other than a Docker-Container tag pushed into the server
                if (!containerObject.CompareTag(containerTag))
                {
                    // throw the wrong object out
                    containerObject.GetComponent<Rigidbody2D>().AddForceX(20f);
                    status = Statuses.Rejected;
                }
                // if it's a Docker-Container, close the door to start the operation
                else
                {
                    if (serverDoor)
                    {
                        serverDoor.DoMove();
                        status = Statuses.Accepted;
                    }
                }
            }
        }

        if (status == Statuses.Checking)
            timeElapsed += Time.deltaTime;
        else if (status == Statuses.Rejected)
        {
            status = Statuses.Idle;
            timeElapsed = 0f;
        }
        else if (status == Statuses.Accepted)
        {
            if (containerObject)
            {
                status = Statuses.Running;
                // --- SEND COMMAND USING SINGLETON ---
                WebSocketManager.Instance.SendMessageToServer($"start_container:{containerObject.GetComponent<Container>().containerName}");
                timeElapsed = 0f;
            }
        }
    }

    private void UpdateInfoBoard()
    {
        infoBoard.DoShowOneMessage("Docking Done!\n\nTotal: " + containers.Count, "Info");
    }

    private void UpdateFanSpeed()
    {
        if (fan1)
        {
            fan1.RPM = containers.Count * -400f;
        }
        if (fan2)
        {
            fan2.RPM = containers.Count * -400f;
        }
    }

    public void DoDockContainer()
    {
        Container container = null;
        if (containerObject.CompareTag(containerTag))
        {
            container = containerObject.GetComponent<Container>();
        }
        if (container)
        {
            containers.Add(container);
            UpdateFanSpeed();
            UpdateInfoBoard();
        }
        else
        {
            if (infoBoard)
            {
                infoBoard.DoShowOneMessage("Error:\n\n No container found!", "Danger");
            }
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
}