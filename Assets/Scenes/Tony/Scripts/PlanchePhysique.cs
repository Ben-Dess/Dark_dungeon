using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlanchePhysique : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // On s'abonne à l'événement de sortie (quand on lâche l'objet)
        grabInteractable.selectExited.AddListener(OnLacherPlanche);
    }

    void OnLacherPlanche(SelectExitEventArgs args)
    {
        // On désactive le mode Kinematic pour que la planche tombe
        rb.isKinematic = false;
    }
}