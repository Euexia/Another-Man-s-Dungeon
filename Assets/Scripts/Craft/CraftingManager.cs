using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private List<CraftingRecipe> recipes;
    [SerializeField] private List<CraftingSlot> craftingSlots; 
    [SerializeField] private GameObject resultSlot;

    private bool resultTaken = false; 

    private void Start()
    {
        foreach (CraftingSlot slot in craftingSlots)
        {
            slot.OnSlotChanged += CheckCraftingConditions;
        }

        var resultSlotComponent = resultSlot.GetComponent<InventorySlot>();
        if (resultSlotComponent != null)
        {
            resultSlotComponent.OnSlotChanged += OnResultSlotChanged;
            Debug.Log("Attached OnSlotChanged to result slot");
        }
    }

    private void CheckCraftingConditions()
    {
        foreach (CraftingRecipe recipe in recipes)
        {
            if (CanCraft(recipe))
            {
                Craft(recipe);
                return; 
            }
        }
    }

    private bool CanCraft(CraftingRecipe recipe)
    {
        Dictionary<ItemSO, int> ingredientCount = new Dictionary<ItemSO, int>();

        foreach (ItemSO ingredient in recipe.ingredients)
        {
            if (!ingredientCount.ContainsKey(ingredient))
            {
                ingredientCount[ingredient] = 0;
            }
            ingredientCount[ingredient]++;
        }

        foreach (ItemSO ingredient in ingredientCount.Keys)
        {
            int totalCount = 0;
            foreach (CraftingSlot slot in craftingSlots)
            {
                if (slot.HeldItem != null)
                {
                    InventoryItem item = slot.HeldItem.GetComponent<InventoryItem>();
                    if (item.itemScriptableObject == ingredient)
                    {
                        totalCount += item.stackCurrent;
                    }
                }
            }

            if (totalCount < ingredientCount[ingredient])
            {
                return false; 
            }
        }

        return true;
    }

    public void AttemptCraft()
    {
        CheckCraftingConditions();
    }

    private void Craft(CraftingRecipe recipe)
    {
      
        Dictionary<ItemSO, int> ingredientsToRemove = new Dictionary<ItemSO, int>();

        foreach (ItemSO ingredient in recipe.ingredients)
        {
            if (!ingredientsToRemove.ContainsKey(ingredient))
            {
                ingredientsToRemove[ingredient] = 0;
            }
            ingredientsToRemove[ingredient]++;
        }

        foreach (ItemSO ingredient in ingredientsToRemove.Keys)
        {
            int amountToRemove = ingredientsToRemove[ingredient];
            foreach (CraftingSlot slot in craftingSlots)
            {
                if (slot.HeldItem != null)
                {
                    InventoryItem item = slot.HeldItem.GetComponent<InventoryItem>();
                    if (item.itemScriptableObject == ingredient)
                    {
                        int amountToTake = Mathf.Min(item.stackCurrent, amountToRemove);
                        amountToRemove -= amountToTake;
                        item.stackCurrent -= amountToTake;
                        if (item.stackCurrent <= 0)
                        {
                            slot.ClearSlot();
                        }

                        if (amountToRemove <= 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        GameObject resultItem = Instantiate(inventoryManager.ItemPrefab);
        resultItem.GetComponent<InventoryItem>().itemScriptableObject = recipe.result;
        resultItem.GetComponent<InventoryItem>().stackCurrent = 1;

        if (resultSlot != null)
        {
            var resultSlotComponent = resultSlot.GetComponent<InventorySlot>();
            if (resultSlotComponent != null)
            {
                resultSlotComponent.SetHeldItem(resultItem);
                resultItem.transform.SetParent(resultSlot.transform);
                resultItem.transform.localScale = Vector3.one;
                Debug.Log("Result item set in result slot");

                resultTaken = false; 
            }
        }
    }

    private void OnResultSlotChanged()
    {
        var resultSlotComponent = resultSlot.GetComponent<InventorySlot>();
        if (resultSlotComponent != null)
        {

            Debug.Log("Result slot changed");

            if (resultSlotComponent.IsEmpty())
            {
                if (!resultTaken)
                {
                    resultTaken = true;
                    Debug.Log("Result item taken, clearing crafting slots");
                    foreach (CraftingSlot slot in craftingSlots)
                    {
                        slot.ClearSlot();
                    }
                }
            }
        }
    }
}
