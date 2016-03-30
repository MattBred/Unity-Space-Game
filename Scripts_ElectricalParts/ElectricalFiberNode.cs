using UnityEngine;
using System.Collections;

public class ElectricalFiberNode : ElectricalNode, ElectricalPart.Fiber {

    //Sensor type node. hasPower bool returns true if it has power, false if it doesn't.

    private bool fiberState;

    protected override void Start()
    {
        base.Start();
        name = "FiberNode";
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
