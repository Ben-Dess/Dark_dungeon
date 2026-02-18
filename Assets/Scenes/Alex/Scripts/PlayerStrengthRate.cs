using System.Collections.Generic;
using UnityEngine;

public static class PlayerStrengthState
{
    // On stocke la force par interactor (main/ray) => suffisant pour ton gameplay
    private static readonly HashSet<int> Stronger = new HashSet<int>();

    public static void MarkStronger(int interactorId)
    {
        Stronger.Add(interactorId);
        Debug.Log($"[Strength] Interactor {interactorId} est maintenant plus fort.");
    }

    public static bool IsStronger(int interactorId)
    {
        return Stronger.Contains(interactorId);
    }

    public static void ResetAll()
    {
        Stronger.Clear();
        Debug.Log("[Strength] ResetAll()");
    }
}
