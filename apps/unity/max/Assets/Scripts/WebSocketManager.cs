using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketManager : MonoBehaviour
{
    // Add this at the top of your class with your other variables
    [SerializeField] private string serverUrl = "ws://localhost:8765";

    // --- SINGLETON SETUP ---
    // This static property allows any script to type 'WebSocketManager.Instance' 
    // to access this specific object from anywhere in the game.
    public static WebSocketManager Instance { get; private set; }

    // --- EVENTS ---
    // Other scripts subscribe to this event to listen for incoming messages.
    public event Action<string> OnMessageReceived;

    // --- NETWORK VARIABLES ---
    private ClientWebSocket webSocket;

    // CRITICAL: A ConcurrentQueue is thread-safe. 
    // We use this to pass messages from the background network thread to Unity's main thread.
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    private void Awake()
    {
        // Enforce the Singleton pattern: If another instance already exists, destroy this new one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance and tell Unity not to destroy this object when loading new scenes.
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        // Automatically connect as soon as the manager is ready
        Debug.Log("Initializing WebSocket connection...");
        await ConnectAsync(serverUrl);
    }

    private void Update()
    {
        // --- MAIN THREAD EXECUTION ---
        // Unity's Update() runs on the main thread. 
        // We check if the background thread put any messages in the queue.
        while (messageQueue.TryDequeue(out string message))
        {
            // Broadcast the message to all subscribed scripts safely on the main thread.
            OnMessageReceived?.Invoke(message);
        }
    }

    /// <summary>
    /// Connects to the WebSocket server. Call this once at the start of your game.
    /// </summary>
    public async Task ConnectAsync(string uri)
    {
        // if web socket is already connected
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            Debug.LogWarning("WebSocket is already connected.");
            return;
        }

        webSocket = new ClientWebSocket();

        try
        {
            await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            Debug.Log("WebSocket connected to: " + uri);

            // Start listening for messages in the background. 
            // The '_' discards the warning about not awaiting this task, 
            // because we want it to run indefinitely in the background.
            _ = ReceiveMessagesAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("WebSocket connection error: " + e.Message);
        }
    }

    /// <summary>
    /// Background loop that continuously listens for incoming network traffic.
    /// </summary>
    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4]; // 4KB buffer

        try
        {
            while (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("Server closed connection.");
                    await DisconnectAsync();
                }
                else
                {
                    // Convert the raw bytes into a string
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Push the string into the queue so Update() can broadcast it on the main thread
                    messageQueue.Enqueue(message);
                }
            }
        }
        catch (Exception e)
        {
            // Only log errors if we aren't intentionally closing the socket
            if (webSocket != null && webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                Debug.LogError("Receive error: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Sends a string message to the connected WebSocket server.
    /// </summary>
    public async void SendMessageToServer(string message)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
            Debug.LogWarning("Cannot send message. WebSocket is not connected.");
        }
    }

    /// <summary>
    /// Safely closes the connection and cleans up memory.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (webSocket != null)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            }
            webSocket.Dispose();
            webSocket = null;
            Debug.Log("WebSocket disconnected.");
        }
    }

    private async void OnDestroy()
    {
        // Clean up when the GameObject is destroyed
        if (Instance == this)
        {
            await DisconnectAsync();
        }
    }

    private async void OnApplicationQuit()
    {
        // Ensure connection closes when the player quits the game
        await DisconnectAsync();
    }
}