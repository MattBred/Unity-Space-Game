using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ShipManager : MonoBehaviour {
    public static int SHIP_STATUS_BUILDER_SHIP = 0;
    public static int SHIP_STATUS_BUILDER_ELECTRICAL = 1;
    public static int SHIP_STATUS_NORMAL = 2;
    public int shipStatus = SHIP_STATUS_BUILDER_ELECTRICAL;

    public static ShipManager playerControlledShip;
    private List<BasePart> partList = new List<BasePart>();
    public List<ElectricalPart> electricalPartList = new List<ElectricalPart>();
    private bool isPlayerShip = false;
    private BasePart shipOwner;
   
    public static BasePart selectedPart;

    void Awake()
    {
        shipOwner = gameObject.GetComponent<BasePart>();
        addPart(gameObject.GetComponent<BasePart>());
    }

    public void setPlayerShip(bool val)
    {
        isPlayerShip = val;
        if (isPlayerShip) playerControlledShip = this;
        else if (playerControlledShip == this) playerControlledShip = null;
    }

    public void setSelectedPart(BasePart part)
    {
        if (selectedPart != null) selectedPart.gameObject.GetComponent<BoundBoxes_BoundBox>().setEnable(false);
        selectedPart = part;
        selectedPart.gameObject.GetComponent<BoundBoxes_BoundBox>().setEnable(true);
        if (shipStatus == SHIP_STATUS_BUILDER_ELECTRICAL)
        {
            GUIController.getElectricalGUI().selectedPartFromShipBuilder(part);
        }
        if (selectedPart.GetType() == typeof(Cockpit))
        {
            foreach (ElectricalPart ePart in getPlayerShip().electricalPartList)
            {
                ePart.hide();
            }
            getPlayerShip().setPlayerShip(false);
            selectedPart.gameObject.GetComponent<ShipManager>().setPlayerShip(true);
            foreach (ElectricalPart ePart in getPlayerShip().electricalPartList)
            {
                ePart.show();
            }
        }
    }


    public void deletePart(BasePart part)
    {
        //partList.Remove(part);
        Destroy(part.gameObject);
    }

	// Update is called once per frame
	void Update () {
        BasePart part;

        foreach (ElectricalPart electricalPart in electricalPartList)
        {
            electricalPart.energyPercent = 0;
            if (electricalPart is ElectricalPart.Fiber) (electricalPart as ElectricalPart.Fiber).setFiberState(false);
        }
        foreach (ElectricalPart electricalPart in electricalPartList)
        {
            if (electricalPart is ElectricalReactor)
            {
                electricalPart.sendEnergy(1);
            }
        }

        for (int x = 0; x < partList.Count; x++)
        {
            part = partList[x];
            if (part == null) {
                //The on destroy method has been called
                partList.Remove(part);

                for (int s = 0; s < partList.Count; s++)
                {
                    BasePart detatchPart = partList[s];
                    if (detatchPart.gameObject.GetComponent<ConfigurableJoint>() != null && getConnectedCount(detatchPart.gameObject) == 0)
                    {
                        List<BasePart> childList = new List<BasePart>();
                        childList.Add(detatchPart);
                        checkConfigurableJointTree(childList, detatchPart);
                        bool stillAttachedToCockpit = false;
                        for (int i = 0; i < childList.Count; i++)
                        {
                            /*ParticleSystem p;
                            if (childList[i].getGameObject().GetComponent<ParticleSystem>() == null) p = childList[i].getGameObject().AddComponent<ParticleSystem>();
                            else p = childList[i].getGameObject().GetComponent<ParticleSystem>();
                            p.startSpeed = 0;
                            p.maxParticles = 1;
                            SerializedObject so = new SerializedObject(p);
                            so.FindProperty("ShapeModule.enabled").boolValue = false;
                            so.ApplyModifiedProperties();*/
                            if (childList.Contains(getShipOwner()))
                            {
                                stillAttachedToCockpit = true;
                                //Debug.Log("Still attached to cockpit");
                            }
                        }
                        if (!stillAttachedToCockpit)
                        {
                            ShipManager manager = detatchPart.gameObject.AddComponent<ShipManager>();
                            Debug.Log("Creating new Ship Manager");
                            for (int i = 0; i < childList.Count; i++)
                            {
                                manager.getPartList().Add(childList[i]);
                                childList[i].setShipManager(manager);
                                setIgnoreCollisions(manager, this, false); //Make the new ship collide with old one
                                setIgnoreCollisions(manager, manager, true); //Make parts of new ship not collide with eachother
                                partList.Remove(childList[i]);
                            }
                        }

                    }
                }
                
            }
        }
	}

    public static void setIgnoreCollisions(ShipManager manager, ShipManager shipManagerToIgnore, bool ignore)
    {
        //Makes all the ship parts of the same ship ignore each others collision
        List<Collider> colliderList = getAllShipColliders(manager);
        List<Collider> otherColliderList = getAllShipColliders(shipManagerToIgnore);

        foreach (Collider c in colliderList)
        {
            foreach (Collider c2 in otherColliderList)
            {
                if (c != c2) Physics.IgnoreCollision(c, c2, ignore);
            }
        }
    }

    private static List<Collider> getAllShipColliders(ShipManager manager)
    {
        //Returns all of the non trigger colliders for all the ship parts (to make setIgnoreCollisions easier)
        List<Collider> list = new List<Collider>();
        List<BasePart> partList = manager.getPartList();

        foreach (BasePart part in partList)
        {
            //Check each ship part
            foreach (Collider c in part.gameObject.GetComponents<Collider>())
            {
                //Check the parent object first
                if (!c.isTrigger) list.Add(c);
            }
            foreach (Transform child in part.gameObject.transform)
            {
                //Then we check each child object (other meshes)
                foreach (Collider c in child.GetComponents<Collider>())
                {
                    if (!c.isTrigger) list.Add(c);
                }
            }
        }
        return list;
    }

    private int getConnectedCount(GameObject obj)
    {
        int connected = 0;
        if (obj.GetComponent<ConfigurableJoint>() != null)
        {
            for (int x = 0; x < obj.GetComponents<ConfigurableJoint>().Length; x++)
            {
                if (obj.GetComponents<ConfigurableJoint>()[x].connectedBody != null) connected++;
            }
        }

        return connected;
    }

    private void checkConfigurableJointTree(List<BasePart> list, BasePart part)
    {
        //Can not run this if two fixed joints are attached to each other!
        BasePart newPart;
        GameObject newPartObj;
        for (int x = 0; x < partList.Count; x++)
        {
            if (partList[x] != null && partList[x].gameObject != null && partList[x] != part)
            {
                newPart = partList[x];
                newPartObj = newPart.gameObject;

                if (part.gameObject.GetComponent<ConfigurableJoint>() != null)
                {
                    for (int y = 0; y < part.gameObject.GetComponents<ConfigurableJoint>().Length; y++)
                    {
                        if (part.gameObject.GetComponents<ConfigurableJoint>()[y].connectedBody == newPartObj.GetComponent<Rigidbody>())
                        {
                            if (!list.Contains(newPart))
                            {
                                list.Add(newPart);
                                checkConfigurableJointTree(list, newPart);
                            }
                        }
                    }
                }
                if (newPartObj.GetComponent<ConfigurableJoint>() != null)
                {
                    for (int y = 0; y < newPartObj.GetComponents<ConfigurableJoint>().Length; y++)
                    {
                        if (!list.Contains(newPart) && newPart.gameObject.GetComponents<ConfigurableJoint>()[y] != null && newPart.gameObject.GetComponents<ConfigurableJoint>()[y].connectedBody == part.gameObject.GetComponent<Rigidbody>())
                        {
                            list.Add(newPart);
                            checkConfigurableJointTree(list, newPart);
                        }
                    }
                }
            }

        }
    }

    public static ShipManager getPlayerShip()
    {
        return playerControlledShip;
    }

    public void setShipOwner(BasePart part)
    {
        shipOwner = part;
    }

    public BasePart getShipOwner()
    {
        return shipOwner;
    }


    public List<BasePart> getPartList()
    {
        return partList;
    }

    public void addPart(BasePart part)
    {
        partList.Add(part);
        part.shipManager = this;
    }

    public void releaseShip()
    {
        shipStatus = SHIP_STATUS_NORMAL;
        BasePart comp;
        for (int x = 0; x < partList.Count; x++)
        {
            comp = partList[x];
            comp.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }



    public void saveShip(string fileName)
    {
        //**************************************
        // Create the 'ship' save
        //**************************************
        SaveLoad.Ship save = new SaveLoad.Ship();
        SaveLoad.SaveShipPart savePart;
        foreach (BasePart part in partList)
        {
            savePart = part.createSave(true);
            if (savePart.isShipOwner) save.pos = new Float3(savePart.gameObject.transform.position);
            save.savedShipParts.Add(savePart);
        }

        foreach (ElectricalPart part in electricalPartList)
        {
            save.savedElectricalParts.Add(part.createSave(true));
        }

        //****************************************************************************
        // Add data to the saved ship parts
        //****************************************************************************
        foreach (SaveLoad.SaveShipPart savedPart in save.savedShipParts)
        {
            //Now that everything is added to the 'saved ship', we go through and add each extra bit
            BasePart basePart = savedPart.gameObject.GetComponent<BasePart>();

            //For this we link the ship objects to the eletrical objects
            foreach (SaveLoad.SaveElectricalPart savePartE in save.savedElectricalParts)
            {
                if (basePart.electricalPart != null && savePartE.gameObject == basePart.electricalPart.gameObject)
                {
                    savedPart.electricalPart = savePartE;
                    savePartE.shipPart = savedPart;
                }
            }

            //For this we add the config joint data
            foreach (ConfigurableJoint joint in savedPart.gameObject.GetComponents<ConfigurableJoint>())
            {
                foreach (SaveLoad.SaveShipPart savedPartConnected in save.savedShipParts)
                {
                    if (savedPartConnected.gameObject == joint.connectedBody.gameObject)
                    {
                        //Adds the 'configurable joint' to the saved object's list
                        savedPart.jointList.Add(savedPartConnected);
                    }
                }
            }
        }


        //****************************************************************************
        // Add data to the saved electrical parts
        //****************************************************************************
        foreach (SaveLoad.SaveElectricalPart savePartE in save.savedElectricalParts)
        {
            //Go through each save electrical part and all the data for their stuff
            ElectricalPart ePart = savePartE.gameObject.GetComponent<ElectricalPart>();

            if (savePartE.gameObject.transform.root.gameObject != savePartE.gameObject)
            {
                //If the parent isn't this, that means this is a node for a different electrical part
                //We then need to find the save parent and link it to the save part
                foreach (SaveLoad.SaveElectricalPart possibleParent in save.savedElectricalParts)
                {
                    if (possibleParent.gameObject == savePartE.gameObject.transform.root.gameObject)
                    {
                        savePartE.parent = possibleParent;
                        break;
                    }
                }
            }

            foreach (ElectricalPart attachedEPart in ePart.getAttachedElectricalList())
            {
                foreach (SaveLoad.SaveElectricalPart savePartEConnected in save.savedElectricalParts)
                {
                    if (savePartEConnected.gameObject == attachedEPart.gameObject)
                    {
                        savePartE.attachedList.Add(savePartEConnected);
                    }
                }
            }
        }



        //****************************************************************************
        // Do the fun file creation type stuff
        //****************************************************************************
        string dirString = Application.dataPath + "/SavedShips/";

        if (!Directory.Exists(dirString)) Directory.CreateDirectory(dirString);
        var stream = new FileStream(dirString + fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, save);
        stream.Close();

        Debug.Log("Ship Saved");
    }
}


