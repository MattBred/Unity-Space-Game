using UnityEngine;
using System.Collections;
using System;

public class ElectricalReactor : ElectricalPart
{
    protected override void Start()
    {
        base.Start();
        name = "Reactor";
    }
    public override void sendEnergy(float energyPercent)
    {
        foreach (ElectricalPart part in getAttachedElectricalList())
        {
            if (part != null && part.energyPercent < energyPercent) part.sendEnergy(energyPercent);
        }
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
