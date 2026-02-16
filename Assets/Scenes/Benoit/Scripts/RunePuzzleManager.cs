using UnityEngine;

public class RunePuzzleManager : MonoBehaviour
{
    [Header("Runes")]
    public RuneDrawingZone[] runes;

    [Header("Chest")]
    public Animator chestAnimator;
    public string openTriggerName = "Open";

    private bool opened = false;

    void Awake()
    {
        foreach (var r in runes)
        {
            if (r == null) continue;
            r.onActivated.AddListener(CheckAllRunes);
        }
    }

    public void CheckAllRunes()
    {
        if (opened) return;

        for (int i = 0; i < runes.Length; i++)
        {
            if (runes[i] == null || !runes[i].Activated)
                return;
        }

        opened = true;

        if (chestAnimator != null)
            chestAnimator.SetTrigger(openTriggerName);

        Debug.Log("All runes activated -> Chest opened!");
    }
}
