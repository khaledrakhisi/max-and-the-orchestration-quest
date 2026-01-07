using UnityEngine;
using System.Collections;

public class LadderClimbing : AbstractBehavior
{

	public float climbingVelocity = .5f;
	public bool onLadderDetected;

	//	private LadderPlatform ladderPlatform;
	protected float defaultGravityScale;
	protected float defaultDrag;
	protected int defaultSortingOrder;
	//private LongJump jumpBehavior;
	private Walk walkBehaviour;
	private SpriteRenderer playerSpriteRenderer;
	// Use this for initialization
	void Start()
	{
		defaultGravityScale = playerRigidBody2d.gravityScale;
		defaultDrag = playerRigidBody2d.linearDamping;
		walkBehaviour = GetComponent<Walk>();
		playerSpriteRenderer = GetComponent<SpriteRenderer>();
		if (playerSpriteRenderer)
			defaultSortingOrder = playerSpriteRenderer.sortingOrder;
		//jumpBehavior = GetComponent<LongJump> ();
	}

	// Update is called once per frame
	protected virtual void Update()
	{

		var upButtonPressed = inputState.GetButtonValue(inputButtons[0]);
		var downButtonPressed = inputState.GetButtonValue(inputButtons[1]);
		var rightButtonPressed = inputState.GetButtonValue(inputButtons[2]);
		var leftButtonPressed = inputState.GetButtonValue(inputButtons[3]);

		// climb up (from ground)
		if (collisionState.onLadder && collisionState.standing && (upButtonPressed))
		{
			if (!onLadderDetected)
			{
				OnStick();
				ToggleScripts(false, this.GetType().ToString().ToString());
				onLadderDetected = true;
			}

			// climb down (from platform)
		}
		else if (collisionState.onLadder && collisionState.standing && (downButtonPressed))
		{
			//if (!onLadderDetected && collisionState.standing.tag.ToLower() == "platform") {
			if (!onLadderDetected && collisionState.onLadder_playerBottom)
			{
				OnStick();
				ToggleScripts(false, this.GetType().ToString().ToString());
				onLadderDetected = true;
			}

			//grab the ladder while in the air
			//} else if ((collisionState.onLadder && !collisionState.standing && !collisionState.onLadderPlatform && (upButtonPressed || downButtonPressed))) {
		}
		else if (collisionState.onLadder && !collisionState.standing && (upButtonPressed || downButtonPressed))
		{
			if (!onLadderDetected)
			{
				OnStick();
				ToggleScripts(false, this.GetType().ToString().ToString());
				onLadderDetected = true;
			}

			//get off the ladder by going left or right
		}
		else if (rightButtonPressed && !collisionState.rightObject)
		{
			if (onLadderDetected)
			{
				var velX = 0f;
				velX = rightButtonPressed ? climbingVelocity * 2 : climbingVelocity * -2;
				playerRigidBody2d.linearVelocity = new Vector2(velX, 0f);

				OffLadder();
			}

		}
		else if (leftButtonPressed && !collisionState.leftObject)
		{
			if (onLadderDetected)
			{
				var velX = 0f;
				velX = rightButtonPressed ? climbingVelocity * 2 : climbingVelocity * -2;
				playerRigidBody2d.linearVelocity = new Vector2(velX, 0f);

				OffLadder();
			}

			//off the ladder by reaching the ground (apart platforms)
		}
		else if (!collisionState.onLadder || !collisionState.onLadder_playerBottom)
		{
			if (onLadderDetected)
			{
				OffLadder();
			}
		}

		if (onLadderDetected)
		{

			var velY = 0f;
			var velX = 0f;

			if (upButtonPressed && collisionState.onLadder_playerHead)
			{
				velY = climbingVelocity;

			}
			else if (downButtonPressed && collisionState.onLadder_playerBottom)
			{
				velY = climbingVelocity * -1;
			}

			if (collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid"))
			{
				velY /= 2f;
			}

			//			// AUDIO
			//			if (upButtonPressed || downButtonPressed) {				
			//				audioPlayer.volume = .5f;
			//				audioPlayer.isLoop = false;
			//				if (collisionState.onLadder) {
			//					audioPlayer.clipsIndexBegin = 9;
			//					audioPlayer.clipsIndexEnd = 9;
			//					audioPlayer.Audio_Play_Clip ();
			//				}
			//			} else {
			//				audioPlayer.Audio_Stop_Clip ();
			//			}

			playerRigidBody2d.linearVelocity = new Vector2(velX, velY);

			if (walkBehaviour.enabled)
			{
				ToggleScripts(false, this.GetType().ToString());
			}
		}
	}

	protected virtual void OnStick()
	{
		//Debug.Log ("injaa");
		playerRigidBody2d.gravityScale = 0;
		playerRigidBody2d.linearDamping = 0;
		playerRigidBody2d.transform.position = new Vector2(collisionState.onLadder.transform.position.x, transform.position.y);
		playerSpriteRenderer.sortingOrder = 20;

		ToggleCollidersTrigger(GetComponent<CircleCollider2D>(), true, this.GetType().ToString());
		ToggleCollidersTrigger(GetComponent<BoxCollider2D>(), true, this.GetType().ToString());
		//playerCircleCollider2d.isTrigger = true;
		//playerBoxCollider2d.isTrigger = true;
	}

	public void OffLadder()
	{
		if (onLadderDetected)
		{
			onLadderDetected = false;
			playerRigidBody2d.gravityScale = defaultGravityScale;
			playerRigidBody2d.linearDamping = defaultDrag;
			playerSpriteRenderer.sortingOrder = defaultSortingOrder;

			ToggleCollidersTrigger(GetComponent<CircleCollider2D>(), false, this.GetType().ToString());
			ToggleCollidersTrigger(GetComponent<BoxCollider2D>(), false, this.GetType().ToString());

			//playerCircleCollider2d.isTrigger = false;
			//playerBoxCollider2d.isTrigger = false;

			//Debug.Log ("off ladder");
			ToggleScripts(true, this.GetType().ToString().ToString());
		}
	}

	void OnDisable()
	{
		OffLadder();
	}
}
