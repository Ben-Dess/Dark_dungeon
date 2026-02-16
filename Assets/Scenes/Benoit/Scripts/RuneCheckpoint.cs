using UnityEngine;

public class RuneCheckpoint : MonoBehaviour
{
    public int checkpointIndex;
    public RuneDrawingZone rune;

    [Header("Filtering")]
    public bool requireTipCollider = true;

    [Header("Draw")]
    [Tooltip("Si assigné, c’est ce point qui sera utilisé pour dessiner la ligne.")]
    public Transform drawPoint;

    private bool consumed = false;

    public Vector3 GetDrawPosition()
    {
        return drawPoint != null ? drawPoint.position : transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (rune == null) return;

        Scepter scepter = other.GetComponentInParent<Scepter>();
        if (scepter == null) return;

        if (requireTipCollider)
        {
            if (scepter.tip == null) return;
            Transform t = other.transform;
            if (t != scepter.tip && !t.IsChildOf(scepter.tip)) return;
        }

        consumed = true;
        rune.ValidateCheckpoint(checkpointIndex, this, scepter);
    }

    void OnTriggerExit(Collider other)
    {
        Scepter scepter = other.GetComponentInParent<Scepter>();
        if (scepter == null) return;

        if (requireTipCollider)
        {
            if (scepter.tip == null) return;
            Transform t = other.transform;
            if (t != scepter.tip && !t.IsChildOf(scepter.tip)) return;
        }

        consumed = false;
    }
}
