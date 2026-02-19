using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;


public class SecretPathManager : MonoBehaviour
{
    public static SecretPathManager Instance { get; private set; }

    public enum StepResult { Correct, Wrong, Ignored }

    [Header("Séquence correcte")]
    public List<int> correctSequence = new List<int>();

    [Header("Reset")]
    public float resetDelay = 1f;
    public bool resetOnWrong = true;

    [Header("XR Teleport")]
    public XROrigin xrOrigin;       // ton XR Origin (XR Rig)
    public Transform startPoint;    // ton StartPoint
    public bool matchRotation = true;

    private int _progressIndex = 0;
    private bool _isResetting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public StepResult StepOnTile(SwitchSecret tile)
    {
        if (_isResetting) return StepResult.Ignored;
        if (correctSequence == null || correctSequence.Count == 0) return StepResult.Ignored;

        int expectedId = correctSequence[_progressIndex];

        if (tile.id == expectedId)
        {
            _progressIndex++;
            return StepResult.Correct;
        }
        else
        {
            if (resetOnWrong) StartCoroutine(ResetAllTilesAfterDelay());
            TeleportToStartXR();
            return StepResult.Wrong;
        }
    }

    private void TeleportToStartXR()
    {
        if (xrOrigin == null || startPoint == null) return;

        // 1) Position: place la CAMERA exactement sur startPoint
        xrOrigin.MoveCameraToWorldLocation(startPoint.position);

        // 2) Rotation (yaw uniquement)
        if (matchRotation && xrOrigin.Camera != null)
        {
            float currentYaw = xrOrigin.Camera.transform.eulerAngles.y;
            float targetYaw = startPoint.eulerAngles.y;
            float deltaYaw = targetYaw - currentYaw;

            // Tourne l'origin autour de la caméra (évite les offsets chelous)
            xrOrigin.Origin.transform.RotateAround(
                xrOrigin.Camera.transform.position,
                Vector3.up,
                deltaYaw
            );
        }
    }

    private IEnumerator ResetAllTilesAfterDelay()
    {
        _isResetting = true;
        yield return new WaitForSeconds(resetDelay);

        _progressIndex = 0;

        var allTiles = FindObjectsOfType<SwitchSecret>();
        foreach (var t in allTiles) t.ResetTile();

        _isResetting = false;
    }
}
