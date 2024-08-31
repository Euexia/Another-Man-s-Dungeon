using UnityEngine;

[System.Serializable]
public class LootItem
{
    public ItemSO item;  // Utilise ItemSO au lieu de GameObject
    public float dropChance;
}
