using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_DoorScript : MonoBehaviour
{
    [Tooltip("If it is false door can't be used")]
    public bool Locked = false;
    [Tooltip("It is true for remote control only")]
    public bool Remote = false;
    [Space]
    [Tooltip("Door can be opened")]
    public bool CanOpen = true;
    [Tooltip("Door can be closed")]
    public bool CanClose = true;
    [Space]
    [Tooltip("Door locked by red key (use key script to declarate any object as key)")]
    public bool RedLocked = false;
    public bool BlueLocked = false;
    [Tooltip("It is used for key script working")]
    AN_HeroInteractive HeroInteractive;
    [Space]
    public bool isOpened = false;
    [Range(0f, 4f)]
    [Tooltip("Speed for door opening, degrees per sec")]
    public float OpenSpeed = 3f;

    [Header("SFX (optional)")]
    public AudioClip doorOpenSfx;
    [Range(0f, 1f)] public float doorOpenVolume = 1f;

    // NearView()
    float distance;
    float angleView;
    Vector3 direction;

    // Hinge
    [HideInInspector]
    public Rigidbody rbDoor;
    HingeJoint hinge;
    JointLimits hingeLim;
    float currentLim;

    void Start()
    {
        rbDoor = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        HeroInteractive = FindObjectOfType<AN_HeroInteractive>();

        if (hinge != null)
        {
            hingeLim = hinge.limits;
            currentLim = hingeLim.max;
        }
    }

    void Update()
    {
        // kept for compatibility - no "E" interaction here in your VR setup
    }

    public void SetOpen(bool open)
    {
        if (Locked) return;

        if (open)
        {
            if (!CanOpen) return;
            if (RedLocked || BlueLocked) return;

            // SFX only when state changes to open
            if (!isOpened && doorOpenSfx != null)
                SFXManager.Instance?.Play3D(doorOpenSfx, transform.position, doorOpenVolume);

            isOpened = true;
            if (rbDoor != null)
                rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
        else
        {
            if (!CanClose) return;
            if (RedLocked || BlueLocked) return;

            isOpened = false;
        }
    }

    bool NearView() // kept for compatibility (unused in VR version)
    {
        if (Camera.main == null) return false;
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(direction, Camera.main.transform.forward);
        return (distance < 3f && angleView < 70f);
    }

    public void Open()
    {
        SetOpen(true);
    }

    public void Close()
    {
        SetOpen(false);
    }
}
