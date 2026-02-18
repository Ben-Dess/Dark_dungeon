using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class RuneDrawingZone : MonoBehaviour
{
    [Header("Rule")]
    public bool resetOnWrongCheckpoint = true;
    public bool allowPreviousCheckpoint = true;
    public float minTimeBetweenValidations = 0.05f;

    [Header("Scepter Requirement")]
    [Tooltip("Si vrai, seuls les checkpoints touchés par ce sceptre précis comptent.")]
    public bool requireSpecificScepter = true;

    [Tooltip("Le sceptre autorisé pour cette rune (drag & drop dans l'inspector).")]
    public Scepter requiredScepter;

    [Header("Line")]
    public bool clearLineOnReset = true;
    public bool addFirstPointOnStart = false;
    public float zOffset = 0f;

    [Header("Corner Smoothing")]
    [Tooltip("Rayon de l'arrondi aux angles (en unités monde).")]
    public float cornerRadius = 0.05f;

    [Tooltip("Nombre de segments ajoutés pour arrondir un angle.")]
    [Range(0, 16)]
    public int cornerSegments = 6;

    [Header("State (read only)")]
    public bool Activated { get; private set; }

    [SerializeField] private int currentIndex = 0;
    [SerializeField] private int totalCheckpoints = 0;

    private float lastValidationTime = -999f;

    private RuneCheckpoint[] checkpoints;
    private LineRenderer line;
    private int linePoints = 0;

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent<int, int> onProgress;
    public UnityEvent onReset;

    public int CurrentIndex => currentIndex;
    public int TotalCheckpoints => totalCheckpoints;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        checkpoints = GetComponentsInChildren<RuneCheckpoint>(true);

        if (totalCheckpoints <= 0)
            totalCheckpoints = checkpoints.Length;

        foreach (var cp in checkpoints)
            if (cp.rune == null) cp.rune = this;

        ResetLine();

        if (addFirstPointOnStart && totalCheckpoints > 0)
        {
            foreach (var cp in checkpoints)
            {
                if (cp.checkpointIndex == 0)
                {
                    AddLinePointSmooth(cp.GetDrawPosition());
                    break;
                }
            }
        }
    }

    private bool HasCorrectScepter(Scepter touchingScepter)
    {
        if (!requireSpecificScepter) return true;

        if (requiredScepter == null)
        {
            Debug.LogWarning($"[{name}] requiredScepter n'est pas assigné (RuneDrawingZone).");
            return false;
        }

        return touchingScepter == requiredScepter;
    }

    public void ValidateCheckpoint(int index, RuneCheckpoint cp, Scepter touchingScepter)
    {
        if (!HasCorrectScepter(touchingScepter)) return;
        if (Activated) return;
        if (Time.time - lastValidationTime < minTimeBetweenValidations) return;

        if (allowPreviousCheckpoint && index == currentIndex - 1)
            return;

        if (index == currentIndex)
        {
            lastValidationTime = Time.time;

            AddLinePointSmooth(cp.GetDrawPosition());

            currentIndex++;
            onProgress?.Invoke(currentIndex, totalCheckpoints);

            if (totalCheckpoints > 0 && currentIndex >= totalCheckpoints)
            {
                Activated = true;
                onActivated?.Invoke();
                Debug.Log($"Rune Activated: {name}");
            }
        }
        else
        {
            if (resetOnWrongCheckpoint)
                ResetProgress();
        }
    }

    private Vector3 ApplyOffset(Vector3 worldPos)
    {
        if (zOffset != 0f)
            worldPos += transform.forward * zOffset;

        return worldPos;
    }
    private void AddLinePointSmooth(Vector3 worldPos)
    {
        worldPos = ApplyOffset(worldPos);

        // Ajout brut si pas de smoothing
        if (cornerSegments <= 0 || cornerRadius <= 0f)
        {
            AddLinePointRaw(worldPos);
            return;
        }

        int count = line.positionCount;
        if (count < 2)
        {
            AddLinePointRaw(worldPos);
            return;
        }

        Vector3 p0 = line.GetPosition(count - 2);
        Vector3 p1 = line.GetPosition(count - 1);
        Vector3 p2 = worldPos;

        Vector3 a = (p0 - p1);
        Vector3 b = (p2 - p1);

        if (a.sqrMagnitude < 1e-8f || b.sqrMagnitude < 1e-8f)
        {
            AddLinePointRaw(worldPos);
            return;
        }

        Vector3 v1 = a.normalized; // depuis p1 vers p0
        Vector3 v2 = b.normalized; // depuis p1 vers p2

        // Angle entre les directions (on veut un coin, pas une ligne)
        float dot = Mathf.Clamp(Vector3.Dot(-v1, v2), -1f, 1f);
        float angle = Mathf.Acos(dot); // 0..pi

        // Presque tout droit => pas d'arrondi utile
        if (angle < 0.001f || Mathf.Abs(Mathf.PI - angle) < 0.001f)
        {
            AddLinePointRaw(worldPos);
            return;
        }

        // Longueur de tangente = r / tan(angle/2)
        float r = cornerRadius;
        float tanLen = r / Mathf.Tan(angle * 0.5f);

        // Clamp pour éviter de dépasser les segments
        float maxLen = Mathf.Min(a.magnitude, b.magnitude) * 0.5f;
        tanLen = Mathf.Min(tanLen, maxLen);
        if (tanLen <= 1e-5f)
        {
            AddLinePointRaw(worldPos);
            return;
        }

        // Points de tangence (on "coupe" le coin)
        Vector3 start = p1 + v1 * tanLen; // vers p0
        Vector3 end = p1 + v2 * tanLen; // vers p2

        // Centre de l'arc : sur la bissectrice
        Vector3 bis = (v1 + v2).normalized;
        float sinHalf = Mathf.Sin(angle * 0.5f);
        if (sinHalf < 1e-5f)
        {
            AddLinePointRaw(worldPos);
            return;
        }
        float centerDist = r / sinHalf;
        Vector3 center = p1 + bis * centerDist;

        // Normale du plan (à adapter si tu dessines sur un autre plan)
        Vector3 n = transform.forward;
        // Si jamais ton plan n'est pas celui-là : n = Vector3.Cross(v2, v1).normalized; (mais attention aux flips)

        // On remplace le dernier point par le début d'arc
        line.SetPosition(count - 1, start);

        Vector3 from = (start - center);
        Vector3 to = (end - center);

        float total = Vector3.SignedAngle(from, to, n);

        // Insère l'arc
        for (int i = 1; i <= cornerSegments; i++)
        {
            float t = i / (float)(cornerSegments + 1);
            float ang = total * t;
            Vector3 pt = center + Quaternion.AngleAxis(ang, n) * from;
            AddLinePointRaw(pt);
        }

        // Puis le nouveau point (p2)
        AddLinePointRaw(p2);
    }


    private void AddLinePointRaw(Vector3 worldPos)
    {
        linePoints++;
        line.positionCount = linePoints;
        line.SetPosition(linePoints - 1, worldPos);
    }

    private void ResetLine()
    {
        linePoints = 0;
        line.positionCount = 0;
    }

    public void ResetProgress()
    {
        currentIndex = 0;
        lastValidationTime = -999f;

        if (clearLineOnReset)
            ResetLine();

        onReset?.Invoke();
    }
}
