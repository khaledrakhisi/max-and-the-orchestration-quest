using UnityEngine;
using System.Collections;

public class Switch3 : MonoBehaviour
{

	public enum Switch3States
	{
		Off = 0,
		On,
		Unfunctional
	}

	public LayerMask collisionLayer;
	public Vector3 collisionCubeSize;
	public Switch3States state = Switch3States.Off;

	private bool _stateChanged;
	public bool StateChanged
	{
		get
		{
			return _stateChanged;
		}
	}
	private Collider2D target;
	private float _t;
	private Switch3States currentState = Switch3States.Off;

	private Animator animator;
	private PlayerInteraction playerInteraction;
	private Vector3 collisionCubeCenterPosition = Vector3.zero;
	private Color debugCollisionColor = Color.red;
	// Use this for initialization
	void Start()
	{
		collisionCubeCenterPosition = Vector3.zero;
		animator = GetComponent<Animator>();
		SetState(state);
	}

	void FixedUpdate()
	{
		if (_stateChanged)
		{
			_t += .1f;
			if (_t > 1.4f)
			{
				_stateChanged = false;
				_t = 0f;
			}

			//if state changed by editor or by direct init
		}
		else if (!_stateChanged && state != currentState)
		{
			DoSwitch();
		}

		var pos = collisionCubeCenterPosition;
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		target = Physics2D.OverlapBox(pos, collisionCubeSize, 1, collisionLayer);
		if (target)
		{
			if (tag == "Switch-3States" && target.tag == "Player" && state != Switch3States.Unfunctional)
			{
				playerInteraction = target.GetComponent<PlayerInteraction>();
				if (playerInteraction.justInteracted && !_stateChanged)
				{

					if (state == Switch3States.Off)
						state = Switch3States.On;
					else
						state = Switch3States.Off;

					DoSwitch();
				}
			}
		}
	}

	void DoSwitch()
	{

		SetState(state);
		_stateChanged = true;
		currentState = state;
	}

	void SetState(Switch3States state)
	{
		animator.SetInteger("AnimState", (int)state);
	}
	Switch3States GetState()
	{
		return (Switch3States)animator.GetInteger("AnimState");
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
