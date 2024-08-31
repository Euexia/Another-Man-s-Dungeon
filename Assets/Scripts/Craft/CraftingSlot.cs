using UnityEngine;

public class CraftingSlot : MonoBehaviour
{
    public GameObject HeldItem;

    public delegate void SlotChanged();
    public event SlotChanged OnSlotChanged;

    public void SetHeldItem(GameObject item)
    {
        HeldItem = item;
        if (item != null)
        {
            HeldItem.transform.position = transform.position;
            HeldItem.transform.SetParent(transform);
        }
        OnSlotChanged?.Invoke();
    }

    public void ClearSlot()
    {
        if (HeldItem != null)
        {
            Destroy(HeldItem);
        }
        HeldItem = null;
        Debug.Log("CraftingSlot: Cleared");
        OnSlotChanged?.Invoke();
    }

    public bool IsEmpty()
    {
        return HeldItem == null;
    }
}
