using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{

	public LayerMask collisionLayer;
	public Vector3 collisionCubeSize;
	public bool interacted = false;
	public AudioClip interactSound;
	public float interactionDelay = 2f;

	private Vector3 collisionCubeCenterPosition = Vector3.zero;
	private Color debugCollisionColor = Color.red;
	private Collider2D target;
	private AudioPlayer audioPlayer;
	private float timeElapsed = 0f;

	// Use this for initialization
	void Start()
	{
		audioPlayer = gameObject.AddComponent<AudioPlayer>();
		audioPlayer.audio_clips = new AudioClip[] { interactSound };
		audioPlayer.volume = 1f;
		audioPlayer.maxDistance = 30;
	}

	// Update is called once per frame
	void Update()
	{
		if (interacted)
			timeElapsed += Time.deltaTime;
		if (timeElapsed >= interactionDelay)
		{
			timeElapsed = 0f;
			interacted = false;
		}

		// player interaction
		var pos = collisionCubeCenterPosition;
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		target = Physics2D.OverlapBox(pos, collisionCubeSize, 1, collisionLayer);
		if (target)
		{

			if (target.GetComponent<PlayerInteraction>().interacted && !interacted)
			{
				interacted = true;
				if (interactSound)
					audioPlayer.Audio_Play_Independently();
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = debugCollisionColor;

		//draw hit area
		var bpos = collisionCubeCenterPosition;
		bpos.x += transform.position.x;
		bpos.y += transform.position.y;
		Gizmos.DrawWireCube(bpos, collisionCubeSize);

	}
}
