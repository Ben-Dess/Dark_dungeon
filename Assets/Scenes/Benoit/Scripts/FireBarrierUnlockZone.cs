using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireBarrierUnlockZone : MonoBehaviour
{
    public FireParticleSuction suction;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (suction == null) suction = GetComponentInParent<FireParticleSuction>();
    }

    void OnTriggerEnter(Collider other)
    {
        var scepter = other.GetComponentInParent<Scepter>();
        if (scepter == null) return;

        // Si tu veux verrouiller sur gem rouge
        if (scepter.equippedGem == null || scepter.equippedGem.type != GemType.Red) return;
        if (scepter.tip == null) return;

        suction.StartSuction(scepter.tip);
    }
}
