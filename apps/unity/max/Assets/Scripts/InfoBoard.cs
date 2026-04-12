using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InfoBoard : MonoBehaviour
{
    // --- 1. DEFINITIONS ---
    public enum SystemState { Info = 0, Success = 1, Warning = 2, Danger = 3 }

    [System.Serializable]
    public struct MessageStep
    {
        [TextArea(2, 5)] public string text;
        public float delay;
        public SystemState state;
    }

    [System.Serializable]
    public struct StateSettings // Renamed from StateColors to include Sprite
    {
        public SystemState state;
        public Color fontColor;
        public Sprite backgroundSprite; // <--- NEW: The image for this state
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

    // REPLACED: Animator is gone, replaced with SpriteRenderer
    [SerializeField] private SpriteRenderer boardRenderer;
    // Note: If using UI Canvas for the background, use 'Image' instead of 'SpriteRenderer'

    // --- 3. LOGIC ---
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
            foreach (MessageStep step in sequence)
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
        if (step.delay <= 0) step.delay = 1f;

        while (timeWaited < step.delay)
        {
            screenText.text = showCursor ? current + cursorChar : current;
            showCursor = !showCursor;
            yield return new WaitForSeconds(blinkSpeed);
            timeWaited += blinkSpeed;
        }
    }

    public void DoAddToList(string text)
    {
        Debug.Log(text);
        sequence.Append(new MessageStep() { delay = 10, state = SystemState.Danger, text = text });
    }
}