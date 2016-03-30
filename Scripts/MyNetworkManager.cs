using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MyNetworkManager : MonoBehaviour {

    public GameObject PartFactory;
    public static PartFactory partFactory;

    void Awake()
    {
        partFactory = PartFactory.GetComponent<PartFactory>();
    }
    
}
