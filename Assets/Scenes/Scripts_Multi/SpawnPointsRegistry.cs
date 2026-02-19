using UnityEngine;

public class SpawnPointsRegistry : MonoBehaviour
{
    public static SpawnPointsRegistry Instance { get; private set; }
    [SerializeField] private Transform[] spawnPoints;

    private void Awake() => Instance = this;

    public static Transform Get(int idx)
    {
        if (Instance == null || Instance.spawnPoints == null || Instance.spawnPoints.Length == 0)
            return null;

        return Instance.spawnPoints[idx % Instance.spawnPoints.Length];
    }
}
