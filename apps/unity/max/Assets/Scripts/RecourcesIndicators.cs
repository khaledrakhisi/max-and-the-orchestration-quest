using System;
using UnityEngine;

public class ResourcesIndicators : MonoBehaviour
{
    public int cpu = 0;
    public int ram = 0;
    public int disk = 0;
    [SerializeField]
    private SevenSegment cpu7seg;
    [SerializeField]
    private SevenSegment ram7seg;
    [SerializeField]
    private SevenSegment disk7seg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cpu7seg && cpu != cpu7seg.displayValue)
        {
            cpu7seg.displayValue = cpu;
        }
        if (ram7seg && ram != ram7seg.displayValue)
        {
            ram7seg.displayValue = ram;
        }
        if (disk7seg && disk != disk7seg.displayValue)
        {
            disk7seg.displayValue = disk;
        }
    }

    public void DoAdd(string component, string value)
    {
        if (component == "cpu" && value != "")
        {
            cpu += Convert.ToInt32(value);
        }
        else if (component == "ram" && value != "")
        {
            ram += Convert.ToInt32(value);
        }
        else if (component == "disk" && value != "")
        {
            disk += Convert.ToInt32(value);
        }
    }
}
