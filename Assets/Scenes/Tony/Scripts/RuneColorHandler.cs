using UnityEngine;

public class RuneColorHandler : MonoBehaviour
{
    private Material materialOriginal;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) materialOriginal = meshRenderer.material;
    }

    public void ChangerCouleur(Material nouveauMat)
    {
        if (meshRenderer != null) meshRenderer.material = nouveauMat;
    }

    public void ResetCouleur()
    {
        if (meshRenderer != null) meshRenderer.material = materialOriginal;
    }
}