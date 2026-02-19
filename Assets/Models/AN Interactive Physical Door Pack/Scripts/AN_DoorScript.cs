using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class AN_DoorScript : NetworkBehaviour
{
    [Header("Door Rules")]
    public bool Locked = false;          // hard lock (admin)
    public bool CanOpen = true;
    public bool CanClose = true;

    [Header("Keys (optional)")]
    public bool RedLocked = false;
    public bool BlueLocked = false;

    [Header("State (Networked)")]
    public NetworkVariable<bool> Unlocked = new(false);  // <- IMPORTANT: persist for everyone
    public NetworkVariable<bool> IsOpen = new(false);

    [Header("Door Physics")]
    public Rigidbody rbDoor;
    public float openTorque = 20f;

    [Header("SFX (optional)")]
    public AudioClip doorOpenSfx;
    [Range(0f, 1f)] public float doorOpenVolume = 1f;

    private bool _lastOpenState;

    void Awake()
    {
        if (rbDoor == null) rbDoor = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        _lastOpenState = IsOpen.Value;

        IsOpen.OnValueChanged += OnOpenChanged;
        Unlocked.OnValueChanged += (_, __) => { /* you can add UI feedback */ };

        // Apply initial state to clients
        ApplyOpenState(IsOpen.Value, playSfx: false);
    }

    void OnDestroy()
    {
        IsOpen.OnValueChanged -= OnOpenChanged;
    }

    private void OnOpenChanged(bool previous, bool current)
    {
        ApplyOpenState(current, playSfx: true);
    }

    private void ApplyOpenState(bool open, bool playSfx)
    {
        // SFX when opening
        if (open && !_lastOpenState && playSfx && doorOpenSfx != null)
            SFXManager.Instance?.Play3D(doorOpenSfx, transform.position, doorOpenVolume);

        _lastOpenState = open;

        if (open && rbDoor != null)
        {
            rbDoor.AddRelativeTorque(new Vector3(0, 0, openTorque));
        }
        // For closing: you didn't have physics torque close, so we just set state.
        // If you want auto-close torque, tell me and I’ll add it.
    }

    // =========================
    // Public API (call these)
    // =========================

    public bool CanInteract()
    {
        if (Locked) return false;
        if (!Unlocked.Value) return false;        // <- Gate here
        if (RedLocked || BlueLocked) return false;
        return true;
    }

    public void RequestOpen() => SetOpenServerRpc(true);
    public void RequestClose() => SetOpenServerRpc(false);

    public void RequestToggle()
    {
        SetOpenServerRpc(!IsOpen.Value);
    }

    public void RequestUnlock()
    {
        UnlockServerRpc();
    }

    // =========================
    // Server authority
    // =========================

    [ServerRpc(RequireOwnership = false)]
    private void UnlockServerRpc(ServerRpcParams rpcParams = default)
    {
        if (Locked) return;
        if (RedLocked || BlueLocked) return;

        Unlocked.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetOpenServerRpc(bool open, ServerRpcParams rpcParams = default)
    {
        if (!CanInteract()) return;

        if (open)
        {
            if (!CanOpen) return;
            IsOpen.Value = true;
        }
        else
        {
            if (!CanClose) return;
            IsOpen.Value = false;
        }
    }
}
