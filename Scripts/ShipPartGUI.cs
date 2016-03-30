using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShipPartGUI : MonoBehaviour {

    [SerializeField]
    private GameObject shipPartScrollContents;
    private Vector3 mousePos;
    private bool onButtonClickBool = false;
    public BasePart dragPart;
    private Vector3 dragAttachPos;
    private List<GameObject> dragAttachObjectList = new List<GameObject>();
    private float placementTime;
    private static Color dragPartColor; //Current color the drag part is
    private static Color dragPartColorGreen = new Color(0,155,0);
    private static Color dragPartColorRed = new Color(155,0,0);
    private static GameObject Debug_Collision_Cube;
    private Vector3 nullVector = new Vector3(0, 0, 0);
    private float mouseButton1DownTime;
    private bool refresh = false;
    private bool firstStart = true;

    void Start()
    {
        firstStart = true;
    }

    private void init()
    {
        loadShipComponents();

        placementTime = Time.realtimeSinceStartup;
        if (false && !Debug_Collision_Cube)
        {
            Debug_Collision_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(Debug_Collision_Cube.GetComponent<Collider>());
        }
    }

    void LateUpdate()
    {
        if (firstStart)
        {
            if (PartFactory.getInstance() != null)
            {
                init();
                firstStart = false;
            }
        }

        else {
            bool mouseMoved = false;


            if ((Input.mousePosition != mousePos || refresh) && dragPart != null)
            {
                //Means the mouse moved with object selected
                onPartMove();
                mouseMoved = true;
            }

            if (onButtonClickBool)
            {
                onButtonClickBool = false;
            }
            else
            {
                if (dragPart != null)
                {
                    if (Input.GetMouseButtonDown(1)) mouseButton1DownTime = Time.time;
                    else if (Input.GetMouseButtonUp(1))
                    {
                        if (Time.time < mouseButton1DownTime + 0.2f)
                        {
                            //If it's only been x amount of time since first pressing the right mouse button, we destroy the ship.
                            Destroy(dragPart.gameObject);
                            dragPart = null;
                        }
                    }
                    else if (Input.GetMouseButtonUp(0)) onPartClick();
                    else if (mouseMoved && Input.GetMouseButton(0)) onPartClick();
                }
            }
            refresh = false;
            if (dragPart != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    dragPart.gameObject.transform.Rotate(0, -15, 0);
                    refresh = true;

                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    dragPart.gameObject.transform.Rotate(0, 15, 0);
                    refresh = true;
                }
            }
            mousePos = Input.mousePosition;
        }
    }

    private void onButtonClick(string name, Quaternion rotation)
    {
        if (dragPart != null)
        {
            Destroy(dragPart.gameObject);
        }
        Vector3 pos = CameraSystem.getMouseWorldPos();

        PartFactory.getInstance().createShipPart(name,false, true);
        dragPartColor = dragPartColorRed;
        mousePos = Input.mousePosition;
        onButtonClickBool = true;
    }

    public void setDragPart(BasePart part)
    {
        Vector3 pos = CameraSystem.getMouseWorldPos();
        dragPart = part;
        GameObject dragObject = dragPart.gameObject;
        dragPart.setLockRotation(false);
        dragObject.transform.position = pos;
        dragObject.GetComponent<Rigidbody>().isKinematic = true;
        dragPart.setLayers(2); //Ignore ray cast (so the mouse position doesnt keep hitting this)
        dragObject.GetComponent<BoundBoxes_BoundBox>().lineColor = dragPartColorRed;
        ShipManager.getPlayerShip().setSelectedPart(dragPart);
    }

    private void onPartClick()
    {
        GameObject dragObject = dragPart.gameObject;
        if (dragPartColor == dragPartColorGreen && Time.realtimeSinceStartup > (placementTime + 0.1f))
        {
            placementTime = Time.realtimeSinceStartup;
            List<BasePart> partList = ShipManager.getPlayerShip().getPartList();
            Collider partCollider;
            Collider dragCollider;
            for (int x = 0; x < partList.Count; x++)
            {
                for (int partC = 0; partC < partList[x].getAllColliders().Count; partC++)
                {
                    partCollider = partList[x].getAllColliders()[partC];
                    if (!partCollider.isTrigger)
                    {
                        for (int dragC = 0; dragC < dragPart.getAllColliders().Count; dragC++)
                        {
                            dragCollider = dragPart.getAllColliders()[dragC];
                            if (!dragCollider.isTrigger)
                            {
                                Physics.IgnoreCollision(dragCollider, partCollider);
                            }
                        }
                    }
                }
            }
            dragObject.transform.position = dragAttachPos;
            //dragPart.setMaterialColorDefault();
            dragObject.GetComponent<BoundBoxes_BoundBox>().lineColor = dragPartColorGreen;
            dragPart.setLayers(0);
            dragPart.setShipManager(ShipManager.getPlayerShip());
            ShipManager.getPlayerShip().addPart(dragPart);
            for (int x = 0; x < dragAttachObjectList.Count; x++)
            {
                ConfigurableJoint joint = dragObject.AddComponent<ConfigurableJoint>();
                joint.projectionMode = JointProjectionMode.PositionAndRotation;
                joint.projectionAngle = 1;
                joint.projectionDistance = 1;
                joint.xMotion = 0;
                joint.yMotion = 0;
                joint.zMotion = 0;
                joint.angularXMotion = 0;
                joint.angularYMotion = 0;
                joint.angularZMotion = 0;
                joint.connectedBody = dragAttachObjectList[x].GetComponent<Rigidbody>();
            }

            dragPart.setLockRotation(true);

            string name = dragPart.name;
            Quaternion rotation = new Quaternion();
            if (Input.GetKey(KeyCode.LeftShift)) rotation = dragPart.gameObject.transform.rotation;
            dragPart = null;
            if (Input.GetKey(KeyCode.LeftShift)) onButtonClick(name, rotation);
        }

    }

    private void onPartMove()
    {
        GameObject dragObject = dragPart.gameObject;
        Vector3 mousePos = CameraSystem.getMouseWorldPos();
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.z);
        dragAttachObjectList.Clear();
        Vector3 dragObjectMovePos = mousePos;
        Vector2 dragObjectPos2d = new Vector2(dragObject.transform.position.x, dragObject.transform.position.z);


        
        List<BasePart> partList = ShipManager.getPlayerShip().getPartList();
        BasePart part;
        GameObject partObject;
        bool canAttach;
        float dragDir = dragObject.transform.rotation.eulerAngles.y;
        Transform shipSnap;
        Transform dragSnap;
        Vector3 shipSnapPos;
        Vector2 shipSnapPos2d = new Vector2();
        Vector3 dragSnapPos;
        Vector2 dragSnapPos2d = new Vector2();
        Vector2 dragAttachPos2d = new Vector2();
        List<Transform> shipSnapPositions = new List<Transform>();
        List<Transform> dragSnapPositions;

        for (int x = 0; x < partList.Count; x++)
        {
            canAttach = false;
            part = partList[x];
            partObject = part.gameObject;
            shipSnapPositions = part.getAllShipConnectors();

            for (int s = 0; s < shipSnapPositions.Count; s++)
            {
                if (!canAttach)
                {
                    shipSnap = shipSnapPositions[s];
                    shipSnapPos = partObject.transform.TransformPoint(shipSnap.transform.localPosition);
                    shipSnapPos2d.x = shipSnapPos.x;
                    shipSnapPos2d.y = shipSnapPos.z;
                    dragSnapPositions = getMatchingConnectors(dragPart, shipSnap.rotation.eulerAngles.y, shipSnap.name);

                    for (int y = 0; y < dragSnapPositions.Count; y++)
                    {
                            dragSnap = dragSnapPositions[y];
                            dragSnapPos = dragObject.transform.TransformPoint(dragSnap.transform.localPosition);
                            dragSnapPos2d.x = dragSnapPos.x;
                            dragSnapPos2d.y = dragSnapPos.z;
                            float dist = Vector2.Distance(dragSnapPos2d, shipSnapPos2d);
                            if (dist < 0.3f)
                            {
                                bool collisionOK = true;
                                float angle = Mathf.Atan2(shipSnapPos.x - partObject.transform.position.x, shipSnapPos.z - partObject.transform.position.z);
                                dist = Vector2.Distance(dragSnapPos2d, dragObjectPos2d);
                                dragAttachPos = shipSnapPos + Get2dPosition(angle, dist*1.025f);
                                dragAttachPos2d.x = dragAttachPos.x;
                                dragAttachPos2d.y = dragAttachPos.z;
                                Quaternion rot = dragObject.transform.rotation;
                                Vector3 overlapPos = dragAttachPos + (rot * dragPart.getTriggerCollider().center);


                                Ray rayMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hitMouse;
                                bool rayCastHitMouse = Physics.Raycast(rayMouse, out hitMouse);

                                Vector3 halfSize = new Vector3(dragPart.getTriggerCollider().size.x / 2, dragPart.getTriggerCollider().size.y / 2, dragPart.getTriggerCollider().size.z / 2);
                                List<Collider> hitBoxList = new List<Collider>();
                                Collider[] overlapArray = Physics.OverlapBox(overlapPos, halfSize, dragObject.transform.rotation);
                                List<Collider> dragColliderList = dragPart.getAllColliders();
                                for (int z = 0; z < overlapArray.Length; z++)
                                {
                                    bool partCollider = false;
                                    for (int c = 0; c < dragColliderList.Count; c++)
                                    {
                                        if (dragColliderList[c] == overlapArray[z]) partCollider = true;
                                    }
                                    if (!partCollider)
                                    {
                                        //Debug.Log("Collision Parent: " + overlapArray[z].transform.root.name);
                                        if (overlapArray[z].isTrigger) hitBoxList.Add(overlapArray[z]);
                                    }
                                }
                                if (hitBoxList.Count > 0)
                                {
                                    //Debug.Log("collisionOK = FALSE - HitBoxList Count > 0");
                                    collisionOK = false;
                                }
                                if (dragPart.collisionTriggerList.Count > 0)
                                {
                                    //Debug.Log("collisionOK = FALSE - dragPart collision trigger list count > 0");
                                    collisionOK = false;
                                }
                                if (rayCastHitMouse)
                                {
                                    for (int z = 0; z < partList.Count; z++)
                                    {
                                        if (hitMouse.collider.gameObject == partList[z].gameObject)
                                        {
                                            //Debug.Log("collisionOK = FALSE - mouse hits part list[z]");
                                            collisionOK = false;
                                        }
                                    }
                                }
                                
                            if (collisionOK && (Time.realtimeSinceStartup > placementTime + 0.1f))
                                {
                                        
                                    if (Vector2.Distance(mousePos2d, dragAttachPos2d) <= 0.3f) dragObjectMovePos = dragAttachPos;
                                    dragAttachObjectList.Add(partObject);
                                    canAttach = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

        if (dragAttachObjectList.Count > 0)
        {
            if (dragPartColor != dragPartColorGreen)
            {
                dragPartColor = dragPartColorGreen;
                //dragPart.setMaterialColor(dragPartColor);
                dragObject.GetComponent<BoundBoxes_BoundBox>().lineColor = dragPartColor;
            }
        }
        else
        {
            if (dragPartColor != dragPartColorRed)
            {
                dragPartColor = dragPartColorRed;
                //dragPart.setMaterialColor(dragPartColor);
                dragObject.GetComponent<BoundBoxes_BoundBox>().lineColor = dragPartColor;
            }
        }

        dragObject.transform.position = dragObjectMovePos;
    }

    private List<Transform> getMatchingConnectors(BasePart part, float angle, string connectorType)
    {
        List<Transform> connectors = part.getAllShipConnectors();
        List<Transform> matchingConnectors = new List<Transform>();
        for (int x = 0; x < connectors.Count; x++)
        {
            if (connectors[x].name.ToLower().Equals(connectorType.ToLower()))
            {
                float rot = connectors[x].rotation.eulerAngles.y;
                float rotDist = Mathf.Abs((int)angle - (int)rot);
                if ((rotDist < 10 && rotDist >= 0) || (rotDist > 170 && rotDist < 190)) matchingConnectors.Add(connectors[x]);
            }
        }
        return matchingConnectors;
    }

    private void loadShipComponents()
    {
        BasePart part;
        Transform GUI;
        part = PartFactory.getInstance().createShipPart("Frame",true);
        part.hide();
        GUI = part.createGUIObject();
        GUI.SetParent(shipPartScrollContents.transform, false);
        if (GUI.GetComponent<Button>() != null)
        {
            Button button = GUI.GetComponent<Button>();
            string partName = GUI.name;
            button.onClick.AddListener(() => onButtonClick(partName, new Quaternion()));
        }

        part = PartFactory.getInstance().createShipPart("Thruster", true);
        part.hide();
        GUI = part.createGUIObject();
        GUI.SetParent(shipPartScrollContents.transform, false);
        if (GUI.GetComponent<Button>() != null)
        {
            Button button = GUI.GetComponent<Button>();
            string partName = GUI.name;
            button.onClick.AddListener(() => onButtonClick(partName, new Quaternion()));
        }

        part = PartFactory.getInstance().createShipPart("Reactor", true);
        part.hide();
        GUI = part.createGUIObject();
        GUI.SetParent(shipPartScrollContents.transform, false);
        if (GUI.GetComponent<Button>() != null)
        {
            Button button = GUI.GetComponent<Button>();
            string partName = GUI.name;
            button.onClick.AddListener(() => onButtonClick(partName, new Quaternion()));
        }

        part = PartFactory.getInstance().createShipPart("GattlingGun", true);
        part.hide();
        GUI = part.createGUIObject();
        GUI.SetParent(shipPartScrollContents.transform, false);
        if (GUI.GetComponent<Button>() != null)
        {
            Button button = GUI.GetComponent<Button>();
            string partName = GUI.name;
            button.onClick.AddListener(() => onButtonClick(partName, new Quaternion()));
        }

        GUIController.getInstance().setShipBuildGUIPositions();
    }

Vector3 Get2dPosition(float rad, float dist)
    {
        return new Vector3(Mathf.Sin(rad) * dist, 0, Mathf.Cos(rad) * dist);
    }
}
