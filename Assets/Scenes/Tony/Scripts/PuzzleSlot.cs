using UnityEngine;

public class PuzzleSlot : MonoBehaviour
{
    public int slotNumber; // 1, 2 ou 3
    public Material couleurAssignee;
    public VRPuzzleManager manager;

    [HideInInspector] public GameObject objetActuel;

    private void OnTriggerEnter(Collider other)
    {
        // On vérifie si l'objet a le script RuneColorHandler (c'est une rune)
        RuneColorHandler handler = other.GetComponent<RuneColorHandler>();

        if (handler != null && objetActuel == null)
        {
            objetActuel = other.gameObject;
            handler.ChangerCouleur(couleurAssignee);
            manager.VerifierCode();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objetActuel)
        {
            RuneColorHandler handler = other.GetComponent<RuneColorHandler>();
            if (handler != null) handler.ResetCouleur();

            objetActuel = null;
            manager.VerifierCode();
        }
    }
}