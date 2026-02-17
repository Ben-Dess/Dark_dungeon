using UnityEngine;

public class FireParticleSuction : MonoBehaviour
{
    [Header("References")]
    public FireBarrier barrier;                 // ton script FireBarrier (celui qui ouvre)
    public ParticleSystem[] fireSystems;         // tes Fire_PS (auto si vide)

    [Tooltip("La cible d'aspiration (tip du sceptre).")]
    public Transform suctionTarget;

    [Header("Suction")]
    public float suctionForce = 6f;             // force de base
    public float suctionForceRamp = 10f;        // accélération au fil du temps
    public float swirl = 2f;                    // petit tourbillon mystique
    public float killDistance = 0.12f;          // distance pour "consommer" la particule
    public float duration = 1.2f;               // durée de l'aspiration

    [Header("One shot")]
    public bool oneShot = true;

    private bool isSucking = false;
    private float t = 0f;

    private ParticleSystem.Particle[] buffer = new ParticleSystem.Particle[4096];

    void Awake()
    {
        if (barrier == null) barrier = GetComponent<FireBarrier>();
        if (fireSystems == null || fireSystems.Length == 0)
            fireSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void StartSuction(Transform target)
    {
        if (oneShot && (barrier != null && barrier.openedOnce)) return;

        suctionTarget = target;
        isSucking = true;
        t = 0f;

        // Optionnel : on coupe l'émission progressivement via FireBarrier (si tu l'utilises)
        // sinon tu peux aussi juste couper direct à la fin.
    }

    void Update()
    {
        if (!isSucking) return;
        if (suctionTarget == null) return;

        t += Time.deltaTime;
        float normalized = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));

        float forceNow = suctionForce + suctionForceRamp * normalized;

        bool anyAlive = false;

        for (int s = 0; s < fireSystems.Length; s++)
        {
            var ps = fireSystems[s];
            if (!ps) continue;

            int alive = ps.GetParticles(buffer);
            if (alive > 0) anyAlive = true;

            for (int i = 0; i < alive; i++)
            {
                Vector3 p = buffer[i].position;
                Vector3 toTarget = (suctionTarget.position - p);
                float dist = toTarget.magnitude;

                if (dist < 0.0001f) dist = 0.0001f;

                // direction principale vers la cible
                Vector3 dir = toTarget / dist;

                // swirl (tourbillon autour de dir)
                Vector3 swirlAxis = Vector3.up;
                Vector3 side = Vector3.Cross(dir, swirlAxis).normalized;
                Vector3 swirlVec = side * Mathf.Sin((Time.time + i) * 10f) * swirl;

                // vitesse vers la cible + swirl
                Vector3 vel = dir * forceNow + swirlVec;

                buffer[i].velocity = Vector3.Lerp(buffer[i].velocity, vel, 0.65f);

                // "consume" quand proche
                if (dist <= killDistance)
                {
                    buffer[i].remainingLifetime = 0f;
                }
            }

            ps.SetParticles(buffer, alive);
        }

        // Fin : si temps écoulé, on vide et on ouvre
        if (t >= duration)
        {
            // stop émission + clear
            for (int s = 0; s < fireSystems.Length; s++)
            {
                var ps = fireSystems[s];
                if (!ps) continue;

                var em = ps.emission;
                em.enabled = false;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            isSucking = false;

            if (barrier != null)
                barrier.OpenPermanently();
        }
    }
}
