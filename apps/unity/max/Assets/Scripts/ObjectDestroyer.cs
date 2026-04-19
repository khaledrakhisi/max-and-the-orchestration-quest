using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
public class ObjectDestroyer : MonoBehaviour {

	public LayerMask Layer;
	public Vector3 collisionCubeSize;

	private Collider2D target;
	private Color debugCollisionColor = Color.red; 
    public ImageDownloaderDevice imageDownloaderDevice;
    public Factory factoryInstance;
    private bool objectDestroy = false;
	// Use this for initialization
	void Start () {
       imageDownloaderDevice = ImageDownloaderDevice.Instance;
       factoryInstance = Factory.Instance;
       WebSocketManager.Instance.OnMessageReceived+=DestroyImage;
	}
	
	// Update is called once per frame
	void Update () {
		var pos = Vector3.zero;
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		target = Physics2D.OverlapBox(pos, collisionCubeSize, 1, Layer);
		if (target) {
			Destroy (target.gameObject);
			Debug.Log($"target tag: {target.gameObject.tag}");
			// objectDestroy = true;
			if (imageDownloaderDevice != null && target.gameObject.CompareTag("Block"))
            {
            	Debug.Log("destroy image");
                Debug.Log($"selected image: {imageDownloaderDevice.selectedImage}");
                WebSocketManager.Instance.SendMessageToServer($"remove_image:{imageDownloaderDevice.selectedImage}");
            }else if (imageDownloaderDevice != null && target.gameObject.CompareTag("container_block"))
            {
            	Debug.Log("destroy container");
                Debug.Log($"selected container: {factoryInstance.FactoryContainerName}");
                WebSocketManager.Instance.SendMessageToServer($"remove_container:{factoryInstance.FactoryContainerName}");
            }
            else{
                Debug.LogError("imageDownloaderDevice selected image Singleton is missing from the scene!");
            }
			
			
		}
	}
	void OnDrawGizmos(){
		Gizmos.color = debugCollisionColor;
		//draw hit area
		var bpos = Vector3.zero;
		bpos.x += transform.position.x;
		bpos.y += transform.position.y;
		Gizmos.DrawCube (bpos, collisionCubeSize);
	}
	void DestroyImage(string message)
    {
        
        try
        {
            // Attempt to parse the message. 
            // Note: If you have multiple objects listening, you should check the message
            // to ensure it's actually meant for this downloader (e.g., check for a "type" field)
            Debug.Log($"message:" + message);
        }
        catch (Exception e)
        {
            // Ignore messages that aren't DockerImage lists (they might be meant for a ChatBox or other object)
            Debug.Log($"Message was not a DockerImage list: {e.Message}");
        }
    }
}
