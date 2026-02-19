using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StrengthLockedObject : NetworkBehaviour
{
    [Header("Interaction")]
    public XRSimpleInteractable interactable;

    [Header("Behavior")]
    public bool disappear = true;

    [Header("SFX (optional)")]
    public AudioClip sfxBreak;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public bool play3D = true;

    [Header("Feedback (optional)")]
    public TMP_Text messageText;
    public float msgDuration = 2f;

    void Awake()
    {
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
        //  CHECK EN LOCAL (comme tu veux)
        int interactorId = args.interactorObject.transform.GetInstanceID();

        if (!PlayerStrengthState.IsStronger(interactorId))
        {
            Show("It's too heavy ... You need more strength");
            return;
        }

        Show("Wow ! You broke the barrel !");
        PlayBreakSfx();

        //  demande au serveur de désactiver
        RequestDisableServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDisableServerRpc()
    {
        // le serveur dit à tout le monde de désactiver
        DisableObjectClientRpc();
    }

    [ClientRpc]
    void DisableObjectClientRpc()
    {
        gameObject.SetActive(false);
    }

    void PlayBreakSfx()
    {
        if (sfxBreak == null) return;

        if (play3D)
            SFXManager.Instance?.Play3D(sfxBreak, transform.position, sfxVolume);
        else
            SFXManager.Instance?.Play2D(sfxBreak, sfxVolume);
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
