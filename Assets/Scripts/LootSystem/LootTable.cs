using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Loot System/Loot Table")]
public class LootTable : ScriptableObject
{
    public LootItem[] lootItems;
    public float noDropChance;
}


