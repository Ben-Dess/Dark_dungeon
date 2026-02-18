using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretPathManager : MonoBehaviour
{
    public static SecretPathManager Instance { get; private set; }

    public enum StepResult { Correct, Wrong, Ignored }

    [Header("Séquence correcte (IDs des dalles dans l'ordre)")]
    public List<int> correctSequence = new List<int>();

    [Header("Reset")]
    public float resetDelay = 1.0f;
    public bool resetOnWrong = true;

    [Header("Optionnel: action quand la séquence est complétée")]
    public GameObject secretDoorToOpen;

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

            // séquence terminée
            if (_progressIndex >= correctSequence.Count)
            {
                OnSequenceCompleted();
            }

            return StepResult.Correct;
        }
        else
        {
            if (resetOnWrong)
                StartCoroutine(ResetAllTilesAfterDelay());

            return StepResult.Wrong;
        }
    }

    private void OnSequenceCompleted()
    {
        // exemple simple : ouvrir une porte / activer un passage
        if (secretDoorToOpen != null)
            secretDoorToOpen.SetActive(false);

        // si tu veux empêcher de recommencer
        // _isResetting = true;
    }

    private IEnumerator ResetAllTilesAfterDelay()
    {
        _isResetting = true;
        yield return new WaitForSeconds(resetDelay);

        _progressIndex = 0;

        // reset toutes les dalles de la scène
        var allTiles = FindObjectsOfType<SwitchSecret>();
        foreach (var t in allTiles)
            t.ResetTile();

        _isResetting = false;
    }
}
