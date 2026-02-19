using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Global SFX settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    AudioSource _oneShot2D;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _oneShot2D = gameObject.AddComponent<AudioSource>();
        _oneShot2D.playOnAwake = false;
        _oneShot2D.loop = false;
        _oneShot2D.spatialBlend = 0f; // 2D
        _oneShot2D.volume = sfxVolume;
    }

    public void Play2D(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        _oneShot2D.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
    }

    public void Play3D(AudioClip clip, Vector3 position, float volumeScale = 1f, float minDistance = 1f, float maxDistance = 15f)
    {
        if (clip == null) return;

        var go = new GameObject("SFX_3D_" + clip.name);
        go.transform.position = position;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f;
        src.volume = Mathf.Clamp01(sfxVolume * volumeScale);
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.playOnAwake = false;

        src.Play();
        Destroy(go, clip.length + 0.2f);
    }
}
