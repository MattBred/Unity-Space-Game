using UnityEngine;
using System.Collections;

public class ShootingMain
{
    public static GameObject bulletFab = (GameObject)Resources.Load("ShipPartFabs/BulletFab");
}

public class Shooting : MonoBehaviour {
    private float lastShotTime;
    private float shootDelay;

    // Use this for initialization
    void Start () {
        new ShootingMain();
        shootDelay = 0.1f;
        lastShotTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Time.time > (lastShotTime + shootDelay))
        {
            lastShotTime = Time.time;
            GameObject bullet = (GameObject)Object.Instantiate(ShootingMain.bulletFab, transform.FindChild("BulletSpawn").transform.position, transform.rotation);
            bullet.GetComponent<Rigidbody>().velocity = (transform.forward * 10);
        }
	}
}
