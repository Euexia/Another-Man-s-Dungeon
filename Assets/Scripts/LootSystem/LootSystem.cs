using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    public static LootSystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateLoot(LootTable lootTable, Vector3 position)
    {
        if (lootTable == null)
        {
            Debug.LogError("LootTable is null.");
            return;
        }

        List<LootItem> selectedLootItems = SelectLootItems(lootTable);

        if (selectedLootItems.Count == 0)
        {
            Debug.Log("No loot was selected.");
        }
        else
        {
            foreach (LootItem selectedLootItem in selectedLootItems)
            {
                Debug.Log("Dropping loot: " + selectedLootItem.item.name);

                if (selectedLootItem.item != null)
                {
                    GameObject loot = Instantiate(selectedLootItem.item.prefab, position, Quaternion.identity);
                    Rigidbody rb = loot.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = loot.AddComponent<Rigidbody>();
                    }
                    rb.isKinematic = false;
                    rb.detectCollisions = true;

                    NetworkServer.Spawn(loot);
                }
            }
        }
    }




    private List<LootItem> SelectLootItems(LootTable lootTable)
    {
        List<LootItem> selectedLootItems = new List<LootItem>();

        // Probabilité de ne pas dropper (en pourcentage)
        float noDropProbability = lootTable.noDropChance / 100f;
        Debug.Log("No Drop Probability: " + noDropProbability);

        // Comparaison avec une valeur aléatoire entre 0 et 1
        if (Random.value < noDropProbability)
        {
            Debug.Log("No loot selected due to no drop probability.");
            return selectedLootItems;
        }

        foreach (LootItem item in lootTable.lootItems)
        {
            // Probabilité de drop de l'item (en pourcentage)
            float itemDropProbability = item.dropChance / 100f;
            Debug.Log("Checking item: " + item.item.name + " Drop Chance: " + itemDropProbability);

            // Comparaison avec une valeur aléatoire entre 0 et 1
            if (Random.value < itemDropProbability)
            {
                Debug.Log("Loot selected: " + item.item.name);
                selectedLootItems.Add(item);
            }
        }

        return selectedLootItems;
    }


}
