using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RuneLineAnimator : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollSpeed = 1.5f;

    [Header("Tiling")]
    public float tilePerUnit = 1.0f;

    [Header("Pulse (optionnel)")]
    public bool pulseWidth = false;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.02f;

    [Header("Width Safety")]
    [Tooltip("Largeur minimale (évite de passer en dessous de 0).")]
    public float minWidth = 0.001f;

    [Tooltip("Largeur maximale (optionnel). Mets 0 pour ignorer.")]
    public float maxWidth = 0f;

    private LineRenderer line;
    private Material mat;
    private float baseStartWidth;
    private float baseEndWidth;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        mat = line.material; // instance

        baseStartWidth = line.startWidth;
        baseEndWidth = line.endWidth;
    }

    void Update()
    {
        AnimateScroll();
        UpdateTiling();
        AnimatePulse();
    }

    void AnimateScroll()
    {
        if (mat == null) return;
        Vector2 offset = mat.mainTextureOffset;
        offset.x += scrollSpeed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }

    void UpdateTiling()
    {
        if (mat == null) return;
        if (line.positionCount < 2) return;

        float length = 0f;
        for (int i = 1; i < line.positionCount; i++)
            length += Vector3.Distance(line.GetPosition(i - 1), line.GetPosition(i));

        Vector2 scale = mat.mainTextureScale;
        scale.x = Mathf.Max(0.01f, length * tilePerUnit);
        mat.mainTextureScale = scale;
    }

    void AnimatePulse()
    {
        if (!pulseWidth) return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        float wStart = baseStartWidth + pulse;
        float wEnd = baseEndWidth + pulse;

        // clamp min
        wStart = Mathf.Max(minWidth, wStart);
        wEnd = Mathf.Max(minWidth, wEnd);

        // clamp max (si > 0)
        if (maxWidth > 0f)
        {
            wStart = Mathf.Min(maxWidth, wStart);
            wEnd = Mathf.Min(maxWidth, wEnd);
        }

        line.startWidth = wStart;
        line.endWidth = wEnd;
    }
}
