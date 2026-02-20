using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuUI;    // Le menu principal (Resume, Settings, Quit)
    public GameObject settingsMenuUI; // Le panneau des réglages

    [Header("Configuration")]
    public Transform playerHead;
    public float spawnDistance = 1.2f;
    public InputActionProperty pauseAction;

    private bool isPaused = false;

    void Awake()
    {
        if (pauseAction.action != null)
            pauseAction.action.Enable();
    }

    void Update()
    {
        if (pauseAction.action.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Au premier appui, on affiche toujours le menu Pause principal
            pauseMenuUI.SetActive(true);
            settingsMenuUI.SetActive(false); // On s'assure que settings est caché
            PositionnerMenu(pauseMenuUI.transform);
        }
        else
        {
            // On ferme tout
            pauseMenuUI.SetActive(false);
            settingsMenuUI.SetActive(false);
        }
    }

    // --- FONCTIONS DE BASCULE ---

    public void OpenSettings()
    {
        // Cache la pause, affiche les réglages
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);

        // On repositionne le menu Settings face au joueur au cas où il aurait bougé
        PositionnerMenu(settingsMenuUI.transform);
    }

    public void CloseSettings()
    {
        // Cache les réglages, revient à la pause
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        // On repositionne la pause
        PositionnerMenu(pauseMenuUI.transform);
    }

    // --- POSITIONNEMENT ---

    void PositionnerMenu(Transform menuTransform)
    {
        if (playerHead == null) return;

        Vector3 headPos = playerHead.position;
        Vector3 headForward = playerHead.forward;
        headForward.y = 0;
        headForward.Normalize();

        menuTransform.position = headPos + (headForward * spawnDistance);

        Vector3 lookAtTarget = new Vector3(headPos.x, menuTransform.position.y, headPos.z);
        menuTransform.LookAt(lookAtTarget);
        menuTransform.Rotate(0, 180, 0);
    }

    public void QuitSession()
    {
        // Quitte l'application
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}