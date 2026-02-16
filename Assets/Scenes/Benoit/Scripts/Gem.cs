using UnityEngine;

public enum GemType { Blue, Green, Yellow, Red }

public class Gem : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody rb;
    public Collider[] colliders;
    public GemType type;

}
