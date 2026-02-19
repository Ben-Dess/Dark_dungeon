using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Network avatar")]
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Local rig refs (scene rig)")]
    public Renderer[] meshToDisable;

    //index de spawn décidé par le serveur
    public NetworkVariable<int> SpawnIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            foreach (var r in meshToDisable)
                if (r) r.enabled = false;

            // quand on reçoit l'index, on place le rig local
            SpawnIndex.OnValueChanged += OnSpawnIndexChanged;
            if (SpawnIndex.Value >= 0) ApplySpawnToLocalRig(SpawnIndex.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            SpawnIndex.OnValueChanged -= OnSpawnIndexChanged;
    }

    private void OnSpawnIndexChanged(int oldValue, int newValue)
    {
        if (IsOwner && newValue >= 0)
            ApplySpawnToLocalRig(newValue);
    }

    private void ApplySpawnToLocalRig(int idx)
    {
        // Le plus simple : un tableau de spawn points accessible partout.
        var sp = SpawnPointsRegistry.Get(idx);
        var rigRoot = VRRigReferences.Singleton.root; // XR Origin root

        // Téléport du rig local
        rigRoot.SetPositionAndRotation(sp.position, sp.rotation);
    }

    void Update()
    {
        if (!IsOwner) return;

        // copie du rig local -> network avatar
        root.position = VRRigReferences.Singleton.root.position;
        root.rotation = VRRigReferences.Singleton.root.rotation;

        head.position = VRRigReferences.Singleton.head.position;
        head.rotation = VRRigReferences.Singleton.head.rotation; // <- plutôt head.rotation

        leftHand.position = VRRigReferences.Singleton.leftHand.position;
        leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

        rightHand.position = VRRigReferences.Singleton.rightHand.position;
        rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
    }
}
