using Unity.Netcode;
using UnityEngine;

public class LocalOnlyXRRig : NetworkBehaviour
{
    public GameObject xrRigRoot; // XR Origin (XR Rig)

    public override void OnNetworkSpawn()
    {
        xrRigRoot.SetActive(IsOwner); // seul le owner garde XR actif
    }
}
