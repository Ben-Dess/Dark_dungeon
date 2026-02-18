using UnityEngine;

[CreateAssetMenu(menuName = "Potions/Potion Definition")]
public class PotionDefinition : ScriptableObject
{
    public string potionName;
    public Color potionColor = Color.white;
}
