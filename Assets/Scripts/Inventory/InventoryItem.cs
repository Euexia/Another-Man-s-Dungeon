using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public ItemSO itemScriptableObject;

    [SerializeField] Image iconImage;
    [SerializeField] Text stackText;

    public int stackCurrent = 1;
    public int stackMax;

    private void Start()
    {
        if (itemScriptableObject == null) return;
        stackMax = itemScriptableObject.stackMax;
    }

    void Update()
    {
        if (itemScriptableObject != null)
        {
            iconImage.sprite = Resources.Load<Sprite>("WeaponImage/" + itemScriptableObject.prefab.name);
        }

        if (stackMax > 1)
        {
            stackText.text = stackCurrent.ToString();
        } else
        {
            stackText.text = "";
        }
    }
}
