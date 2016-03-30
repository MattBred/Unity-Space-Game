using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MyNetworkPlayer : NetworkBehaviour {
    private static MyNetworkPlayer networkPlayer;

    void Awake()
    {
        networkPlayer = this;
    }

    void Start()
    {
        if (isLocalPlayer) connectionToServer.RegisterHandler(testMSG.msgId, test);
    }

    void Update()
    {

    }

    public static MyNetworkPlayer getInstance()
    {
        return networkPlayer;
    }


    void test(NetworkMessage msg)
    {
        Debug.Log("test");
        NetworkInstanceId id = msg.ReadMessage<testMSG>().spawnObjId;
        GUIController.getShipBuilderGUI().setDragPart(ClientScene.FindLocalObject(id).GetComponent<BasePart>());
    }

    public void spawnObject(GameObject fab)
    {
        if (isServer)
        {
            GameObject obj = (GameObject)Instantiate(fab);
            NetworkServer.Spawn(obj);
            GUIController.getShipBuilderGUI().setDragPart(obj.GetComponent<BasePart>());
        }
        else CmdSpawnPrefab(fab.GetComponent<NetworkIdentity>().assetId);
    }

    [Command]
    public void CmdSpawnPrefab(NetworkHash128 fabId)
    {
        foreach (GameObject fab in GameObject.Find("NetworkManager").GetComponent<NetworkManager>().spawnPrefabs)
        {
            if (fab.GetComponent<NetworkIdentity>().assetId.Equals(fabId))
            {
                GameObject obj = (GameObject)Instantiate(fab);
                NetworkServer.SpawnWithClientAuthority(obj,connectionToClient);
                testMSG newMSG = new testMSG();
                newMSG.spawnObjId = obj.GetComponent<NetworkIdentity>().netId;
                NetworkServer.SendToClient(connectionToClient.connectionId,testMSG.msgId, newMSG);
                break;
            }
        }
    }

    private class testMSG : MessageBase
    {
        public static short msgId = MsgType.Highest + 1;
        public NetworkInstanceId spawnObjId;
    }
}
