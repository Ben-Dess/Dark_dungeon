using UnityEngine;
using UnityEngine.XR;

public class FreezeHead : MonoBehaviour
{
    public GameObject xrCamera;

    public void Freeze()
    {
        InputTracking.disablePositionalTracking = true;
    }

    public void Unfreeze()
    {
        InputTracking.disablePositionalTracking = false;
    }
}
