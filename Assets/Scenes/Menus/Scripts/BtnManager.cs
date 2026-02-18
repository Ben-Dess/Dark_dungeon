using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class BtnManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Références")]
    public TextMeshProUGUI textTarget;
    public string sceneToLoad;

    [Header("Couleurs")]
    private Color normalColor = new Color(0.784f, 0.624f, 0.624f);
    public Color hoverColor = new Color(0.5f, 0.3f, 0.3f);
    public Color pressedColor = new Color(0.2f, 0.1f, 0.1f);

    void Start()
    {
        if (textTarget != null)
            textTarget.color = normalColor;
    }

    // --- GESTION DU SON ---

    // Modifie le volume global du jeu (entre 0 et 1)
    public void SetGlobalVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // --- NAVIGATION SETTINGS ---

    public void OpenSettings(GameObject settingsToOpen)
    {
        if (settingsToOpen != null)
        {
            settingsToOpen.SetActive(true);
            if (textTarget != null) textTarget.color = pressedColor;
        }
    }

    // Cette fonction sert de bouton "Retour"
    public void CloseSettings(GameObject settingsToClose)
    {
        if (settingsToClose != null)
        {
            settingsToClose.SetActive(false);
        }
    }

    // --- Play Game ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textTarget != null) textTarget.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (textTarget != null) textTarget.color = normalColor;
    }

    public void ChangeScene()
    {
        if (textTarget != null) textTarget.color = pressedColor;
        if (!string.IsNullOrEmpty(sceneToLoad)) SceneManager.LoadScene(sceneToLoad);
    }

    // --- Exit Game ---

    public void QuitApp()
    {
        // Pour le jeu final (Build)
        Application.Quit();

        // Pour arrêter le mode Play dans l'éditeur Unity
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}