using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class InventoryManager : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public bool isStorageOpened;

    [SerializeField] private CombatController combatController;

    [SerializeField] private GameObject[] hotbarSlots = new GameObject[3];

    [SerializeField] private GameObject[] slots = new GameObject[20];

    [SyncVar]
    private List<GameObject> playerItems = new List<GameObject>();

    [SerializeField] private GameObject inventoryParent;
    [SerializeField] private GameObject storageParent;
    [SerializeField] private Transform handParent;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Camera cam;

    public GameObject[] Slots => slots;
    public GameObject ItemPrefab => itemPrefab;

    GameObject draggedObject;
    GameObject lastItemSlot;

    Storage lastStorage;

    bool isInventoryOpened;

    int selectedHotbarSlot = 0;
    void Start()
    {
        HotbarItemChanged();
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        CheckForHotbarInput();

        storageParent.SetActive(isStorageOpened);
        inventoryParent.SetActive(isInventoryOpened);

        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isInventoryOpened)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                isInventoryOpened = false;
                isStorageOpened = false;
            }
            else
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                isInventoryOpened = true;
            }
        }
    }

    private void CheckForHotbarInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedHotbarSlot = 0;
            HotbarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedHotbarSlot = 1;
            HotbarItemChanged();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedHotbarSlot = 2;
            HotbarItemChanged();
        }
    }

    private void HotbarItemChanged()
    {
        for (int i = 0; i < handParent.childCount; i++)
        {
            handParent.GetChild(i).gameObject.SetActive(false);
        }

        foreach (GameObject slot in hotbarSlots)
        {
            Vector3 scale;

            if (slot == hotbarSlots[selectedHotbarSlot])
            {
                scale = new Vector3(1.2f, 1.2f, 1.2f);

                GameObject heldItem = slot.GetComponent<InventorySlot>().HeldItem;

                if (heldItem != null)
                {
                    ItemSO itemData = heldItem.GetComponent<InventoryItem>().itemScriptableObject;
                    combatController.weaponType = itemData.type;
                    combatController.isRange = itemData.isRange;
                    Debug.Log(combatController.weaponType);

                    for (int i = 0; i < handParent.childCount; i++)
                    {
                        if (handParent.GetChild(i).GetComponent<ItemHand>().itemScriptableObject
                            == hotbarSlots[selectedHotbarSlot].GetComponent<InventorySlot>().HeldItem.GetComponent<InventoryItem>().itemScriptableObject)
                        {
                            handParent.GetChild(i).gameObject.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                scale = new Vector3(1f, 1f, 1f);
            }

            slot.transform.localScale = scale;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            //There is item in the slot - pick it up
            if (slot != null && slot.HeldItem != null)
            {
                draggedObject = slot.HeldItem;
                slot.HeldItem = null;
                lastItemSlot = clickedObject;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (draggedObject != null && eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
                InventorySlot inventorySlot = clickedObject.GetComponent<InventorySlot>();
                CraftingSlot craftingSlot = clickedObject.GetComponent<CraftingSlot>();

                // Si on relâche l'item sur un slot de crafting vide
                if (craftingSlot != null && craftingSlot.IsEmpty())
                {
                    craftingSlot.SetHeldItem(draggedObject);
                    draggedObject.transform.SetParent(craftingSlot.transform);
                }
                // Si on relâche l'item sur un slot d'inventaire vide
                else if (inventorySlot != null && inventorySlot.IsEmpty())
                {
                    inventorySlot.SetHeldItem(draggedObject);
                    draggedObject.transform.SetParent(inventorySlot.transform);
                }
                // Si on relâche l'item sur un slot d'inventaire qui contient déjà un item
                else if (inventorySlot != null && !inventorySlot.IsEmpty())
                {
                    lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(inventorySlot.HeldItem);
                    inventorySlot.HeldItem.transform.SetParent(lastItemSlot.transform);

                    inventorySlot.SetHeldItem(draggedObject);
                    draggedObject.transform.SetParent(inventorySlot.transform);
                }
                // Si on relâche l'item sur un slot de crafting qui contient déjà un item
                else if (craftingSlot != null && !craftingSlot.IsEmpty())
                {
                    lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(craftingSlot.HeldItem);
                    craftingSlot.HeldItem.transform.SetParent(craftingSlot.transform.parent);

                    craftingSlot.SetHeldItem(draggedObject);
                    draggedObject.transform.SetParent(craftingSlot.transform);
                }
                // Si on relâche l'item ailleurs (retourne l'item au dernier slot)
                else
                {
                    lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
                    draggedObject.transform.SetParent(lastItemSlot.transform);
                }

                draggedObject.transform.rotation = draggedObject.transform.rotation;
            }
            else
            {
                CmdDropItem(draggedObject.GetComponent<InventoryItem>().itemScriptableObject.name);

                lastItemSlot.GetComponent<InventorySlot>().HeldItem = null;
                Destroy(draggedObject);
            }

            draggedObject = null;
            HotbarItemChanged();
        }
    }


    public void ItemPicked(GameObject pickedItem)
    {
        if (!isClient) return;

        GameObject emptySlot = null;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            InventorySlot slot = hotbarSlots[i].GetComponent<InventorySlot>();

            if (slot.HeldItem == null)
            {
                emptySlot = hotbarSlots[i];
                break;
            }
        }

        if (emptySlot == null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                InventorySlot slot = slots[i].GetComponent<InventorySlot>();

                if (slot.HeldItem == null)
                {
                    emptySlot = slots[i];
                    break;
                }
            }
        }



        if (emptySlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab);
            newItem.GetComponent<InventoryItem>().itemScriptableObject = pickedItem.GetComponent<ItemPickable>().itemScriptableObject;
            newItem.transform.SetParent(emptySlot.transform);
            newItem.GetComponent<InventoryItem>().stackCurrent = 1;

            emptySlot.GetComponent<InventorySlot>().SetHeldItem(newItem);
            newItem.transform.localScale = new Vector3(1, 1, 1);

            CmdPickItem(pickedItem.GetComponent<NetworkIdentity>());

            pickedItem.SetActive(false);
        }

        HotbarItemChanged();
    }

    public void OnCraftButtonPressed()
    {
        CraftingManager craftingManager = FindObjectOfType<CraftingManager>();
        if (craftingManager != null)
        {
            craftingManager.AttemptCraft();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdPickItem(NetworkIdentity itemToRemoveId)
    {
        if (itemToRemoveId.GetComponent<ItemPickable>().isPicked) return;

        itemToRemoveId.GetComponent<ItemPickable>().isPicked = true;

        playerItems.Add(itemToRemoveId.gameObject);

        RpcPickItem(itemToRemoveId.gameObject);

        Debug.Log(itemToRemoveId.gameObject.GetComponent<ItemPickable>().itemScriptableObject);

        itemToRemoveId.gameObject.SetActive(false);

        //NetworkServer.UnSpawn(itemToRemoveId.gameObject);
    }

    [ClientRpc]
    void RpcPickItem(GameObject pickedItem)
    {
        pickedItem.SetActive(false);

        /*        GameObject newItem = Instantiate(itemPrefab);
                newItem.GetComponent<InventoryItem>().itemScriptableObject = pickedItem.GetComponent<ItemPickable>().itemScriptableObject;
                newItem.transform.SetParent(lastSlot.transform);
                newItem.GetComponent<InventoryItem>().stackCurrent = 1;
                newItem.transform.localScale = new Vector3(1, 1, 1);*/

        Debug.Log(pickedItem.GetComponent<ItemPickable>().itemScriptableObject.prefab);
    }

    [Command]
    void CmdDropItem(string testItem)
    {
        Debug.Log(testItem);

        Vector3 position = gameObject.transform.position + gameObject.transform.forward * 2;

        for (int i = 0; i < playerItems.Count; i++)
        {
            GameObject playerItem = playerItems[i];
            Debug.Log("itering: " + playerItem);

            if (playerItem.GetComponent<ItemPickable>().itemScriptableObject.name == testItem)
            {
                Debug.Log("trouve");

                playerItem.SetActive(true);
                playerItem.transform.position = position;

                playerItem.GetComponent<ItemPickable>().isPicked = false;

                //NetworkServer.Spawn(playerItem);

                playerItems.RemoveAt(i);

                RpcDropItem(playerItem, position);

                break;
            }
        }
    }

    [ClientRpc]
    void RpcDropItem(GameObject playerItem, Vector3 position)
    {
        playerItem.SetActive(true);
        playerItem.transform.position = position;
    }
}

