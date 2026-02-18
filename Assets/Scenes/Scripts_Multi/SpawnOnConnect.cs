using Unity.Netcode;
using UnityEngine;

public class SpawnOnConnect : MonoBehaviour
{
    public Transform[] spawnPoints;
    int next;

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (player == null) return;

        var sp = spawnPoints[next % spawnPoints.Length];
        next++;

        player.transform.SetPositionAndRotation(sp.position, sp.rotation);
    }
}