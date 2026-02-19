using UnityEngine;

public class ScepterSocket : MonoBehaviour
{
    [SerializeField] private Scepter scepter;
    
    private void Awake()
    {
        if (!scepter)
            scepter = GetComponentInParent<Scepter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (scepter == null || scepter.HasGem) return;

        Gem gem = other.GetComponent<Gem>();
        if (!gem) return;

        scepter.EquipGem(gem);
    }
}
