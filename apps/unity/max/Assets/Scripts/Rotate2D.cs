using System.Xml.Serialization;
using UnityEngine;

public class Rotate2D : MonoBehaviour
{

    public float RPM = 500.0f;
    public bool isOn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (isOn)
            transform.Rotate(0f, 0f, 6.0f * RPM * Time.deltaTime);
    }
}
