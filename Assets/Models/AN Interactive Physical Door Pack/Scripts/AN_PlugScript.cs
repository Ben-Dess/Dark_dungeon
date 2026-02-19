using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class AN_PlugScript : NetworkBehaviour
{
    [Tooltip("Feature for one using only")]
    public bool OneTime = false;

    [Tooltip("SocketObject with collider (isTrigger = true)")]
    public Collider Socket;

    [Tooltip("Door controlled by the socket (NETWORK VERSION)")]
    public AN_DoorScript DoorObject;

    [Header("Optional local follow (VR/Hands)")]
    public Transform HeroHandsPosition; // only for local visuals if you keep follow

    private NetworkVariable<bool> Connected = new(false);

    Rigidbody rb;
    bool youCan = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        Connected.OnValueChanged += (_, __) => ApplySnap();
        ApplySnap();
    }

    void OnDestroy()
    {
        Connected.OnValueChanged -= (_, __) => ApplySnap();
    }

    void ApplySnap()
    {
        if (!Connected.Value) return;
        if (Socket == null) return;

        // Snap to socket on everyone
        transform.position = Socket.transform.position;
        transform.rotation = Socket.transform.rotation;

        // Freeze physics when connected
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only server decides connection (authoritative)
        if (!IsServer) return;
        if (!youCan) return;

        if (other == Socket && !Connected.Value)
        {
            Connected.Value = true;

            // Unlock door for everyone (persistent)
            if (DoorObject != null)
            {
                DoorObject.RequestUnlock();

                // Optionnel : ouvrir direct quand on branche
                // DoorObject.RequestOpen();
            }

            if (OneTime) youCan = false;
        }
    }

    // ---------- OPTIONAL ----------
    // If you want "take plug" in VR, don’t use E/Camera.main in multiplayer.
    // Call these from XR events on the LOCAL player and send ServerRpc to request ownership/move.
    // For now, simplest is: only server moves it or use NetworkTransform ownership.
}
