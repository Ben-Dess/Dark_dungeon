using UnityEngine;
using Unity.XR.CoreUtils; // XROrigin

public class MenuSpawnFacing : MonoBehaviour
{
    [Header("References")]
    public XROrigin xrOrigin;        // ton XR Origin
    public Transform xrCamera;       // Main Camera (dans le rig)
    public Transform menuSpawnPoint; // empty placé devant le menu (position = tête)

    [Header("Options")]
    public bool matchYawRotation = true; // aligne la rotation horizontale

    void Start()
    {
        SpawnNow();
    }

    public void SpawnNow()
    {
        if (xrOrigin == null || xrCamera == null || menuSpawnPoint == null) return;

        // 1) calculer l'offset entre l'origine du rig et la caméra
        Vector3 cameraOffset = xrOrigin.transform.position - xrCamera.position;

        // 2) placer l'origine pour que la caméra arrive pile au spawnPoint
        xrOrigin.transform.position = menuSpawnPoint.position + cameraOffset;

        // 3) option : aligner la rotation Y (yaw) pour regarder le menu
        if (matchYawRotation)
        {
            float targetYaw = menuSpawnPoint.eulerAngles.y;
            float currentYaw = xrCamera.eulerAngles.y;
            float deltaYaw = targetYaw - currentYaw;

            xrOrigin.transform.Rotate(0f, deltaYaw, 0f, Space.World);
        }
    }
}
