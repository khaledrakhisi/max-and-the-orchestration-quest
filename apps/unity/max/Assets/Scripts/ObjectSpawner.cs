using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{

	public GameObject[] prefabs;

	private float rotationZ = 45f;
	public bool isOn = false;
	private float delay = 2.0f;
	public Vector2 delayRange = new Vector2(1, 2);
	public Vector2 rotationZRange = new Vector2(-380, 380);

	private bool running;
	// Use this for initialization
	void Start()
	{
		//		ResetDelay ();
		//		StartCoroutine (EnemyGenerator ());
	}

	void FixedUpdate()
	{

		if (isOn && !running)
		{
			running = true;
			ResetDelay();
			StartCoroutine(MethodToIterate());
		}
		else
		{
		}
	}

	IEnumerator MethodToIterate()
	{

		yield return new WaitForSeconds(delay);

		//		if (isOn) {
		var newTransform = transform;
		Instantiate(prefabs[Random.Range(0, prefabs.Length)], newTransform.position, Quaternion.Euler(0f, 0f, rotationZ));
		ResetDelay();

		//		}
		//			StartCoroutine (EnemyGenerator ());
		running = false;
	}

	void ResetDelay()
	{
		delay = Random.Range(delayRange.x, delayRange.y);
		rotationZ = Random.Range(rotationZRange.x, rotationZRange.y);
	}

	public void DoSpawn()
	{
		var newTransform = transform;
		Instantiate(prefabs[Random.Range(0, prefabs.Length)], newTransform.position, Quaternion.Euler(0f, 0f, rotationZ));
	}
}
