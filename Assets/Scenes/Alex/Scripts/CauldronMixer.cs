using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CauldronMixer : MonoBehaviour
{
    [Header("Links")]
    public PotionRecipeManager recipeManager;
    public ResetManager resetManager;

    [Header("Socket (drop potions here)")]
    public XRSocketInteractor potionSocket;

    [Header("Drink Interaction (VR)")]
    [Tooltip("Ajoute XR Simple Interactable sur le chaudron (ou un enfant) et r�f�rence-le ici.")]
    public XRSimpleInteractable drinkInteractable;

    [Tooltip("Message world-space affich� apr�s avoir bu (TMP_Text).")]
    public TMP_Text drinkMessageText;

    [Tooltip("Dur�e d'affichage du message.")]
    public float drinkMessageDuration = 2.5f;

    [Header("Liquid Visual (optional)")]
    public Renderer liquidRenderer;
    public string colorProperty = "_BaseColor"; // URP: _BaseColor | Built-in: _Color

    [Tooltip("Couleur du chaudron pendant le m�lange (d�part).")]
    public Color baseLiquidColor = Color.black;

    [Tooltip("Couleur OR quand la recette est r�ussie.")]
    public Color goldColor = new Color(1f, 0.84f, 0.0f);

    [Header("SFX")]
    public AudioClip sfxGoodBottle;
    public AudioClip sfxWrongBottle;
    public AudioClip sfxRecipeSolved;
    public AudioClip sfxDrink;
    public AudioClip sfxDoorOpen;
    [Range(0f, 1f)] public float sfx2DVolume = 1f;
    [Range(0f, 1f)] public float sfx3DVolume = 1f;

    [Header("Rules")]
    public int requiredCount = 3;

    [Tooltip("Laisse FALSE pour pouvoir reset sans respawn.")]
    public bool consumeBottleOnAdd = false;

    [Header("Anti-double trigger")]
    public float addCooldown = 0.2f;

    [Header("Door Unlock")]

    public GameObject doorToDisable;

    public bool disableWholeDoorObject = true;

    public bool RecipeSolved { get; private set; } = false;

    int _step = 0;
    Color _currentColor;
    readonly StringBuilder _addedLog = new StringBuilder();

    readonly HashSet<int> _consumedInstanceIds = new HashSet<int>();
    float _nextAllowedAddTime = 0f;


    readonly HashSet<int> _drinkers = new HashSet<int>();

    void Awake()
    {
        _currentColor = baseLiquidColor;

        if (potionSocket != null)
            potionSocket.selectEntered.AddListener(OnSocketSelectEntered);

        if (drinkInteractable != null)
            drinkInteractable.selectEntered.AddListener(OnDrinkSelected);

        if (drinkMessageText != null)
            drinkMessageText.gameObject.SetActive(false);

        ApplyLiquidColor(_currentColor);
    }

    void OnDestroy()
    {
        if (potionSocket != null)
            potionSocket.selectEntered.RemoveListener(OnSocketSelectEntered);

        if (drinkInteractable != null)
            drinkInteractable.selectEntered.RemoveListener(OnDrinkSelected);
    }

    void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        if (RecipeSolved) return;

        if (Time.time < _nextAllowedAddTime)
            return;

        var bottle = args.interactableObject.transform.GetComponentInParent<PotionBottle>();
        if (bottle == null) return;

        int id = bottle.gameObject.GetInstanceID();
        if (_consumedInstanceIds.Contains(id))
            return;

        _nextAllowedAddTime = Time.time + addCooldown;

        TryAddBottle(bottle);
    }

    void TryAddBottle(PotionBottle bottle)
    {
        if (recipeManager == null || recipeManager.CurrentRecipe == null || recipeManager.CurrentRecipe.Count == 0)
        {
            Debug.LogWarning("[Cauldron] Pas de recette disponible (recipeManager manquant ?).");
            return;
        }

        if (_step >= requiredCount)
            return;

        _consumedInstanceIds.Add(bottle.gameObject.GetInstanceID());

        _addedLog.Append($"{_step + 1}) {bottle.shelfID}-{bottle.colorName}  ");
        Debug.Log("[Cauldron Added] " + _addedLog.ToString());

        var expected = recipeManager.CurrentRecipe[_step];
        bool shelfOk = bottle.shelfID == expected.shelfID;
        bool colorOk = Normalize(bottle.colorName) == Normalize(expected.colorName);

        if (!shelfOk || !colorOk)
        {
            SFXManager.Instance?.Play2D(sfxWrongBottle, sfx2DVolume);

            Debug.LogWarning(
                $"Mauvaise potion � l'�tape {_step + 1}. " +
                $"Attendu: {expected.shelfID}-{expected.colorName} | Re�u: {bottle.shelfID}-{bottle.colorName}\n" +
                $"-> Reset total, recommencer la recette."
            );

            ResetCauldron();

            if (resetManager != null)
                resetManager.DoReset();
            else
                ResetCauldron();

            RefreshSocket();
            return;
        }

        SFXManager.Instance?.Play2D(sfxGoodBottle, sfx2DVolume);

        _step++;


        _currentColor = Color.Lerp(_currentColor, bottle.liquidColor, 1f / Mathf.Max(1, _step));
        ApplyLiquidColor(_currentColor);

        if (consumeBottleOnAdd)
        {
            Destroy(bottle.gameObject);
        }
        else
        {
            ConsumeBottleNonDestructive(bottle);
        }

        if (_step >= requiredCount)
        {
            RecipeSolved = true;

            Debug.Log("Recette termin�e ! Le chaudron est pr�t.");
            SFXManager.Instance?.Play2D(sfxRecipeSolved, sfx2DVolume);


            _currentColor = goldColor;
            ApplyLiquidColor(_currentColor);


            DisableDoorIfAssigned();
        }
    }

    void DisableDoorIfAssigned()
    {
        if (doorToDisable == null) return;

        if (sfxDoorOpen != null)
            SFXManager.Instance?.Play3D(sfxDoorOpen, doorToDisable.transform.position, sfx3DVolume);

        if (disableWholeDoorObject)
        {
            doorToDisable.SetActive(false);
        }
        else
        {

            foreach (var c in doorToDisable.GetComponentsInChildren<Collider>(true))
                c.enabled = false;
        }

        Debug.Log("[Cauldron] Porte d�sactiv�e (sortie ouverte).");
    }

    void ConsumeBottleNonDestructive(PotionBottle bottle)
    {
        var rb = bottle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            bool wasKinematic = rb.isKinematic;
            if (wasKinematic) rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (var col in bottle.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        var grab = bottle.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = false;

        foreach (var r in bottle.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        var hover = bottle.GetComponent<PotionHoverName>();
        if (hover != null)
            hover.enabled = false;
    }

    void OnDrinkSelected(SelectEnterEventArgs args)
    {
        if (!RecipeSolved)
        {
            ShowDrinkMessage("The potion is not ready...");
            return;
        }

        int interactorId = args.interactorObject.transform.GetInstanceID();

        if (_drinkers.Contains(interactorId))
        {
            ShowDrinkMessage("You have already drunk.");
            return;
        }

        _drinkers.Add(interactorId);

        PlayerStrengthState.MarkStronger(interactorId);

        SFXManager.Instance?.Play2D(sfxDrink, sfx2DVolume);
        ShowDrinkMessage("You feel stronger!");
    }

    void ShowDrinkMessage(string msg)
    {
        if (drinkMessageText == null) return;

        StopAllCoroutines();
        StartCoroutine(ShowMessageRoutine(msg));
    }

    IEnumerator ShowMessageRoutine(string msg)
    {
        drinkMessageText.text = msg;
        drinkMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(drinkMessageDuration);
        drinkMessageText.gameObject.SetActive(false);
    }

    public void ResetCauldron()
    {
        RecipeSolved = false;
        _step = 0;
        _currentColor = baseLiquidColor;
        _addedLog.Clear();
        _consumedInstanceIds.Clear();
        _drinkers.Clear();
        _nextAllowedAddTime = 0f;

        ApplyLiquidColor(_currentColor);

        if (drinkMessageText != null)
            drinkMessageText.gameObject.SetActive(false);

        Debug.Log("[Cauldron] Reset");
    }

    void RefreshSocket()
    {
        if (potionSocket == null) return;
        potionSocket.enabled = false;
        potionSocket.enabled = true;
    }

    void ApplyLiquidColor(Color c)
    {
        if (liquidRenderer == null) return;

        var mats = liquidRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null && mats[i].HasProperty(colorProperty))
                mats[i].SetColor(colorProperty, c);
        }
    }

    static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        return s.Trim().ToLowerInvariant();
    }
}
