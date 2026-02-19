using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class AN_Button : NetworkBehaviour
{
    [Tooltip("True for rotation like valve (used for ramp/elevator only)")]
    public bool isValve = false;

    [Tooltip("SelfRotation speed of valve")]
    public float ValveSpeed = 10f;

    [Tooltip("If it isn't valve, it can be lever or button (animated)")]
    public bool isLever = false;

    [Tooltip("If it is false button/lever can't be used")]
    public bool Locked = false;

    [Tooltip("The door for remote control (NETWORK VERSION)")]
    public AN_DoorScript DoorObject;

    [Space]
    [Tooltip("Any object for ramp/elevator behaviour (used when isValve = true)")]
    public Transform RampObject;

    [Tooltip("Door can be opened")]
    public bool CanOpen = true;

    [Tooltip("Door can be closed")]
    public bool CanClose = true;

    [Tooltip("Current status of the valve target")]
    public bool isOpened = false;

    [Space]
    [Tooltip("True for rotation by X local rotation by valve")]
    public bool xRotation = true;

    [Tooltip("True for vertical movenment by valve (if xRotation is false)")]
    public bool yPosition = false;

    public float max = 90f, min = 0f, speed = 5f;

    [Header("VR Animation (optional)")]
    public Animator leverAnimator;
    public string leverHoldBoolName = "LeverUp";
    public string buttonPressTriggerName = "ButtonPre";

    // Valve internals
    bool valveBool = true;
    float current, startYPosition;
    Quaternion startQuat, rampQuat;

    // VR hold state
    bool _isHeld = false;

    void Start()
    {
        if (leverAnimator == null) leverAnimator = GetComponent<Animator>();

        startQuat = transform.rotation;

        if (RampObject != null)
        {
            startYPosition = RampObject.position.y;
            rampQuat = RampObject.rotation;
        }
    }

    void Update()
    {
        if (Locked) return;

        // Valve simulation: choose authority
        // Option A (simple): server drives valve for everyone
        if (!IsServer) return;

        if (isValve && RampObject != null)
        {
            if (_isHeld)
            {
                if (valveBool)
                {
                    if (!isOpened && CanOpen && current < max) current += speed * Time.deltaTime;
                    if (isOpened && CanClose && current > min) current -= speed * Time.deltaTime;

                    if (current >= max) { isOpened = true; valveBool = false; }
                    else if (current <= min) { isOpened = false; valveBool = false; }
                }
            }
            else
            {
                if (!isOpened && current > min) current -= speed * Time.deltaTime;
                if (isOpened && current < max) current += speed * Time.deltaTime;
                valveBool = true;
            }

            // Apply visuals on server (clients will see transform via NetworkTransform if you add it)
            transform.rotation = startQuat * Quaternion.Euler(0f, 0f, current * ValveSpeed);

            if (xRotation)
                RampObject.rotation = rampQuat * Quaternion.Euler(current, 0f, 0f);
            else if (yPosition)
                RampObject.position = new Vector3(RampObject.position.x, startYPosition + current, RampObject.position.z);
        }
    }

    // =========================
    // ===== VR ENTRY POINTS ===
    // =========================

    public void BeginHold()
    {
        if (Locked) return;
        _isHeld = true;

        // Local animation feedback instantly
        if (leverAnimator != null)
        {
            if (isLever) leverAnimator.SetBool(leverHoldBoolName, true);
            else leverAnimator.SetTrigger(buttonPressTriggerName);
        }

        // IMPORTANT CHANGE:
        // Instead of "hold-to-open", we UNLOCK for everyone (persistent).
        if (!isValve && DoorObject != null)
        {
            DoorObject.RequestUnlock();
        }

        // If you want the lever press to also OPEN immediately, uncomment:
        // if (!isValve && DoorObject != null) DoorObject.RequestOpen();
    }

    public void EndHold()
    {
        if (Locked) return;
        _isHeld = false;

        // Lever animation release only (NO closing door anymore)
        if (leverAnimator != null && isLever)
        {
            leverAnimator.SetBool(leverHoldBoolName, false);
        }

        // Do NOT call SetOpen(false) here.
    }
}
