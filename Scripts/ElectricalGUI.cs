using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class ElectricalGUI : MonoBehaviour
{
    [SerializeField]
    private GameObject shipPartScrollContents;
    [SerializeField]
    private GameObject electricalPartScrollContents;

    public bool isDraggingPart = false;
    public ElectricalPart selectedPart = null;
    private ElectricalNode startNode = null;
    private ElectricalNode currentNode = null;
    private ElectricalWire currentLine;
    private bool drawingLine = false;
    private bool onButtonClickBool = false;
    private Camera cam;
    private Vector2 lineStartPos;
    private Vector2 lineEndPos;
    private List<ElectricalPart> electricalComponentList;
    public int shipPartContentCount = 0;
    public bool partClickEnabled = true;
    private bool firstStart = true;
    private bool onEnable = false;

    void Start()
    {
        firstStart = true;
    }

    void OnEnable()
    {
        onEnable = true;
    }

    private void onEnableInit()
    {
        foreach (Transform child in shipPartScrollContents.transform)
        {
            //Delete all the GUI ship parts from the list
            if (child != child.root) Destroy(child.gameObject);
        }
        shipPartContentCount = 0;

        if (ShipManager.getPlayerShip() != null)
        {
            //If the player ship exists
            electricalComponentList = ShipManager.getPlayerShip().electricalPartList;
            List<BasePart> shipParts = ShipManager.getPlayerShip().getPartList();
            foreach (BasePart shipPart in shipParts)
            {
                //Go through each ship part in the player's ship
                if (shipPart.energyConsumingMax > 0 || shipPart.energyOutputMax > 0)
                {
                    //Part deals with energy, check to see if it's part of the electrical components
                    bool alreadyPlaced = false;
                    foreach (ElectricalPart ePart in electricalComponentList)
                    {
                        if (ePart.attachedShipPart != null && ePart.attachedShipPart == shipPart)
                        {
                            //If it's already a part of the ship's electrical system, then we say its already placed and don't put it in the GUI list
                            alreadyPlaced = true;
                            break;
                        }
                    }
                    if (!alreadyPlaced)
                    {
                        shipPartContentCount++;
                        createGUIButton(shipPart);
                    }
                }
            }
        }
    }

    public void disable()
    {
        if (isDraggingPart)
        {
            if (electricalComponentList.Contains(selectedPart)) electricalComponentList.Remove(selectedPart);
            destroyComponent(selectedPart);
            isDraggingPart = false;
            selectedPart = null;
        } else
        {
            if (drawingLine)
            {
                drawingLine = false;
                destroyNode(currentNode);
                electricalComponentList.Remove(currentLine);
                Destroy(currentLine.gameObject);
            }
        }
    }


    private void onShipPartButtonClick(GameObject GUIObj, BasePart part, Quaternion rotation)
    {
        if (isDraggingPart)
        {
            createGUIButton(selectedPart.attachedShipPart);
            destroyComponent(selectedPart);
        }
        selectedPart = PartFactory.getInstance().createElectricalPart(part.name);
        setSelectedPart(selectedPart);
        isDraggingPart = true;
        selectedPart.attachedShipPart = part;
        part.electricalPart = selectedPart;
        initNodes(selectedPart);
        onButtonClickBool = true;
        selectedPart.transform.position = findNearestGridSnap(selectedPart.transform.position);
        electricalComponentList.Add(selectedPart);
        Destroy(GUIObj);
    }

    private void onElectricalPartButtonClick(GameObject GUIObj, Quaternion rotation)
    {
        //When user clicks a specific electrical part (Sensors, IF gates, ANDs, etc)
        if (isDraggingPart)
        {
            destroyComponent(selectedPart);
        }
        GameObject newObj = PartFactory.getInstance().createElectricalPart(GUIObj.name).gameObject;
        newObj.transform.SetParent(null);
        ElectricalPart electricalBase = newObj.GetComponent<ElectricalPart>();
        initNodes(electricalBase);
        selectedPart = electricalBase;
        setSelectedPart(selectedPart);
        isDraggingPart = true;
        onButtonClickBool = true;
        electricalBase.transform.position = findNearestGridSnap(electricalBase.transform.position);
        electricalComponentList.Add(electricalBase);
    }

    public void setSelectedPart(ElectricalPart part)
    {
        foreach (ElectricalPart ePart in electricalComponentList)
        {
            if (ePart.gameObject.GetComponent<BoundBoxes_BoundBox>() != null) ePart.gameObject.GetComponent<BoundBoxes_BoundBox>().setEnable(false);
        }
        part.gameObject.GetComponent<BoundBoxes_BoundBox>().setEnable(true);
        selectedPart = part;
    }

    private ElectricalNode getNodeAtPos(Vector2 pos)
    {
        return getNodeAtPos(pos, null);
    }

    private ElectricalNode getNodeAtPos(Vector2 pos, ElectricalNode ignoreNode)
    {
        List<ElectricalNode> nodeList = getNodeList();
        for (int x = 0; x < nodeList.Count; x++)
        {
            Vector2 newPos = nodeList[x].transform.position;
            if (Vector2.Distance(newPos,pos) < 1 && nodeList[x] != ignoreNode)
            {
                return nodeList[x];
            }
        }
        return null;
    }

    void Update()
    {
        if (firstStart || onEnable)
        {
            if (firstStart && PartFactory.getInstance() != null)
            {
                cam = GUIController.getElectricalCamera().GetComponent<Camera>();
                loadElectricalParts();
                firstStart = false;
            }
            if (onEnable && PartFactory.getInstance() != null)
            {
                onEnableInit();
                onEnable = false;
            }
        }
        else {
            if (cam.gameObject.GetComponent<ElectricalPanelCamera>().viewportContainsMousePos())
            {
                if (isDraggingPart)
                {
                    //If we're currently dragging an electrical component
                    selectedPart.transform.position = findNearestGridSnap(selectedPart.transform.position); //Setting it to a snap position

                    if (onButtonClickBool)
                    {
                        //If user clicked an electrical component in the list, we make sure it doesn't update in this
                        onButtonClickBool = false;
                    }
                    else {
                        if (Input.GetMouseButtonUp(0) && partClickEnabled)
                        {
                            //Mouse click, so we check to see if it collides with anything. If no, we place it down
                            bool collision = false;
                            Transform child;
                            ElectricalPart childPart;
                            if (selectedPart.gameObject.GetComponent<Collision2DMonitor>().colliding > 0) collision = true;
                            else
                            {
                                //We first checked if the main object is colliding with anything. If not, we check the children (nodes)
                                for (int x = 0; x < selectedPart.transform.childCount; x++)
                                {
                                    child = selectedPart.transform.GetChild(x);
                                    childPart = child.GetComponent<ElectricalPart>();
                                    if (child.GetComponent<Collision2DMonitor>().colliding > 0)
                                    {
                                        //Child is colliding with something
                                        if (childPart != null && (childPart.GetType() == typeof(ElectricalNode) || childPart.GetType() == typeof(ElectricalFiberNode)))
                                        {
                                            //Child is a node, check to see if it's colliding with anything other than matching wire
                                            List<Collider2D> list = child.GetComponent<Collision2DMonitor>().collisionList;
                                            ElectricalPart part;
                                            for (int c = 0; c < list.Count; c++)
                                            {
                                                part = list[c].gameObject.GetComponent<ElectricalPart>();
                                                if (part != null)
                                                {
                                                    //If node is colliding with another electrical part
                                                    if (childPart.GetType() == typeof(ElectricalNode))
                                                    {
                                                        //First we check the type of node the child node is, then we check if it's colliding with matching wire. If not, we set collision=true so we can't place it here
                                                        if (part.GetType() != typeof(ElectricalWire)) { collision = true; break; }
                                                    }
                                                    else if (childPart.GetType() == typeof(ElectricalFiberNode))
                                                    {
                                                        //First we check the type of node the child node is, then we check if it's colliding with matching wire. If not, we set collision=true so we can't place it here
                                                        if (part.GetType() != typeof(ElectricalFiberWire)) { collision = true; break; }
                                                    }
                                                }
                                            }
                                        }
                                        else {
                                            collision = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!collision)
                            {
                                //No collisions, so we can place it.
                                for (int c = 0; c < selectedPart.transform.childCount; c++)
                                {
                                    //No non allowable collisions. Check all node children to see if we can splice their wires in.
                                    if (selectedPart.transform.GetChild(c).GetComponent<ElectricalNode>() != null) finishLineCheckIntersect(selectedPart.transform.GetChild(c).GetComponent<ElectricalNode>());
                                }
                                isDraggingPart = false;
                            }
                        }
                        else if (Input.GetMouseButtonUp(1))
                        {
                            //User cancels placing object. Destroy drag object and put the GUI image back in the list (if it's a ship part)
                            destroyComponent(selectedPart);
                        }
                    }
                }
                else {
                    //Not dragging a aprt
                    if (Input.GetMouseButtonUp(0) && partClickEnabled)
                    {
                        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(cam.ScreenPointToRay(Input.mousePosition), 1000);

                        if (hitInfo.transform != null && (hitInfo.transform.tag.Equals("ElectricalGUINode") || hitInfo.transform.tag.Equals("ElectricalGUIFiberNode")))
                        {
                            //Node clicked directly by user
                            if (!drawingLine)
                            {
                                //Not drawing a line at the moment, so start drawing a new one
                                startLine(getNodeAtPos(hitInfo.transform.position));
                            }
                            else if (hitInfo.transform.gameObject != startNode.gameObject && (currentLine.gameObject.GetComponent<Collision2DMonitor>().colliding == 0))
                            {
                                //User clicked a Node. If we're not colliding with anything, we're golden
                                ElectricalNode nodeAtPos = getNodeAtPos(hitInfo.transform.position, currentNode);
                                if (nodeAtPos.isSameTypeAs(currentLine))
                                {
                                    finishLine(true, nodeAtPos);
                                }
                            }
                        }
                        else if (drawingLine)
                        {
                            //No Node clicked
                            ElectricalNode Node = getNodeAtPos(lineEndPos);
                            if (Node != null)
                            {
                                //User didn't touch a Node, but the line still ended up at one. Make sure line isn't colliding with anything
                                if (Node.isSameTypeAs(currentLine))
                                {
                                    if (currentLine.gameObject.GetComponent<Collision2DMonitor>().colliding == 0) finishLine(false, Node);
                                }
                            }
                            else
                            {
                                //Completley new Node. Make sure line isn't colliding with anything
                                if (currentLine.gameObject.GetComponent<Collision2DMonitor>().colliding == 0) finishLine(false, currentNode);
                            }
                        }
                        else
                        {
                            //TODO check if player hit a part of a line and if so, add a Node there

                        }
                    }
                    else if (Input.GetMouseButtonUp(1))
                    {
                        if (drawingLine)
                        {
                            drawingLine = false;
                            destroyNode(currentNode);
                            electricalComponentList.Remove(currentLine);
                            Destroy(currentLine.gameObject);
                        }
                    }

                    if (drawingLine)
                    {

                        if (getNodeAtPos(lineEndPos) == startNode)
                        {
                            currentLine.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                        }
                        else currentLine.gameObject.GetComponent<SpriteRenderer>().enabled = true;

                        lineEndPos = findNearestGridSnap(startNode.transform.position);
                        float dist = Vector2.Distance(lineEndPos, startNode.transform.position);
                        dist -= startNode.radius * 2;
                        int angle = Mathf.RoundToInt(Mathf.Atan2(lineEndPos.x - startNode.transform.position.x, lineEndPos.y - startNode.transform.position.y) * Mathf.Rad2Deg);
                        currentLine.reorientSelf(angle, startNode.radius);
                        //Debug.Log("Radius: " + startNode.radius);
                        //Debug.Log("Angle: " + angle);

                        currentNode.transform.position = lineEndPos;
                        currentLine.transform.localScale = new Vector2(1.5f, dist);

                    }
                }
            }
        }
    }

    private bool isClickLayerOK()
    {
        return true;
    }

    private Vector2 getMousePos()
    {
        Vector3 inputPosition = Input.mousePosition;
        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3((Screen.width + (Screen.width * cam.rect.x)) - inputPosition.x, Screen.height - inputPosition.y, cam.transform.position.z));
        return mousePos;
    }

    private Vector2 findNearestGridSnap(Vector2 pos)
    {
        Vector2 snapPos;
        Vector2 mousePos = getMousePos();
        float xDist = Mathf.Abs(mousePos.x - pos.x);
        float yDist = Mathf.Abs(mousePos.y - pos.y);
        if (xDist < yDist) snapPos = new Vector2(pos.x, pos.y + Mathf.RoundToInt((mousePos.y / 5 - pos.y / 5)) * 5);
        else snapPos = new Vector2(pos.x + Mathf.RoundToInt((mousePos.x / 5 - pos.x / 5)) * 5, pos.y);
        return snapPos;
    }

    private void startLine(ElectricalNode startNode)
    {
        if (startNode is ElectricalFiberNode) currentNode = (ElectricalFiberNode)PartFactory.getInstance().createElectricalPart("FiberNode");
        else currentNode = (ElectricalNode)PartFactory.getInstance().createElectricalPart("Node");
        currentNode.transform.position = startNode.transform.position;
        electricalComponentList.Add(currentNode);
        currentNode.gameObject.GetComponent<CircleCollider2D>().enabled = false;

        lineStartPos = startNode.transform.position;
        if (startNode is ElectricalFiberNode) currentLine = (ElectricalFiberWire)PartFactory.getInstance().createElectricalPart("FiberWire");
        else currentLine = currentLine = (ElectricalWire)PartFactory.getInstance().createElectricalPart("Wire");
        currentLine.transform.position = lineStartPos;
        currentLine.addElectricalToList(startNode);
        electricalComponentList.Add(currentLine);
        currentNode.addElectricalToList(currentLine);
        startNode.addElectricalToList(currentLine);

        lineEndPos = startNode.transform.position;
        this.startNode = startNode;
        drawingLine = true;
    }

    private void finishLine(bool createAnother, ElectricalNode finishNode)
    {
        //Drawing line already, so we click this Node
        //Debug.Log("FinishNode: " + finishNode.gameObject.name);
        if (finishNode != currentNode) destroyNode(currentNode);
        else {
            if (finishLineCheckIntersect(currentNode))
            {
                currentNode.gameObject.GetComponent<CircleCollider2D>().enabled = true;
            }
            else
            {
                return;
            }
        }
        currentLine.addElectricalToList(finishNode);
        finishNode.addElectricalToList(currentLine);
        //Debug.Log("Distance: " + Vector2.Distance(NodeStart.transform.position, obj.transform.position));
        float dist = Vector2.Distance(finishNode.transform.position, startNode.transform.position);
        startNode = null;
        drawingLine = false;
        //if (createAnother) startLine(finishNode);
    }

    private bool finishLineCheckIntersect(ElectricalNode intersectingNode)
    {
        //Checks to see if we're intersecting another line with it, and if we are, seperate all the lines. Returns false if the node hits a wire that isn't the same type
        bool returnBool = true;
        Collider2D finishCollider;
        Collider2D[] finishColliders = Physics2D.OverlapCircleAll(intersectingNode.transform.position, intersectingNode.radius);
        if (finishColliders.Length > 0)
        {
            for (int c = 0; c < finishColliders.Length; c++)
            {
                finishCollider = finishColliders[c];
                if (finishCollider.gameObject.GetComponent<ElectricalWire>() != null)
                {
                    Debug.Log("WIRE NOT NULL");
                    List<ElectricalWire> wireList = getWireList();
                    for (int x = 0; x < wireList.Count; x++)
                    {
                        if (wireList[x].gameObject.GetComponent<BoxCollider2D>() == finishCollider)
                        {
                            if (intersectingNode.isSameTypeAs(wireList[x]))
                            {
                                ElectricalNode startNode = (ElectricalNode)wireList[x].getAttachedElectricalList()[0];
                                ElectricalNode endNode = (ElectricalNode)wireList[x].getAttachedElectricalList()[1];
                                float halfDist1 = Vector2.Distance(startNode.transform.position, intersectingNode.transform.position);
                                float halfDist2 = Vector2.Distance(endNode.transform.position, intersectingNode.transform.position);
                                halfDist1 -= intersectingNode.radius * 2;
                                halfDist2 -= intersectingNode.radius * 2;
                                wireList[x].transform.localScale = new Vector2(0.5f, halfDist1);
                                wireList[x].removeElectricalFromList(endNode);
                                wireList[x].addElectricalToList(intersectingNode);
                                endNode.removeElectricalFromList(wireList[x]);

                                ElectricalWire otherHalfWire;
                                if (wireList[x].GetType() == typeof(ElectricalFiberWire)) otherHalfWire = (ElectricalFiberWire)PartFactory.getInstance().createElectricalPart("FiberWire");
                                else otherHalfWire = (ElectricalWire)PartFactory.getInstance().createElectricalPart("Wire");
                                otherHalfWire.transform.position = intersectingNode.transform.position;
                                otherHalfWire.transform.rotation = wireList[x].transform.rotation;
                                otherHalfWire.addElectricalToList(intersectingNode);
                                otherHalfWire.addElectricalToList(endNode);
                                otherHalfWire.transform.localScale = new Vector2(0.5f, halfDist2);
                                otherHalfWire.reorientSelf((int)wireList[x].transform.rotation.eulerAngles.z, intersectingNode.radius);
                                electricalComponentList.Add(otherHalfWire);
                                intersectingNode.addElectricalToList(otherHalfWire);
                                endNode.addElectricalToList(otherHalfWire);
                                break;
                            } else
                            {
                                returnBool = false;
                            }
                        }
                    }
                }
            }
        }
        return returnBool;

    }

    public void selectedPartFromShipBuilder(BasePart part)
    {
        //User clicked a ship part, so we select it in the GUI
        bool inGUIList = false;

        foreach (Transform child in shipPartScrollContents.transform)
        {
            child.GetComponent<Button>().colors = child.GetComponent<ShipPartScrollListGUI>().defaultColorBlock;
        }

        foreach (ElectricalPart ePart in electricalComponentList)
        {
            if (ePart.gameObject.GetComponent<BoundBoxes_BoundBox>() != null) ePart.gameObject.GetComponent<BoundBoxes_BoundBox>().setEnable(false);
        }

        foreach (Transform child in shipPartScrollContents.transform)
        {
            if (child.GetComponent<ShipPartScrollListGUI>().attachedShipPart == part)
            {
                //The ship part is still in the GUI scroll list
                inGUIList = true;
                child.GetComponent<Button>().colors = child.GetComponent<ShipPartScrollListGUI>().selectedColorBlock;
                break;
            }
        }
        if (!inGUIList)
        {
            //Ship part was not in the GUI list, so we search the placed parts
            foreach (ElectricalPart ePart in electricalComponentList)
            {
                if (ePart.attachedShipPart == part)
                {
                    setSelectedPart(ePart);
                    break;
                }
            }
        }
    }

    private void createGUIButton(BasePart part)
    {
        Transform GUI = part.createGUIObject();

        GUI.SetParent(shipPartScrollContents.transform, false);
        string partName = GUI.name;
        GUI.GetComponent<Button>().onClick.AddListener(() => onShipPartButtonClick(GUI.gameObject, part, new Quaternion()));
    }

    public void destroyComponent(ElectricalPart comp)
    {
        if (isDraggingPart)
        {
            isDraggingPart = false;
            selectedPart = null;
        }
        if (comp.attachedShipPart != null)
        {
            createGUIButton(comp.attachedShipPart);
        }
        for (int x = 0; x < electricalComponentList.Count; x++)
        {
            if (electricalComponentList[x] is ElectricalNode && electricalComponentList[x].transform.root == comp.transform) {
                destroyNode((ElectricalNode)electricalComponentList[x]);
                x--;
            }
        }
        if (electricalComponentList.Contains(comp)) electricalComponentList.Remove(comp);
        Destroy(comp.gameObject);
    }

    private void destroyNode(ElectricalNode node)
    {
        if (electricalComponentList.Contains(node)) electricalComponentList.Remove(node);

        Destroy(node.gameObject);
    }

    private List<ElectricalNode> getNodeList()
    {
        List<ElectricalNode> list = new List<ElectricalNode>();
        for (int x = 0; x < electricalComponentList.Count; x++)
        {
            if (electricalComponentList[x] is ElectricalNode) list.Add((ElectricalNode)electricalComponentList[x]);
        }
        return list;
    }

    private List<ElectricalWire> getWireList()
    {
        List<ElectricalWire> list = new List<ElectricalWire>();
        for (int x = 0; x < electricalComponentList.Count; x++)
        {
            if (electricalComponentList[x] is ElectricalWire) list.Add((ElectricalWire)electricalComponentList[x]);
        }
        return list;
    }

    private void loadElectricalParts()
    {
        Debug.Log("loadElecParts");
        GameObject ifObj = PartFactory.getInstance().createElectricalPart("If").gameObject;
        ifObj.transform.localScale /= 5;
        ifObj.transform.SetParent(electricalPartScrollContents.transform, false);
        ifObj.GetComponent<Button>().onClick.AddListener(() => onElectricalPartButtonClick(ifObj, new Quaternion()));

        GameObject notIfObj = PartFactory.getInstance().createElectricalPart("NotIf").gameObject;
        notIfObj.transform.localScale /= 5;
        notIfObj.transform.SetParent(electricalPartScrollContents.transform, false);
        notIfObj.GetComponent<Button>().onClick.AddListener(() => onElectricalPartButtonClick(notIfObj, new Quaternion()));

        GameObject onOffObj = PartFactory.getInstance().createElectricalPart("OnOff").gameObject;
        onOffObj.transform.localScale /= 5;
        onOffObj.transform.SetParent(electricalPartScrollContents.transform, false);
        onOffObj.GetComponent<Button>().onClick.AddListener(() => onElectricalPartButtonClick(onOffObj, new Quaternion()));

        GameObject varResObj = PartFactory.getInstance().createElectricalPart("VariableResistor").gameObject;
        varResObj.transform.localScale /= 5;
        varResObj.transform.SetParent(electricalPartScrollContents.transform, false);
        varResObj.GetComponent<Button>().onClick.AddListener(() => onElectricalPartButtonClick(varResObj, new Quaternion()));

        GUIController.getInstance().setElectricalGUIPositions();
    }

    private void initNodes(ElectricalPart electricalBase)
    {
        foreach (Transform child in electricalBase.transform)
        {
            if (child.GetComponent<ElectricalPart>() != null && child.GetComponent<ElectricalPart>() is ElectricalNode)
            {
                electricalComponentList.Add(child.GetComponent<ElectricalPart>());
            }
        }
    }
}
