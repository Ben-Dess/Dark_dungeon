using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    [Header("Ambience")]
    public AudioClip ambienceClip;
    [Range(0f, 1f)] public float ambienceVolume = 0.6f;
    public bool loop = true;

    private AudioSource ambienceSource;

    const string PREF_MASTER_VOL = "MASTER_VOLUME";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSource();

        // Charger volume sauvegardé
        float saved = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 0.5f);
        SetMasterVolume(saved);
    }

    void SetupAudioSource()
    {
        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = loop;
        ambienceSource.spatialBlend = 0f; // 2D global
        ambienceSource.playOnAwake = false;
        ambienceSource.volume = ambienceVolume;

        if (ambienceClip != null)
            ambienceSource.Play();
    }

    public void ChangeAmbience(AudioClip newClip)
    {
        if (newClip == null || ambienceSource == null) return;

        ambienceSource.Stop();
        ambienceSource.clip = newClip;
        ambienceSource.Play();
    }

    // Volume global du jeu (toutes sources)
    public void SetMasterVolume(float v01)
    {
        v01 = Mathf.Clamp01(v01);
        AudioListener.volume = v01;          // master volume global Unity
        PlayerPrefs.SetFloat(PREF_MASTER_VOL, v01);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume()
    {
        return AudioListener.volume;
    }
}
