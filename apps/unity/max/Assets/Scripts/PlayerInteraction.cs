using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerInteraction : AbstractBehavior
{

	public bool interacted = false;
	// ADD THIS: Use this for Switches (One-time Press)
	public bool justInteracted = false;
	// float holdTime = 0f;

	// 	void Update()
	// 	{
	// 		bool isPressing = false;
	// 		bool isJustDown = false;

	// #if UNITY_STANDALONE || UNITY_WEBPLAYER
	// 		var buttonPressed = inputState.GetButtonValue(inputButtons[0]);
	// 		var holdTime = inputState.GetButtonHoldTime(inputButtons[0]);
	// #elif UNITY_ANDROID || UNITY_IOS

	// 		var buttonPressed = CrossPlatformInputManager.GetButtonDown("Interaction");
	// 		if(!inputState.inputEnabled)
	// 			buttonPressed = false;

	// 		if(buttonPressed){
	// 			holdTime += Time.deltaTime;
	// 		}
	// #endif

	// 		// Debug.Log (buttonPressed +"---"+ holdTime);
	// 		//if ((buttonPressed && holdTime < .0099f)) {
	// 		if (buttonPressed)
	// 		{
	// 			interacted = true;
	// 		}
	// 		else if (!buttonPressed)
	// 		{
	// 			//Debug.Log ("false");
	// 			interacted = false;
	// 			holdTime = 0f;
	// 		}
	// 	}
	void Update()
	{
		bool isPressing = false;
		bool isJustDown = false;

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
		// ------------------
		// PC / WEB CONTROLS
		// ------------------
		// 1. Check if we are holding the button (For Boxes)
		isPressing = inputState.GetButtonValue(inputButtons[0]);

		// 2. Check if we just pressed it this frame (For Switches)
		// Note: This relies on the GetButtonDown method we added to InputState previously
		isJustDown = inputState.GetButtonDown(inputButtons[0]);

#elif UNITY_ANDROID || UNITY_IOS
            // ------------------
            // MOBILE CONTROLS
            // ------------------
            if(inputState.inputEnabled) {
                // "GetButton" returns true while held
                isPressing = CrossPlatformInputManager.GetButton("Interaction"); 
                
                // "GetButtonDown" returns true only on the first frame
                isJustDown = CrossPlatformInputManager.GetButtonDown("Interaction");
            }
#endif

		// Assign to public variables for other scripts to read
		interacted = isPressing;
		justInteracted = isJustDown;
	}
}
