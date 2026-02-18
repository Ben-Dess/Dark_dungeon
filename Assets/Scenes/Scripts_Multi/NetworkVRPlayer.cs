using Unity.Netcode;
using UnityEngine;

public class NetworkVRPlayer : NetworkBehaviour
{
    public GameObject xrRoot;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            xrRoot.SetActive(false);
        }
    }
}
