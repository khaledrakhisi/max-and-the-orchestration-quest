using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMovingwalkway : MonoBehaviour
{

	[Header("Settings")]
	public Vector2 targetSpeed; // The speed when the belt is fully ON
	public float smoothRate = 2.0f; // How fast it speeds up/slows down (Higher = Faster)
	public bool isOn;

	private SurfaceEffector2D surfaceEffector2d;
	private AnimatedTexture animatedTexture; // Assuming this script has a .speed Vector2 property

	private float currentSpeedX;

	void Start()
	{
		surfaceEffector2d = GetComponent<SurfaceEffector2D>();
		animatedTexture = GetComponent<AnimatedTexture>();

		// Initialize at 0 or full speed depending on starting state
		currentSpeedX = isOn ? targetSpeed.x : 0f;
	}

	void FixedUpdate()
	{
		// 1. Determine the goal speed based on the isOn toggle
		float goalSpeed = isOn ? targetSpeed.x : 0f;

		// 2. Smoothly transition currentSpeedX towards the goalSpeed
		// MoveTowards is better than Lerp here because it ensures it actually reaches 0
		currentSpeedX = Mathf.MoveTowards(currentSpeedX, goalSpeed, smoothRate * Time.fixedDeltaTime);

		// 3. Apply the calculated speed to the Texture Animation
		if (animatedTexture != null)
		{
			animatedTexture.speed = new Vector2(currentSpeedX, targetSpeed.y);
		}

		// 4. Apply the calculated speed to the Physics Effector
		if (surfaceEffector2d != null)
		{
			// Using negative currentSpeedX to match your original logic
			surfaceEffector2d.speed = -currentSpeedX;

			// Optimization: Disable effector if speed is 0 to save physics calculations
			surfaceEffector2d.enabled = currentSpeedX != 0;
		}
	}
}