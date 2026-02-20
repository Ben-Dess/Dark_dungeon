using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    public void CloseGame()
    {
        // ÉTAPE CRUCIALE : On remet le temps à la normale.
        // Si on change de scène pendant une pause (Time.timeScale = 0),
        // la scène suivante sera freeze dès le départ.
        Time.timeScale = 1f;

        // On ferme le Jeu
        Application.Quit();
    }
}
