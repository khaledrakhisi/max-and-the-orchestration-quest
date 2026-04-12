using UnityEngine;
using UnityStandardAssets.CrossPlatformInput; // Added this since you use it in the Android block

public class LongJump : Jump
{

	public float longJumpDelay = .15f;
	public float longJumpMultiplier = 1.5f;
	private bool canLongJump;
	public bool isLongJumping;
	private bool jumpButtonPressed;

	private float holdTime = 0f;
	private bool isManual = false;

	protected override void Update()
	{

#if UNITY_STANDALONE || UNITY_WEBPLAYER
		// Fixed: Actually assign the value to jumpButtonPressed
		jumpButtonPressed = inputState.GetButtonValue(inputButtons[0]);
		holdTime = inputState.GetButtonHoldTime(inputButtons[0]);
#elif UNITY_ANDROID || UNITY_IOS
            // Fixed: Added brackets to ensure correct logic flow
            if(!isManual) {
                jumpButtonPressed = CrossPlatformInputManager.GetButton("Jump");
                if(jumpButtonPressed){
                    holdTime += Time.deltaTime;
                }else{
                    holdTime = 0f;
                }
            }
#endif

		if (collisionState.standing && isLongJumping)
		{
			isLongJumping = false;
			jumpButtonPressed = false;
			isManual = false;
		}

		base.Update();

		// THE FIX: Check for early release instead of delayed burst
		if (canLongJump && !collisionState.standing)
		{

			// If the player lets go of the button early, cancel the long jump
			if (!jumpButtonPressed && holdTime <= longJumpDelay)
			{
				var vel = playerRigidBody2d.linearVelocity;

				// Only cut velocity if they are still moving upwards
				if (vel.y > 0)
				{
					playerRigidBody2d.linearVelocity = new Vector2(vel.x, vel.y / longJumpMultiplier);
				}

				canLongJump = false; // Lock out further checks
				isLongJumping = false;
			}
			// If they successfully held it past the delay, lock in the Long Jump
			else if (holdTime > longJumpDelay)
			{
				canLongJump = false;
			}
		}
	}

	protected override void OnJump()
	{
		base.OnJump(); // Sets base jumpSpeed

		canLongJump = true;
		isLongJumping = true;

		// THE FIX: IMMEDIATELY apply long jump velocity to prevent mid-air stutter
		var vel = playerRigidBody2d.linearVelocity;
		var tempSpeed = jumpSpeed;

		if (collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid"))
		{
			tempSpeed /= 3f;
		}

		playerRigidBody2d.linearVelocity = new Vector2(vel.x, tempSpeed * longJumpMultiplier);
	}

	public void DoLongJump()
	{
		isManual = true;
		holdTime = 1f;
		jumpButtonPressed = true;
		OnJump();
	}
}