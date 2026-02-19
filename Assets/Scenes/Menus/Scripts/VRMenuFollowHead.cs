using UnityEngine;

public class VRMenuFollowHead : MonoBehaviour
{
    public Transform head;
    public float distance = 1.5f;

    public void ShowMenu()
    {
        transform.position = head.position + head.forward * distance;

        // Garde le menu droit (pas incliné)
        Vector3 lookDir = transform.position - head.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
