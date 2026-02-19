using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeRequest : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void RequestSceneChangeServerRpc(string sceneName)
    {
        Debug.Log($"[SERVER RPC] RequestSceneChangeServerRpc received. IsServer={IsServer} scene={sceneName}");

        if (!IsServer) return;

        // Sécurité: scène bien dans Build Settings
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SERVER] Scene '{sceneName}' not in Build Settings or name mismatch.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        Debug.Log("[SERVER] LoadScene called");
    }
}
