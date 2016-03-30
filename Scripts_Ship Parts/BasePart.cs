using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BasePart : NetworkBehaviour
{
    public ElectricalPart electricalPart;
    public int healthMax;
    public int health;
    public float energyOutputMax;
    public float energyOutput;
    public float energyConsumingMax;
    public float energyConsuming;
    private bool destroyed;
    public ShipManager shipManager;
    public List<Collider> collisionTriggerList;
    private float lockedHeight;
    private bool lockRotationBool;
    private List<Transform> shipConnectors;
    private NetworkHandler networkHandler;

    private class NetworkHandler : NetworkBehaviour
    {
        [SyncVar]
        public Vector3 pos;

        public void updatePos(BasePart part)
        {
            if (!isServer) part.transform.position = pos;
            else pos = part.transform.position;
        }
    }

    protected virtual void Awake()
    {
        //networkHandler = gameObject.AddComponent<NetworkHandler>();
        lockedHeight = 0;
        collisionTriggerList = new List<Collider>();
        gameObject.tag = "BasePart";
        BoundBoxes_BoundBox boundBoxes = gameObject.AddComponent<BoundBoxes_BoundBox>();
        boundBoxes.enabled = false;
        lockRotationBool = true;
        destroyed = false;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update() {
        //networkHandler.updatePos(this);
        if (!isDestroyed())
        {
            if (health <= 0) onDestroy();
        }
        if (ShipManager.getPlayerShip() != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Input.GetMouseButtonUp(2))
            {
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.transform.root == gameObject.transform.root)
                    {
                        CameraSystem.getInstance().setTarget(gameObject.transform);
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.transform.root == gameObject.transform.root)
                    {
                        if (GUIController.getShipBuilderGUI().dragPart == null) ShipManager.getPlayerShip().setSelectedPart(this);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Delete) && shipManager == ShipManager.getPlayerShip() && ShipManager.getPlayerShip().shipStatus == ShipManager.SHIP_STATUS_BUILDER_SHIP && ShipManager.selectedPart == this)
            {
                ShipManager.getPlayerShip().deletePart(this);
            }
            if (lockRotationBool)
            {
                gameObject.transform.rotation = Quaternion.Euler(0, gameObject.transform.rotation.eulerAngles.y, 0);
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, lockedHeight, gameObject.transform.position.z);
            }
        }
    }

    public void setLayers(int lay)
    {
        gameObject.layer = lay;
        for (int x = 0; x < gameObject.transform.childCount; x++)
        {
            gameObject.transform.GetChild(x).gameObject.layer = lay;
        }
    }

    public void setShipManager(ShipManager man)
    {
        shipManager = man;
    }

    public ShipManager getShipManager()
    {
        return shipManager;
    }

    public virtual void onDestroy() {
        destroyed = true;
    }

    public bool isDestroyed()
    {
        return destroyed;
    }

    public BoxCollider getTriggerCollider()
    {
        for (int x = 0; x < gameObject.GetComponents<BoxCollider>().Length; x++)
        {
            if (gameObject.GetComponents<BoxCollider>()[x].isTrigger) return gameObject.GetComponents<BoxCollider>()[x];
        }
        return null;
    }

    public List<Collider> getAllColliders()
    {
        List<Collider> list = new List<Collider>();
        list.AddRange(gameObject.GetComponents<Collider>().ToList());
        for (int x = 0; x < gameObject.transform.childCount; x++)
        {
            list.AddRange(gameObject.transform.GetChild(x).GetComponents<Collider>().ToList());
        }
        return list;
    }

    public Transform createGUIObject()
    {
        GameObject s = PartFactory.getInstance().createShipPart(name,true).gameObject;
        Transform GUI = (s.transform.GetChild(0));
        GUI.SetParent(null);
        GUI.name = name;
        GUI.gameObject.AddComponent<ShipPartScrollListGUI>();
        GUI.gameObject.GetComponent<ShipPartScrollListGUI>().attachedShipPart = this;
        Destroy(s);
        return GUI;
    }

    public void setLockRotation(bool val)
    {
        lockRotationBool = val;
        if (val)
        {
            lockedHeight = gameObject.transform.position.y;
        }
    }

   
    public List<Transform> getAllShipConnectors()
    {
        if (shipConnectors == null) findShipConnectors();
        return shipConnectors;
    }

    private void findShipConnectors()
    {
        shipConnectors = new List<Transform>();
        for (int c = 0; c < gameObject.transform.childCount; c++)
        {
            if (gameObject.transform.GetChild(c).name.StartsWith("ShipConnector"))
            {
                shipConnectors.Add(gameObject.transform.GetChild(c));
            }
        }
    }

    public void hide()
    {
        //Sets all the renders & colliders to disable. For when switching from ship to ship
        foreach (Renderer ren in gameObject.GetComponents<Renderer>())
        {
            ren.enabled = false;
        }
        foreach (Collider col in gameObject.GetComponents<Collider>())
        {
            col.enabled = false;
        }
        foreach (Transform child in gameObject.transform)
        {
            foreach (Renderer ren in child.GetComponents<Renderer>())
            {
                ren.enabled = false;
            }
            foreach (Collider col in child.GetComponents<Collider>())
            {
                col.enabled = false;
            }
        }
    }

    public void show()
    {
        //Sets all the renders & colliders to enabled. For when switching from ship to ship
        foreach (Renderer ren in gameObject.GetComponents<Renderer>())
        {
            ren.enabled = true;
        }
        foreach (Collider col in gameObject.GetComponents<Collider>())
        {
            col.enabled = true;
        }
        foreach (Transform child in gameObject.transform)
        {
            foreach (Renderer ren in child.GetComponents<Renderer>())
            {
                ren.enabled = false;
            }
            foreach (Collider col in child.GetComponents<Collider>())
            {
                col.enabled = true;
            }
        }
    }

    public abstract SaveLoad.SaveShipPart createSave(bool exactInstance);

    public abstract void loadFromSave(SaveLoad.SaveShipPart save);

    protected void setSaveDefaults(SaveLoad.SaveShipPart save, bool exactInstance)
    {
        //Saves position, gameobject, name, rotation, health
        save.relPos = new Float3(getShipManager().getShipOwner().gameObject.transform.position - transform.position);
        if (shipManager.getShipOwner() == this) save.isShipOwner = true;
        if (transform.GetComponent<Rigidbody>() != null) save.vel = new Float3(transform.GetComponent<Rigidbody>().velocity);
        save.typeName = name;
        save.rot = new Float3(transform.rotation.eulerAngles);
        save.gameObject = gameObject;
        if (exactInstance) save.health = health;
    }

    protected void loadSaveDefaults(SaveLoad.SaveShipPart save)
    {
        if (!save.isShipOwner)
        {
            //If the part is not the ship owner, then we set it's relative position and rotation
            gameObject.transform.position = shipManager.getShipOwner().gameObject.transform.position - Float3.toVector3(save.relPos);
            gameObject.transform.Rotate(Float3.toVector3(save.rot));
        }
        setLockRotation(true);
        health = save.health;

        if (save.electricalPart != null)
        {
            //Links the ship & electrical part
            electricalPart = save.electricalPart.gameObject.GetComponent<ElectricalPart>();
            electricalPart.attachedShipPart = this;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision) { }

    public virtual void onParticleCollision(GameObject other) { }

    public virtual void OnWeaponHit(Collision collision, GameObject weaponObj) { }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) collisionTriggerList.Add(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) collisionTriggerList.Remove(other);
    }
}
