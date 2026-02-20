using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonState
{
	public bool value;
	public float holdTime = 0;
	public bool justPressed = false;
}

public enum Directions
{
	Right = 1,
	Left = -1
}

public class InputState : MonoBehaviour
{

	public bool inputEnabled = true;
	public float zeroNumber = .02f;
	public bool isManual = false;
	public Directions direction = Directions.Right;
	public float absVelX = 0f;
	public float absVelY = 0f;
	//public float VelY = 0f;

	private Rigidbody2D body2d;
	private Dictionary<Buttons, ButtonState> buttonStates = new Dictionary<Buttons, ButtonState>();

	void Awake()
	{
		body2d = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		absVelX = body2d.linearVelocity.x;
		absVelY = body2d.linearVelocity.y;

		try
		{
			var mobileInput = GameObject.Find("MobileSingleStickControl");
			var canvas = mobileInput.GetComponent<Canvas>();
			canvas.enabled = inputEnabled;

		}
		catch
		{
		}
	}

	public void SetButtonValue(Buttons key, bool value)
	{
		if (!buttonStates.ContainsKey(key))
			buttonStates.Add(key, new ButtonState());

		var state = buttonStates[key];

		// 1. Check for "Just Pressed" (Was false, now true)
		if (state.value && !value)
		{
			state.justPressed = true;
			state.holdTime = 0;// Reset hold time on fresh press
		}
		else
		{
			state.justPressed = false;// Reset immediately after the first frame

			// Handle hold time logic
			if (state.value && value)
			{
				state.holdTime += Time.deltaTime;
			}
			else if (state.value && !value)
			{
				state.holdTime = 0;
			}
		}
		state.value = value;
	}

	public bool GetButtonDown(Buttons key)
	{
		if (!inputEnabled)
			return false;

		if (buttonStates.ContainsKey(key))
			return buttonStates[key].justPressed;
		else
			return false;
	}

	public bool GetButtonValue(Buttons key)
	{
		if (!inputEnabled)
			return false;

		if (buttonStates.ContainsKey(key))
			return buttonStates[key].value;
		else
			return false;
	}

	public float GetButtonHoldTime(Buttons key)
	{
		if (!inputEnabled)
			return 0;

		if (buttonStates.ContainsKey(key))
			return buttonStates[key].holdTime;
		else
			return 0;
	}

	public void DoReverseDirection()
	{
		if (direction == Directions.Left)
		{
			direction = Directions.Right;
		}
		else
		{
			direction = Directions.Left;
		}
	}
}
