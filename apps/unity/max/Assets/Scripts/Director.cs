using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

//[CustomEditor(typeof(Director))]
//public class DirectorEditor : Editor
//{
//	override public void OnInspectorGUI()
//	{
//		var myScript = target as Director;
//
//		//myScript.flag = GUILayout.Toggle(myScript.flag, "Flag");
//		//myScript.triggerType =
//
//		bool disable;
//		if (myScript.triggerType == DirectorTriggerType.Collision)
//			//myScript.triggerObjectConditions = EditorGUILayout.IntSlider("I field:", myScript.i , 1 , 100);
//			disable = false;
//		else
//			disable = true;
//		
//		using (new EditorGUI.DisabledScope(disable))
//		{
//			myScript.triggerObjectConditions.scriptName = EditorGUILayout.TextField ("Member Type", myScript.triggerObjectConditions.scriptName);
//			//EditorGUILayout.group
//		}
//	}
//}

public enum MemberType
{
	Field = 0,
	Property,
	Method
}
public enum OperatorType
{
	Equals = 0,
	NotEquals,
	GreaterThan,
	LessThan
}

[System.Serializable]
public class Thing
{
	public GameObject target;
	public LayerMask layer;

	/// <summary>
	/// a part of the script name in lower case
	/// </summary>
	public string scriptName;

	/// <summary>
	/// The type of the member.
	/// </summary>
	public MemberType memberType = MemberType.Field;

	/// <summary>
	/// The name of the field.
	/// </summary>
	public string memberName;

	/// <summary>
	/// The type of the operator.
	/// </summary>
	public OperatorType operatorType = OperatorType.Equals;

	/// <summary>
	/// The field value(s).
	/// </summary>
	public string memberValueX, memberValueY;

	public GameObject member_GO_Value;

	/// <summary>
	/// The trigger at.
	/// </summary>
	public float triggerAt = 0f;

	[System.NonSerialized]
	public bool hasRun = false;
}

public class Director : MonoBehaviour
{

	public Vector3 collisionCubePosition = Vector3.zero;
	public Vector3 collisionCubeSize;
	public Thing triggerObjectConditions;

	public Thing[] things;
	public bool applyOnce = true;
	public bool loop = false;
	public bool triggerTargetIsFixedInPlace = false;
	public int currentTaskIndex = 0;

	private Checkpoint checkpoint;
	private bool triggered = false;
	private bool startTimer = false;
	private Collider2D collidedObject;
	private bool atDirectorZone;
	private bool isAllTaskPerfomed = false;
	private Behaviour[] components;
	private Behaviour component;

	//private Physics2D[] physic2ds;
	//private Physics2D physic2d;
	private Color debugCollisionColor = Color.cyan;
	public float timeElapsed = 0f;
	private float biggestTriggerTime = 0f;
	private float prevClockTik = -1f, nowClockTik = 0;
	private object InvokeReturnObject = null;

	// Use this for initialization
	void Start()
	{
		timeElapsed = 0f;
		biggestTriggerTime = FindTheBiggestThingTriggerTime();

		// If no target AND no layer are defined, start the timer immediately
		if (triggerObjectConditions.target == null && triggerObjectConditions.layer == 0)
		{
			if (!startTimer)
			{
				startTimer = true;
				triggered = true; // Mark as triggered so Update doesn't look for collisions
			}
		}
	}

	void OnEnable()
	{
		// this feauture provide a way to let us trigger Director Whence it is enabled.
		if (triggerObjectConditions.target && triggerObjectConditions.scriptName == "GameObject")
		{
			if (triggerObjectConditions.memberType == MemberType.Property && triggerObjectConditions.memberName == "enabled" && gameObject.activeSelf.ToString() == triggerObjectConditions.memberValueX)
			{
				//atDirectorZone = true;
				//triggered = true;
				prevClockTik = -1f;
				nowClockTik = 0;
				currentTaskIndex = 0;
				timeElapsed = 0f;
				startTimer = true;
			}
			//startTimer = true;
		}
	}

