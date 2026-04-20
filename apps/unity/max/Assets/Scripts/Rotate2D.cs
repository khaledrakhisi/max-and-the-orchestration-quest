using System.Xml.Serialization;
using UnityEngine;

public class Rotate2D : MonoBehaviour
{

    public float RPM = 50.0f;
    public bool isOn;

    // Update is called once per frame
    void Update()
    {
        if (isOn)
            transform.Rotate(0f, 0f, RPM * Time.deltaTime);
    }


}
