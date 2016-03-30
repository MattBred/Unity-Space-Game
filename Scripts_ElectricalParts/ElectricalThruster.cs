using UnityEngine;
using System.Collections;
using System;

public class ElectricalThruster : ElectricalPart, ElectricalPart.Fiber
{
    private bool fiberState;

    protected override void Start()
    {
        base.Start();
        name = "Thruster";
        fiberState = false;
    }

    protected override void Update()
    {
        base.Update();
        if (attachedShipPart != null)
        {
            ((Thruster)attachedShipPart).thrustOutput = ((Thruster)attachedShipPart).thrustOutputMax * energyPercent;
        }
    }

    public override void sendEnergy(float energyPercent)
    {
        //Debug.Log("Thruster receiving energy: " + energyPercent);
        this.energyPercent = energyPercent;

        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (part != null && part.energyPercent < energyPercent) part.sendEnergy(energyPercent);
        }
        if (energyPercent > 0) sendFiber(true);
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


    public override SaveLoad.SaveElectricalPart createSave(bool exactInstance)
    {
        save save = new save();
        setSaveDefaults(save);
        return save;
    }

    public override void loadFromSave(SaveLoad.SaveElectricalPart save)
    {
        loadSaveDefaults(save);
    }

    [System.Serializable]
    private class save : SaveLoad.SaveElectricalPart
    {

    }
}
