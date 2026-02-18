using UnityEngine;
using Unity.Netcode;


public class GrabDebug : NetworkBehaviour
{
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Awake() => grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

    public override void OnNetworkSpawn()
    {
        grab.selectEntered.AddListener(_ =>
        {
            Debug.Log($"[GRAB] IsOwner={IsOwner} OwnerClientId={OwnerClientId} LocalClientId={NetworkManager.Singleton.LocalClientId}");
        });
    }
}