	void Update()
	{

		// if all task has been ran
		if (currentTaskIndex >= things.Length)
		{
			//Debug.Log (this.name + " --- tasks: " + things.Length + " currenttask: " + currentTaskIndex);
			isAllTaskPerfomed = true;
			if (!loop) startTimer = false;
			// destroying the director when all its task has been ran
			if (applyOnce && triggered && transform.childCount == 0)
				Destroy(gameObject);
		}

		// if (startTimer && timeElapsed < biggestTriggerTime && !isAllTaskPerfomed)
		// 	timeElapsed += Time.deltaTime;
		if (startTimer && !isAllTaskPerfomed)
			timeElapsed += Time.deltaTime;

		else
		{
			timeElapsed = 0f;
			prevClockTik = -1f;
			currentTaskIndex = 0;

			if (triggerTargetIsFixedInPlace)
			{
				atDirectorZone = false;
				if (!applyOnce)
				{
					triggered = false;
					//isAllTaskPerfomed = false;
					//startTimer = false;

					foreach (var t in things)
					{
						t.hasRun = false;
					}
				}

			}
			if (!loop)
				startTimer = false;
			else
			{
				isAllTaskPerfomed = false;
				foreach (var t in things)
				{
					t.hasRun = false;
				}
			}

		}

		var pos = Vector3.zero;
		pos.x += transform.position.x;
		pos.y += transform.position.y;

		if (triggerObjectConditions.target)
			collidedObject = Physics2D.OverlapBox(pos, collisionCubeSize, 1, 1 << triggerObjectConditions.target.layer);
		else if (!triggerObjectConditions.target && triggerObjectConditions.layer != 0)
		{
			collidedObject = Physics2D.OverlapBox(pos, collisionCubeSize, 1, triggerObjectConditions.layer);
			if (collidedObject != null)
			{
				triggerObjectConditions.target = collidedObject.gameObject;
			}
		}

		//Collider2D theObject = IsTargetAtVisibleRange (triggerObject, collidedObjects);
		if (collidedObject && triggerObjectConditions.target && collidedObject.gameObject == triggerObjectConditions.target &&
			!atDirectorZone && !triggered)
		{

			// if trigger has condition
			if (triggerObjectConditions.scriptName != string.Empty)
			{

				components = triggerObjectConditions.target.GetComponents<Behaviour>();
				component = System.Array.Find(components, p => p.GetType().Name.ToString() == (triggerObjectConditions.scriptName));

				if (triggerObjectConditions.memberType == MemberType.Field)
				{
					FieldInfo fi = component.GetType().GetField(triggerObjectConditions.memberName);

					if (triggerObjectConditions.operatorType == OperatorType.Equals)
					{ // if ==
						if (fi.GetValue(component).ToString() != triggerObjectConditions.memberValueX) { isAllTaskPerfomed = false; startTimer = false; return; }

					}
					else if (triggerObjectConditions.operatorType == OperatorType.NotEquals)
					{ // if !=
						if (fi.GetValue(component).ToString() == triggerObjectConditions.memberValueX) { isAllTaskPerfomed = false; startTimer = false; return; }
					}

				}
				else if (triggerObjectConditions.memberType == MemberType.Property)
				{
					PropertyInfo pi = component.GetType().GetProperty(triggerObjectConditions.memberName);

					if (triggerObjectConditions.operatorType == OperatorType.Equals)
					{ // if ==
						if (pi.GetValue(component, null).ToString() != triggerObjectConditions.memberValueX) { isAllTaskPerfomed = false; startTimer = false; return; }

					}
					else if (triggerObjectConditions.operatorType == OperatorType.NotEquals)
					{ // if !=
						if (pi.GetValue(component, null).ToString() == triggerObjectConditions.memberValueX) { isAllTaskPerfomed = false; startTimer = false; return; }
					}
				}
			}
			atDirectorZone = true;
			triggered = true;
			startTimer = true;

		}
		// else if ((!collidedObject && atDirectorZone) || (!collidedObject && isAllTaskPerfomed))
		// {
		// 	atDirectorZone = false;

		// 	if (!applyOnce)
		// 	{
		// 		triggered = false;
		// 		isAllTaskPerfomed = false;

		// 		foreach (var t in things)
		// 		{
		// 			t.hasRun = false;
		// 		}
		// 	}
		// }
		else if (!collidedObject && atDirectorZone)
		{
			// 1. Only track that the player stepped out. Do NOT reset the tasks yet!
			atDirectorZone = false;
		}

		// 2. ONLY re-arm the Director if the sequence is completely finished AND the player has left the zone
		if (!applyOnce && isAllTaskPerfomed && !atDirectorZone)
		{
			triggered = false;
			isAllTaskPerfomed = false;

			foreach (var t in things)
			{
				t.hasRun = false;
			}
		}


		// this feauture provide a way to let us trigger Director Once it is enabled.
		//else if (!collidedObject && string.IsNullOrEmpty(triggerObjectConditions.scriptName))
		//{
		//    if (gameObject.activeSelf.ToString() == triggerObjectConditions.memberValueX && !atDirectorZone && !triggered)
		//    {
		//        atDirectorZone = true;
		//        triggered = true;
		//        startTimer = true;
		//    }
		//}

		// nowClockTik = Mathf.Round(timeElapsed * 10.0f) / 10.0f;
		// //Debug.Log (nowClockTik + "  " + prevClockTik);
		// if (startTimer && nowClockTik != prevClockTik)
		// {
		// 	foreach (var thing in things)
		// 	{

		// 		// Skip And Ignore The Rest If CheckPoint Or Temporary Checkpoint Exist
		// 		//				if ((checkpoint != null && checkpoint.isSaved) ||
		// 		//				    (CrossSceneInfo.checkppointTemp != null && CrossSceneInfo.checkppointTemp.isSaved)
		// 		//				    && currentTaskIndex > 0) {
		// 		//					Debug.Log ("Director : Any task after checkpoint loading will be ignored!");
		// 		//					break;
		// 		//				}
		// 		if ((checkpoint != null && checkpoint.isSaved) && currentTaskIndex > 0)
		// 		{
		// 			Debug.Log("Director " + this.name + " : Any task after checkpoint loading will be ignored!");
		// 			startTimer = false;
		// 			break;
		// 		}

		// 		if (nowClockTik >= thing.triggerAt && !isAllTaskPerfomed)
		// 		{
		// 			PerformATask(thing);
		// 			currentTaskIndex++;
		// 			//Debug.Log ("ran " + thing.target + thing.memberValueX);
		// 		}
		// 	}

		// 	prevClockTik = nowClockTik;
		// }

		if (startTimer && !isAllTaskPerfomed)
		{
			for (int i = 0; i < things.Length; i++)
			{
				var thing = things[i];

				// If the time has passed the trigger point, AND it hasn't run yet
				if (!thing.hasRun && timeElapsed >= thing.triggerAt)
				{
					// if (checkpoint != null && checkpoint.isSaved && currentTaskIndex > 0)
					// {
					// 	Debug.Log("Director " + this.name + " : Any task after checkpoint loading will be ignored!");
					// 	startTimer = false;
					// 	break;
					// }

					PerformATask(thing);
					thing.hasRun = true; // Mark as run so it doesn't fire every frame
					currentTaskIndex++;
				}
			}
		}
	}

