using UnityEngine;

public class LongJump : Jump {

	public float longJumpDelay = .15f;
	public float longJumpMultiplier = 1.5f;
	private bool canLongJump;
	public bool isLongJumping;
	private bool jumpButtonPressed;

	private float holdTime = 0f;
	private bool isManual = false;

	protected override void Update(){

		#if UNITY_STANDALONE || UNITY_WEBPLAYER
			var jumpButton = inputState.GetButtonValue (inputButtons [0]);
			holdTime = inputState.GetButtonHoldTime(inputButtons[0]);
		#elif UNITY_ANDROID || UNITY_IOS
		if(!isManual)
			jumpButtonPressed = CrossPlatformInputManager.GetButton("Jump");
			if(jumpButtonPressed){
				holdTime += Time.deltaTime;
			}else{
				holdTime = 0f;
			}
		#endif

		//Debug.Log (holdTime);

		// if (!jumpButtonPressed)
		// 	canLongJump = false;

		if (collisionState.standing && isLongJumping) {
			isLongJumping = false;
			jumpButtonPressed = false;
			isManual = false;
		}

		base.Update ();

		if (canLongJump && !collisionState.standing && holdTime > longJumpDelay) {
			var vel = playerRigidBody2d.linearVelocity;
			var tempSpeed = jumpSpeed;

			if (collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower ().Contains ("liquid")) {				
				tempSpeed /= 3f;
			}

			playerRigidBody2d.linearVelocity = new Vector2 (vel.x, tempSpeed * longJumpMultiplier);

			canLongJump = false;
			isLongJumping = true;
		}
	}

	protected override void OnJump(){
		base.OnJump ();
		canLongJump = true;
	}

	public void DoLongJump(){
		isManual = true;
		holdTime = 1f;
		jumpButtonPressed = true;
		OnJump ();
	}

}
