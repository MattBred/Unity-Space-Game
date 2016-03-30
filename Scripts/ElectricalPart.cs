using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public abstract class ElectricalPart : MonoBehaviour {
    public BasePart attachedShipPart;
    public string fabObjectPath;
    public GameObject fabObject;
    public float energyPercent;
    protected List<ElectricalPart> attachedElectricalList;
    public bool isPopupGUIOn;
    protected Color spriteColor;

    protected virtual void Awake()
    {
        attachedElectricalList = new List<ElectricalPart>();
        isPopupGUIOn = false;
        BoundBoxes_BoundBox boundBoxes = gameObject.AddComponent<BoundBoxes_BoundBox>();
        boundBoxes.enabled = false;
        boundBoxes.colliderBased = true;
        boundBoxes.setCamera(GUIController.getElectricalCamera());
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (Input.GetKeyUp(KeyCode.Delete) && ShipManager.getPlayerShip().shipStatus == ShipManager.SHIP_STATUS_BUILDER_ELECTRICAL && GUIController.getElectricalGUI().selectedPart == this)
        {
            GUIController.getElectricalGUI().destroyComponent(this);
        }
    }

    protected virtual void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (attachedShipPart != null && !GUIController.getElectricalGUI().isDraggingPart) ShipManager.getPlayerShip().setSelectedPart(attachedShipPart);
            if (!GUIController.getElectricalGUI().isDraggingPart) GUIController.getElectricalGUI().setSelectedPart(this);
        }
    }

    public List<ElectricalPart> getAttachedElectricalList()
    {
        if (attachedElectricalList == null) attachedElectricalList = new List<ElectricalPart>();
        return attachedElectricalList;
    }

    public void addElectricalToList(ElectricalPart part)
    {
        if (attachedElectricalList == null) attachedElectricalList = new List<ElectricalPart>();
        if (!attachedElectricalList.Contains(part)) attachedElectricalList.Add(part);
    }

    public void removeElectricalFromList(ElectricalPart part)
    {
        if (attachedElectricalList.Contains(part)) attachedElectricalList.Remove(part);
    }

    public abstract void sendEnergy(float energyPercent);

    public bool isSameTypeAs(ElectricalPart otherPart)
    {
        if ((!(this is Fiber) && !(otherPart is Fiber)) || (this is Fiber && otherPart is Fiber)) return true;
        return false;
    }

    public void hide()
    {
        //Sets all the renders & colliders to disable. For when switching from ship to ship
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<Collider2D>().enabled = false;

        foreach (Transform child in gameObject.transform)
        {
            child.GetComponent<SpriteRenderer>().enabled = false;
            child.GetComponent<Collider2D>().enabled = false;
        }
    }

    public void show()
    {
        //Sets all the renders & colliders to enabled. For when switching from ship to ship
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        gameObject.GetComponent<Collider2D>().enabled = true;

        foreach (Transform child in gameObject.transform)
        {
            child.GetComponent<SpriteRenderer>().enabled = true;
            child.GetComponent<Collider2D>().enabled = true;
        }
    }

    public abstract SaveLoad.SaveElectricalPart createSave(bool exactInstance);

    public abstract void loadFromSave(SaveLoad.SaveElectricalPart save);

    protected void setSaveDefaults(SaveLoad.SaveElectricalPart save)
    {
        //Saves position, gameobject, name and scale
        save.pos = new Float3(transform.position);
        save.gameObject = gameObject;
        save.typeName = name;
        save.scale = new Float3(transform.lossyScale);
        if (gameObject.transform.root != transform) save.pos = new Float3(transform.localPosition);
    }

    protected void loadSaveDefaults(SaveLoad.SaveElectricalPart save)
    {
        //Sets default position, scale, and parent
        transform.position = Float3.toVector3(save.pos);
        transform.localScale = Float3.toVector3(save.scale);
        if (save.parent != null)
        {
            transform.SetParent(save.parent.gameObject.transform);
            transform.localPosition = Float3.toVector3(save.pos);
        }
    }


    public interface Fiber
    {
        void sendFiber(bool power);
        bool getFiberState();
        void setFiberState(bool val);
    }
}

