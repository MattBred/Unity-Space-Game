using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collision2DMonitor : MonoBehaviour {
    public int colliding;
    public List<Collider2D> collisionList;

    void Start()
    {
        colliding = 0;
        collisionList = new List<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        //Debug.Log(gameObject.name + " is Colliding with: " + coll.gameObject.name);
        colliding++;
        collisionList.Add(coll);
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        colliding--;
        if (collisionList.Contains(coll)) collisionList.Remove(coll);
        if (colliding < 0) colliding = 0;
    }
}
