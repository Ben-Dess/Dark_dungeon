using UnityEngine;
using System.Collections.Generic;

public class VRPuzzleManager : MonoBehaviour
{
    [Header("--- CONFIGURATION ---")]
    public List<string> nomsBaseRunes = new List<string>();

    [Header("--- JOUEUR A (AFFICHAGE) ---")]
    public List<GameObject> modelesSlot1 = new List<GameObject>();
    public List<GameObject> modelesSlot2 = new List<GameObject>();
    public List<GameObject> modelesSlot3 = new List<GameObject>();

    [Header("--- JOUEUR B (LES 3 SOCLES) ---")]
    public PuzzleSlot slotScript1;
    public PuzzleSlot slotScript2;
    public PuzzleSlot slotScript3;

    [Header("--- SORTIE ---")]
    public Animator animatorPorteA;
    public Animator animatorPorteB;

    private int[] indicesSolution = new int[3];
    private bool estResolu = false;

    void Start()
    {
        ResetAffichage();
        GenererEnigme();
    }

    void ResetAffichage()
    {
        foreach (GameObject go in modelesSlot1) go.SetActive(false);
        foreach (GameObject go in modelesSlot2) go.SetActive(false);
        foreach (GameObject go in modelesSlot3) go.SetActive(false);
    }

    void GenererEnigme()
    {
        List<int> pool = new List<int>();
        for (int i = 0; i < nomsBaseRunes.Count; i++) pool.Add(i);

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            indicesSolution[i] = pool[randomIndex];
            pool.RemoveAt(randomIndex);
        }

        modelesSlot1[indicesSolution[0]].SetActive(true);
        modelesSlot2[indicesSolution[1]].SetActive(true);
        modelesSlot3[indicesSolution[2]].SetActive(true);
    }

    public void VerifierCode()
    {
        if (estResolu) return;

        bool s1 = TestSlot(slotScript1, 0);
        bool s2 = TestSlot(slotScript2, 1);
        bool s3 = TestSlot(slotScript3, 2);

        print($"État des slots : S1={s1}, S2={s2}, S3={s3}");

        if (s1 && s2 && s3)
        {
            OuvrirPortes();
        }
    }

    bool TestSlot(PuzzleSlot slot, int indexEnigme)
    {
        if (slot.objetActuel == null) return false;

        string tagAttendu = nomsBaseRunes[indicesSolution[indexEnigme]] + "B";
        string tagActuel = slot.objetActuel.tag;

        bool estCorrect = slot.objetActuel.CompareTag(tagAttendu);

        if (!estCorrect)
        {
            print($"Slot {indexEnigme + 1} faux : Reçu {tagActuel}, Attendu {tagAttendu}");
        }

        return estCorrect;
    }

    void OuvrirPortes()
    {
        estResolu = true;
        print("BRAVO : Le code est bon, j'envoie le signal d'ouverture !");

        animatorPorteA.SetTrigger("Ouvrir");
        animatorPorteB.SetTrigger("Ouvrir");
    }
}