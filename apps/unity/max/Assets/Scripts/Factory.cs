using System;
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
    private States prevState = States.Off;
    public Rotate2D gear1;
    public Rotate2D gear2;
    public Smoke smoke;
    public AutoMovingwalkway conveyBelt;
    public MoveToPoint fence;
    [SerializeField] private InfoBoard infoBoard;
    [SerializeField] private ResourcesIndicators resourcesIndicators;
    [SerializeField] private ObjectSpawner spawner;
    private bool isStatusPosted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (state == States.Off)
        {
            if (prevState == States.Input)
            {
                if (isStatusPosted)
                {
                    isStatusPosted = false;
                }
                if (!isStatusPosted && infoBoard)
                {
                    infoBoard.DoShowOneMessage(">>> Error: No Image Found!", "Danger");
                    prevState = state;
                }
            }

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
            if (prevState == States.Off)
            {
                if (resourcesIndicators)
                {
                    if (resourcesIndicators.cpu >= 1 && resourcesIndicators.ram >= 128)
                    {
                        prevState = States.Input;

                    }
                    else
                    {
                        infoBoard.DoShowOneMessage(">>> Error: Not enough resources collected to create container!\n\nRAM required: 128+\nCPU required: 1+", "Danger");
                        DoResetFactory();
                        return;
                    }
                }
                else
                {
                    infoBoard.DoShowOneMessage(">>> Error: No resources attached!", "Danger");
                    DoResetFactory();
                    return;
                }
            }

            if (!isStatusPosted && infoBoard)
            {
                infoBoard.DoShowOneMessage(">>> Now Converting Image to Container . . . . . . . . . . . . . .", "Warning");
                isStatusPosted = true;
                prevState = state;
            }
            if (gear1)
            {
                gear1.isOn = true;
                gear1.RPM = -Math.Abs(gear1.RPM);
            }
            if (gear2)
            {
                gear2.isOn = true;
                gear2.RPM = Math.Abs(gear2.RPM);
            }
            if (smoke)
                smoke.isOn = true;

            if (conveyBelt)
            {
                conveyBelt.isOn = true;
                conveyBelt.targetSpeed = new Vector2(-1.5f, 0f);
            }
        }

        // output state decided using Director
        else if (state == States.Output)
        {
            if (prevState == States.Input)
            {
                if (isStatusPosted)
                {
                    isStatusPosted = false;
                }
                if (!isStatusPosted && infoBoard)
                {
                    if (spawner)
                    {
                        GameObject dockerContainer = spawner.DoSpawn();
                        if (dockerContainer && resourcesIndicators)
                        {
                            dockerContainer.GetComponent<Container>().cpu = resourcesIndicators.cpu;
                            dockerContainer.GetComponent<Container>().ram = resourcesIndicators.ram;

                            resourcesIndicators.cpu = 0;
                            resourcesIndicators.ram = 0;
                        }
                    }

                    infoBoard.DoShowOneMessage(">>> Image Converted to Container Successfully", "Success");
                    isStatusPosted = true;
                    prevState = state;
                }
            }
            if (gear1)
            {
                gear1.isOn = true;
                gear1.RPM = Math.Abs(gear1.RPM);
            }
            if (gear2)
            {
                gear2.isOn = true;
                gear2.RPM = -Math.Abs(gear2.RPM);
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

    public void DoResetFactory()
    {
        state = States.Off;
        prevState = States.Off;
        isStatusPosted = false;
    }
}
