using UnityEngine;
using System.Collections;

public class GattlingGun : BasePart {

    protected override void Start()
    {
        base.Start();
        name = "GattlingGun";
        GetComponent<BoundBoxes_BoundBox>().colliderBased = true;
    }
    protected override void Update () {
        base.Update();
        Transform shootEffects = gameObject.transform.FindChild("ShootEffects");
        ParticleSystem sparks = shootEffects.FindChild("ParticleSparks").GetComponent<ParticleSystem>();
        ParticleSystem flare = shootEffects.FindChild("ParticleFlare").GetComponent<ParticleSystem>();
        ParticleSystem bullets = shootEffects.FindChild("ParticleBullets").GetComponent<ParticleSystem>();

        if (Input.GetKey(KeyCode.Space))
        {
            //FIRE ZE WEAPON
            

            if (!gameObject.GetComponent<Animation>().IsPlaying("Shoot"))
            {

                gameObject.GetComponent<Animation>().wrapMode = WrapMode.Loop;
                gameObject.GetComponent<Animation>().Play("Shoot");
                shootEffects.FindChild("Light").GetComponent<Light>().enabled = true;
            }
                if (!sparks.isPlaying) sparks.Play();
                if (!flare.isPlaying) flare.Play();
                if (!bullets.emission.enabled) bullets.enableEmission = true;
            
            
        }
        else
        {
            if (gameObject.GetComponent<Animation>().IsPlaying("Shoot"))
            {
                gameObject.GetComponent<Animation>().Stop("Shoot");
                shootEffects.FindChild("Light").GetComponent<Light>().enabled = false;
            }

                if (sparks.isPlaying)
                {
                    sparks.Clear();
                    sparks.Stop();
                    
                }
                if (flare.isPlaying)
                {
                    flare.Clear();
                    flare.Stop();
                    
                }
                if (bullets.emission.enabled)
                {
                bullets.enableEmission = false;
                    
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
