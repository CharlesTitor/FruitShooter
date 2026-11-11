using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _characters = new List<GameObject>();


    public void Spawn(int spawnID/*,NetworkConnection conn*/)
    {
        try {
        GameObject player = Instantiate(_characters[spawnID]);
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.Spawn();
        }catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }


}
