using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameWhenReady : MonoBehaviour
{
    public int expectedPlayers = 2;
    public string gameSceneName = "Salle2_Clean";
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

    void OnGUI()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 700, 220));
        GUILayout.Label($"Listening: {nm.IsListening}  Host:{nm.IsHost}  Server:{nm.IsServer}  Client:{nm.IsClient}");
        if (nm.IsListening) GUILayout.Label($"Connected: {nm.ConnectedClientsList.Count}/{expectedPlayers}");
        GUILayout.Label($"SceneManager null? {(nm.SceneManager == null)}");
        GUILayout.EndArea();
    }
}
