using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI et Caméra")]
    public GameObject pauseMenuUI; // Glisse 'MenuPause' ici
    public Transform playerHead;   // Glisse 'Main Camera' ici
    public float spawnDistance = 1.2f;
    public SceneManager SceneHostJoin;

    [Header("Paramètres de sortie")]
    public string sceneToLoad = "SelectMenu"; // Le nom de la scène par défaut

    [Header("Bouton Manette")]
    public InputActionProperty pauseAction; // Sélectionne 'Controller/Menu'

    private bool isPaused = false;

    void OnEnable() => pauseAction.action.Enable();

    void Awake()
    {
        // Très important : On s'assure que l'action est activée manuellement
        if (pauseAction.action != null)
            pauseAction.action.Enable();
    }

    void Update()
    {
        // Teste les deux méthodes de détection pour être sûr
        if (pauseAction.action.WasPressedThisFrame())
        {
            Debug.Log("Bouton Menu détecté !");
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Pause
            pauseMenuUI.SetActive(true);
            PositionnerMenu();
            print("Menu Ouvert");
        }
        else
        {
            Time.timeScale = 1f; // Reprise
            pauseMenuUI.SetActive(false);
            print("Menu Fermé");
        }
    }

    void PositionnerMenu()
    {
        if (playerHead == null) return;

        // 1. On récupère la position précise du casque
        Vector3 headPos = playerHead.position;

        // 2. On récupère la direction exacte où regarde le joueur
        Vector3 lookDirection = playerHead.forward;

        // OPTIONNEL : Si tu veux que le menu soit toujours horizontal (pas penché si tu regardes en l'air)
        // lookDirection.y = 0; 
        // lookDirection.Normalize();

        // 3. On place le menu à la distance définie (ex: 1.5m) pile devant
        pauseMenuUI.transform.position = headPos + (lookDirection * spawnDistance);

        // 4. On force le menu à regarder le joueur
        // Pour un Canvas, on veut qu'il soit perpendiculaire au regard
        pauseMenuUI.transform.LookAt(headPos);

        // 5. Correction de 180° (car le Canvas "regarde" vers l'arrière par défaut)
        pauseMenuUI.transform.Rotate(0, 180, 0);
    }

    public void QuitSession()
    {
        // 1. On réactive le temps d'abord
        Time.timeScale = 1f;
        isPaused = false;

        // 3. On charge la scène
        SceneManager.LoadScene(sceneToLoad);
    }
}