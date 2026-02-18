using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwitchSecret : MonoBehaviour
{
    [Header("Identification de la dalle")]
    public int id; // un numéro unique par dalle

    [Header("Rendu")]
    public Renderer targetRenderer; // assigne le MeshRenderer ici
    public Color neutralColor = Color.gray;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        _mpb = new MaterialPropertyBlock();

        // sécurité : collider en trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        SetColor(neutralColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        // adapte le tag selon ton projet
        if (!other.CompareTag("Player")) return;

        SecretPathManager mgr = SecretPathManager.Instance;
        if (mgr == null) return;

        var result = mgr.StepOnTile(this);

        if (result == SecretPathManager.StepResult.Correct)
            SetColor(correctColor);
        else if (result == SecretPathManager.StepResult.Wrong)
            SetColor(wrongColor);
        // si Ignored -> ne rien faire
    }

    public void ResetTile()
    {
        SetColor(neutralColor);
    }

    private void SetColor(Color c)
    {
        // MaterialPropertyBlock = pas besoin d'instancier des matériaux
        targetRenderer.GetPropertyBlock(_mpb);

        // Selon ton shader: souvent "_BaseColor" (URP/Lit) ou "_Color" (Standard)
        if (targetRenderer.sharedMaterial != null && targetRenderer.sharedMaterial.HasProperty("_BaseColor"))
            _mpb.SetColor("_BaseColor", c);
        else
            _mpb.SetColor("_Color", c);

        targetRenderer.SetPropertyBlock(_mpb);
    }
}
