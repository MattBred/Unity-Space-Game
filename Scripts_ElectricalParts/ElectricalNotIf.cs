using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class ElectricalNotIf : ElectricalPart, ElectricalPart.Fiber
{
    private bool isOn = true;
    private bool fiberState = false;

    private Vector2 createGUIMousePos = new Vector2();
    private GameObject GUIFab;
    private GameObject GUI;
    private bool GUIClick = false;

    protected override void Awake()
    {
        base.Awake();
        name = "NotIf";
        GUIFab = (GameObject)Resources.Load("UI/ElectricalGUIVariableResistor");
    }

    protected override void OnMouseOver()
    {
        base.OnMouseOver();
        if (Input.GetMouseButtonDown(1) && !isPopupGUIOn && !GUIController.getElectricalGUI().isDraggingPart)
        {
            GUIClick = true;
            isPopupGUIOn = true;
            GameObject.Find("ElectricalPanelGUI").GetComponent<ElectricalGUI>().partClickEnabled = false;
            createGUIMousePos = Input.mousePosition;
            createGUIMousePos.x += 50;
            GUI = (GameObject)Instantiate(GUIFab, createGUIMousePos, new Quaternion());
            GUI.transform.parent = GameObject.Find("ElectricalPanelGUI").transform;
        }
    }

    public override void sendEnergy(float energyPercent)
    {
        this.energyPercent = energyPercent;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (isOn && part != null && !(part is Fiber) && part.energyPercent < energyPercent) part.sendEnergy(energyPercent);
        }
    }

    public void sendFiber(bool power)
    {
        fiberState = power;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (isOn && part != null && part is Fiber && (part as Fiber).getFiberState() != power) (part as Fiber).sendFiber(power);
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

    protected override void Update()
    {
        base.Update();
        isOn = true;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (part is ElectricalFiberNode)
            {
                if (((ElectricalFiberNode)part).getFiberState() == true)
                {
                    isOn = false;
                    break;
                }
            }
        }

        if (GUI != null)
        {
            float energyOut = 0;
            if (isOn) energyOut = energyPercent;
            GUI.transform.FindChild("Input").GetComponent<Text>().text = ("Power Input: " + (energyPercent * 100));
            GUI.transform.FindChild("Output").GetComponent<Text>().text = ("Power Output: " + (energyOut * 100));
        }

        if (!GUIClick)
        {
            if (isPopupGUIOn && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                RectTransform rect = GUI.GetComponent<RectTransform>();
                Vector2 mousePos = Input.mousePosition;
                if (!RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
                {
                    isPopupGUIOn = false;
                    GameObject.Find("ElectricalPanelGUI").GetComponent<ElectricalGUI>().partClickEnabled = true;
                    Destroy(GUI);
                }
            }
        }
        else GUIClick = false;
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
