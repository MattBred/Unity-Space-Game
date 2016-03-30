using UnityEngine;
using System.Collections;

public class Debris : MonoBehaviour {
    //Debris monitors space debris (pieces detatched from main BaseParts) and deletes them after awhile, or deletes them if they take enough damage
    int health = 20;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (health <= 0) Destroy(gameObject);
	}

    public void OnWeaponHit(Collision collision, GameObject weaponObj)
    {
        string weaponType = weaponObj.tag;
        if (weaponType.Equals("WeaponKinetic"))
        {
            float force = collision.relativeVelocity.magnitude * weaponObj.GetComponent<Rigidbody>().mass;
            health -= (int)(force * 100);
        }
    }
}
