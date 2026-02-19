using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnIndexAssigner : NetworkBehaviour
{
    private int next = 0;
    private readonly Dictionary<ulong, int> indexByClient = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        OnClientConnected(NetworkManager.Singleton.LocalClientId); // host
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        if (indexByClient.ContainsKey(clientId)) return;

        indexByClient[clientId] = next++;

        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (player == null) return;

        var np = player.GetComponent<NetworkPlayer>();
        if (np == null) return;

        np.SpawnIndex.Value = indexByClient[clientId]; //sync vers owner
    }
}
