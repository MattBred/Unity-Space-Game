using UnityEngine;
using System.Collections;
using System;

public class ElectricalIf : ElectricalPart
{
    private bool isOn;

    protected override void Awake()
    {
        base.Awake();
        isOn = false;
    }

    public override void sendEnergy(float energyPercent)
    {
        this.energyPercent = energyPercent;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (isOn && part != null && part.energyPercent < energyPercent) part.sendEnergy(energyPercent);
        }
    }

    protected override void Update()
    {
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (part is ElectricalFiberNode)
            {
                //if (((ElectricalFiberNode)part).hasPower) isOn = true;
                //else isOn = false;
            }
        }
    }

    public override SaveLoad.SaveElectricalPart createSave(bool exactInstance)
    {
        save save = new save();
        setSaveDefaults(save);
        if (exactInstance) save.isOn = isOn;
        return save;
    }

    public override void loadFromSave(SaveLoad.SaveElectricalPart save)
    {
        loadSaveDefaults(save);
        isOn = ((save)save).isOn;
    }

    [System.Serializable]
    private class save : SaveLoad.SaveElectricalPart
    {
        public bool isOn = false;
    }
}
