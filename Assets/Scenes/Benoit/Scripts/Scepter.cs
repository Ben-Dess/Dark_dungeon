using UnityEngine;

public class Scepter : MonoBehaviour
{
    public Transform tip;
    public Transform gemAttachPoint;
    public Gem equippedGem;

    public void Equip(Gem gem)
    {
        // On garde juste la référence, le Socket gère le parenting
        if (equippedGem != null && equippedGem != gem)
        {
            Debug.Log($"[Scepter] Switching from {equippedGem.name} to {gem.name}");
        }

        equippedGem = gem;
        Debug.Log($"[Scepter] Equipped {gem.name}");
    }

    public void Unequip(Gem gem)
    {
        if (equippedGem == gem)
        {
            equippedGem = null;
            Debug.Log($"[Scepter] Unequipped {gem.name}");
        }
    }
}