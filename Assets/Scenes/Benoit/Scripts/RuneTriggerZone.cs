using UnityEngine;

public class RuneTriggerZone : MonoBehaviour
{
    public RuneStation station;         // ton script de snap/casting
    public RuneDrawingZone drawingZone; // la rune

    void OnTriggerStay(Collider other)
    {
        if (station != null && !station.IsCasting) return;

        // On cherche le Scepter via la pointe (tip collider) ou via parent
        var scepter = other.GetComponentInParent<Scepter>();
        if (scepter == null || scepter.tip == null) return;

    }

    void OnTriggerExit(Collider other)
    {
        // Optionnel : si tu veux reset quand on sort
        // drawingZone.ResetProgress();
    }
}
