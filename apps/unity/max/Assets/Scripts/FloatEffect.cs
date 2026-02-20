using UnityEngine;

public class FloatEffect : MonoBehaviour
{
	private float startX;
	private float startY;

	[Header("Y-Axis Oscillation")]
	[Tooltip("How far up and down the object moves.")]
	public float floatRangeY = 0.25f;
	[Tooltip("How fast the object oscillates up and down.")]
	public float floatSpeedY = 2f;
	private float randomPhaseOffsetY;

	[Header("X-Axis Oscillation")]
	[Tooltip("How far left and right the object moves.")]
	public float floatRangeX = 0.15f;
	[Tooltip("How fast the object oscillates left and right.")]
	public float floatSpeedX = 1.3f; // Set slightly different from Y to create organic loops
	private float randomPhaseOffsetX;

	void OnEnable()
	{
		// Record the initial X and Y positions as the center point
		startX = transform.localPosition.x;
		startY = transform.localPosition.y;

		// Pick random starting points for BOTH axes (between 0 and 2*PI)
		randomPhaseOffsetX = Random.Range(0f, Mathf.PI * 2f);
		randomPhaseOffsetY = Random.Range(0f, Mathf.PI * 2f);
	}

	void FixedUpdate()
	{
		// Calculate the new X and Y positions independently
		float newX = startX + Mathf.Cos((Time.time * floatSpeedX) + randomPhaseOffsetX) * floatRangeX;
		float newY = startY + Mathf.Cos((Time.time * floatSpeedY) + randomPhaseOffsetY) * floatRangeY;

		// Apply both to the transform, while leaving Z exactly as it was
		transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
	}
}