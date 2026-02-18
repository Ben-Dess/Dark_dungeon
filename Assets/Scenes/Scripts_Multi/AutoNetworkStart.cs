using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class AutoNetworkStart : MonoBehaviour
{
    public bool isHost = false;
    public string ip = "192.168.1.30"; // IP DU PC

    void Start()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (isHost)
        {
            transport.SetConnectionData("0.0.0.0", 7777);
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started as HOST");
        }
        else
        {
            transport.SetConnectionData(ip, 7777);
            NetworkManager.Singleton.StartClient();
            Debug.Log("Started as CLIENT");
        }
    }
}
