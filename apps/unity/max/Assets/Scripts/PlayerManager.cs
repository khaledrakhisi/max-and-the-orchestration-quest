using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerManager : AbstractBehavior
{

	public bool isManual = false;


	private bool gameOver;
	private bool idle;
	private float animationPlaySpeed;
	//private CollisionState collisionState;

	private Walk walkBehaviour;
	private LongJump longJump;
	private Duck duckBehaviour;
	private Push pushBehaviour;
	private LadderClimbing ladderClimbingBehaviour;
	private LedgeGrabbing ledgeGrabbingBehaviour;
	private WallSlide wallSlide;
	private RopeSlide ropeSlide;
	private PlayerSliping playerSlipBehavior;
	private float timeElapsed = 0f;

	void Start()
	{
		//inputState = GetComponent<InputState> ();
		//collisionState = GetComponent<CollisionState> ();

		walkBehaviour = GetComponent<Walk>();
		duckBehaviour = GetComponent<Duck>();
		pushBehaviour = GetComponent<Push>();
		wallSlide = GetComponent<WallSlide>();
		ropeSlide = GetComponent<RopeSlide>();
		longJump = GetComponent<LongJump>();
		ladderClimbingBehaviour = GetComponent<LadderClimbing>();
		ledgeGrabbingBehaviour = GetComponent<LedgeGrabbing>();
		playerSlipBehavior = GetComponent<PlayerSliping>();

		gameOver = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (gameOver)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed >= 2f && timeElapsed <= 3f)
			{
				inputState.inputEnabled = false;

				var dimScreen = GameObject.Find("DimScreen");
				if (dimScreen)
				{
					var dim = dimScreen.GetComponent<FadeObject>();
					dim.delay = 3f;
					dim.fadeMethod = FadeObject.FadeMethod.FadeIn;
				}

			}
			else if (timeElapsed >= 5f)
			{
				//SceneManager.LoadScene ("Jungle");
				SceneManager.LoadScene("presentation");
			}
		}

		if (collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid"))
		{
			animationPlaySpeed = .5f;

		}
		else
		{
			animationPlaySpeed = 1f;
		}

		if (isManual)
			return;

		// Dieing
		if (playerPhysicsBehavior.isDead)
		{
			if (!gameOver)
			{
				gameOver = true;

				if (playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.bySharpThing || playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.byStrongStrike)
				{
					ChangeAnimationState(11, animationPlaySpeed);

				}
				else if (playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.byFalling)
				{
					ChangeAnimationState(30, animationPlaySpeed);

				}
				else if (playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.byBullet)
				{
					ChangeAnimationState(35, animationPlaySpeed);

				}
				else if (playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.byPressing)
				{
					ChangeAnimationState(11, animationPlaySpeed);

				}
				else if (playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.bySpike)
				{
					ChangeAnimationState(21, animationPlaySpeed);

				}
				else if (playerPhysicsBehavior.dieWay == PlayerPhysics.DieWay.byDrowning)
				{
					ChangeAnimationState(23, animationPlaySpeed);
				}

				//ToggleEnabled (false);
				ToggleScripts(false, this.GetType().Name);
			}
			return;

			// Passing out
		}
		else if (playerPhysicsBehavior.isPassedOut)
		{
			ChangeAnimationState(30, animationPlaySpeed);
			return;
		}

		// Standing up
		if (playerPhysicsBehavior.isStandingUp && collisionState.standing)
		{
			if (animator.speed > 0f)
				ChangeAnimationState(31, animationPlaySpeed);
			return;
		}

		//idle
		//if (collisionState.standing && (inputState.absVelX > -inputState.zeroNumber && inputState.absVelX < inputState.zeroNumber) && !duckBehavior.ducking && !pushBehavior.onPullBlockDetected) {//idle
		if (collisionState.standing && !walkBehaviour.walking && !walkBehaviour.running &&
			!duckBehaviour.ducking && !pushBehaviour.onPullBlockDetected && !ladderClimbingBehaviour.onLadderDetected)
		{

			ChangeAnimationState(0, animationPlaySpeed);
			if (!idle)
			{
				idle = true;
			}
		}

		//run
		if (collisionState.standing && walkBehaviour.running)
		{
			ChangeAnimationState(1, animationPlaySpeed);
			idle = false;

			//walk
			//} else if ((inputState.absVelX < -inputState.zeroNumber || inputState.absVelX > inputState.zeroNumber) && collisionState.standing && !collisionState.onFense && !pushBehavior.onPushBlockDetected && !duckBehavior.ducking && walkBehavior.walking) {//walk
		}
		else if (walkBehaviour.walking)
		{
			ChangeAnimationState(12, animationPlaySpeed);
			idle = false;

			// walk reverse
		}
		else if (walkBehaviour.walkingReverse)
		{
			ChangeAnimationState(32, animationPlaySpeed);
			idle = false;

			//fourswalk
		}
		else if (duckBehaviour.fourswalking)
		{
			ChangeAnimationState(5, animationPlaySpeed);
			idle = false;

			//duck
			//}else if ((inputState.absVelX > -inputState.zeroNumber && inputState.absVelX < inputState.zeroNumber) && !collisionState.onFense && duckBehavior.ducking) {
		}
		else if (duckBehaviour.ducking)
		{
			ChangeAnimationState(3, animationPlaySpeed);
			idle = false;
		}

		//jump
		if (inputState.absVelY != 0 && !collisionState.standing && !collisionState.onRope)
		{
			//if(longJump.isShortJumping){
			idle = false;

			//in liquid
			if (collisionState.onEnvironmentElement_playerHead && collisionState.onEnvironmentElement_playerHead.tag.ToLower().Contains("liquid") &&
				!ladderClimbingBehaviour.onLadderDetected && !ledgeGrabbingBehaviour.onLedgeGrabbingDetected)
			{
				ChangeAnimationState(24, animationPlaySpeed);
				return;
			}

			//falling animation
			if (playerPhysicsBehavior.fallingTimeElapsed >= playerPhysicsBehavior.passingOutByFallingThreshold)
			{
				ChangeAnimationState(29, animationPlaySpeed);
			}
			else
			{

				if (inputState.absVelX != 0)
				{//run and jump
					ChangeAnimationState(26, animationPlaySpeed);

				}
				else if (inputState.absVelX == 0)
				{//if jump in place
					ChangeAnimationState(2, animationPlaySpeed);//2 is jump in place
				}
			}
		}

		//Kneeing before jumping
		if (longJump.isKneeingBeforeJump)
		{
			ChangeAnimationState(28, animationPlaySpeed);
		}

		//Kneeing After Jump Or Big Fall
		if (playerPhysicsBehavior.isKneeing && idle)
		{
			ChangeAnimationState(27, animationPlaySpeed);

		}
		else if (playerPhysicsBehavior.isKneeing && !idle)
		{
			playerPhysicsBehavior.isKneeing = false;
		}

		//push and pull
		if (collisionState.standing && collisionState.onPushBlock && !duckBehaviour.ducking)
		{
			//			Debug.Log (playerRigidBody2d.velocity.x);
			//			animationPlaySpeed += Mathf.Abs(playerRigidBody2d.velocity.x);
			//push
			if ((inputState.direction == Directions.Right && pushBehaviour.onPushBlockDetected) ||
				(inputState.direction == Directions.Left && pushBehaviour.onPushBlockDetected))
			{
				ChangeAnimationState(4, animationPlaySpeed);
				idle = false;

				//pull
			}
			else if ((inputState.direction == Directions.Right && pushBehaviour.onPullBlockDetected) ||
						(inputState.direction == Directions.Left && pushBehaviour.onPullBlockDetected))
			{
				ChangeAnimationState(9, animationPlaySpeed);
				idle = false;

				//just press "push/pull button"
			}
			else if ((inputState.direction == Directions.Right && pushBehaviour.onGrabBlockDetected) ||
						(inputState.direction == Directions.Left && pushBehaviour.onGrabBlockDetected))
			{
				ChangeAnimationState(10, animationPlaySpeed);
				idle = false;
			}
		}

		if (!collisionState.standing && wallSlide.onFenseDetected && inputState.absVelY == 0)
		{//stick to wall			
			ChangeAnimationState(7, animationPlaySpeed);
			idle = false;

		}
		else if (!collisionState.standing && wallSlide.onFenseDetected && inputState.absVelY != 0)
		{//wall sliding
			ChangeAnimationState(6, animationPlaySpeed);
			idle = false;

		}
		else if (!collisionState.standing && ropeSlide.slidingUpDetected)
		{//rope sliding up
			ChangeAnimationState(14, animationPlaySpeed);
			idle = false;

		}
		else if (!collisionState.standing && ropeSlide.slidingDownDetected)
		{//rope sliding down
			ChangeAnimationState(34, animationPlaySpeed);
			idle = false;

		}
		else if (!collisionState.standing && ropeSlide.swingingDetected)
		{//rope swinging right
			ChangeAnimationState(33, animationPlaySpeed);
			idle = false;

		}
		else if (!collisionState.standing && collisionState.onRope && (inputState.absVelY > -inputState.zeroNumber && inputState.absVelY < inputState.zeroNumber))
		{//stick to rope
			ChangeAnimationState(13, animationPlaySpeed);
			idle = false;

		}
		else if (ladderClimbingBehaviour.onLadderDetected && (inputState.absVelY < -inputState.zeroNumber || inputState.absVelY > inputState.zeroNumber))
		{//ladder
			ChangeAnimationState(15, animationPlaySpeed);
			idle = false;

		}
		else if (ladderClimbingBehaviour.onLadderDetected && (inputState.absVelY > -inputState.zeroNumber && inputState.absVelY < inputState.zeroNumber))
		{//ladder stick
			ChangeAnimationState(15, 0);
			idle = false;

			//Ledge
		}
		else if (ledgeGrabbingBehaviour.onLedgeGrabbingDetected && (inputState.absVelY > -inputState.zeroNumber && inputState.absVelY < inputState.zeroNumber))
		{//ledge grab
			ChangeAnimationState(18, animationPlaySpeed);
			idle = false;

		}
		else if (ledgeGrabbingBehaviour.onLedgeClimbingUpDetected)
		{//ledge climbing up
			ChangeAnimationState(19, animationPlaySpeed);
			idle = false;

		}
		else if (ledgeGrabbingBehaviour.onLedgeClimbingDownDetected)
		{//ledge climbing down
			ChangeAnimationState(20, animationPlaySpeed);
			idle = false;

		}
		else if (ledgeGrabbingBehaviour.onLedgeTeetering)
		{//ledge teetering
		 //Debug.Log("teetering");
			ChangeAnimationState(22, animationPlaySpeed);
			idle = false;
		}

		//sliping
		if ((playerSlipBehavior.isOnSlippySlash || playerSlipBehavior.isOnSlippyBackslash) && collisionState.standing)
		{
			ChangeAnimationState(8, animationPlaySpeed);
			idle = false;
		}
	}

	void ChangeAnimationState(int value, float speed = 1f)
	{
		animator.SetInteger("AnimState", value);
		animator.speed = speed;
	}
}
