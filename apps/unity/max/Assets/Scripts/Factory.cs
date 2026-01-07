using UnityEngine;

public class Factory : MonoBehaviour
{
    public enum States
    {
        Off = 0,
        Input,
        Output,
    }
    public States state = States.Off;
    public Rotate2D gear1;
    public Rotate2D gear2;
    public Smoke smoke;
    public AutoMovingwalkway conveyBelt;
    public MoveToPoint fence;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (state == States.Off)
        {
            if (gear1)
                gear1.isOn = false;
            if (gear2)
                gear2.isOn = false;
            if (smoke)
                smoke.isOn = false;
            if (conveyBelt)
                conveyBelt.isOn = false;
        }
        else if (state == States.Input)
        {
            if (gear1)
            {
                gear1.isOn = true;
                gear1.RPM = -50;
            }
            if (gear2)
            {
                gear2.isOn = true;
                gear2.RPM = -50;
            }
            if (smoke)
                smoke.isOn = true;

            if (conveyBelt)
            {
                conveyBelt.isOn = true;
                conveyBelt.targetSpeed = new Vector2(-1.5f, 0f);
            }
        }
        else if (state == States.Output)
        {
            if (gear1)
            {
                gear1.isOn = true;
                gear1.RPM = 50;
            }
            if (gear2)
            {
                gear2.isOn = true;
                gear2.RPM = 50;
            }
            if (smoke)
                smoke.isOn = true;

            if (conveyBelt)
            {
                conveyBelt.isOn = true;
                conveyBelt.targetSpeed = new Vector2(1.5f, 0f);
            }
        }
    }
}
