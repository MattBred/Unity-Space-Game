using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletMonitor : MonoBehaviour {
    private float spawnTime;
    private float lifeTime;
	// Use this for initialization
	void Start () {
        spawnTime = Time.time;
        lifeTime = 3;
        tag = "WeaponKinetic";
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time > (spawnTime + lifeTime)) Destroy(gameObject);
	}

    public virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("BasePart"))
        {
            BasePart part = collision.gameObject.GetComponent<BasePart>();
            part.OnWeaponHit(collision, gameObject);
        }
        else if (collision.gameObject.tag.Equals("BasePartDebris"))
        {
            Debris debris = collision.gameObject.GetComponent<Debris>();
            debris.OnWeaponHit(collision, gameObject);
        }
    }
}
