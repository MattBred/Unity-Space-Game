using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class ElectricalVarRes : ElectricalPart {
    private Vector2 createGUIMousePos;

    public bool isOn;

    private GameObject GUIFab;
    private GameObject GUI;
    private bool GUIClick;
    private bool bindButtonListenUp;
    private bool bindButtonListenDown;
    private KeyCode bindKeyUp;
    private KeyCode bindKeyDown;
    private float outputUpSpeed;
    private float outputDownSpeed;
    private float outputPercent;

    protected override void Awake()
    {
        base.Awake();
        name = "VariableResistor";
        createGUIMousePos = new Vector2();
        GUIFab = (GameObject)Resources.Load("ElectricalPartFabs/ElectricalGUIVariableResistor");
        isOn = false;
        GUIClick = false;
        bindButtonListenUp = false;
        bindButtonListenDown = false;
        bindKeyUp = KeyCode.None;
        bindKeyDown = KeyCode.None;
        outputUpSpeed = 0.5f;
        outputDownSpeed = 0.5f;
        outputPercent = 0;
    }

    // Update is called once per frame
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
            GUI.transform.FindChild("OutputUpSlider").GetComponent<Slider>().value = outputUpSpeed;
            GUI.transform.FindChild("OutputDownSlider").GetComponent<Slider>().value = outputDownSpeed;

            Button keyBindButtonUp = GUI.transform.FindChild("OutputUpButton").GetComponent<Button>();
                keyBindButtonUp.onClick.AddListener(() => bindButtonUp());
                if (bindKeyUp != KeyCode.None)
                {
                    keyBindButtonUp.transform.FindChild("BindKeyText").GetComponent<Text>().text = (bindKeyUp.ToString());
                }

            Button keyBindButtonDown = GUI.transform.FindChild("OutputDownButton").GetComponent<Button>();
                keyBindButtonDown.onClick.AddListener(() => bindButtonDown());
                if (bindKeyDown != KeyCode.None)
                {
                    keyBindButtonDown.transform.FindChild("BindKeyText").GetComponent<Text>().text = (bindKeyDown.ToString());
                }

        }
    }


    void LateUpdate()
    {
        if (energyPercent > 0)
        {
            //If the resistor is currently receiving energy, we can then interact with it.
            if (Input.GetKey(bindKeyUp))
            {
                outputPercent += Time.deltaTime * outputUpSpeed;
                if (outputPercent > 1) outputPercent = 1;
            }
            if (Input.GetKey(bindKeyDown))
            {
                outputPercent -= Time.deltaTime * outputDownSpeed;
                if (outputPercent < 0) outputPercent = 0;
            }
            if (outputPercent == 0)
            {
                isOn = false;
            } else
            {
                isOn = true;
            }
        }
        if (GUI != null)
        {
            float outputEnergyCalculated;
            if (outputPercent <= energyPercent) outputEnergyCalculated = outputPercent;
            else outputEnergyCalculated = energyPercent;
            GUI.transform.FindChild("Input").GetComponent<Text>().text = ("Power Input: " + (energyPercent * 100));
            GUI.transform.FindChild("Output").GetComponent<Text>().text = ("Power Output: " + (outputEnergyCalculated * 100));
            outputUpSpeed = GUI.transform.FindChild("OutputUpSlider").GetComponent<Slider>().value;
            outputDownSpeed = GUI.transform.FindChild("OutputDownSlider").GetComponent<Slider>().value;
        }
        if (!GUIClick)
        {
            if (bindButtonListenUp && Input.anyKeyDown)
            {
                bindKeyUp = findKeyPressed();
                Button keyBindButtonUp = GUI.transform.FindChild("OutputUpButton").GetComponent<Button>();
                keyBindButtonUp.transform.FindChild("BindKeyText").GetComponent<Text>().text = ("("+bindKeyUp.ToString()+")");
                bindButtonListenUp = false;
            }
            else if (bindButtonListenDown && Input.anyKeyDown)
            {
                bindKeyDown = findKeyPressed();
                Button keyBindButtonDown = GUI.transform.FindChild("OutputDownButton").GetComponent<Button>();
                keyBindButtonDown.transform.FindChild("BindKeyText").GetComponent<Text>().text = ("("+bindKeyDown.ToString()+")");
                bindButtonListenDown = false;
            }
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



    private void bindButtonUp()
    {
        bindButtonListenUp = true;
        Button keyBindButtonUp = GUI.transform.FindChild("OutputUpButton").GetComponent<Button>();
        keyBindButtonUp.transform.FindChild("BindKeyText").GetComponent<Text>().text = ("Press a Key");
        if (bindButtonListenDown)
        {
            bindButtonListenDown = false;
            Button keyBindButtonDown = GUI.transform.FindChild("OutputDownButton").GetComponent<Button>();
            keyBindButtonDown.transform.FindChild("BindKeyText").GetComponent<Text>().text = ("(" + bindKeyDown.ToString() + ")");
        }
    }

    private void bindButtonDown()
    {
        bindButtonListenDown = true;
        Button keyBindButtonDown = GUI.transform.FindChild("OutputDownButton").GetComponent<Button>();
        keyBindButtonDown.transform.FindChild("BindKeyText").GetComponent<Text>().text = ("Press a Key");
        if (bindButtonListenUp)
        {
            bindButtonListenUp = false;
            Button keyBindButtonUp = GUI.transform.FindChild("OutputUpButton").GetComponent<Button>();
            keyBindButtonUp.transform.FindChild("BindKeyText").GetComponent<Text>().text = ("("+bindKeyUp.ToString()+")");
        }
    }





    private KeyCode findKeyPressed()
    {
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
                return kcode;
        }
        return KeyCode.None;
    }




    void OnDestroy()
    {
        if (GUI != null)
        {
            GameObject.Find("ElectricalPanelGUI").GetComponent<ElectricalGUI>().partClickEnabled = true;
            Destroy(GUI);
        }
    }

    public override void sendEnergy(float energyPercent)
    {
        float outputEnergyCalculated;
        this.energyPercent = energyPercent;
        if (outputPercent <= energyPercent) outputEnergyCalculated = outputPercent;
        else outputEnergyCalculated = energyPercent;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (isOn && part != null && part.energyPercent < outputEnergyCalculated) part.sendEnergy(outputEnergyCalculated);
        }
    }

    public override SaveLoad.SaveElectricalPart createSave(bool exactInstance)
    {
        save save = new save();
        setSaveDefaults(save);
        if (exactInstance)
        {
            save.isOn = isOn;
            save.bindKeyUp = bindKeyUp;
            save.bindKeyDown = bindKeyDown;
            save.outputUpSpeed = outputUpSpeed;
            save.outputDownSpeed = outputDownSpeed;
            save.outputPercent = outputPercent;
        }
        return save;
    }

    public override void loadFromSave(SaveLoad.SaveElectricalPart save)
    {
        loadSaveDefaults(save);
        isOn = ((save)save).isOn;
        bindKeyUp = ((save)save).bindKeyUp;
        bindKeyDown = ((save)save).bindKeyDown;
        outputUpSpeed = ((save)save).outputUpSpeed;
        outputDownSpeed = ((save)save).outputDownSpeed;
        outputPercent = ((save)save).outputPercent;
    }

    [System.Serializable]
    private class save : SaveLoad.SaveElectricalPart
    {
        public bool isOn = false;
        public KeyCode bindKeyUp;
        public KeyCode bindKeyDown;
        public float outputUpSpeed;
        public float outputDownSpeed;
        public float outputPercent;
    }
}
