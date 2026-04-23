using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerPhysics : AbstractBehavior
{

	public enum DieWay
	{
		bySharpThing = 0,
		byBullet,
		byFalling,
		byPressing,
		bySpike,
		byDrowning,
		byStrongStrike,
		none
	}
	public bool eternal;
	public DieWay dieWay;
	public bool isDead = false;
	public bool isPassedOut = false;
	public bool isStandingUp = false;
	public bool isKneeing = false;
	public bool isDizzy = false;
	public float dieByFallingThreshold = 1.3f;
	public float passingOutByFallingThreshold = .8f;
	public float kneeingByFallingThreshold = .3f;
	public float passingOutDuration = 2f;
	public float kneeingDuration = .5f;
	public float dieByFallingVelocity = 5f;
	public float underwaterMaxTime = 5f;

	[SerializeField]
	private float _fallingTimeElapsed = 0f;
	public float fallingTimeElapsed
	{
		get { return _fallingTimeElapsed; }
	}
	[SerializeField]
	private float _inLiquidTimeElapsed = 0f;
	public float inLiquidTimeElapsed
	{
		get { return _inLiquidTimeElapsed; }
	}

	public GameObject blood;
	private GameObject clone;
	public Smoke smoke;
	private Smoke cloneSmoke;
	private GameObject bubbles;
	private GameObject electricityShoke;

	private Walk walkBehaviour;
	private Duck duckBehaviour;
	private LadderClimbing ladderClimbingBehaviour;
	private bool allSet;
	private bool scriptsDisabled;
	private int bleedMax = 7;
	private int bleedCount = 0;
	private float timeElapsed = 0f;

	// Use this for initialization
	void Start()
	{
		walkBehaviour = GetComponent<Walk>();
		duckBehaviour = GetComponent<Duck>();
		ladderClimbingBehaviour = GetComponent<LadderClimbing>();
	}

	// Update is called once per frame
	void Update()
	{

		if (eternal && isDead)
		{
			isDead = false;
			dieWay = DieWay.none;
			_fallingTimeElapsed = 0f;
			_inLiquidTimeElapsed = 0f;
		}

		if (bubbles && !(collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid")))
		{
			Destroy(bubbles);
		}

		// disable box collider when player drowning has finished
		if (isDead && playerBoxColliderEnabled && !(collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid")))
		{
			//playerBoxCollider2d.enabled = false;
			ToggleColliders(GetComponent<BoxCollider2D>(), false, this.GetType().ToString());
		}

		// calculate falling duration
		if ((!collisionState.standing || collisionState.standing.tag.Contains("slash")) &&
			!(collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid")) &&
			inputState.absVelY < inputState.zeroNumber &&
			!collisionState.onLadder && !collisionState.onRope &&
			!collisionState.onLedgeGrabbing && !collisionState.onLedgeStanding &&
			!isDead)
		{

			_fallingTimeElapsed += Time.deltaTime;
			//Debug.Log ("duration=" + fallingDuration + "  " + "velY=" + inputState.absVelY);
		}
		else if (collisionState.onLadder || collisionState.onRope || collisionState.onLedgeGrabbing || collisionState.onLedgeStanding)
		{
			_fallingTimeElapsed = 0f;
		}

		if (collisionState.standing && !collisionState.standing.tag.Contains("slash") && kneeingByFallingThreshold > 0f && fallingTimeElapsed > 0f)
		{
			//			Audio_Player_Thud_Play ();
			//Debug.Log ("thud");
			try
			{
				audioPlayer.clipsIndexBegin = 7;
				audioPlayer.clipsIndexEnd = 7;
				audioPlayer.volume = .5f;
				audioPlayer.isLoop = false;
				audioPlayer.Audio_Play_Clip();

			}
			catch (Exception ex)
			{
				Debug.Log("Audio Error : " + ex.Message);
			}

		}

		// die by falling
		if (collisionState.standing && dieByFallingThreshold > 0 && fallingTimeElapsed >= dieByFallingThreshold && inputState.absVelY <= -dieByFallingVelocity)
		{
			isDead = true;
			dieWay = DieWay.byFalling;
		}

		// passing out by falling
		if (collisionState.standing && kneeingByFallingThreshold > 0 && fallingTimeElapsed >= passingOutByFallingThreshold)
		{
			//			Debug.Log ("kneeeeeeeeeeeee");
			isPassedOut = true;
			_fallingTimeElapsed = 0f;
			ToggleScripts(false, this.GetType().ToString());
		}
		if (isPassedOut)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed >= passingOutDuration && !isDead)
			{
				isPassedOut = false;
				isStandingUp = true;
				timeElapsed = 0f;
			}
		}
		if (isDizzy && isStandingUp && animator.speed == 0f)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed >= 5f)
			{
				isDizzy = false;
				animator.speed = 1f;
				timeElapsed = 0f;
			}
		}
		// kneeing by falling
		if (collisionState.standing && kneeingByFallingThreshold > 0 && fallingTimeElapsed >= kneeingByFallingThreshold)
		{
			//			Debug.Log ("kneeeeeeeeeeeee");
			isKneeing = true;
			_fallingTimeElapsed = 0f;

		}
		else if (collisionState.standing && fallingTimeElapsed < kneeingByFallingThreshold)
		{
			_fallingTimeElapsed = 0f;
		}
		if (isKneeing)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed >= kneeingDuration)
			{
				isKneeing = false;
				timeElapsed = 0f;
			}
		}

		// die by drowning
		if (collisionState.onEnvironmentElement_playerHead && collisionState.onEnvironmentElement_playerHead.tag.ToLower().Contains("liquid") && !isDead)
		{
			_inLiquidTimeElapsed += Time.deltaTime;
			Bubbling();
			//Debug.Log ("duration=" + fallingDuration + "  " + "velY=" + inputState.absVelY);
		}
		else
		{
			_inLiquidTimeElapsed = 0f;
		}
		if (underwaterMaxTime > 0 && (inLiquidTimeElapsed >= underwaterMaxTime))
		{
			//|| (inLiquidTimeElapsed >= underwaterMaxTime / 2f && collisionState.onEnvironmentElement_playerHead.GetComponent<Liquid>().liquidType == Liquid.LiquidTypes.Acid)
			isDead = true;
			dieWay = DieWay.byDrowning;

			Destroy(bubbles);
		}

		// die by being pressed
		if (collisionState.standing && collisionState.ceiling)
		{
			//Debug.Log ("pressing");
			if (collisionState.ceiling.gameObject.tag.StartsWith("AutoMoving"))
			{
				if (bleedCount <= bleedMax)
				{
					Bleeding();
					bleedCount++;
				}
				isDead = true;
				dieWay = DieWay.byPressing;

				playerRigidBody2d.linearVelocity = Vector2.zero;
				playerRigidBody2d.isKinematic = true;

				ToggleColliders(GetComponent<BoxCollider2D>(), false, this.GetType().ToString());
				ToggleColliders(GetComponent<CircleCollider2D>(), false, this.GetType().ToString());
			}
		}
	}

	void FixedUpdate()
	{

		if (!collisionState.onEnvironmentElement)
		{

			if (scriptsDisabled && !isPassedOut && !isStandingUp)
			{
				ToggleScripts(true, GetType().ToString());
				scriptsDisabled = false;
			}
			if ((walkBehaviour.running || walkBehaviour.walking || duckBehaviour.fourswalking) && dieWay != DieWay.byPressing)
			{
				if (circleColliderSharedMaterial.name == "NormalFriction")
				{
					circleColliderSharedMaterial = (PhysicsMaterial2D)Resources.Load("PhysicMaterials/ZeroFriction");
					//ToggleColliders (GetComponent<CircleCollider2D>(), false, this.GetType ().ToString ());
					//ToggleColliders (GetComponent<CircleCollider2D>(), true, this.GetType ().ToString ());
					//playerCircleCollider2d.enabled = false;
					//playerCircleCollider2d.enabled = true;
				}

			}
			else if ((!walkBehaviour.running && !walkBehaviour.walking && !duckBehaviour.fourswalking) && dieWay != DieWay.byPressing)
			{
				if (circleColliderSharedMaterial.name == "ZeroFriction")
				{
					circleColliderSharedMaterial = (PhysicsMaterial2D)Resources.Load("PhysicMaterials/NormalFriction");
					//ToggleColliders (GetComponent<CircleCollider2D>(), false, this.GetType ().ToString ());
					//ToggleColliders (GetComponent<CircleCollider2D>(), true, this.GetType ().ToString ());
					//playerCircleCollider2d.enabled = false;
					//playerCircleCollider2d.enabled = true;
				}
			}

		}
		else if (collisionState.onEnvironmentElement && collisionState.onEnvironmentElement.tag.ToLower().Contains("liquid"))
		{
			isPassedOut = false;

			// if in the water and not standing (not reaching the bottom)
			if (!collisionState.standing && !ladderClimbingBehaviour.onLadderDetected)
			{
				ToggleScripts(false, GetType().ToString());
				scriptsDisabled = true;

				// if in the water and standing (reaching the bottom) or on ladder
			}
			else if ((collisionState.standing || ladderClimbingBehaviour.onLadderDetected) && !isDead)
			{
				if (scriptsDisabled && !isPassedOut && !isStandingUp)
				{
					ToggleScripts(true, GetType().ToString());
					scriptsDisabled = false;
				}
			}
		}

		//Debug.Log (circleCollider2d.sharedMaterial.name);
	}

	void Bleeding()
	{
		if (blood)
		{
			clone = Instantiate(blood, transform.position, Quaternion.identity) as GameObject;
			clone.transform.parent = this.transform;
			clone.gameObject.tag = "Untagged";
		}
	}
	void Smoking()
	{
		if (smoke && !cloneSmoke)
		{
			cloneSmoke = Instantiate(smoke, new Vector3(transform.position.x, transform.position.y - .2f, transform.position.z), Quaternion.identity) as Smoke;
			cloneSmoke.transform.parent = this.transform;
			cloneSmoke.duration = 4f;
			cloneSmoke.smokeScale = .5f;
		}
	}
	void Bubbling()
	{
		if (!bubbles)
		{
			bubbles = Instantiate(Resources.Load("Bubbles"), new Vector3(transform.position.x, transform.position.y + .2f, transform.position.z), Quaternion.identity) as GameObject;
			bubbles.transform.parent = this.transform;
		}
	}
	void ElectricityShoke()
	{
		if (!electricityShoke)
		{
			electricityShoke = Instantiate(Resources.Load("BoltSizzle"), new Vector3(transform.position.x, transform.position.y - .2f, transform.position.z), Quaternion.identity, this.transform) as GameObject;
			electricityShoke.GetComponent<Animator>().SetInteger("AnimState", 1);
			electricityShoke.GetComponent<SpriteRenderer>().sortingOrder = 18;
			electricityShoke.transform.localScale = new Vector3(.2f, .1f, 0f);
			//electricityShoke.transform.parent = ;
		}
	}

	void DoReaction(Collision2D collisionTarget, Collider2D colliderTarget)
	{
		GameObject target = collisionTarget != null ? collisionTarget.gameObject : colliderTarget.gameObject;
		//Vector2 targetPosision = collisionTarget != null ? collisionTarget.transform.position : colliderTarget.transform.position;
		//Rigidbody2D targetBody2d = collisionTarget != null ? collisionTarget.gameObject.GetComponent<Rigidbody2D> () : colliderTarget.gameObject.GetComponent<Rigidbody2D> ();
		string targetTag = collisionTarget != null ? collisionTarget.gameObject.tag : colliderTarget.gameObject.tag;
		var targetRigidBody2d = target.GetComponent<Rigidbody2D>();

		//Debug.Log (targetTag);
		if (targetTag == "Deadly-saw" || targetTag == "Enemy-Hellhound")
		{
			Bleeding();
			isDead = true;
			dieWay = DieWay.bySharpThing;

		}
		else if (targetTag == "Deadly-spyray" || targetTag == "Deadly-explosion" || targetTag == "Deadly-laser")
		{
			if (targetTag == "Deadly-explosion" && !target.GetComponent<Explode1>().explode)
				return;
			Bleeding();
			Smoking();
			ElectricityShoke();
			isDead = true;
			dieWay = DieWay.byBullet;

		}
		else if (targetTag == "Deadly-spike")
		{
			Bleeding();
			isDead = true;
			dieWay = DieWay.bySpike;

		}
		else if (targetTag == "ChainLink")
		{
			return;

		}
		else if (targetTag == "Deadly-rock")
		{

			if (Mathf.Abs(targetRigidBody2d.linearVelocity.x) > 2.2f || Mathf.Abs(targetRigidBody2d.linearVelocity.y) > 2.2f)
			{
				Bleeding();
				isDead = true;
				dieWay = DieWay.byStrongStrike;

				playerRigidBody2d.linearVelocity = targetRigidBody2d.linearVelocity;
			}

			//Die by physics roles (such as getting hitted by something with hight mass or velocity)
		}
		else
		{
			// if (targetRigidBody2d) {
			// 	if (targetRigidBody2d.gravityScale > 0) {
			// 		//Debug.Log (targetRigidBody2d.velocity);
			// 		if (Mathf.Abs (targetRigidBody2d.linearVelocity.x) > 2.2f || Mathf.Abs (targetRigidBody2d.linearVelocity.y) > 2.2f) {
			// 			isDead = true;
			// 			dieWay = DieWay.byStrongStrike;
			// 			ToggleScripts (false, GetType ().ToString ());
			// 		}
			// 		playerRigidBody2d.linearVelocity = targetRigidBody2d.linearVelocity;
			// 	}
			// }
		}

	}
	void OnCollisionEnter2D(Collision2D target)
	{
		DoReaction(target, null);
	}
	void OnTriggerEnter2D(Collider2D target)
	{
		DoReaction(null, target);
	}
	void OnCollisionStay2D(Collision2D target)
	{
		//DoReaction (target, null);
	}

	void OffStandingUp()
	{
		isStandingUp = false;
		ToggleScripts(true, this.GetType().ToString());
	}

	private void Dizzy()
	{
		if (isDizzy)
		{
			Debug.Log("dizzzzzzzyyyyyy");
			animator.speed = 0;
		}
	}
}
