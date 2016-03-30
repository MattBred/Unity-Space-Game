using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoad {

    public static void loadShip(string fileName)
    {
        //****************************************************************************
        // Loads a ship from a file name 
        //****************************************************************************
        string shipPath = Application.dataPath + "/SavedShips/" + fileName;
        using (var stream = new FileStream(shipPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var formatter = new BinaryFormatter();
            var ship = (Ship)formatter.Deserialize(stream);
            ShipManager manager = null;
            BasePart basePart;


            //****************************************************************************
            // Creates all the ship parts and sets the ship manager
            //****************************************************************************
            foreach (SaveShipPart shipPartSave in ship.savedShipParts)
            {
                //Create the prefab of each object via PartFactory
                GameObject obj = PartFactory.getInstance().createShipPart(shipPartSave.typeName,true).gameObject;
                shipPartSave.gameObject = obj;
                if (shipPartSave.isShipOwner)
                {
                    //If this object is the 'owner' of the ship (part with the ShipManager script) then we add the appropriate jazz
                    manager = obj.AddComponent<ShipManager>();
                    obj.transform.position = Float3.toVector3(ship.pos);
                    obj.transform.Translate(Vector3.right * 8);
                    obj.transform.Rotate(Float3.toVector3(shipPartSave.rot));
                    obj.GetComponent<Rigidbody>().velocity = Float3.toVector3(shipPartSave.vel);
                }
            }


            //****************************************************************************
            // Creates all the electrical parts that were placed in the electrical GUI
            //****************************************************************************
            foreach (SaveElectricalPart ePartSave in ship.savedElectricalParts)
            {
                ElectricalPart newEPart = PartFactory.getInstance().createElectricalPart(ePartSave.typeName);
                ePartSave.gameObject = newEPart.gameObject;
                manager.electricalPartList.Add(newEPart);
            }


            //****************************************************************************
            // Adds the data to the new ship parts
            //****************************************************************************
            foreach (SaveShipPart shipPartSave in ship.savedShipParts)
            {
                basePart = shipPartSave.gameObject.GetComponent<BasePart>();
                if (!shipPartSave.isShipOwner) manager.addPart(basePart);
                basePart.loadFromSave(shipPartSave);

                foreach (SaveShipPart attachedPart in shipPartSave.jointList)
                {
                    ConfigurableJoint joint = shipPartSave.gameObject.AddComponent<ConfigurableJoint>();
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    joint.projectionAngle = 1;
                    joint.projectionDistance = 1;
                    joint.xMotion = 0;
                    joint.yMotion = 0;
                    joint.zMotion = 0;
                    joint.angularXMotion = 0;
                    joint.angularYMotion = 0;
                    joint.angularZMotion = 0;
                    joint.connectedBody = attachedPart.gameObject.GetComponent<Rigidbody>();
                }
            }
            ShipManager.setIgnoreCollisions(manager, manager, true); //Make each ship part ignore the collision of each other part on the same ship


            //****************************************************************************
            // Adds the data to the new electrical parts
            //****************************************************************************
            foreach (SaveElectricalPart ePartSave in ship.savedElectricalParts)
            {
                ElectricalPart ePartScript = ePartSave.gameObject.GetComponent<ElectricalPart>();
                ePartScript.loadFromSave(ePartSave);
                foreach (SaveElectricalPart attachedPart in ePartSave.attachedList)
                {
                    ePartScript.addElectricalToList(attachedPart.gameObject.GetComponent<ElectricalPart>());
                }
            }


            //****************************************************************************
            // Removes duplicate electrical parts
            //****************************************************************************
            /*
                This looks for duplicate electrical parts and removes them (nodes, so far). What happens is when we save, we copy all the nodes from the original ship, and then in this script
                we re-make the ship part prefabs which have their own nodes. So we need to delete the new pre-fabbed nodes so there are no duplicates.
            */
            foreach (SaveElectricalPart ePartSave in ship.savedElectricalParts)
            {
                foreach (ElectricalPart ePart in manager.electricalPartList) {
                    foreach (Transform child in ePart.gameObject.transform)
                    {
                        if (child != ePartSave.gameObject.transform && Vector2.Distance(child.position, ePartSave.gameObject.transform.position) < 1)
                        {
                            ePart.removeElectricalFromList(child.GetComponent<ElectricalPart>());
                            Object.Destroy(child.gameObject);
                        }
                    }
                }
            }
        }

        Debug.Log("LOADED!");
    }


    [System.Serializable]
    public class Ship
    {
        public Float3 pos; //Pos of the part of the ship that owns the shipManager (usually a cockpit)
        public List<SaveShipPart> savedShipParts = new List<SaveShipPart>();
        public List<SaveElectricalPart> savedElectricalParts = new List<SaveElectricalPart>();
    }

    [System.Serializable]
    public class SaveShipPart
    {
        [System.NonSerialized]
        public GameObject gameObject;

        public SaveElectricalPart electricalPart;
        public bool isShipOwner; //Is this the part with the ship manager script running? If so, we need to load it on load
        public int health;
        public Float3 relPos; //Relative position to the ship manager object (which will be (0,0,0))
        public Float3 vel;
        public Float3 rot;
        public string typeName;
        public List<SaveShipPart> jointList = new List<SaveShipPart>();
    }

    [System.Serializable]
    public abstract class SaveElectricalPart
    {
        [System.NonSerialized]
        public GameObject gameObject;

        public SaveShipPart shipPart;
        public SaveElectricalPart parent;
        public string typeName;
        public Float3 pos;
        public Float3 scale;
        public List<SaveElectricalPart> attachedList = new List<SaveElectricalPart>();

    }
}

[System.Serializable]
public class Float3
{
    public float x;
    public float y;
    public float z;
    public Float3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Float3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    public static Vector3 toVector3(Float3 float3)
    {
        Vector3 v = new Vector3();
        v.x = float3.x;
        v.y = float3.y;
        v.z = float3.z;
        return v;
    }
}