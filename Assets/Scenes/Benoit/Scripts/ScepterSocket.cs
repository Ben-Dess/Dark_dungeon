using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class ScepterSocket : MonoBehaviour
{
    public Scepter scepter;

    XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnEntered);
        socket.selectExited.AddListener(OnExited);
    }

    void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnEntered);
        socket.selectExited.RemoveListener(OnExited);
    }

    void OnEntered(SelectEnterEventArgs args)
    {
        var gem = args.interactableObject.transform.GetComponent<Gem>();
        if (gem != null) scepter.Equip(gem);
    }

    void OnExited(SelectExitEventArgs args)
    {
        var gem = args.interactableObject.transform.GetComponent<Gem>();
        if (gem != null) scepter.Unequip(gem);
    }
}
