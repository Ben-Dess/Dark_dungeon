using UnityEngine;
using UnityEngine.UI;

public class SettingsVolumeUI : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;

    void Awake()
    {
        if (volumeSlider != null)
            volumeSlider.minValue = 0f;

        if (volumeSlider != null)
            volumeSlider.maxValue = 1f;
    }

    void Start()
    {
        if (volumeSlider == null) return;

        float current = (GlobalAudioManager.Instance != null)
            ? GlobalAudioManager.Instance.GetMasterVolume()
            : AudioListener.volume;

        // IMPORTANT : forcer la valeur après que Unity ait initialisé le slider
        volumeSlider.value = current;

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float v)
    {
        if (GlobalAudioManager.Instance != null)
            GlobalAudioManager.Instance.SetMasterVolume(v);
        else
            AudioListener.volume = Mathf.Clamp01(v);
    }
}
