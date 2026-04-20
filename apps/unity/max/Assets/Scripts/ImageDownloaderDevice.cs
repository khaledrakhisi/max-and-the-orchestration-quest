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
using UnityEngine.UIElements;

public class ImageDownloaderDevice : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://localhost:8765";
    [SerializeField] private string[] AvailableImages;
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private ObjectSpawner spawner;
    [SerializeField] private DirectorWorker orbitBall;
    [SerializeField] private InfoBoard infoBoard;

    private ClientWebSocket webSocket;
    public string selectedImage;
    private bool isBusy = false;
    private int selectedIndex = 0;
    private float timeElapsed = 0.0f;
    private bool isTriggered = false;
    public static List<string> DockerImages = new();
    public static bool hasImages = false;
    private bool isWaiting = false;

    public static ImageDownloaderDevice Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public class DockerImage
    {
        public string image_id;
        public string image_name;
    }

    async void Start()
    {
        // await ConnectWebSocket("ws://localhost:8765");
        //         influxdb                                          latest      771dee7da05d   6 weeks ago    380MB
        // akshaychikhalkar/carla-driving-simulator-client   1.3.29      4df6b9308ee2   5 months ago   1.89GB
        // akshaychikhalkar/carla-driving-simulator-client   latest      4df6b9308ee2   5 months ago   1.89GB
        // traefik                                           latest      91f33a9ff29b   5 months ago   226MB
        // certbot/certbot                                   latest      95d0b35bc337   6 months ago   114MB
        // postgres                                          17-alpine   f40315d0e8a6   6 months ago   279MB
        // alpine                                            latest      cea2ff433c61   7 months ago   8.31MB
        // traefik/whoami                                    latest      6fee7566e427   9 months ago   7.16MB
        // grafana/grafana                                   10.2.3      8387f19108f9   2 years ago    399MB        

//              "pihole/pihole 147mb",
//                 "nginx 400mb",
//                 "rarenicks/alphine 7mb",
//                 "excalidraw/excalidraw 144mb",
//                 "uzyexe/tetris 100mb",

        AvailableImages = new string[] {
                "rarenicks/alphine",
                "pihole/pihole",
                "nginx",
                "excalidraw/excalidraw",
                "uzyexe/tetris",
        };

        // Example usage
        if (AvailableImages.Length > 0)
            selectedImage = AvailableImages[0];
        else
            selectedImage = "NO IMAGE AVAILABLE";

        UpdateDisplay(selectedImage);

        // --- SUBSCRIBE TO GLOBAL MESSAGES ---
        // Tell the global manager: "Whenever you receive a message, trigger my PrintImages function"
        Debug.Log(WebSocketManager.Instance);
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnMessageReceived += PrintImages;
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
            WebSocketManager.Instance.OnMessageReceived -= PrintImages;
        }
    }

    /// <summary>
    /// Triggered automatically by WebSocketManager when data arrives.
    /// </summary>
    void PrintImages(string message)
    {
        Debug.Log($"message:" + message);
        try
        {
            // Attempt to parse the message. 
            // Note: If you have multiple objects listening, you should check the message
            // to ensure it's actually meant for this downloader (e.g., check for a "type" field)
            List<DockerImage> images = JsonConvert.DeserializeObject<List<DockerImage>>(message);

            if (images != null)
            {
                screenText.text = "";
                hasImages = true;
                Debug.Log($"Successfully parsed {images.Count} docker images.");
            }
        }
        catch (Exception e)
        {
            // Ignore messages that aren't DockerImage lists (they might be meant for a ChatBox or other object)
            Debug.Log($"Message was not a DockerImage list: {e.Message}");
        }
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

        // --- SEND COMMAND USING SINGLETON ---
        WebSocketManager.Instance.SendMessageToServer($"pull_image:{selectedImage}");
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
    }
    IEnumerator callWebSocket()
    {
        isWaiting = true;
        yield return new WaitForSeconds(0.5f);

        // --- SEND COMMAND USING SINGLETON ---
        WebSocketManager.Instance.SendMessageToServer("list_images");

        isWaiting = false;
    }

    public void DoShowAvailableImages()
    {
        string displayText = "Available Images: \n\n";
        foreach (string img in AvailableImages)
        {
            displayText += img + "\n";
        }

        if (infoBoard != null)
        {
            infoBoard.DoRemoveItem("0");
            infoBoard.DoAddToList(displayText, "50");
        }
    }
}
