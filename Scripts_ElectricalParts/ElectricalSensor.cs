using UnityEngine;
using System.Collections;
using System;

public class ElectricalSensor : ElectricalPart
{
    public override void sendEnergy(float energyPercent)
    {
        throw new NotImplementedException();
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
        public bool isOn = false;
    }
}
