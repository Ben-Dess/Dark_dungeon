using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SecretPathManager : NetworkBehaviour
{
    public static SecretPathManager Instance { get; private set; }

    public enum StepResult { Correct, Wrong, Ignored }

    [Header("Séquence correcte")]
    public List<int> correctSequence = new List<int>();

    [Header("Reset")]
    public float resetDelay = 1f;
    public bool resetOnWrong = true;

    [Header("Fin de séquence")]
    public GameObject doorToDestroy;
    public float destroyDelay = 0f;
    public bool resetAfterSuccess = false;

    int _progressIndex = 0;
    bool _isResetting = false;
    bool _completed = false;

    // Cache id -> tile (local, sur chaque client)
    Dictionary<int, SwitchSecret> _tilesById = new Dictionary<int, SwitchSecret>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        RebuildTileCache();
    }

    void RebuildTileCache()
    {
        _tilesById.Clear();
        var tiles = FindObjectsOfType<SwitchSecret>(true);
        foreach (var t in tiles)
            _tilesById[t.id] = t;
    }

    // Appelé par SwitchSecret localement
    public void StepOnTileLocal(SwitchSecret tile)
    {
        if (!IsSpawned) return;

        // On envoie au serveur l'id de la dalle
        RequestStepOnTileServerRpc(tile.id);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestStepOnTileServerRpc(int tileId)
    {
        var result = StepOnTileServer(tileId);

        if (result == StepResult.Correct)
            SetTileStateClientRpc(tileId, 1);
        else if (result == StepResult.Wrong)
            SetTileStateClientRpc(tileId, 2);
    }

    StepResult StepOnTileServer(int tileId)
    {
        if (_completed) return StepResult.Ignored;
        if (_isResetting) return StepResult.Ignored;
        if (correctSequence == null || correctSequence.Count == 0) return StepResult.Ignored;

        int expectedId = correctSequence[_progressIndex];

        if (tileId == expectedId)
        {
            _progressIndex++;

            if (_progressIndex >= correctSequence.Count)
                OnSequenceCompleted();

            return StepResult.Correct;
        }
        else
        {
            if (resetOnWrong)
                StartCoroutine(ResetAllTilesAfterDelay());

            return StepResult.Wrong;
        }
    }

    void OnSequenceCompleted()
    {
        _completed = true;

        if (doorToDestroy != null)
        {
            if (destroyDelay <= 0f) DisableDoorClientRpc();
            else StartCoroutine(DisableDoorAfterDelay());
        }

        if (resetAfterSuccess)
            StartCoroutine(ResetAllTilesAfterDelay());
    }

    IEnumerator DisableDoorAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        DisableDoorClientRpc();
    }

    IEnumerator ResetAllTilesAfterDelay()
    {
        _isResetting = true;
        yield return new WaitForSeconds(resetDelay);

        _progressIndex = 0;
        _completed = false;

        ResetAllTilesClientRpc();

        _isResetting = false;
    }

    // state: 0 neutral, 1 correct, 2 wrong
    [ClientRpc]
    void SetTileStateClientRpc(int tileId, int state)
    {
        if (_tilesById.Count == 0) RebuildTileCache();

        if (!_tilesById.TryGetValue(tileId, out var tile) || tile == null)
            return;

        if (state == 1) tile.SetCorrect();
        else if (state == 2) tile.SetWrong();
        else tile.ResetTile();
    }

    [ClientRpc]
    void ResetAllTilesClientRpc()
    {
        if (_tilesById.Count == 0) RebuildTileCache();

        foreach (var kv in _tilesById)
            if (kv.Value != null) kv.Value.ResetTile();
    }

    [ClientRpc]
    void DisableDoorClientRpc()
    {
        if (doorToDestroy == null) return;
        doorToDestroy.SetActive(false);
    }
}
