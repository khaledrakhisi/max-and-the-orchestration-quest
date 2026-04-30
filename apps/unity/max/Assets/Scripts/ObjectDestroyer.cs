using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{

	enum DockerObjectType
	{
		NoDockerObject = 0,
		Image,
		Container
	}
	private bool dockerCommandTriggered = false;

	public LayerMask Layer;
	public Vector3 collisionCubeSize;

	private Collider2D target;
	private Color debugCollisionColor = Color.red;

	[SerializeField] private DockerObjectType dockerObjectType = DockerObjectType.NoDockerObject;
	public class Results
	{
		public string type;
		public string status;
		public string message;
		// public Response response;
	}
	private Results results;

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

			if (results != null && results.type == "remove_image")
			{
				if (results.status == "ok")
				{
					Destroy(target.gameObject);
					dockerCommandTriggered = false;
				}
				else if (results.status == "failed")
				{
					Debug.LogError(results.message);
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
		var pos = Vector3.zero;
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		target = Physics2D.OverlapBox(pos, collisionCubeSize, 1, Layer);

		if (target)
		{
			if (!dockerCommandTriggered)
			{
				string dockerComponentPhysicalName = "";
				dockerCommandTriggered = true;
				if (dockerObjectType == DockerObjectType.Image)
				{
					dockerComponentPhysicalName = target.gameObject.GetComponent<DockerImage>().imageName;

					// --- SEND COMMAND USING SINGLETON ---
					WebSocketManager.Instance.SendMessageToServer($"remove_image:{dockerComponentPhysicalName}");
				}
				else if (dockerObjectType == DockerObjectType.Container)
				{
					dockerComponentPhysicalName = target.gameObject.GetComponent<Container>().containerName;

					// --- SEND COMMAND USING SINGLETON ---
					WebSocketManager.Instance.SendMessageToServer($"remove_container:{dockerComponentPhysicalName}");
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = debugCollisionColor;
		//draw hit area
		var bpos = Vector3.zero;
		bpos.x += transform.position.x;
		bpos.y += transform.position.y;
		Gizmos.DrawCube(bpos, collisionCubeSize);
	}
}
