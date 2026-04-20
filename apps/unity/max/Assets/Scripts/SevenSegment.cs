using UnityEngine;
using System.Collections.Generic;

public class SevenSegment : MonoBehaviour
{
    [Header("Editor Preview & Value")]
    [Tooltip("Changes the number dynamically in the Scene view and during gameplay.")]
    public int displayValue = 0;

    // Tracks changes so we don't update the display every single frame unnecessarily
    private int lastDisplayValue = -1;

    [Header("Display Settings")]
    [Range(1, 10)]
    public int numberOfDigits = 3;
    public float digitSpacing = 0.8f;

    [Header("Colors")]
    public Color colorOn = Color.green;
    public Color colorOff = new Color(0f, 0.1f, 0f, 0.3f);

    [Header("Sorting Layer")]
    public string sortingLayerName = "Default";
    public int orderInLayer = 5;

    private List<SpriteRenderer[]> allDigits = new List<SpriteRenderer[]>();
    private Sprite blockSprite;

    private readonly byte[,] digitPatterns = new byte[10, 7] {
        {1, 1, 1, 1, 1, 1, 0}, {0, 1, 1, 0, 0, 0, 0}, {1, 1, 0, 1, 1, 0, 1},
        {1, 1, 1, 1, 0, 0, 1}, {0, 1, 1, 0, 0, 1, 1}, {1, 0, 1, 1, 0, 1, 1},
        {1, 0, 1, 1, 1, 1, 1}, {1, 1, 1, 0, 0, 0, 0}, {1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 0, 1, 1}
    };

    void Awake()
    {
        CreateSprite();
        GenerateDisplay();

        // Set the initial starting value
        // Debug.Log($"awake display value: {displayValue}");
        DoSetNumber(displayValue.ToString());
    }

    void Update()
    {
        // If the variable was changed (e.g., via Inspector or another script), update the visuals
        if (displayValue != lastDisplayValue)
        {
        // Debug.Log($"update display value: {displayValue}");

            DoSetNumber(displayValue.ToString());
        }
    }

    private void CreateSprite()
    {
        if (blockSprite != null) return;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        blockSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void GenerateDisplay()
    {
        allDigits.Clear();

        if (transform.childCount == numberOfDigits)
        {
            for (int i = 0; i < numberOfDigits; i++)
            {
                Transform digitTransform = transform.GetChild(i);
                SpriteRenderer[] segments = new SpriteRenderer[7];
                for (int s = 0; s < 7; s++)
                {
                    segments[s] = digitTransform.GetChild(s).GetComponent<SpriteRenderer>();
                    if (segments[s] != null)
                    {
                        segments[s].sprite = blockSprite;
                        segments[s].sortingLayerName = sortingLayerName;
                        segments[s].sortingOrder = orderInLayer;
                    }
                }
                allDigits.Add(segments);
            }
            return;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }

        float startX = -(numberOfDigits - 1) * digitSpacing / 2f;

        for (int i = 0; i < numberOfDigits; i++)
        {
            GameObject digitObj = new GameObject($"Digit_{i}");
            digitObj.transform.SetParent(this.transform, false);
            digitObj.transform.localPosition = new Vector3(startX + (i * digitSpacing), 0, 0);

            SpriteRenderer[] segments = new SpriteRenderer[7];

            Vector2 horizSize = new Vector2(0.4f, 0.1f);
            Vector2 vertSize = new Vector2(0.1f, 0.3f);

            segments[0] = CreateSegment(digitObj.transform, "A", new Vector2(0, 0.4f), horizSize);
            segments[1] = CreateSegment(digitObj.transform, "B", new Vector2(0.2f, 0.2f), vertSize);
            segments[2] = CreateSegment(digitObj.transform, "C", new Vector2(0.2f, -0.2f), vertSize);
            segments[3] = CreateSegment(digitObj.transform, "D", new Vector2(0, -0.4f), horizSize);
            segments[4] = CreateSegment(digitObj.transform, "E", new Vector2(-0.2f, -0.2f), vertSize);
            segments[5] = CreateSegment(digitObj.transform, "F", new Vector2(-0.2f, 0.2f), vertSize);
            segments[6] = CreateSegment(digitObj.transform, "G", new Vector2(0, 0), horizSize);

            allDigits.Add(segments);
        }
    }

    private SpriteRenderer CreateSegment(Transform parent, string name, Vector2 position, Vector2 scale)
    {
        GameObject segObj = new GameObject($"Segment_{name}");
        segObj.transform.SetParent(parent, false);
        segObj.transform.localPosition = position;
        segObj.transform.localScale = scale;

        SpriteRenderer sr = segObj.AddComponent<SpriteRenderer>();
        sr.sprite = blockSprite;
        sr.color = colorOff;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = orderInLayer;

        return sr;
    }

    public void DoSetNumber(string number)
    {
        // Debug.Log($"do_set_number: {number}");
        if (allDigits.Count == 0) return;

        // Try to parse the string to an int. If successful, sync our tracking variables
        if (int.TryParse(number, out int parsedValue))
        {
            displayValue = parsedValue;
            lastDisplayValue = parsedValue;
        }

        string numStr = number.PadLeft(numberOfDigits, '0');
        if (numStr.Length > numberOfDigits) numStr = new string('9', numberOfDigits);

        for (int i = 0; i < numberOfDigits; i++)
        {
            UpdateDigitVisuals(allDigits[i], numStr[i] - '0');
        }
    }

    private void UpdateDigitVisuals(SpriteRenderer[] segments, int num)
    {
        if (num < 0 || num > 9) return;
        for (int i = 0; i < 7; i++)
        {
            segments[i].color = (digitPatterns[num, i] == 1) ? colorOn : colorOff;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            CreateSprite();
            if (transform.childCount != numberOfDigits) GenerateDisplay();

            if (allDigits.Count == 0 && transform.childCount == numberOfDigits)
            {
                for (int i = 0; i < numberOfDigits; i++)
                {
                    Transform digitTransform = transform.GetChild(i);
                    SpriteRenderer[] segments = new SpriteRenderer[7];
                    for (int s = 0; s < 7; s++) segments[s] = digitTransform.GetChild(s).GetComponent<SpriteRenderer>();
                    allDigits.Add(segments);
                }
            }

            if (allDigits.Count > 0) DoSetNumber(displayValue.ToString());
        };
    }
#endif
}