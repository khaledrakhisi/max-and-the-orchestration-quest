using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Jump : AbstractBehavior
{

	public float jumpSpeed = 200f;
	public float jumpDelay = .1f;
	public bool isShortJumping;
	public bool isKneeingBeforeJump;
	private float timeElapsed = 0f;

	void Start()
	{
	}

	protected virtual void Update()
	{

		bool canJump = false;
		float holdTime = 0f;

#if UNITY_STANDALONE || UNITY_WEBPLAYER
		canJump = inputState.GetButtonValue(inputButtons[0]);
		holdTime = inputState.GetButtonHoldTime(inputButtons[0]);
#elif UNITY_ANDROID || UNITY_IOS
            canJump = CrossPlatformInputManager.GetButtonDown("Jump");
            holdTime = inputState.GetButtonHoldTime (inputButtons [0]);

            if(!inputState.inputEnabled)
                canJump = false;
#endif

		if (collisionState.standing && isShortJumping)
		{
			isShortJumping = false;
		}

		if (collisionState.standing && !isShortJumping)
		{
			if (canJump && holdTime < .3f)
			{
				//if (inputState.absVelX == 0)
				BeforeJump();
				//else
				//  OnJump ();
			}

			if (isKneeingBeforeJump)
			{
				timeElapsed += Time.deltaTime;
				if (timeElapsed >= .3f || inputState.absVelX != 0)
				{
					OnJump();
					timeElapsed = 0f;
				}
			}
		}
	}

	protected virtual void OnJump()
	{
		var vel = playerRigidBody2d.linearVelocity;
		playerRigidBody2d.linearVelocity = new Vector2(vel.x, jumpSpeed);
		isShortJumping = true;
		isKneeingBeforeJump = false;
	}

	void OnDisable()
	{
		isShortJumping = false;
		isKneeingBeforeJump = false;
	}

	void BeforeJump()
	{
		isKneeingBeforeJump = true;
	}
}