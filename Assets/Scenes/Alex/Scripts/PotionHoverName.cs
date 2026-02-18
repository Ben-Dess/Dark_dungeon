using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class PotionHoverName : MonoBehaviour
{
    public TMP_Text label3D;

    [Header("Placement")]
    public float heightOffset = 0.18f;
    public float forwardOffset = 0.15f;

    XRBaseInteractable _xri;
    PotionBottle _bottle;

    void Awake()
    {
        _xri = GetComponent<XRBaseInteractable>();
        _bottle = GetComponent<PotionBottle>();

        _xri.hoverEntered.AddListener(OnHoverEnter);
        _xri.hoverExited.AddListener(OnHoverExit);

        if (label3D != null)
            label3D.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (_xri == null) return;
        _xri.hoverEntered.RemoveListener(OnHoverEnter);
        _xri.hoverExited.RemoveListener(OnHoverExit);
    }

    void LateUpdate()
    {
        if (label3D == null || !label3D.gameObject.activeSelf) return;

        if (Camera.main == null) return;

        // Toujours faire face à la caméra
        Vector3 lookDir = label3D.transform.position - Camera.main.transform.position;
        lookDir.y = 0f;
        label3D.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (label3D == null || _bottle == null) return;

        label3D.text = _bottle.GetDisplayName();

        if (Camera.main == null) return;

        Vector3 basePos = transform.position;

        // Au-dessus
        Vector3 pos = basePos + Vector3.up * heightOffset;

        // Vers la caméra
        Vector3 dirToCam = (Camera.main.transform.position - basePos).normalized;
        pos += dirToCam * forwardOffset;

        label3D.transform.position = pos;
        label3D.gameObject.SetActive(true);
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        if (label3D == null) return;
        label3D.gameObject.SetActive(false);
    }
}
