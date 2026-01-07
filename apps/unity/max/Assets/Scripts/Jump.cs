using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Jump : AbstractBehavior {

	public float jumpSpeed = 200f;
	public float jumpDelay = .1f;
	public bool isShortJumping;
	public bool isKneeingBeforeJump;
	private float timeElapsed = 0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	protected virtual void Update () {

		#if UNITY_STANDALONE || UNITY_WEBPLAYER
			var canJump = inputState.GetButtonValue (inputButtons [0]);
			var holdTime = inputState.GetButtonHoldTime (inputButtons [0]);
		#elif UNITY_ANDROID || UNITY_IOS
			var canJump = CrossPlatformInputManager.GetButtonDown("Jump");
			var holdTime = inputState.GetButtonHoldTime (inputButtons [0]);

			if(!inputState.inputEnabled)
				canJump = false;
		#endif

		if (collisionState.standing && isShortJumping) {
			isShortJumping = false;
		}

		if (collisionState.standing && !isShortJumping) {
			if (canJump && holdTime < .3f) {
				//if (inputState.absVelX == 0)
					BeforeJump ();
				//else
				//	OnJump ();

			}
			
			if (isKneeingBeforeJump) {
				timeElapsed += Time.deltaTime;
				if (timeElapsed >= .3f || inputState.absVelX != 0) {
					OnJump ();
					timeElapsed = 0f;
				}
			}
		}
	}

	protected virtual void OnJump(){
		var vel = playerRigidBody2d.linearVelocity;
		playerRigidBody2d.linearVelocity = new Vector2 (vel.x, jumpSpeed);
		isShortJumping = true;
		isKneeingBeforeJump = false;
		//body2d.AddForce(new Vector2(0, 10), ForceMode2D.Impulse);
	}

	void OnDisable(){
		isShortJumping = false;
		isKneeingBeforeJump = false;
	}

	void BeforeJump(){
		isKneeingBeforeJump = true;
	}
}
