using UnityEngine;

public class FireBarrier : MonoBehaviour
{
    [Header("Blocking")]
    public Collider blockingCollider;

    [Header("Particles")]
    public ParticleSystem[] particleSystems;

    [Header("Visual")]
    public float lerpSpeed = 8f;
    public float closedEmissionMult = 1f;
    public float openEmissionMult = 0f;

    [Header("State")]
    public bool openedOnce = false;   

    private float targetOpen01 = 0f;
    private float currentOpen01 = 0f;
    private float[] baseRates;

    void Awake()
    {
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider>();

        if (particleSystems == null || particleSystems.Length == 0)
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        baseRates = new float[particleSystems.Length];
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            if (!ps) continue;
            var em = ps.emission;
            baseRates[i] = em.rateOverTimeMultiplier;
        }

        SetOpenInstant(false);
    }

    void Update()
    {
        if (Mathf.Approximately(currentOpen01, targetOpen01)) return;

        currentOpen01 = Mathf.Lerp(currentOpen01, targetOpen01, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));
        float mult = Mathf.Lerp(closedEmissionMult, openEmissionMult, currentOpen01);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            if (!ps) continue;
            var em = ps.emission;
            em.rateOverTimeMultiplier = baseRates[i] * mult;
        }
    }

    public void OpenPermanently()
    {
        if (openedOnce) return;
        openedOnce = true;

        // Ouvre et ne se referme plus
        targetOpen01 = 1f;

        if (blockingCollider != null)
            blockingCollider.enabled = false;
    }

    public void SetOpenInstant(bool open)
    {
        targetOpen01 = open ? 1f : 0f;
        currentOpen01 = targetOpen01;

        if (blockingCollider != null)
            blockingCollider.enabled = !open;

        float mult = open ? openEmissionMult : closedEmissionMult;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            if (!ps) continue;
            var em = ps.emission;
            em.rateOverTimeMultiplier = baseRates[i] * mult;
        }
    }
}
