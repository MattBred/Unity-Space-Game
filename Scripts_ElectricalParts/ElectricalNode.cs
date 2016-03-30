using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ElectricalNode : ElectricalPart
{
    public float radius;

    protected override void Start()
    {
        base.Start();
        spriteColor = Color.green;
        setRadius();
        if (gameObject != transform.root.gameObject && transform.root.GetComponent<ElectricalPart>() != null)
        {
            attachedElectricalList.Add(transform.root.GetComponent<ElectricalPart>());
            transform.root.GetComponent<ElectricalPart>().addElectricalToList(this);
        }
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

    public override void sendEnergy(float energyPercent)
    {
        this.energyPercent = energyPercent;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (part != null && part.energyPercent < energyPercent) part.sendEnergy(energyPercent);
        }
    }

    public void setRadius()
    {
        radius = gameObject.GetComponent<CircleCollider2D>().radius * gameObject.transform.lossyScale.x;
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
