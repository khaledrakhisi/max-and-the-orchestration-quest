using System;
using UnityEngine;

public class Container : MonoBehaviour
{
    public string containerName;
    public int cpu = 0;
    public int ram = 0;
    [SerializeField]
    private SevenSegment cpu7seg;
    [SerializeField]
    private SevenSegment ram7seg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cpu != cpu7seg.displayValue)
        {
            cpu7seg.displayValue = cpu;
        }
        if (ram != ram7seg.displayValue)
        {
            ram7seg.displayValue = ram;
        }
    }

    // public void DoAdd(string component, string value)
    // {
    //     if (component == "cpu" && value != "")
    //     {
    //         cpu += Convert.ToInt32(value);
    //     }
    //     else if (component == "ram" && value != "")
    //     {
    //         ram += Convert.ToInt32(value);
    //     }
    //     else if (component == "disk" && value != "")
    //     {
    //         disk += Convert.ToInt32(value);
    //     }
    // }
}
