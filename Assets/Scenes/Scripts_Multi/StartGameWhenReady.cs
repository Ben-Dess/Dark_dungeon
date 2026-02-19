using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameWhenReady : MonoBehaviour
{
    public int expectedPlayers = 2;
    public string gameSceneName = "Salle_Cellules";
    bool launched;

    void Update()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (!nm.IsListening) return;
        if (!nm.IsServer) return;

        int connected = nm.ConnectedClientsList.Count;

        if (!launched && connected >= expectedPlayers)
        {
            launched = true;
            Debug.Log($"[FLOW] Loading network scene: {gameSceneName} (clients={connected})");

            nm.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    
}
