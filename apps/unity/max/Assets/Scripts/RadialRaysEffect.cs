using UnityEngine;

[ExecuteAlways]
public class RadialRaysEffect : MonoBehaviour
{
    [Header("Line Settings")]
    [Tooltip("Number of lines (n).")]
    public int numberOfLines = 12;

    [Tooltip("Where the lines start. 0 = exact center.")]
    public float startRadius = 0f;

    [Header("Random End Lengths")]
    [Tooltip("The minimum possible distance a line will end at.")]
    public float minEndRadius = 1.5f;
    [Tooltip("The maximum possible distance a line will end at.")]
    public float maxEndRadius = 3f;

    [Header("Appearance")]
    [Tooltip("Thickness of the lines.")]
    public float lineWidth = 1f;
    public Color lineColor = Color.green;

    [Header("Animation")]
    [Tooltip("How fast the lines rotate around the object.")]
    public float rotationSpeed = 30f;

    [Header("Sorting")]
    public string sortingLayerName = "Default";
    public int orderInLayer = -1;

    private GameObject raysContainer;

    void Start()
    {
        GenerateRays();
    }

    void Update()
    {
        // Rotate the container only during gameplay
        if (Application.isPlaying && raysContainer != null)
        {
            raysContainer.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void GenerateRays()
    {
        // Safety check to prevent dropping orphaned objects into the Scene Root
        if (this == null || gameObject == null || !gameObject.scene.IsValid()) return;

        // 1. Find the container or create it if it doesn't exist
        Transform containerTransform = transform.Find("RaysContainer");
        if (containerTransform != null)
        {
            raysContainer = containerTransform.gameObject;

            // Safely clear out the old lines without deleting the container itself
            for (int i = raysContainer.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = raysContainer.transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    child.SetActive(false); // Hide immediately to prevent visual overlap
                    Destroy(child);         // Destroyed at the end of the frame
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }
        else
        {
            raysContainer = new GameObject("RaysContainer");
            raysContainer.transform.SetParent(this.transform, false);
            raysContainer.transform.localPosition = Vector3.zero;
        }

        if (numberOfLines <= 0) return;

        Material lineMat = new Material(Shader.Find("Sprites/Default"));

        // IMPORTANT: Seed the randomizer based on this specific object's ID.
        // This stops it from flickering in the editor, while guaranteeing that 
        // 5 different RAM modules don't look identically randomized!
        Random.InitState(gameObject.GetInstanceID());

        // 2. Loop through and create each line
        for (int i = 0; i < numberOfLines; i++)
        {
            float angle = i * Mathf.PI * 2f / numberOfLines;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            float randomEndDistance = Random.Range(minEndRadius, maxEndRadius);

            GameObject lineObj = new GameObject($"Ray_{i}");
            lineObj.transform.SetParent(raysContainer.transform, false);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;

            lr.positionCount = 2;
            lr.SetPosition(0, direction * startRadius);
            lr.SetPosition(1, direction * randomEndDistance);

            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            lr.sharedMaterial = lineMat;
            lr.startColor = lineColor;
            lr.endColor = lineColor;

            lr.sortingLayerName = sortingLayerName;
            lr.sortingOrder = orderInLayer;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Don't run the editor generator if we are currently playing
        if (Application.isPlaying) return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            // Only generate if the object is active and actually placed in a scene
            if (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                GenerateRays();
            }
        };
    }
#endif
}