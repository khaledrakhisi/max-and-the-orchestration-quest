using System.Collections.Generic;
using UnityEngine;

public class ServerMachine : MonoBehaviour
{
    enum Statuses { Idle = 0, Checking, Accepted, Rejected }
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
                DoDockContainer();
                DirectorWorker dw = containerObject.GetComponent<DirectorWorker>();
                if (dw) dw.DoDistroyObject();
                timeElapsed = 0f;
                status = Statuses.Idle;
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