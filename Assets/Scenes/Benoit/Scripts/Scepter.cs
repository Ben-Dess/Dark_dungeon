using UnityEngine;

public class Scepter : MonoBehaviour
{
    [Header("Gem Visuals")]
    [SerializeField] private GameObject redGemVisual;
    public Transform tip;
    public Gem equippedGem;

    public bool HasGem => equippedGem != null;

    public void EquipGem(Gem gem)
    {
        if (HasGem) return;

        equippedGem = gem;

        ActivateGemVisual(gem.Type);
        gem.GetComponent<Renderer>().enabled = false;
    }

    private void ActivateGemVisual(GemType type)
    {
        switch (type)
        {
            case GemType.Red:
                if (redGemVisual)
                    redGemVisual.SetActive(true);
                break;
        }
    }
}
