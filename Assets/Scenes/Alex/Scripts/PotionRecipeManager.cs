using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class PotionRecipeManager : MonoBehaviour
{
    [System.Serializable]
    public struct RecipeEntry
    {
        public int shelfID;
        public string colorName; // "Red", "Orange", ...
    }

    [Header("Pools")]
    public List<int> availableShelves = new() { 1, 2, 3, 4, 5, 6, 7 };

    // Noms utilisés sur tes bouteilles (EN)
    public List<string> availableColors = new()
    {
        "Red", "Orange", "Green", "Blue", "Yellow", "Black", "Grey", "Purple"
    };

    [Header("UI")]
    public TMP_Text boardText;

    [Header("Recipe")]
    public int recipeCount = 3;

    public List<RecipeEntry> CurrentRecipe { get; private set; } = new();

    void Start()
    {
        GenerateRecipe(recipeCount);
        UpdateBoard();

        // Log de la recette (ordre important)
        Debug.Log("[Recipe] " + RecipeToString());
    }

    public void GenerateRecipe(int count)
    {
        CurrentRecipe.Clear();

        if (availableShelves == null || availableShelves.Count == 0) return;
        if (availableColors == null || availableColors.Count == 0) return;

        // On empêche les doublons: même couple shelfID + color
        var used = new HashSet<string>();

        // Safety pour éviter boucle infinie si pool trop petit
        int safety = 0;
        int maxCombos = availableShelves.Count * availableColors.Count;
        int target = Mathf.Min(count, maxCombos);

        while (CurrentRecipe.Count < target && safety < 500)
        {
            safety++;

            int shelf = availableShelves[Random.Range(0, availableShelves.Count)];
            string color = availableColors[Random.Range(0, availableColors.Count)];

            // Normalisation (minuscule / trim / accents)
            string key = $"{shelf}:{Normalize(color)}";

            if (used.Contains(key))
                continue;

            used.Add(key);

            CurrentRecipe.Add(new RecipeEntry
            {
                shelfID = shelf,
                colorName = color
            });
        }

        if (CurrentRecipe.Count < count)
        {
            Debug.LogWarning($"[PotionRecipeManager] Pool trop petit pour {count} entrées uniques. " +
                             $"Généré: {CurrentRecipe.Count}/{count} (combos max={maxCombos}).");
        }
    }

    void UpdateBoard()
    {
        if (boardText == null) return;
        if (CurrentRecipe == null || CurrentRecipe.Count == 0) return;

        boardText.richText = true;
        boardText.text = "";

        for (int i = 0; i < CurrentRecipe.Count; i++)
        {
            var entry = CurrentRecipe[i];

            Color c = ColorFromName(entry.colorName);
            string hex = ColorUtility.ToHtmlStringRGB(c);

            // Affiche uniquement le numéro d'étagère coloré
            boardText.text += $"<size=160%><b><color=#{hex}>{entry.shelfID}</color></b></size>";

            // Séparateur visuel (tu peux remplacer par + si tu veux)
            if (i < CurrentRecipe.Count - 1)
                boardText.text += "  <size=50%><b> </b></size>  ";
        }

        boardText.ForceMeshUpdate();
    }

    // ====== LOG UTILS ======

    public string RecipeToString()
    {
        // Exemple: 1) 6-Red | 2) 1-Blue | 3) 4-Grey
        var sb = new StringBuilder();
        for (int i = 0; i < CurrentRecipe.Count; i++)
        {
            sb.Append($"{i + 1}) {CurrentRecipe[i].shelfID}-{CurrentRecipe[i].colorName}");
            if (i < CurrentRecipe.Count - 1) sb.Append(" | ");
        }
        return sb.ToString();
    }

    // ----- Couleurs robustes -----

    Color ColorFromName(string name)
    {
        string n = Normalize(name);

        switch (n)
        {
            // FR
            case "rouge": return Color.red;
            case "orange": return new Color(1f, 0.5f, 0f);
            case "vert": return Color.green;
            case "bleu": return Color.blue;
            case "jaune": return Color.yellow;
            case "noir": return Color.black;
            case "gris": return Color.gray;
            case "violet": return new Color(0.6f, 0f, 0.8f);

            // EN
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "yellow": return Color.yellow;
            case "black": return Color.black;
            case "grey": return Color.gray;
            case "gray": return Color.gray;
            case "purple": return new Color(0.6f, 0f, 0.8f);
        }

        Debug.LogWarning($"[PotionRecipeManager] Couleur inconnue: '{name}' (normalisée: '{n}'). Retourne blanc.");
        return Color.white;
    }

    string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim().ToLowerInvariant();

        // enlever accents (é -> e)
        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char ch in formD)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        s = sb.ToString().Normalize(NormalizationForm.FormC);

        return s;
    }

    // Option debug
    string DebugRecipeLine()
    {
        var sb = new StringBuilder("Recipe raw: ");
        for (int i = 0; i < CurrentRecipe.Count; i++)
        {
            sb.Append($"[{CurrentRecipe[i].shelfID}:{CurrentRecipe[i].colorName}] ");
        }
        return sb.ToString();
    }
}
