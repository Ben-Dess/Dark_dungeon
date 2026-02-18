using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GrabOwnership : NetworkBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    public override void OnNetworkSpawn()
    {
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        if (grab == null) return;
        grab.selectEntered.RemoveListener(OnSelectEntered);
        grab.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Le client qui grab demande au serveur l'ownership
        if (IsClient)
            RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        // Optionnel : rendre l'ownership au serveur � la release
        if (IsClient)
            ReturnOwnershipToServerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        if (NetworkObject.OwnerClientId == clientId) return;
        NetworkObject.ChangeOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReturnOwnershipToServerServerRpc()
    {
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
    }
}
