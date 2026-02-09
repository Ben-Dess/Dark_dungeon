using UnityEngine;

public class RuneDrawingZone : MonoBehaviour
{
    public int totalCheckpoints = 4;
    public bool Activated { get; private set; }

    int currentIndex = 0;

    public void ValidateCheckpoint(int index)
    {
        if (Activated) return;

        if (index == currentIndex)
        {
            currentIndex++;

            if (currentIndex >= totalCheckpoints)
            {
                Activated = true;
                Debug.Log("Rune Activated !");
            }
        }
        else
        {
            currentIndex = 0;
        }
    }

    public void ResetProgress()
    {
        currentIndex = 0;
    }
}
