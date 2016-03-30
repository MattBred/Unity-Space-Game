using UnityEngine;
using System.Collections;
using System;

public class ElectricalWire : ElectricalPart
{

    protected override void Start()
    {
        base.Start();
        spriteColor = Color.green;
    }

    protected override void Update()
    {
        base.Update();
        if (energyPercent == 0) gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        else gameObject.GetComponent<SpriteRenderer>().color = spriteColor;
        if (this is Fiber)
        {
            if (!(this as Fiber).getFiberState()) gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            else gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void reorientSelf(int angle, float NodeRadius)
    {
        if (angle == 0 || angle == 360) gameObject.transform.position = new Vector2(attachedElectricalList[0].gameObject.transform.position.x, attachedElectricalList[0].gameObject.transform.position.y + NodeRadius);
        else if (angle == 90) gameObject.transform.position = new Vector2(attachedElectricalList[0].gameObject.transform.position.x + NodeRadius, attachedElectricalList[0].gameObject.transform.position.y);
        else if (angle == 180) gameObject.transform.position = new Vector2(attachedElectricalList[0].gameObject.transform.position.x, attachedElectricalList[0].gameObject.transform.position.y - NodeRadius);
        else if (angle == -90 || angle == 270) gameObject.transform.position = new Vector2(attachedElectricalList[0].gameObject.transform.position.x - NodeRadius, attachedElectricalList[0].gameObject.transform.position.y);
        gameObject.transform.rotation = Quaternion.Euler(0, 180, angle);
    }

    public override void sendEnergy(float energyPercent)
    {
        this.energyPercent = energyPercent;
        foreach (ElectricalPart part in attachedElectricalList)
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