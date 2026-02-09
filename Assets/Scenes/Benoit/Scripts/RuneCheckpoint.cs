using UnityEngine;

public class RuneCheckpoint : MonoBehaviour
{
    public int checkpointIndex;
    public RuneDrawingZone rune;

    void OnTriggerEnter(Collider other)
    {
        Scepter scepter = other.GetComponentInParent<Scepter>();
        if (scepter != null)
        {
            rune.ValidateCheckpoint(checkpointIndex);
        }
    }
}
