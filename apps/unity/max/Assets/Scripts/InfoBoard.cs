using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class InfoBoard : MonoBehaviour
{
    public enum SystemState { Info = 0, Success = 1, Warning = 2, Danger = 3 }

    [Serializable]
    public struct MessageStep
    {
        [TextArea(2, 5)] public string text;
        public float delay;
        public SystemState state;
    }

    [Serializable]
    public struct StateSettings
    {
        public SystemState state;
        public Color fontColor;
        public Sprite backgroundSprite; // the info board sprite for specific color
    }

    // --- 2. INSPECTOR SETTINGS ---
    [Header("Sequence Data")]
    [SerializeField] private List<MessageStep> sequence;
    [SerializeField] private bool loopSequence = true;

    [Header("Visual Configuration")]
    [SerializeField] private List<StateSettings> stateVisuals; // Link colors AND sprites here

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float blinkSpeed = 0.5f;
    [SerializeField] private string cursorChar = "_";

    [Header("References")]
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private SpriteRenderer boardRenderer;
    // Add this to break out of infinite waits
    private bool forceSkipWait = false;
    // Note: If using UI Canvas for the background, use 'Image' instead of 'SpriteRenderer'

    void Start()
    {
        if (sequence != null && sequence.Count > 0)
        {
            StartCoroutine(RunSequence());
        }
    }

    private IEnumerator RunSequence()
    {
        do
        {
            List<MessageStep> currentSequence = new(sequence);
            foreach (MessageStep step in currentSequence)
            {
                ApplyVisuals(step);
                yield return StartCoroutine(TypeRoutine(step));
            }
        } while (loopSequence);
    }

    private void ApplyVisuals(MessageStep step)
    {
        // Find the configuration for the current state
        StateSettings config = stateVisuals.Find(x => x.state == step.state);

        // 1. SPRITE SWAP: Change the background image
        if (boardRenderer != null && config.backgroundSprite != null)
        {
            boardRenderer.sprite = config.backgroundSprite;
        }

        // 2. COLOR: Apply text color
        // Safety check: if color is transparent (forgot to set it), use white
        if (config.fontColor.a == 0)
            screenText.color = Color.white;
        else
            screenText.color = config.fontColor;
    }

    private IEnumerator TypeRoutine(MessageStep step)
    {
        screenText.text = "";
        string current = "";

        // Typing Loop
        foreach (char letter in step.text)
        {
            current += letter;
            screenText.text = current + cursorChar;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Waiting Loop
        float timeWaited = 0f;
        bool showCursor = true;

        // Ensure we wait at least a frame to avoid infinite loops if delay is 0
        //if (step.delay <= 0) step.delay = 1f;

        while ((timeWaited < step.delay || step.delay <= 0f) && !forceSkipWait)
        {
            screenText.text = showCursor ? current + cursorChar : current;
            showCursor = !showCursor;
            yield return new WaitForSeconds(blinkSpeed);
            timeWaited += blinkSpeed;
        }

        forceSkipWait = false;
    }

    public void DoShowOneMessage(string text, string state)
    {
        // clear the list to show only one single message with 0.0f delay
        sequence.Clear();
        Enum.TryParse(state, out SystemState result);
        sequence.Add(new MessageStep() { delay = 0f, state = result, text = text });

        forceSkipWait = true;
    }

    public void DoAddToList(string text, string delay)
    {
        float dly = float.Parse(delay);
        sequence.Add(new MessageStep() { delay = dly, state = SystemState.Info, text = text });

        forceSkipWait = true;
    }

    public void DoRemoveItem(string index)
    {
        sequence.RemoveAt(Convert.ToInt32(index));
    }
}