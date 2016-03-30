using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject cockpitObj;

    [SerializeField]
    private GameObject electricalPanelGUI;
    [SerializeField]
    private GameObject shipBuilderGUI;
    public Camera shipCamera;
    public Camera electricalCamera;

    private float screenHeight;
    private float screenWidth;
    private bool initialised = false;

    private static GUIController controller;

    public static GUIController getInstance()
    {
        return controller;
    }

    public static ElectricalGUI getElectricalGUI()
    {
        return controller.electricalPanelGUI.GetComponent<ElectricalGUI>();
    }

    public static ShipPartGUI getShipBuilderGUI()
    {
        return controller.shipBuilderGUI.GetComponent<ShipPartGUI>();
    }

    public static Camera getShipCamera()
    {
        return controller.shipCamera;
    }

    public static Camera getElectricalCamera()
    {
        return controller.electricalCamera;
    }

    void Start()
    {
        controller = this;
        screenHeight = Screen.height;
        screenWidth = Screen.width;

        if (cockpitObj != null)
        {
            BasePart cockpit = cockpitObj.GetComponent<BasePart>();
            ShipManager manager = cockpit.gameObject.AddComponent<ShipManager>();
            manager.setPlayerShip(true);
            manager.shipStatus = ShipManager.SHIP_STATUS_BUILDER_ELECTRICAL;
            cockpit.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F5)) ShipManager.getPlayerShip().saveShip("testShip.ship");
        if (Input.GetKeyUp(KeyCode.F9)) SaveLoad.loadShip("testShip.ship");

        if (!initialised)
        {
            shipBuilderGUI.SetActive(false);
            electricalPanelGUI.SetActive(false);
            electricalPanelButton();
            initialised = true;
        }
        if (ShipManager.getPlayerShip() != null)
        {
            if (ShipManager.getPlayerShip().shipStatus == ShipManager.SHIP_STATUS_BUILDER_ELECTRICAL)
            {
                if (Screen.height != screenHeight || Screen.width != screenWidth)
                {
                    screenHeight = Screen.height;
                    screenWidth = Screen.width;
                    setElectricalGUIPositions();
                }
            }
            else if (ShipManager.getPlayerShip().shipStatus == ShipManager.SHIP_STATUS_BUILDER_SHIP)
            {

            }
        }
    }

    public void releaseShipButton()
    {
        ShipManager.getPlayerShip().releaseShip();
        electricalPanelGUI.SetActive(false);
        shipBuilderGUI.SetActive(false);
    }

    public void setElectricalGUIPositions()
    {
        //Sets all the sliders positions and sizes relative to what is in them
        RectTransform elecParts = electricalPanelGUI.transform.FindChild("ElectricalParts").GetComponent<RectTransform>();
        RectTransform elecPartsViewport = elecParts.transform.FindChild("Viewport").GetComponent<RectTransform>();
        RectTransform elecPartsContent = elecPartsViewport.transform.FindChild("Content").GetComponent<RectTransform>();
        elecParts.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth * electricalCamera.rect.width);
        elecParts.anchoredPosition = new Vector2(Screen.width * electricalCamera.rect.x, elecParts.anchoredPosition.y);
        elecPartsContent.anchoredPosition = new Vector2(0, 0);
        float gridWidth = elecPartsContent.gameObject.GetComponent<GridLayoutGroup>().cellSize.x;
        float gridWidthSpacing = elecPartsContent.gameObject.GetComponent<GridLayoutGroup>().spacing.x;
        int gridCount = elecPartsContent.transform.childCount;
        elecPartsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (gridWidth*gridCount)+(gridWidthSpacing*gridCount));

        RectTransform shipParts = electricalPanelGUI.transform.FindChild("ShipParts").GetComponent<RectTransform>();
        RectTransform shipPartsViewport = shipParts.transform.FindChild("Viewport").GetComponent<RectTransform>();
        RectTransform shipPartsContent = shipPartsViewport.transform.FindChild("Content").GetComponent<RectTransform>();
        shipParts.anchoredPosition = new Vector2(screenWidth * electricalCamera.rect.x, shipParts.anchoredPosition.y);
        float gridHeight = shipPartsContent.gameObject.GetComponent<GridLayoutGroup>().cellSize.y;
        float gridHeightSpacing = shipPartsContent.gameObject.GetComponent<GridLayoutGroup>().spacing.y;
        gridCount = electricalPanelGUI.GetComponent<ElectricalGUI>().shipPartContentCount;
        shipPartsContent.anchoredPosition = new Vector2(0, 0);
        shipPartsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (gridHeight * gridCount) + (gridHeightSpacing * gridCount));
    }

    public void setShipBuildGUIPositions()
    {
        //Sets all the sliders positions and sizes relative to what is in them
        RectTransform shipParts = shipBuilderGUI.transform.FindChild("ShipParts").GetComponent<RectTransform>();
        RectTransform shipPartsViewport = shipParts.transform.FindChild("Viewport").GetComponent<RectTransform>();
        RectTransform shipPartsContent = shipPartsViewport.transform.FindChild("Content").GetComponent<RectTransform>();
        shipParts.anchoredPosition = new Vector2(screenWidth * shipCamera.rect.x, shipParts.anchoredPosition.y);
        float gridHeight = shipPartsContent.gameObject.GetComponent<GridLayoutGroup>().cellSize.y;
        float gridHeightSpacing = shipPartsContent.gameObject.GetComponent<GridLayoutGroup>().spacing.y;
        int gridCount = shipPartsContent.transform.childCount;
        shipPartsContent.anchoredPosition = new Vector2(0,0);
        shipPartsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (gridHeight * gridCount) + (gridHeightSpacing * gridCount));
    }

    public void electricalPanelButton()
    {
        //electricalCamera.enabled = true;
        ShipManager.getPlayerShip().shipStatus = ShipManager.SHIP_STATUS_BUILDER_ELECTRICAL;
        shipBuilderGUI.SetActive(false);
        electricalPanelGUI.SetActive(true);

        electricalCamera.rect = new Rect(0.3f, 0, 0.7f, 1);
        shipCamera.rect = new Rect(0, 0, 0.3f, 1);

        setElectricalGUIPositions();
        if (ShipManager.getPlayerShip() != null && ShipManager.selectedPart != null)
        {
            ShipManager.getPlayerShip().setSelectedPart(ShipManager.selectedPart);
        }
    }

    public void shipBuilderButton()
    {
        //electricalCamera.enabled = false;
        electricalPanelGUI.GetComponent<ElectricalGUI>().disable();
        ShipManager.getPlayerShip().shipStatus = ShipManager.SHIP_STATUS_BUILDER_SHIP;
        shipBuilderGUI.SetActive(true);
        electricalPanelGUI.SetActive(false);

        shipCamera.rect = new Rect(0, 0, 1, 1);

        setShipBuildGUIPositions();
    }

    public void shipBuilderPartButton()
    {

    }
}