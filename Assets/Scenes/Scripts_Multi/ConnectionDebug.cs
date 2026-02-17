using Unity.Netcode;
using UnityEngine;

public class ConnectionDebug : MonoBehaviour
{
    void Start()
    {
        var nm = NetworkManager.Singleton;

        nm.OnServerStarted += () =>
            Debug.Log("[NET] Server started (Host OK)");

        nm.OnClientConnectedCallback += (clientId) =>
            Debug.Log($"[NET] Client connected: {clientId} | IsServer={nm.IsServer} IsClient={nm.IsClient}");

        nm.OnClientDisconnectCallback += (clientId) =>
            Debug.Log($"[NET] Client disconnected: {clientId}");

        nm.OnTransportFailure += () =>
            Debug.LogError("[NET] Transport failure");
    }

    void OnGUI()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 600, 200));
        GUILayout.Label($"IsHost: {nm.IsHost}  IsServer: {nm.IsServer}  IsClient: {nm.IsClient}");
        GUILayout.Label($"LocalClientId: {nm.LocalClientId}");
        if (nm.IsServer) GUILayout.Label($"ConnectedClients: {nm.ConnectedClientsList.Count}");
        GUILayout.EndArea();
    }
}