	private void PerformATask(Thing thing)
	{
		try
		{
			// FALLBACK LOGIC: 
			// Use the specific thing's target if it exists. 
			// If not, use the target that triggered the Director.
			GameObject activeTarget = (thing.target != null) ? thing.target : triggerObjectConditions.target;

			if (activeTarget == null)
			{
				Debug.LogWarning($"Director ({name}): No target found for task at {thing.triggerAt}s. Skipping.");
				return;
			}
			components = activeTarget.GetComponents<Behaviour>();
			component = Array.Find(components, p => p.GetType().Name.ToString() == thing.scriptName);

			if (thing.memberType == MemberType.Field)
			{
				thing.memberValueX = thing.memberValueX == string.Empty ? "0" : thing.memberValueX;
				thing.memberValueY = thing.memberValueY == string.Empty ? "0" : thing.memberValueY;

				FieldInfo fi = component.GetType().GetField(thing.memberName);

				if (fi == null)
					Debug.Log("Director : " + thing.memberName + " Field Not Found!!");

				SetFieldValue(fi, thing.memberValueX, thing.memberValueY, thing.member_GO_Value);
				//Debug.Log (fi.FieldType.ToString ().ToLower ());
				//				if (fi.FieldType.ToString ().ToLower ().Contains ("bool")) {
				//					fi.SetValue (component, bool.Parse (thing.memberValueX));
				//
				//				} else if (fi.FieldType.ToString ().ToLower ().Contains ("vector2")) {
				//					fi.SetValue (component, new Vector2 (float.Parse (thing.memberValueX), float.Parse (thing.memberValueY)));
				//
				//				} else if (fi.FieldType.ToString ().ToLower ().Contains ("vector3")) {
				//					//Debug.Log ("here");
				//					fi.SetValue (component, new Vector3 (float.Parse (thing.memberValueX), float.Parse (thing.memberValueY), 0f));
				//
				//				} else if (fi.FieldType.ToString ().ToLower ().Contains ("int")) {
				//					fi.SetValue (component, int.Parse (thing.memberValueX));
				//
				//				} else if (fi.FieldType.ToString ().ToLower ().Contains ("string")) {
				//					fi.SetValue (component, thing.memberValueX.ToString ());
				//
				//				} else if (fi.FieldType.ToString ().ToLower ().Contains ("single")) {
				//					fi.SetValue (component, float.Parse (thing.memberValueX));
				//
				//				} else if (fi.FieldType.ToString ().ToLower ().Contains ("gameobject")) {
				//					fi.SetValue (component, thing.member_GO_Value);
				//
				//				} else { //enum
				//					fi.SetValue (component, Enum.Parse (fi.FieldType, thing.memberValueX));
				//				}

			}
			else if (thing.memberType == MemberType.Property)
			{
				thing.memberValueX = thing.memberValueX == string.Empty ? "0" : thing.memberValueX;
				thing.memberValueY = thing.memberValueY == string.Empty ? "0" : thing.memberValueY;

				PropertyInfo pi = component.GetType().GetProperty(thing.memberName);

				if (pi == null)
					Debug.Log("Director : " + thing.memberName + " Property Not Found!!");

				if (pi.PropertyType.ToString().ToLower().Contains("bool"))
				{
					pi.SetValue(component, bool.Parse(thing.memberValueX), null);

				}
				else if (pi.PropertyType.ToString().ToLower().Contains("int"))
				{
					pi.SetValue(component, int.Parse(thing.memberValueX), null);

				}
				else if (pi.PropertyType.ToString().ToLower().Contains("single"))
				{
					pi.SetValue(component, float.Parse(thing.memberValueX), null);

				}
				else if (pi.PropertyType.ToString().ToLower().Contains("gameobject"))
				{
					pi.SetValue(component, thing.member_GO_Value, null);
				}

			}
			else if (thing.memberType == MemberType.Method)
			{
				Type ourType = Type.GetType(thing.scriptName);
				MethodInfo mi = ourType.GetMethod(thing.memberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (mi != null)
				{
					//Debug.Log ("runnn  " + thing.scriptName);
					if (thing.memberValueX != string.Empty && thing.memberValueY != string.Empty && thing.member_GO_Value != null)
						InvokeReturnObject = mi.Invoke(component, new object[] { thing.member_GO_Value, thing.memberValueX, thing.memberValueY });

					else if (thing.memberValueX != string.Empty && thing.memberValueY != string.Empty && thing.member_GO_Value == null)
						InvokeReturnObject = mi.Invoke(component, new object[] { thing.memberValueX, thing.memberValueY });

					else if (thing.memberValueX != string.Empty && thing.memberValueY == string.Empty && thing.member_GO_Value == null)
						InvokeReturnObject = mi.Invoke(component, new object[] { thing.memberValueX });

					else if (thing.memberValueX == string.Empty && thing.memberValueY == string.Empty && thing.member_GO_Value != null)
						InvokeReturnObject = mi.Invoke(component, new object[] { thing.member_GO_Value });

					else if (thing.memberValueX != string.Empty && thing.memberValueY == string.Empty && thing.member_GO_Value != null)
						InvokeReturnObject = mi.Invoke(component, new object[] { thing.member_GO_Value, thing.memberValueX });

					else
						InvokeReturnObject = mi.Invoke(component, null);

				}
				else
				{
					Debug.Log("Error - Director [+" + nowClockTik + "] : " + thing.memberName + " Method Not Found!!");
				}

			}

		}
		catch (Exception ex)
		{
			Debug.Log("Error - Director (name=\"" + name + "\"; atTime=" + nowClockTik + ") :" + ex.Message);
		}
	}

	public void SetFieldValue(FieldInfo fi, string value1, string value2, GameObject go_value)
	{
		if (fi.FieldType.ToString().ToLower().Contains("bool"))
		{
			fi.SetValue(component, bool.Parse(value1));

		}
		else if (fi.FieldType.ToString().ToLower().Contains("vector2"))
		{
			fi.SetValue(component, new Vector2(float.Parse(value1), float.Parse(value2)));

		}
		else if (fi.FieldType.ToString().ToLower().Contains("vector3"))
		{
			//Debug.Log ("here");
			fi.SetValue(component, new Vector3(float.Parse(value1), float.Parse(value2), 0f));

		}
		else if (fi.FieldType.ToString().ToLower().Contains("int"))
		{
			fi.SetValue(component, int.Parse(value1));

		}
		else if (fi.FieldType.ToString().ToLower().Contains("string"))
		{
			fi.SetValue(component, value1);

		}
		else if (fi.FieldType.ToString().ToLower().Contains("single"))
		{
			fi.SetValue(component, float.Parse(value1));

		}
		else if (fi.FieldType.ToString().ToLower().Contains("gameobject"))
		{
			fi.SetValue(component, go_value);

		}
		else
		{ //enum
			fi.SetValue(component, Enum.Parse(fi.FieldType, value1));
		}
	}

	public void SetInvokeReturnObject(GameObject go, string destScriptName, string destFieldName)
	{
		components = go.GetComponents<Behaviour>();
		component = System.Array.Find(components, p => p.GetType().Name.ToString() == (destScriptName));
		FieldInfo fi = component.GetType().GetField(destFieldName);
		SetFieldValue(fi, InvokeReturnObject.ToString(), null, null);
	}

	public void DoSkipCurrentTask()
	{
		if (currentTaskIndex + 1 <= things.Length)
		{
			currentTaskIndex++;
			timeElapsed = things[currentTaskIndex].triggerAt;
			things[currentTaskIndex].hasRun = true;
		}
	}

	public void DoDestroyObject()
	{
		Destroy(gameObject);
	}

	private float FindTheBiggestThingTriggerTime()
	{
		return things.Max(x => x.triggerAt);
	}

	private Collider2D IsTargetAtVisibleRange(Transform key, Collider2D[] collObjects)
	{
		foreach (Collider2D scannedObject in collObjects)
		{
			if (scannedObject.transform == key)
			{
				return scannedObject;
			}
		}
		return null;
	}

	public static IEnumerable<FieldInfo> GetAllFields(Type t)
	{
		if (t == null)
			return Enumerable.Empty<FieldInfo>();

		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Static | BindingFlags.Instance |
			BindingFlags.DeclaredOnly;
		return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
	}

	public void SaveCheckpointTemp(string checkpointIndex, string zoneName)
	{
		try
		{

			Debug.Log("checkpointTemp] Attempting save : #" + checkpointIndex);

			var player = GameObject.Find("Player");
			Debug.Log("checkpointTemp] player reference got successfully.");

			// prevent #1 : dont save checkpoint if player is dead
			if (player && player.GetComponent<PlayerPhysics>().isDead)
				return;

			if (CrossSceneInfo.Instance.checkppointTemp == null)
				CrossSceneInfo.Instance.checkppointTemp = new Checkpoint();

			CrossSceneInfo.Instance.checkppointTemp.isSaved = true;
			CrossSceneInfo.Instance.checkppointTemp.checkpointIndex = int.Parse(checkpointIndex);
			CrossSceneInfo.Instance.checkppointTemp.zoneName = zoneName;
			CrossSceneInfo.Instance.checkppointTemp.sceneIndex = SceneManager.GetActiveScene().buildIndex;

			// saving player
			if (player)
			{
				CrossSceneInfo.Instance.checkppointTemp.playerPositionX = player.transform.position.x;
				CrossSceneInfo.Instance.checkppointTemp.playerPositionY = player.transform.position.y;
				CrossSceneInfo.Instance.checkppointTemp.playerPositionZ = player.transform.position.z;
			}
			// saving meter
			var meter = GameObject.Find("Meter");
			if (meter)
			{
				CrossSceneInfo.Instance.checkppointTemp.meterState = meter.GetComponent<Meter>().enabled;
				CrossSceneInfo.Instance.checkppointTemp.batteryAmount = meter.GetComponent<Meter>().battery;
			}

			Debug.Log("checkpointTemp] saved successfully: #" + CrossSceneInfo.Instance.checkppointTemp.checkpointIndex);

		}
		catch (Exception ex)
		{
			Debug.Log("checkpointTemp] cannot save :" + ex.Message);
		}
	}

	public void SaveCheckpoint(string checkpointIndex, string zoneName)
	{

		FileStream file = null;

		try
		{
			CrossSceneInfo.Instance.checkppointTemp.isSaved = false;

		}
		catch (Exception ex)
		{
			Debug.Log("checkpointTemp] cannot clear :" + ex.Message);
		}

		try
		{
			var player = GameObject.Find("Player");

			// prevent #1 : dont save checkpoint if player is dead
			if (player && player.GetComponent<PlayerPhysics>().isDead)
				return;

			// prevent #2 : dont save current checkpoint
			LoadLastCheckpoint("checkpoint] checking checkpoint index before saving...");
			if (checkpoint != null && checkpoint.checkpointIndex == int.Parse(checkpointIndex))
			{
				Debug.Log("checkpoint] this checkpoint is already saved. checkpoint index :" + checkpoint.checkpointIndex);
				return;
			}

			checkpoint = new Checkpoint();
			checkpoint.isSaved = true;
			checkpoint.checkpointIndex = int.Parse(checkpointIndex);
			checkpoint.zoneName = zoneName;
			checkpoint.sceneIndex = SceneManager.GetActiveScene().buildIndex;

			// saving player
			if (player)
			{
				checkpoint.playerPositionX = player.transform.position.x;
				checkpoint.playerPositionY = player.transform.position.y;
				checkpoint.playerPositionZ = player.transform.position.z;
			}

			// saving camera
			//			checkpoint.cameraPositionX = Camera.main.transform.position.x;
			//			checkpoint.cameraPositionY = Camera.main.transform.position.y;
			//			checkpoint.cameraPositionZ = Camera.main.transform.position.z;

			// saving meter
			var meter = GameObject.Find("Meter");
			if (meter)
			{
				checkpoint.meterState = meter.GetComponent<Meter>().enabled;
				checkpoint.batteryAmount = meter.GetComponent<Meter>().battery;
			}

			// Saving Physically
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			Debug.Log("checkpoint] " + checkpoint.checkpointIndex + "saveing started...");

			file = File.Create(CrossSceneInfo.Instance.checkpointFilePath);
			Debug.Log("checkpoint] #1 file created.");

			binaryFormatter.Serialize(file, checkpoint);
			Debug.Log("checkpoint] #2 data serilized.");

			Debug.Log("checkpoint] saved succefully. checkpoint index : " + checkpoint.checkpointIndex);
		}
		catch (Exception ex)
		{
			Debug.Log("checkpoint] Save Error : " + ex.Message);

		}
		finally
		{
			if (file != null)
			{
				file.Close();
				Debug.Log("checkpoint] #3 file closed.");
			}
		}
	}
	public void LoadAndApplyLastCheckpoint()
	{
		LoadLastCheckpoint();
		ApplyCheckpoint(checkpoint);
	}
	public void LoadLastCheckpoint(string msg = "")
	{
		FileStream file = null;

		// Temporary Checkpoint Loading Firstly
		try
		{
			Debug.Log("CheckpointTemp] checking...");
			if (CrossSceneInfo.Instance.checkppointTemp.isSaved)
			{
				Debug.Log("CheckpointTemp] loading...");
				checkpoint = CrossSceneInfo.Instance.checkppointTemp;
				ApplyCheckpoint(checkpoint);
				Debug.Log("CheckpointTemp] temporary checkpoint loaded sucessfully. #" + CrossSceneInfo.Instance.checkppointTemp.checkpointIndex);
				return;

			}
			else
			{
				Debug.Log("CheckpointTemp] no any temporary checkpoint.");
			}
		}
		catch (Exception ex)
		{
			Debug.Log("CheckpointTemp] cannot loading: " + ex.Message);
		}

		try
		{
			if (File.Exists(CrossSceneInfo.Instance.checkpointFilePath))
			{

				BinaryFormatter binaryFormatter = new BinaryFormatter();
				Debug.Log("checkpoint] loading started...");

				file = File.Open(CrossSceneInfo.Instance.checkpointFilePath, FileMode.Open);
				Debug.Log("checkpoint] #1 file opened.");

				checkpoint = (Checkpoint)binaryFormatter.Deserialize(file);
				Debug.Log("checkpoint] #2 data loaded.");

				if (msg == "")
					Debug.Log("checkpoint] last checkpoint loaded succefully. checkpoint index :" + checkpoint.checkpointIndex);
				else
					Debug.Log(msg + " checkpoint index :" + checkpoint.checkpointIndex);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("checkpoint] Loading Error : " + ex.Message);

		}
		finally
		{
			if (file != null)
			{
				file.Close();
				Debug.Log("checkpoint] #3 file closed.");
			}
		}
	}
	public void ApplyCheckpoint(Checkpoint checkpoint)
	{
		try
		{
			if (checkpoint != null)
			{
				//loading zone
				GameObject[] rootGOs = SceneManager.GetActiveScene().GetRootGameObjects();
				Debug.Log("all root gameobjects: " + rootGOs.Length);
				var results = Array.FindAll(rootGOs, s => s.name.Equals(checkpoint.zoneName));
				GameObject zone = results[0];
				Debug.Log("zone found : " + zone.name);

				if (zone)
				{
					zone.SetActive(true);

				}
				else
				{
					Debug.Log("checkpoint] Unable to load zone \"" + checkpoint.zoneName + "\". game loading will be ignored!!!");
					return;
				}

				// loading player
				var player = GameObject.Find("Player").transform;
				if (player)
				{
					player.position = new Vector3(checkpoint.playerPositionX, checkpoint.playerPositionY, checkpoint.playerPositionZ);
				}

				// loading camera
				//Camera.main.transform.position = new Vector3 (checkpoint.cameraPositionX, checkpoint.cameraPositionY, checkpoint.cameraPositionZ);
				Camera.main.transform.position = new Vector3(checkpoint.playerPositionX, checkpoint.playerPositionY, checkpoint.playerPositionZ);

				// loading meter
				Meter meter = GameObject.Find("Meter").GetComponent<Meter>();
				if (meter)
				{
					meter.enabled = checkpoint.meterState;
					meter.battery = checkpoint.batteryAmount;
				}

				Debug.Log("checkpoint] Applied succefully. index :" + checkpoint.checkpointIndex);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("checkpoint] appling Checkpoint Error : " + ex.Message);
		}
	}

	public void LoadAScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void ShowDialog(GameObject dialog)
	{
		dialog.SetActive(true);
	}
	public void CloseDialog(GameObject dialog)
	{
		dialog.SetActive(false);
	}
	public void WaitForSeconds(int seconds)
	{
		StartCoroutine(Waiter(seconds));
	}
	private IEnumerator Waiter(int seconds)
	{
		//Wait for 2 seconds
		yield return new WaitForSeconds(seconds);
	}


	void OnDrawGizmos()
	{

		if (triggerTargetIsFixedInPlace)
			debugCollisionColor = Color.magenta;
		else
			debugCollisionColor = Color.cyan;

		if (triggerObjectConditions.layer != 0)
			debugCollisionColor = Color.greenYellow;

		Color color = debugCollisionColor;
		color.a = .6f;
		if (!triggerObjectConditions.target && triggerObjectConditions.layer == 0)
			color.a = .1f;
		Gizmos.color = color;

		//draw hit area
		var bpos = collisionCubePosition;
		bpos.x += transform.position.x;
		bpos.y += transform.position.y;
		Gizmos.DrawCube(bpos, collisionCubeSize);

		Gizmos.color = color;
		bpos = collisionCubePosition;
		bpos.x += transform.position.x;
		bpos.y += transform.position.y;
		Gizmos.DrawWireCube(bpos, collisionCubeSize);
	}
}
