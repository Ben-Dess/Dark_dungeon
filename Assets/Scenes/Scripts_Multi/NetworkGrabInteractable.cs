using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// VR grab sync (MVP) for XR Interaction Toolkit + Netcode for GameObjects.
/// - On grab: request ownership from server, lock object (isHeld), disable grab for others.
/// - While held: owner drives transform locally (XRGrabInteractable), NetworkTransform replicates.
/// - On release: unlock, restore physics, optionally return ownership to server.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
public class NetworkGrabInteractable : NetworkBehaviour
{
    [Header("Behavior")]
    [Tooltip("If true, when released, ownership is returned to server/host (recommended).")]
    public bool returnOwnershipToServerOnRelease = true;

    [Tooltip("If true, sets Rigidbody.isKinematic=true while held for stability.")]
    public bool kinematicWhileHeld = true;

    [Tooltip("If true, disables XRGrabInteractable for non-owners while held.")]
    public bool lockGrabForOthers = true;

    [Header("Optional Debug")]
    public bool debugLogs = false;

    // Network state
    private NetworkVariable<bool> isHeld = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private NetworkObject netObj;

    // Local-only tracking to avoid double-handling
    private bool localSubscribed;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        netObj = GetComponent<NetworkObject>();

        // XRGrabInteractable generally expects a Rigidbody for physics grabs
        // but it's not strictly required. We'll handle rb == null safely.
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe once per instance
        if (!localSubscribed)
        {
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
            localSubscribed = true;
        }

        // Apply initial lock state (important for late joiners)
        ApplyGrabLockState(isHeld.Value);

        isHeld.OnValueChanged += OnHeldChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (localSubscribed)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
            localSubscribed = false;
        }

        isHeld.OnValueChanged -= OnHeldChanged;
    }

    private void OnHeldChanged(bool previousValue, bool newValue)
    {
        ApplyGrabLockState(newValue);
    }

    private void ApplyGrabLockState(bool held)
    {
        // If locked, only the owner can grab while held
        if (lockGrabForOthers)
        {
            // Owner can keep interacting, others can't
            // If object is not held, everyone can interact
            grab.enabled = !held || IsOwner;
        }

        // Physics: kinematic while held is best controlled by the owner,
        // but we can also force it for everyone for consistent behavior.
        if (rb != null && kinematicWhileHeld)
        {
            // While held: kinematic on everyone reduces physics fights.
            // On release: kinematic off (but owner will be the one truly driving).
            rb.isKinematic = held;
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Only proceed for local player interactions:
        // This callback runs locally in each instance, but only the local interactor
        // will trigger it for the local grab.
        if (!IsClient) return;

        // If already held by someone else, cancel locally (server will also reject)
        if (isHeld.Value)
        {
            if (debugLogs) Debug.Log($"[NETGRAB] Tried to grab but already held: {name}");
            // Optional: forcibly drop selection (rarely needed)
            // grab.interactionManager.SelectExit(args.interactorObject, grab);
            return;
        }

        // Request to grab on server (sets isHeld + ownership)
        RequestGrabServerRpc(NetworkManager.Singleton.LocalClientId);

        if (debugLogs) Debug.Log($"[NETGRAB] RequestGrab sent by client {NetworkManager.Singleton.LocalClientId} for {name}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!IsClient) return;

        // Only the owner should release in network terms
        if (!IsOwner)
        {
            if (debugLogs) Debug.Log($"[NETGRAB] Non-owner tried to release: {name}");
            return;
        }

        ReleaseServerRpc();

        if (debugLogs) Debug.Log($"[NETGRAB] Release sent by owner {OwnerClientId} for {name}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestGrabServerRpc(ulong requestingClientId)
    {
        // Server-side validation
        if (isHeld.Value)
        {
            if (debugLogs) Debug.Log($"[NETGRAB][Server] Reject grab (already held): {name}");
            return;
        }

        // Mark held first (locks others immediately)
        isHeld.Value = true;

        // Transfer ownership to the grabber so their local XR can drive the transform
        // (with Owner-authoritative NetworkTransform / ClientNetworkTransform)
        netObj.ChangeOwnership(requestingClientId);

        if (debugLogs) Debug.Log($"[NETGRAB][Server] Ownership -> {requestingClientId}, held=true : {name}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseServerRpc()
    {
        // Unlock
        isHeld.Value = false;

        // Optionally return ownership to server to avoid "stale owner"
        if (returnOwnershipToServerOnRelease)
        {
            // ServerClientId is the host/server
            netObj.ChangeOwnership(NetworkManager.ServerClientId);
            if (debugLogs) Debug.Log($"[NETGRAB][Server] Ownership -> Server, held=false : {name}");
        }
        else
        {
            if (debugLogs) Debug.Log($"[NETGRAB][Server] held=false (ownership kept) : {name}");
        }
    }
}
