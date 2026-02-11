using UnityEngine;

public class KeyLockDetector : MonoBehaviour
{
    public Animator doorAnimator; // Glisse l'Animator de ta porte ici

    private void OnTriggerEnter(Collider other)
    {
        // Ce message s'affichera pour CHAQUE objet qui touche la serrure
        print("OBJET DÉTECTÉ : Nom = " + other.gameObject.name + " | Tag = " + other.gameObject.tag);

        if (other.CompareTag("Key"))
        {
            print("SUCCÈS : La clé a été reconnue ! Envoi du trigger DoorGuardOpen.");
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Ouvrir");
            }
        }
    }
}