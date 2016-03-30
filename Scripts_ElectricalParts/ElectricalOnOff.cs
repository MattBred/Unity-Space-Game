using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ElectricalOnOff : ElectricalPart
{
    public Sprite onTexture;
    public Sprite offTexture;
    private Vector2 createGUIMousePos;

    public bool isOn;
    public bool isMomentary;
    public bool isMomentaryDefaultOn;

    private GameObject GUIFab;
    private GameObject GUI;
    private GameObject GUIDefaultOn;
    private bool GUIClick;
    private bool bindButtonListen;
    private KeyCode bindKey;

    protected override void Awake()
    {
        base.Awake();
        name = "OnOff";
        isOn = false;
        isMomentary = false;
        isMomentaryDefaultOn = false;
        GUIClick = false;
        bindButtonListen = false;
        createGUIMousePos = new Vector2();
        bindKey = KeyCode.None;
        GUIFab = (GameObject)Resources.Load("ElectricalPartFabs/ElectricalGUIOnOff");
    }

    protected override void Start()
    {
        base.Start();
        if (isOn) gameObject.GetComponent<SpriteRenderer>().sprite = onTexture;
        else gameObject.GetComponent<SpriteRenderer>().sprite = offTexture;

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

            GUIDefaultOn = GUI.transform.FindChild("DefaultOn").gameObject;
                GUIDefaultOn.SetActive(isMomentary);
           
            Toggle onOffToggle = GUI.transform.FindChild("OnToggle").GetComponent<Toggle>();
                onOffToggle.isOn = isOn;
                onOffToggle.onValueChanged.AddListener((value) => switchOnOff(value,true));

            Toggle momentaryToggle = GUI.transform.FindChild("MomentaryToggle").GetComponent<Toggle>();
                momentaryToggle.isOn = isMomentary;
                momentaryToggle.onValueChanged.AddListener((value) => switchMomentary(value));

            Toggle momentaryDefaultToggle = GUI.transform.FindChild("DefaultOn").GetComponent<Toggle>();
                momentaryDefaultToggle.isOn = isMomentaryDefaultOn;
                momentaryDefaultToggle.onValueChanged.AddListener((value) => switchMomentaryDefault(value));

            Button keyBindButton = GUI.transform.FindChild("BindingButton").GetComponent<Button>();
                keyBindButton.onClick.AddListener(() => bindButton());

            if (bindKey != KeyCode.None)
            {
                GUI.transform.FindChild("KeyBindingText").GetComponent<Text>().text = ("Key Binding: " + bindKey.ToString());
            }

        }
    }

    void LateUpdate()
    {
        if (energyPercent > 0)
        {
            if (Input.GetKeyDown(bindKey))
            {
                if (!isMomentary) switchOnOff(!isOn, false);
                else switchOnOff(!isMomentaryDefaultOn, false);
            }
            else if (Input.GetKeyUp(bindKey) && isMomentary)
            {
                switchOnOff(isMomentaryDefaultOn, false);
            }
        }
        if (!GUIClick)
        {
            if (bindButtonListen && Input.anyKeyDown)
            {
                bindKey = findKeyPressed();
                GUI.transform.FindChild("KeyBindingText").GetComponent<Text>().text = ("Key Binding: " + bindKey.ToString());
                bindButtonListen = false;
            }
            if (isPopupGUIOn && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                RectTransform rect = GUI.GetComponent<RectTransform>();
                Vector2 mousePos = Input.mousePosition;
                if (!RectTransformUtility.RectangleContainsScreenPoint(rect,mousePos))
                {
                    isPopupGUIOn = false;
                    GameObject.Find("ElectricalPanelGUI").GetComponent<ElectricalGUI>().partClickEnabled = true;
                    Destroy(GUI);
                }
            }
        }
        else GUIClick = false;
    }

    void OnDestroy()
    {
        if (GUI != null)
        {
            GameObject.Find("ElectricalPanelGUI").GetComponent<ElectricalGUI>().partClickEnabled = true;
            Destroy(GUI);
        }
    }

    private void bindButton()
    {
        bindButtonListen = true;
        GUI.transform.FindChild("KeyBindingText").GetComponent<Text>().text = "Press a Key";
    }

    private void switchOnOff(bool val, bool fromGUI) {

        if (!fromGUI || (fromGUI && !isMomentary))
        {
            if (val)
            {
                isOn = true;
                gameObject.GetComponent<SpriteRenderer>().sprite = onTexture;
            }
            else {
                isOn = false;
                gameObject.GetComponent<SpriteRenderer>().sprite = offTexture;
            }
        }
        if (GUI != null)
        {
            Toggle onOffToggle = GUI.transform.FindChild("OnToggle").GetComponent<Toggle>();
            onOffToggle.onValueChanged.RemoveAllListeners();
            onOffToggle.isOn = isOn;
            onOffToggle.onValueChanged.AddListener((value) => switchOnOff(value, true));
        }
    }

    private void switchMomentary(bool val)
    {
        isMomentary = val;
        if (GUI != null)
        {
            GUIDefaultOn.SetActive(isMomentary);
        }
        switchOnOff(isMomentaryDefaultOn, false);
    }

    private void switchMomentaryDefault(bool val)
    {
        isMomentaryDefaultOn = val;
        switchOnOff(isMomentaryDefaultOn, false);
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

    public override void sendEnergy(float energyPercent)
    {
        this.energyPercent = energyPercent;
        foreach (ElectricalPart part in attachedElectricalList)
        {
            if (isOn && part != null && part.energyPercent < energyPercent) part.sendEnergy(energyPercent);
        }
    }

    public override SaveLoad.SaveElectricalPart createSave(bool exactInstance)
    {
        save save = new save();
        setSaveDefaults(save);
        if (exactInstance)
        {
            save.isOn = isOn;
            save.isMomentary = isMomentary;
            save.isMomentaryDefaultOn = isMomentaryDefaultOn;
            save.bindKey = bindKey;
        }
        return save;
    }

    public override void loadFromSave(SaveLoad.SaveElectricalPart saveFile)
    {
        loadSaveDefaults(saveFile);
        isOn = ((save)saveFile).isOn;
        isMomentary = ((save)saveFile).isMomentary;
        isMomentaryDefaultOn = ((save)saveFile).isMomentaryDefaultOn;
        bindKey = ((save)saveFile).bindKey;
    }

    [System.Serializable]
    private class save : SaveLoad.SaveElectricalPart
    {
        public bool isOn = false;
        public bool isMomentary = false;
        public bool isMomentaryDefaultOn = false;
        public KeyCode bindKey = KeyCode.None;
    }
}
