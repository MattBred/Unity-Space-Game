using UnityEngine;
using System.Collections;
using System;

public class ElectricalFiberWire : ElectricalWire, ElectricalPart.Fiber
{
    //Sensor type wire. hasPower bool returns true if it has power, false if it doesn't.
    public bool fiberState;

    protected override void Start()
    {
        base.Start();
        name = "FiberWire";
        fiberState = false;
    }

    public void sendFiber(bool power)
    {
        fiberState = power;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (part != null && part is Fiber && (part as Fiber).getFiberState() != power) (part as Fiber).sendFiber(power);
        }
    }

    public bool getFiberState()
    {
        return fiberState;
    }

    public void setFiberState(bool val)
    {
        fiberState = val;
    }
}
