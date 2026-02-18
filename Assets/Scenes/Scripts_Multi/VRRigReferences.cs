using UnityEngine;

public class VRRigReferences : MonoBehaviour
{

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public static VRRigReferences Singleton;

    public void Awake()
    {
        Singleton = this;
    }
}
