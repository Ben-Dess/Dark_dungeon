using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Tape le nom exact de la scène de destination")]
    public string sceneName;

    public void ChangeScene()
    {
        // ÉTAPE CRUCIALE : On remet le temps à la normale.
        // Si on change de scène pendant une pause (Time.timeScale = 0),
        // la scène suivante sera freeze dès le départ.
        Time.timeScale = 1f;

        // On charge la scène
        if (!string.IsNullOrEmpty(sceneName))
        {
            Application.Quit();
        }
        else
        {
            Debug.LogError("Nom de scène manquant sur l'objet " + gameObject.name);
        }
    }
}
