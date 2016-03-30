using UnityEngine;
using System.Collections;

public class Cockpit : BasePart
{
    protected override void Start()
    {
        base.Start();
        name = "Cockpit";
        healthMax = 200;
        health = 200;
        energyConsumingMax = 500;
    }

    protected override void Update()
    {
        //Unless the object is destroyed, this is called every frame
        base.Update();

    }

    public override void onDestroy()
    {
        //Called when this part gets destroyed
        base.onDestroy();

    }

    public override SaveLoad.SaveShipPart createSave(bool exactInstance)
    {
        Save save = new Save();
        setSaveDefaults(save, exactInstance);
        return save;
    }

    public override void loadFromSave(SaveLoad.SaveShipPart save)
    {
        loadSaveDefaults(save);
    }

    [System.Serializable]
    private class Save : SaveLoad.SaveShipPart
    {

    }
}
