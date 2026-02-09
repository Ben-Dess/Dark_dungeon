using UnityEngine;

public class Scepter : MonoBehaviour
{
    public Transform tip;               // la pointe (on la fera après)
    public Transform gemAttachPoint;     // ton GemAttachPoint
    public Gem equippedGem;

    public void Equip(Gem gem)
    {
        equippedGem = gem;

        // snap propre (au cas où)
        gem.transform.SetParent(gemAttachPoint);
        gem.transform.localPosition = Vector3.zero;
        gem.transform.localRotation = Quaternion.identity;

        // pour éviter qu’elle tombe/vibre une fois attachée
        var rb = gem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    public void Unequip(Gem gem)
    {
        if (equippedGem == gem) equippedGem = null;

        var rb = gem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        gem.transform.SetParent(null);
    }
}
