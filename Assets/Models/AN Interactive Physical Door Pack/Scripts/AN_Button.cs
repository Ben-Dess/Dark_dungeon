using UnityEngine;

public class AN_Button : MonoBehaviour
{
    [Tooltip("True for rotation like valve (used for ramp/elevator only)")]
    public bool isValve = false;

    [Tooltip("SelfRotation speed of valve")]
    public float ValveSpeed = 10f;

    [Tooltip("If it isn't valve, it can be lever or button (animated)")]
    public bool isLever = false;

    [Tooltip("If it is false button/lever can't be used")]
    public bool Locked = false;

    [Tooltip("The door for remote control")]
    public AN_DoorScript DoorObject;

    [Space]
    [Tooltip("Any object for ramp/elevator behaviour (used when isValve = true)")]
    public Transform RampObject;

    [Tooltip("Door can be opened")]
    public bool CanOpen = true;

    [Tooltip("Door can be closed")]
    public bool CanClose = true;

    [Tooltip("Current status of the door")]
    public bool isOpened = false;

    [Space]
    [Tooltip("True for rotation by X local rotation by valve")]
    public bool xRotation = true;

    [Tooltip("True for vertical movenment by valve (if xRotation is false)")]
    public bool yPosition = false;

    public float max = 90f, min = 0f, speed = 5f;

    // -------------------------
    // VR Animation config
    // -------------------------
    [Header("VR Animation (optional)")]
    [Tooltip("Animator to play lever/button animation. If empty, uses Animator on same GameObject.")]
    public Animator leverAnimator;

    [Tooltip("Bool parameter name for lever hold state (your controller shows: LeverUp).")]
    public string leverHoldBoolName = "LeverUp";

    [Tooltip("Trigger parameter name for button press (your controller shows: ButtonPre).")]
    public string buttonPressTriggerName = "ButtonPre";

    // Valve internals
    bool valveBool = true;
    float current, startYPosition;
    Quaternion startQuat, rampQuat;

    Animator anim;

    // VR hold state (replaces old Input.GetKey)
    bool _isHeld = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (leverAnimator == null) leverAnimator = anim;

        startQuat = transform.rotation;

        // Fix: RampObject can be null
        if (RampObject != null)
        {
            startYPosition = RampObject.position.y;
            rampQuat = RampObject.rotation;
        }
    }

    void Update()
    {
        if (Locked) return;

        // VR: no UnityEngine.Input here.
        // - Lever/Button uses BeginHold/EndHold (called by XR events)
        // - Valve continues to update while _isHeld is true

        if (isValve && RampObject != null)
        {
            // Changing value in script
            if (_isHeld)
            {
                if (valveBool)
                {
                    if (!isOpened && CanOpen && current < max) current += speed * Time.deltaTime;
                    if (isOpened && CanClose && current > min) current -= speed * Time.deltaTime;

                    if (current >= max)
                    {
                        isOpened = true;
                        valveBool = false;
                    }
                    else if (current <= min)
                    {
                        isOpened = false;
                        valveBool = false;
                    }
                }
            }
            else
            {
                // Return / relax behaviour when not held
                if (!isOpened && current > min) current -= speed * Time.deltaTime;
                if (isOpened && current < max) current += speed * Time.deltaTime;
                valveBool = true;
            }

            // Apply values to objects
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

    /// <summary>
    /// Called when player STARTS holding Select on XR Simple Interactable.
    /// - Lever: sets LeverUp = true, opens door
    /// - Button: triggers ButtonPre, opens door
    /// - Valve: starts rotation while held
    /// </summary>
    public void BeginHold()
    {
        if (Locked) return;
        _isHeld = true;

        // Animation feedback
        if (leverAnimator != null)
        {
            if (isLever)
            {
                // Your animator uses a Bool named "LeverUp"
                leverAnimator.SetBool(leverHoldBoolName, true);
            }
            else
            {
                // Your animator uses a Trigger named "ButtonPre"
                leverAnimator.SetTrigger(buttonPressTriggerName);
            }
        }

        // Door control (hold-to-open)
        if (!isValve && DoorObject != null && DoorObject.Remote)
        {
            DoorObject.SetOpen(true);
        }
    }

    /// <summary>
    /// Called when player STOPS holding Select.
    /// - Lever: sets LeverUp = false, closes door
    /// - Button: (no trigger on release), closes door
    /// - Valve: stops rotation (Update handles relax)
    /// </summary>
    public void EndHold()
    {
        if (Locked) return;
        _isHeld = false;

        // Animation feedback
        if (leverAnimator != null && isLever)
        {
            leverAnimator.SetBool(leverHoldBoolName, false);
        }

        // Door control (release-to-close)
        if (!isValve && DoorObject != null && DoorObject.Remote)
        {
            DoorObject.SetOpen(false);
        }
    }
}
