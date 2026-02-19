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
    public XROrigin xrOrigin;
    public Transform startPoint;
    public bool matchRotation = true;

    [Header("Fin de séquence")]
    public GameObject doorToDestroy;     // glisse ta porte ici dans l’inspecteur
    public float destroyDelay = 0f;      // 0 = immédiat
    public bool resetAfterSuccess = false;

    private int _progressIndex = 0;
    private bool _isResetting = false;
    private bool _completed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public StepResult StepOnTile(SwitchSecret tile)
    {
        if (_completed) return StepResult.Ignored;
        if (_isResetting) return StepResult.Ignored;
        if (correctSequence == null || correctSequence.Count == 0) return StepResult.Ignored;

        int expectedId = correctSequence[_progressIndex];

        if (tile.id == expectedId)
        {
            _progressIndex++;

            if (_progressIndex >= correctSequence.Count)
            {
                OnSequenceCompleted();
            }

            return StepResult.Correct;
        }
        else
        {
            if (resetOnWrong) StartCoroutine(ResetAllTilesAfterDelay());
            //TeleportToStartXR(); // si tu veux le garder
            return StepResult.Wrong;
        }
    }

    private void OnSequenceCompleted()
    {
        _completed = true;

        if (doorToDestroy != null)
        {
            Destroy(doorToDestroy, destroyDelay);
        }

        if (resetAfterSuccess)
        {
            StartCoroutine(ResetAllTilesAfterDelay());
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
