using UnityEngine;

public class RuneStation : MonoBehaviour
{
    [Header("Station")]
    public Transform anchor;

    [Header("Rig root to move (IMPORTANT)")]
    public Transform xrRigRoot; // généralement XR Origin (VR) ou ton Player parent

    [Header("Optional: disable locomotion components while casting")]
    public Behaviour[] locomotionToDisable;

    public bool IsCasting { get; private set; }

    public void ToggleCasting()
    {
        if (IsCasting) StopCasting();
        else StartCasting();
    }

    public void StartCasting()
    {
        IsCasting = true;

        // Snap position + rotation
        xrRigRoot.position = anchor.position;
        xrRigRoot.rotation = anchor.rotation;

        // Disable locomotion (if any)
        if (locomotionToDisable != null)
        {
            foreach (var b in locomotionToDisable)
                if (b) b.enabled = false;
        }
    }

    public void StopCasting()
    {
        IsCasting = false;

        // Enable locomotion back
        if (locomotionToDisable != null)
        {
            foreach (var b in locomotionToDisable)
                if (b) b.enabled = true;
        }
    }
}
