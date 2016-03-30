using UnityEngine;

public class Thruster : BasePart {
    public int thrustOutputMax;
    public float thrustOutput;

    protected override void Start()
    {
        base.Start();
        name = "Thruster";
        health = 200;
        healthMax = 200;
        energyConsumingMax = 100;
        thrustOutputMax = 100;
        thrustOutput = 0;
    }

    protected override void Update()
    {
        //Unless the object is destroyed, this is called every frame
        base.Update();

        if (getShipManager() != null && ShipManager.getPlayerShip() != null && getShipManager() == ShipManager.getPlayerShip())
        {
            if (thrustOutput > 10)
            {
                //Debug.Log("Thrust Output " + thrustOutput);
                gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * thrustOutput);
                //gameObject.transform.parent.GetComponent<Rigidbody>().AddForceAtPosition(gameObject.transform.forward * thrustOutput, gameObject.transform.position);
                float minSize = 0.3f * (thrustOutput / thrustOutputMax);
                if (minSize < 0.1f) minSize = 0.1f;
                gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().minParticleSize = minSize;
                if (!gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying)
                {
                    gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().loop = true;
                    gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                }
            }
            else {
                if (gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying)
                {
                    gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().loop = false;
                }
            }
        }
    }

    public override void onDestroy()
    {
        //Called when this part gets destroyed
        base.onDestroy();
        //GameObject fab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Done/Done_Prefabs/Done_VFX/Done_Explosions/done_explosion_player.prefab", typeof(GameObject));
        //GameObject explosion = (GameObject)Object.Instantiate(fab, gameObject.transform.position, gameObject.transform.rotation);
        gameObject.GetComponent<Rigidbody>().AddExplosionForce(800, gameObject.transform.position, 20);
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
