using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StrengthLockedObject : MonoBehaviour
{
    [Header("Interaction")]
    public XRSimpleInteractable interactable;

    [Header("Behavior")]
    public bool disappear = true;
    public Transform moveTarget;          // optionnel si disappear=false
    public float moveDuration = 1.0f;

    [Header("Feedback (optional)")]
    public TMP_Text messageText;
    public float msgDuration = 2f;

    Vector3 _startPos;
    Quaternion _startRot;

    void Awake()
    {
        _startPos = transform.position;
        _startRot = transform.rotation;

        if (interactable == null)
            interactable = GetComponent<XRSimpleInteractable>();

        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelected);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelected);
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        int interactorId = args.interactorObject.transform.GetInstanceID();

        if (!PlayerStrengthState.IsStronger(interactorId))
        {
            Show("It’s too heavy... You don’t have enough strength");
            return;
        }

        // Action
        if (disappear)
        {
            Show("Wow, you broke the barrel !");
            gameObject.SetActive(false);
        }
        else
        {
            if (moveTarget != null)
            {
                Show("Wow, you broke the barrel !");
                StopAllCoroutines();
                StartCoroutine(MoveTo(moveTarget.position, moveTarget.rotation));
            }
        }
    }

    IEnumerator MoveTo(Vector3 pos, Quaternion rot)
    {
        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveDuration);
            transform.position = Vector3.Lerp(p0, pos, t);
            transform.rotation = Quaternion.Slerp(r0, rot, t);
            yield return null;
        }
    }

    void Show(string msg)
    {
        if (messageText == null) return;

        StopAllCoroutines();
        StartCoroutine(ShowMsg(msg));
    }

    IEnumerator ShowMsg(string msg)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(msgDuration);
        messageText.gameObject.SetActive(false);
    }
}
