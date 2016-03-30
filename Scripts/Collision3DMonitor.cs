using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collision3DMonitor : MonoBehaviour {
    public int colliding = 0;
    public int triggerColliding = 0;
    public List<Collision> collisionList = new List<Collision>();
    public List<Collider> triggerList = new List<Collider>();


    void OnCollisionEnter(Collision collision)
    {
        collisionList.Add(collision);
        colliding++;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collisionList.Contains(collision)) collisionList.Remove(collision);
        colliding--;
        if (colliding < 0) colliding = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            triggerList.Add(other);
            triggerColliding++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger && triggerList.Contains(other))
        {
            triggerColliding--;
            if (triggerColliding < 0) triggerColliding = 0;
            triggerList.Remove(other);
        }
    }
}
