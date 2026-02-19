using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwitchSecret : MonoBehaviour
{
    [Header("Identification de la dalle")]
    public int id; // un numéro unique par dalle

    [Header("Rendu")]
    public Renderer targetRenderer;
    public Color neutralColor = Color.gray;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    MaterialPropertyBlock _mpb;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        _mpb = new MaterialPropertyBlock();

        var col = GetComponent<Collider>();
        col.isTrigger = true;

        SetColor(neutralColor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var mgr = SecretPathManager.Instance;
        if (mgr == null) return;

        //  on envoie juste l'id
        mgr.StepOnTileLocal(this);
    }

    public void ResetTile() => SetColor(neutralColor);
    public void SetCorrect() => SetColor(correctColor);
    public void SetWrong() => SetColor(wrongColor);

    void SetColor(Color c)
    {
        if (targetRenderer == null) return;

        targetRenderer.GetPropertyBlock(_mpb);

        if (targetRenderer.sharedMaterial != null && targetRenderer.sharedMaterial.HasProperty("_BaseColor"))
            _mpb.SetColor("_BaseColor", c);
        else
            _mpb.SetColor("_Color", c);

        targetRenderer.SetPropertyBlock(_mpb);
    }
}
