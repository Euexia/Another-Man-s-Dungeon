using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject HeldItem;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    public delegate void SlotChanged();
    public event SlotChanged OnSlotChanged;

    public void SetHeldItem(GameObject item)
    {
        HeldItem = item;
        if (item != null)
        {
            HeldItem.transform.position = transform.position;
            HeldItem.transform.SetParent(transform);
            HeldItem.transform.localScale = Vector3.one;
        }
        //Debug.Log("InventorySlot: Item set or changed");
        OnSlotChanged?.Invoke();
    }

    public void ClearSlot()
    {
        if (HeldItem != null)
        {
            Destroy(HeldItem);
        }
        HeldItem = null;
        Debug.Log("InventorySlot: Cleared");
        OnSlotChanged?.Invoke();
    }

    public bool IsEmpty()
    {
        return HeldItem == null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = Color.yellow;
        GetComponent<AudioSource>().PlayOneShot(hoverSound);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().color = Color.white;
    }
}
