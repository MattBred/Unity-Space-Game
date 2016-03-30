using UnityEngine;
using UnityEngine.Networking;



public class PartFactory : MonoBehaviour
{
    private static PartFactory partFac;

    void Awake()
    {
        partFac = this;
    }

    public static PartFactory getInstance()
    {
        return partFac;
    }

    public BasePart createShipPart(string type, bool isLocal)
    {
        return createShipPart(type, isLocal, false);
    }

    public BasePart createShipPart(string type, bool isLocal, bool isDragPart)
    {
        string fabPath = "ShipPartFabs/" + type + "Fab";
        BasePart part = null;
        
        GameObject fabGame = (GameObject)Resources.Load(fabPath);
        NetworkManager netMan = null;
        if (GameObject.Find("NetworkManager") != null)
        {
            netMan = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        }

        if (netMan != null && netMan.isNetworkActive && !isLocal) MyNetworkPlayer.getInstance().spawnObject(fabGame);
        else
        {
            GameObject obj = (GameObject)Instantiate(fabGame, new Vector3(0, 0, 0), new Quaternion());
            part = obj.GetComponent<BasePart>();
            part.name = type;
            if (isDragPart) GUIController.getShipBuilderGUI().setDragPart(part);
        }

        return part;
    }

    public ElectricalPart createElectricalPart(string type)
    {
        ElectricalPart part = null;
        GameObject obj;
        GameObject fabGame = (GameObject)Resources.Load("ElectricalPartFabs/Electrical" + type + "Fab");
        obj = (GameObject)Object.Instantiate(fabGame, new Vector3(0, 0, 0), new Quaternion());
        foreach (Transform child in obj.transform)
        {
            if (child.name.Equals("ElectricalNode"))
            {
                ElectricalNode node = (ElectricalNode)createElectricalPart("Node");
                node.transform.position = child.position;
                node.transform.SetParent(child.transform.root);
                Destroy(child.gameObject);
            }
            else if (child.name.Equals("ElectricalFiberNode"))
            {
                ElectricalFiberNode node = (ElectricalFiberNode)createElectricalPart("FiberNode");
                node.transform.position = child.position;
                node.transform.SetParent(child.transform.root);
                Destroy(child.gameObject);
            }
        }
        part = obj.GetComponent<ElectricalPart>();
        part.name = type;
        return part;
    }
}
