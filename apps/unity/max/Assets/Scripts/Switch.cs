using UnityEngine;
using System.Collections;

public class Switch : MonoBehaviour
{
	public enum SwitchTypes
	{
		TwoStates = 0,
		ThreeStates,
		Button
	}
	public enum States
	{
		Off = 0,
		On,
		Unfunctional,
		Error
	}
	public LayerMask collisionLayer;
	public Vector3 collisionCubeSize;

	public SwitchTypes switchType = SwitchTypes.TwoStates;
	public States state = States.Off;

	public Machine powerSupply;

	[SerializeField]
	private bool _stateChanged;
	public bool StateChanged
	{
		get
		{
			return _stateChanged;
		}
	}

	private States prevState;
	private States currentState = States.Unfunctional;
	private Collider2D target;
	private float timeElapsed;
	private Animator animator;
	private Vector3 collisionCubeCenterPosition = Vector3.zero;
	private Color debugCollisionColor = Color.red;

	// Use this for initialization
	void Start()
	{
		collisionCubeCenterPosition = Vector3.zero;
		animator = GetComponent<Animator>();
		if (!animator)
		{
			animator = GetComponentInChildren<Animator>();
		}
		SetState(state);
	}

	void Update()
	{
		if (powerSupply)
		{
			if (powerSupply.machineState == Machine.MachineState.On && currentState == States.Unfunctional)
			{
				if (switchType == SwitchTypes.TwoStates)
					state = States.Off;

			}
			else if (powerSupply.machineState == Machine.MachineState.Off)
			{
				state = States.Unfunctional;
			}
		}

		if (_stateChanged)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > .3f)
			{
				_stateChanged = false;
				timeElapsed = 0f;
			}

			// if state changed by editor or by direct init
		}
		else if (!_stateChanged && state != currentState)
		{
			prevState = currentState;
			DoSwitch();
		}


		// player interaction
		var pos = collisionCubeCenterPosition;
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		target = Physics2D.OverlapBox(pos, collisionCubeSize, 1, collisionLayer);
		if (target)
		{

			if (switchType != SwitchTypes.Button && target.GetComponent<PlayerInteraction>().justInteracted && !_stateChanged)
			{

				if (powerSupply && powerSupply.machineState == Machine.MachineState.Off)
				{

					//Get Animator state after storing the Animator reference in my var 'animate'.
					AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
					int stateName = currentState.fullPathHash;
					//Then later I can call...
					//Reset Animation on first frame.
					animator.Play(stateName, 0, 0.0f);

					state = States.Error;
					_stateChanged = true;

					return;
				}

				if (state == States.Off)
				{
					state = switchType == SwitchTypes.TwoStates ? States.On : States.Unfunctional;
					prevState = States.Off;

				}
				else if (state == States.On)
				{
					state = switchType == SwitchTypes.TwoStates ? States.Off : States.Unfunctional;
					prevState = States.On;

				}
				else if (state == States.Unfunctional)
				{
					state = switchType == SwitchTypes.TwoStates ? States.Unfunctional/*remains unfunctional*/ : (prevState == States.Off ? States.On : States.Off);
				}

				DoSwitch();

			}
			else if (switchType == SwitchTypes.Button)
			{
				if (target.tag.ToLower() == "player" && target.GetComponent<PlayerPhysics>().isDead)
					state = States.Off;

				else
					state = States.On;
			}

		}
		else if (!target)
		{
			if (switchType == SwitchTypes.Button && state != States.Off && !_stateChanged)
				state = States.Off;
		}
	}

	void DoSwitch()
	{
		SetState(state);
		_stateChanged = true;
		currentState = state;
	}


	void SetState(States state)
	{
		try
		{
			animator.SetInteger("AnimState", (int)state);
		}
		catch
		{
		}
	}
	States GetState()
	{
		return (States)animator.GetInteger("AnimState");
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
