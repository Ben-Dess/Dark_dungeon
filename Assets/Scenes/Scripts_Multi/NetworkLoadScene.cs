using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkLoadScene : NetworkBehaviour
{
    public string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRIGGER] Enter by: {other.name}  tag={other.tag}  layer={LayerMask.LayerToName(other.gameObject.layer)}");

        // XR: souvent c'est le CharacterController ou la MainCamera qui rentre, pas "Player"
        bool isXrPlayer =
            other.CompareTag("Player") ||
            other.CompareTag("MainCamera") ||
            other.GetComponent<CharacterController>() != null;

        if (!isXrPlayer) return;

        // Cherche le PlayerObject réseau dans les parents
        var req = other.GetComponentInParent<SceneChangeRequest>();
        Debug.Log($"[TRIGGER] Found SceneChangeRequest? {(req != null)}  IsOwner={(req != null && req.IsOwner)}  IsClient={(req != null && req.IsClient)}");

        if (req != null)
        {
            // IMPORTANT: ne bloque pas avec IsOwner tant que tu débug (ça peut empêcher le host ou certains colliders)
            req.RequestSceneChangeServerRpc(sceneName);
            Debug.Log("[TRIGGER] Sent ServerRpc");
        }
    }
}
