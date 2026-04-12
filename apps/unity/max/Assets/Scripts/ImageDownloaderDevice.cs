using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class ImageDownloaderDevice : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private ObjectSpawner spawner;
    [SerializeField] private DirectorWorker orbitBall;

    private ClientWebSocket webSocket;

    public string[] AvailableImages;
    public string selectedImage;
    private bool isBusy = false;
    private int selectedIndex = 0;
    private float timeElapsed = 0.0f;
    private bool isTriggered = false;
    public static List<string> DockerImages = new();
    public static bool hasImages = false;
    string myText = "";
    private bool isWaiting = false;

    public class DockerImage
    {
        public string image_id;
        public string image_name;
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
        AvailableImages = DockerImages.ToArray();

        // screenText.text = myText;
        hasImages = true;

    }

    public new async void SendMessage(string message)
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

    async void Start()
    {
        await ConnectWebSocket("ws://localhost:8765");
        //         influxdb                                          latest      771dee7da05d   6 weeks ago    380MB
        // akshaychikhalkar/carla-driving-simulator-client   1.3.29      4df6b9308ee2   5 months ago   1.89GB
        // akshaychikhalkar/carla-driving-simulator-client   latest      4df6b9308ee2   5 months ago   1.89GB
        // traefik                                           latest      91f33a9ff29b   5 months ago   226MB
        // certbot/certbot                                   latest      95d0b35bc337   6 months ago   114MB
        // postgres                                          17-alpine   f40315d0e8a6   6 months ago   279MB
        // alpine                                            latest      cea2ff433c61   7 months ago   8.31MB
        // traefik/whoami                                    latest      6fee7566e427   9 months ago   7.16MB
        // grafana/grafana                                   10.2.3      8387f19108f9   2 years ago    399MB
        AvailableImages = new string[] {
                "influxdb",
                "akshaychikhalkar/carla-driving-simulator-client",
                "akshaychikhalkar/carla-driving-simulator-client",
                "traefik",
                "certbot/certbot",
                "postgres",
                "alpine",
                "traefik/whoami",
                "grafana/grafana",
        };
        // Example usage
        if (AvailableImages.Length > 0)
            selectedImage = AvailableImages[0];
        else
            selectedImage = "NO IMAGE AVAILABLE";

        UpdateDisplay(selectedImage);
    }

    void Update()
    {
        if (isTriggered)
            timeElapsed += Time.deltaTime;

        if (timeElapsed > 3f)
        {
            timeElapsed = 0f;
            isTriggered = false;

            if (spawner)
            {
                spawner.DoSpawn();
                isBusy = true;
            }
            if (orbitBall)
            {
                orbitBall.DoRunAnimation("0");
            }
            UpdateDisplay("Recovering ...");
        }

    }

    public void UpdateDisplay(string newText)
    {
        screenText.text = newText;
    }

    // Optional: Retro "boot-up" typing effect
    public void TypeMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(TypeScreeRoutine(message));
    }

    private IEnumerator TypeScreeRoutine(string message)
    {
        screenText.text = "";
        foreach (char c in message.ToCharArray())
        {
            screenText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void DoSelectNextImage()
    {
        selectedIndex++;
        if (selectedIndex >= AvailableImages.Length) selectedIndex = 0;

        selectedImage = AvailableImages[selectedIndex];
        UpdateDisplay(selectedImage);
    }

    public void DoDownloadImage()
    {
        if (isBusy)
        {
            UpdateDisplay("* System busy! *");
            return;
        }

        UpdateDisplay("Downloading ...");
        isTriggered = true;
        if (orbitBall)
        {
            orbitBall.DoRunAnimation("1");
        }

        SendMessage("pull_image:alpine");
    }

    public void DoReset()
    {
        isBusy = false;
        UpdateDisplay(selectedImage);
    }

    public void DoGetList()
    {
        if (isWaiting) return;
        StartCoroutine(callWebSocket());
        Debug.Log("here...");
    }
}
