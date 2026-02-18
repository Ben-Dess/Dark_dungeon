using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PotionBottle : MonoBehaviour
{
    [Header("Potion")]
    public string potionName = "Potion";
    public string colorName;
    public Color liquidColor = Color.red;

    [Header("Shelf (auto)")]
    public int shelfID;

    void Awake()
    {
        var shelf = GetComponentInParent<Shelf>();
        if (shelf != null)
            shelfID = shelf.shelfID;
    }

    public string GetDisplayName()
    {
        return $"{shelfID} - {potionName}";
    }
}
