using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public bool lockY = true; // garde le texte vertical

    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 toCam = transform.position - Camera.main.transform.position;

        if (lockY)
            toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(toCam);
    }
}
