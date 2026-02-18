using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class VRPuzzleManager : NetworkBehaviour
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

    // Solution synchronisée
    private NetworkVariable<int> sol0 = new NetworkVariable<int>(-1);
    private NetworkVariable<int> sol1 = new NetworkVariable<int>(-1);
    private NetworkVariable<int> sol2 = new NetworkVariable<int>(-1);

    private NetworkVariable<bool> estResolu = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        // Quand la solution change, on met à jour l'affichage (sur tous)
        sol0.OnValueChanged += (_, __) => RefreshAffichage();
        sol1.OnValueChanged += (_, __) => RefreshAffichage();
        sol2.OnValueChanged += (_, __) => RefreshAffichage();

        if (IsServer)
        {
            ResetAffichage();
            GenererEnigmeServeur();
        }
        else
        {
            // Les clients attendent la solution puis affichent
            ResetAffichage();
            RefreshAffichage();
        }
    }

    void ResetAffichage()
    {
        foreach (GameObject go in modelesSlot1) go.SetActive(false);
        foreach (GameObject go in modelesSlot2) go.SetActive(false);
        foreach (GameObject go in modelesSlot3) go.SetActive(false);
    }

    void GenererEnigmeServeur()
    {
        if (nomsBaseRunes.Count < 3)
        {
            Debug.LogError("Il faut au moins 3 runes dans nomsBaseRunes.");
            return;
        }

        List<int> pool = new List<int>();
        for (int i = 0; i < nomsBaseRunes.Count; i++) pool.Add(i);

        int Pick()
        {
            int randomIndex = Random.Range(0, pool.Count);
            int v = pool[randomIndex];
            pool.RemoveAt(randomIndex);
            return v;
        }

        sol0.Value = Pick();
        sol1.Value = Pick();
        sol2.Value = Pick();

        // L'affichage se fera via RefreshAffichage()
        RefreshAffichage();
    }

    void RefreshAffichage()
    {
        // Si la solution n’est pas encore reçue chez le client
        if (sol0.Value < 0 || sol1.Value < 0 || sol2.Value < 0) return;

        ResetAffichage();
        modelesSlot1[sol0.Value].SetActive(true);
        modelesSlot2[sol1.Value].SetActive(true);
        modelesSlot3[sol2.Value].SetActive(true);
    }

    // --- Appelée par un bouton / événement quand on veut vérifier ---
    public void VerifierCode()
    {
        if (estResolu.Value) return;

        // En multi : on demande au serveur de vérifier
        VerifierCodeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void VerifierCodeServerRpc()
    {
        if (estResolu.Value) return;

        bool s1 = TestSlotServeur(slotScript1, sol0.Value, 1);
        bool s2 = TestSlotServeur(slotScript2, sol1.Value, 2);
        bool s3 = TestSlotServeur(slotScript3, sol2.Value, 3);

        Debug.Log($"[SERVER] État des slots : S1={s1}, S2={s2}, S3={s3}");

        if (s1 && s2 && s3)
        {
            estResolu.Value = true;
            OuvrirPortesClientRpc();
        }
    }

    private bool TestSlotServeur(PuzzleSlot slot, int indexSolution, int numeroSlot)
    {
        if (slot == null || slot.objetActuel == null) return false;

        string tagAttendu = nomsBaseRunes[indexSolution] + "B";
        bool ok = slot.objetActuel.CompareTag(tagAttendu);

        if (!ok)
            Debug.Log($"[SERVER] Slot {numeroSlot} faux : Reçu {slot.objetActuel.tag}, Attendu {tagAttendu}");

        return ok;
    }

    [ClientRpc]
    private void OuvrirPortesClientRpc()
    {
        Debug.Log("BRAVO : ouverture des portes (sync) !");
        animatorPorteA.SetTrigger("Ouvrir");
        animatorPorteB.SetTrigger("Ouvrir");
    }
}
