using System;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
public class Factory : MonoBehaviour
{
    public enum States
    {
        Off = 0,
        Input,
        Output,
    }
    public States state = States.Off;
    private States prevState = States.Off;
    public Rotate2D gear1;
    public Rotate2D gear2;
    public Smoke smoke;
    public AutoMovingwalkway conveyBelt;
    public MoveToPoint fence;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        imageDownloaderDevice = ImageDownloaderDevice.Instance;
        WebSocketManager.Instance.OnMessageReceived += Container_data;
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (state != previousState)
        {
            Debug.Log($"state_test:{state}");
            OnStateChanged(state);
            previousState = state;
        }
        if (state == States.Off)
        {
            if (prevState == States.Input)
            {
                if (statusPosted)
                {
                    statusPosted = false;
                }
                if (!statusPosted && infoBoard)
                {
                    infoBoard.DoShowOneMessage(">>> Error: No Image Found!", "Danger");
                    prevState = state;
                    Debug.Log("here");
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
            if (!statusPosted && infoBoard)
            {
                infoBoard.DoShowOneMessage(">>> Now Converting Image to Container . . . . . . . . . . . . . .", "Warning");
                statusPosted = true;
                prevState = state;
            }
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
                conveyBelt.targetSpeed = new Vector2(-1.5f, 0f);
            }

        }
        else if (state == States.Output)
        {
            if (prevState == States.Input)
            {
                if (statusPosted)
                {
                    statusPosted = false;
                }
                if (!statusPosted && infoBoard)
                {
                    infoBoard.DoShowOneMessage(">>> Image Converted to Container Successfully", "Success");
                    statusPosted = true;
                    prevState = state;
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
                conveyBelt.targetSpeed = new Vector2(1.5f, 0f);
            }
        }
    }

    void OnStateChanged(States newState)
    {
        if (newState == States.Output)
        {

            if (imageDownloaderDevice != null)
            {
                Debug.Log($"selected image: {imageDownloaderDevice.selectedImage}");
                WebSocketManager.Instance.SendMessageToServer($"create_container:{imageDownloaderDevice.selectedImage}:2:600");
            }
            else
            {
                Debug.LogError("imageDownloaderDevice selected image Singleton is missing from the scene!");
            }


            // // Subscribe only once
            WebSocketManager.Instance.OnMessageReceived += Container_data;
        }
    }
    void OnDestroy()
    {
        WebSocketManager.Instance.OnMessageReceived -= Container_data;
    }

    void Container_data(string message)
    {

        try
        {
            // Attempt to parse the message. 
            // Note: If you have multiple objects listening, you should check the message
            // to ensure it's actually meant for this downloader (e.g., check for a "type" field)
            Debug.Log($"message:" + message);
            var obj = JObject.Parse(message);
            FactoryContainerName = obj["response"]["container_name"].ToString();

            Debug.Log($"container_name: {FactoryContainerName}");

        }
        catch (Exception e)
        {
            // Ignore messages that aren't DockerImage lists (they might be meant for a ChatBox or other object)
            Debug.Log($"Message was not a DockerImage list: {e.Message}");
        }
    }
}
