using TMPro;
using UnityEngine;

public class DockerImage : MonoBehaviour
{
    public string imageName;
    [SerializeField] private TextMeshProUGUI labelText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DoSetName(string name)
    {
        labelText.text = name;
        imageName = name;
    }
}
