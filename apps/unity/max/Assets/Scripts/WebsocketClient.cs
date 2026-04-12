using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
public class WebSocketClient : MonoBehaviour
{
    private PlayerInteraction playerInteraction;
    private ClientWebSocket webSocket;
    private Vector3 cubecenterPosition = new Vector3(0f, 0.15f, 0);
    public Vector3 cubeSize;
    private Collider2D target;
    public LayerMask collisionLayer;
    private bool isWaiting = false;
    private bool hasNewText = false;
    public static List<string> DockerImages = new();
    public static bool hasImages = false;
    string myText = "";
    [SerializeField] private TextMeshProUGUI screenText;
    [Serializable]
    public class DockerImage
    {
        public string image_id;
        public string image_name;
    }
    async void Start()
    {
        await ConnectWebSocket("ws://localhost:8765");

    }

    private async Task ConnectWebSocket(string uri)
    {
        webSocket = new ClientWebSocket();

        try
        {
            await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            Debug.Log("WebSocket connected!");

            // Start listening for messages
            await ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError("WebSocket connection error: " + e.Message);
        }
    }

    void printImages(string message)
    {
        List<DockerImage> images = JsonConvert.DeserializeObject<List<DockerImage>>(message);

        screenText.text = "";



        foreach (var image in images)
        {
            DockerImages.Add(image.image_name);
            myText += $"ID: image.image_id \nName: {image.image_name}\n\n";
        }

        // public string[] MyDockerImages = DockerImages.ToArray();
        // myText.ToString();
        // hasNewText = true;
        // Debug.Log(myText);
        Debug.Log("docker Images:" + DockerImages);

        screenText.text = myText;
        hasImages = true;

    }

    private async Task ReceiveMessages()
    {
        var buffer = new byte[1024];
        using var memorystream = new MemoryStream();
        // WebSocketReceiveResult result;

        try
        {
            while (webSocket != null &&
                   webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("Server closed connection.");
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closed by client",
                        CancellationToken.None);
                }
                else
                {
                    Debug.Log("message:type" + result.GetType());
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log("Received: " + message);
                    printImages(message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Receive error: " + e.Message);
        }
    }


    public async void SendMessage(string message)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async void OnApplicationQuit()
    {
        if (webSocket != null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Application quitting", CancellationToken.None);
            webSocket.Dispose();
        }
    }
    IEnumerator callWebSocket()
    {
        isWaiting = true;
        yield return new WaitForSeconds(0.5f);
        SendMessage("list_images");
        isWaiting = false;
    }
    // void Update(){
    //      if (hasNewText)
    // {
    //     screenText.text = myText;
    //     hasNewText = false;
    // }

    // }
    void FixedUpdate()
    {
        var pos = cubecenterPosition;
        pos.x += transform.position.x;
        pos.y += transform.position.y;
        target = Physics2D.OverlapBox(pos, cubeSize, 1, collisionLayer);


        // Debug.Log(target);


        if (target != null && target.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction =
                target.GetComponent<PlayerInteraction>();


            if (playerInteraction != null &&
                playerInteraction.interacted && screenText.text == "")
            {
                if (isWaiting) return;
                StartCoroutine(callWebSocket());
                Debug.Log("interatcted_bool:" + playerInteraction.interacted);

            }
            // else if(playerInteraction != null &&
            //     !playerInteraction.web_socket_button){
            //     webSocket_state = false;
            // }

        }

    }

}
