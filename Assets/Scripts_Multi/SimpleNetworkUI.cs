using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class SimpleNetworkUI : MonoBehaviour
{
    public string ip = "192.168.1.30";
    public ushort port = 7777;

    public void Host()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", port);
        NetworkManager.Singleton.StartHost();
    }

    public void Join()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);
        NetworkManager.Singleton.StartClient();
    }
}
