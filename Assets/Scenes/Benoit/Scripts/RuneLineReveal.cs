using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RuneLineReveal : MonoBehaviour
{
    [Header("Bind")]
    public RuneDrawingZone drawingZone;

    [Header("Reveal")]
    [Range(0f, 1f)]
    public float reveal = 0f;              // 0 = invisible, 1 = full visible
    public float revealSmooth = 12f;       // vitesse de lissage

    [Header("Optional Scroll")]
    public bool scroll = true;
    public float scrollSpeed = 1.5f;

    private LineRenderer lr;
    private Material mat;
    private float targetReveal = 0f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        mat = lr.material; // instance
    }

    void Update()
    {
        if (drawingZone != null)
        {
            // Progress = currentIndex / total
            float total = Mathf.Max(1f, drawingZone.TotalCheckpoints);
            targetReveal = Mathf.Clamp01(drawingZone.CurrentIndex / total);
        }

        // smooth
        reveal = Mathf.Lerp(reveal, targetReveal, 1f - Mathf.Exp(-revealSmooth * Time.deltaTime));

        // Reveal: on scale X de 0..1
        Vector2 scale = mat.mainTextureScale;
        scale.x = Mathf.Max(0.001f, reveal);
        mat.mainTextureScale = scale;

        // Optionnel: scroll léger (sur Y pour éviter d’interférer avec le reveal en X)
        if (scroll)
        {
            Vector2 offset = mat.mainTextureOffset;
            offset.y += scrollSpeed * Time.deltaTime;
            mat.mainTextureOffset = offset;
        }
    }
}
