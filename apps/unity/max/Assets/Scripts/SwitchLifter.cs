using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLifter : MonoBehaviour
{

	public enum States
	{
		Wait = 0,
		On,
		Up,
		Down,
		Unfunctional
	}

	public LayerMask collisionLayer;
	public Vector3 cubeSize;
	public Lifter lifter;
	public int levelNumber = 0;
	public States lifterSwitchTask;

	public States state;
	private Animator animator;
	private PlayerInteraction playerInteraction;
	private Collider2D target;
	private Vector3 cubecenterPosition = new Vector3(0f, 0.15f, 0);

	[SerializeField]
	private bool _stateChanged;
	public bool StateChanged
	{
		get
		{
			return _stateChanged;
		}
	}
	private float timeElapsed;

	private Color debugCollisionColor = Color.yellow;
	// Use this for initialization
	void Start()
	{
		animator = GetComponent<Animator>();
		animator.SetInteger("AnimState", (int)lifterSwitchTask);
	}

	void Update()
	{
		if (lifter)
		{
			if (lifter.states == Lifter.States.Unfunctional && !_stateChanged)
				state = States.Unfunctional;

			else if (lifter.states != Lifter.States.Unfunctional && !_stateChanged)
				state = lifterSwitchTask;
		}

		if (state != States.Unfunctional && !_stateChanged)
		{

			if (lifter.isMoving)
			{
				state = States.Wait;

			}
			else
			{
				if (lifter.currentPoint == levelNumber)
				{
					state = lifterSwitchTask;

				}
				else if (lifter.currentPoint > levelNumber)
				{
					state = States.Down;

				}
				else if (lifter.currentPoint < levelNumber)
				{
					state = States.Up;
				}
			}
		}

		animator.SetInteger("AnimState", (int)state);
	}

	void FixedUpdate()
	{

		if (_stateChanged)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > .3f)
			{
				_stateChanged = false;
				timeElapsed = 0f;
			}
		}

		var pos = cubecenterPosition;
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		target = Physics2D.OverlapBox(pos, cubeSize, 1, collisionLayer);
		if (target)
		{
			if (target.tag == "Player")
			{
				playerInteraction = target.GetComponent<PlayerInteraction>();
				if (playerInteraction.justInteracted)
				{
					if (!lifter)
						return;

					if (state == States.Up && !_stateChanged)
					{
						lifter.destinationPoint = lifter.currentPoint == levelNumber ? ++lifter.destinationPoint : lifter.destinationPoint = levelNumber;

					}
					else if (state == States.Down && !_stateChanged)
					{
						lifter.destinationPoint = lifter.currentPoint == levelNumber ? --lifter.destinationPoint : lifter.destinationPoint = levelNumber;

					}
					else if (state == States.Unfunctional && !_stateChanged)
					{
						//beep
						state = States.On;
						//Debug.Log ("tud tud " + state);
					}
					_stateChanged = true;
				}
			}
		}
		else
		{

		}
	}


	void OnDrawGizmos()
	{

		//draw hit area
		Gizmos.color = Color.red;
		var bpos = cubecenterPosition;
		bpos.x += transform.position.x;
		bpos.y += transform.position.y;
		Gizmos.DrawWireCube(bpos, cubeSize);

		//draw connections
		Gizmos.color = debugCollisionColor;
		Vector2 pos = Vector2.zero;
		if (lifter)
		{
			pos = lifter.transform.position;
		}
		Gizmos.DrawWireSphere(pos, .05f);
		Gizmos.DrawLine(transform.position, pos);
	}
}
