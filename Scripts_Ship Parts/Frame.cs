﻿using UnityEngine;

public class Frame : BasePart
{
    protected override void Start()
    {
        base.Start();
        name = "Frame";
        healthMax = 200;
        health = 200;
        energyConsumingMax = 100;
    }

    protected override void Update()
    {
        //Unless the object is destroyed, this is called every frame
        base.Update();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        
    }

    public override void OnWeaponHit(Collision collision, GameObject bulletObj)
    {
        string weaponType = bulletObj.tag;
        if (weaponType.Equals("WeaponKinetic")) {
            //health -= collision.relativeVelocity
            float force = collision.relativeVelocity.magnitude * bulletObj.GetComponent<Rigidbody>().mass;
            //Debug.Log("Health: " + health + "Health - " + (int)(force * 100) + " Mag " + collision.relativeVelocity.magnitude + " Mass: " + bulletObj.GetComponent<Rigidbody>().mass + " Force: " + force);
            health -= (int)(force*100);
        }
    }

    public override void onDestroy()
    {
        //Called when this part gets destroyed
        base.onDestroy();
        //GameObject fab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Done/Done_Prefabs/Done_VFX/Done_Explosions/done_explosion_player.prefab",typeof(GameObject));
       // GameObject explosion = (GameObject)Object.Instantiate(fab, gameobj.transform.position, gameobj.transform.rotation);
        for (int x = 0; x < gameObject.transform.childCount; x++)
        {
            if (gameObject.transform.GetChild(x).tag.Equals("BasePartDetachable"))
            {
                Transform part = gameObject.transform.GetChild(x);
                part.SetParent(null);
                part.gameObject.AddComponent<Rigidbody>();
                part.gameObject.GetComponent<Rigidbody>().useGravity = false;
                part.gameObject.GetComponent<Rigidbody>().AddExplosionForce(10, gameObject.transform.position, 10);
                part.gameObject.GetComponent<Rigidbody>().angularVelocity = (new Vector3(UnityEngine.Random.Range(1, 360), UnityEngine.Random.Range(1, 360), UnityEngine.Random.Range(1, 360)));
                part.gameObject.AddComponent<BoxCollider>();
                part.gameObject.GetComponent<BoxCollider>().size *= 0.9f;
                part.gameObject.AddComponent<Debris>();
                part.tag = "BasePartDebris";
            }
        }
    }

    public override SaveLoad.SaveShipPart createSave(bool exactInstance)
    {
        Save save = new Save();
        setSaveDefaults(save, exactInstance);
        return save;
    }

    public override void loadFromSave(SaveLoad.SaveShipPart save)
    {
        loadSaveDefaults(save);
    }

    [System.Serializable]
    private class Save : SaveLoad.SaveShipPart
    {

    }
}

