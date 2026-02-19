using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

using System; // <-- ajoute ça en haut du fichier

public class PotionRecipeManager : NetworkBehaviour
{
    [System.Serializable]
    public struct RecipeEntry
    {
        public int shelfID;
        public string colorName;
    }


public struct RecipeEntryNet : INetworkSerializable, IEquatable<RecipeEntryNet>
{
    public int shelfID;
    public int colorIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref shelfID);
        serializer.SerializeValue(ref colorIndex);
    }

    public bool Equals(RecipeEntryNet other)
        => shelfID == other.shelfID && colorIndex == other.colorIndex;

    public override bool Equals(object obj)
        => obj is RecipeEntryNet other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(shelfID, colorIndex);
}


    private NetworkList<RecipeEntryNet> networkRecipe = new();

    [Header("Pools")]
    public List<int> availableShelves = new() { 1, 2, 3, 4, 5, 6, 7 };

    public List<string> availableColors = new()
    {
        "Red", "Orange", "Green", "Blue", "Yellow", "Black", "Grey", "Purple"
    };

    [Header("UI")]
    public TMP_Text boardText;

    [Header("Recipe")]
    public int recipeCount = 3;

    // ON GARDE EXACTEMENT CE NOM
    public List<RecipeEntry> CurrentRecipe { get; private set; } = new();

    void Awake()
    {
        networkRecipe.OnListChanged += _ =>
        {
            RebuildLocalRecipeFromNetwork();
            UpdateBoard();
        };
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateRecipe(recipeCount);
        }
        else
        {
            RebuildLocalRecipeFromNetwork();
            UpdateBoard();
        }
    }

    // ==============================
    // SERVER GENERATION
    // ==============================
    public void GenerateRecipe(int count)
    {
        if (!IsServer) return;

        networkRecipe.Clear();
        CurrentRecipe.Clear();

        if (availableShelves == null || availableShelves.Count == 0) return;
        if (availableColors == null || availableColors.Count == 0) return;

        var used = new HashSet<string>();

        int safety = 0;
        int maxCombos = availableShelves.Count * availableColors.Count;
        int target = Mathf.Min(count, maxCombos);

        while (networkRecipe.Count < target && safety < 500)
        {
            safety++;

            int shelf = availableShelves[UnityEngine.Random.Range(0, availableShelves.Count)];
            int colorIndex = UnityEngine.Random.Range(0, availableColors.Count);

            string key = $"{shelf}:{colorIndex}";
            if (used.Contains(key))
                continue;

            used.Add(key);

            networkRecipe.Add(new RecipeEntryNet
            {
                shelfID = shelf,
                colorIndex = colorIndex
            });
        }

        Debug.Log("[Recipe] " + RecipeToString());
    }

    // ==============================
    // CLIENT REBUILD
    // ==============================
    void RebuildLocalRecipeFromNetwork()
    {
        CurrentRecipe.Clear();

        foreach (var entry in networkRecipe)
        {
            CurrentRecipe.Add(new RecipeEntry
            {
                shelfID = entry.shelfID,
                colorName = availableColors[entry.colorIndex]
            });
        }
    }

    // ==============================
    // UI
    // ==============================
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

            boardText.text += $"<size=160%><b><color=#{hex}>{entry.shelfID}</color></b></size>";

            if (i < CurrentRecipe.Count - 1)
                boardText.text += "  ";
        }

        boardText.ForceMeshUpdate();
    }

    // ==============================
    // DEBUG
    // ==============================
    public string RecipeToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < CurrentRecipe.Count; i++)
        {
            sb.Append($"{i + 1}) {CurrentRecipe[i].shelfID}-{CurrentRecipe[i].colorName}");
            if (i < CurrentRecipe.Count - 1) sb.Append(" | ");
        }
        return sb.ToString();
    }

    // ==============================
    // COLORS (inchangé)
    // ==============================
    Color ColorFromName(string name)
    {
        string n = name.ToLowerInvariant();

        switch (n)
        {
            case "red": return Color.red;
            case "orange": return new Color(1f, 0.5f, 0f);
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "yellow": return Color.yellow;
            case "black": return Color.black;
            case "grey": return Color.gray;
            case "gray": return Color.gray;
            case "purple": return new Color(0.6f, 0f, 0.8f);
        }

        return Color.white;
    }
}
