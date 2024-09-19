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

    [SerializeField] private AudioClip sound1;
    [SerializeField] private AudioClip sound2;
    private AudioSource audioSource;


    void Start()
    {
        HotbarItemChanged();
        //Cursor.lockState = CursorLockMode.Locked;
        audioSource = GetComponent<AudioSource>(); 

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

    [Command(requiresAuthority = false)]
    private void CmdChangeDamage(int newDamage)
    {
        combatController.damage = newDamage;
    }

    private void HotbarItemChanged()
    {
        for (int i = 0; i < handParent.childCount; i++)
        {
            handParent.GetChild(i).gameObject.SetActive(false);
            handParent.GetChild(i).gameObject.transform.SetParent(null);
        }

        CmdUnequipItem();

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

                    if (itemData is PotionSO)
                    {
                        Debug.Log("Potion selected, waiting for right-click to use.");
                    }
                    else
                    {
                        combatController.weaponType = itemData.type;
                        combatController.isRange = itemData.isRange;
                        CmdChangeDamage(itemData.damage);

                        for (int i = 0; i < playerItems.Count; i++)
                        {
                            GameObject playerItem = playerItems[i];

                            if (playerItem.GetComponent<ItemPickable>().itemScriptableObject.name == hotbarSlots[selectedHotbarSlot].GetComponent<InventorySlot>().HeldItem.GetComponent<InventoryItem>().itemScriptableObject.name)
                            {
                                playerItem.SetActive(true);
                                playerItem.GetComponent<BoxCollider>().isTrigger = true;
                                playerItem.GetComponent<Rigidbody>().useGravity = false;
                                playerItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                                playerItem.transform.parent = handParent.transform;
                                playerItem.transform.position = handParent.transform.position;

                                CmdEquipItem(playerItem);

                                break;
                            }
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
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

        // Vérification si un item est présent dans le slot
        if (slot != null && slot.HeldItem != null)
        {
            // Si c'est un clic gauche
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                draggedObject = slot.HeldItem;
                slot.HeldItem = null;
                lastItemSlot = clickedObject;
            }
            // Si c'est un clic droit
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                InventoryItem item = slot.HeldItem.GetComponent<InventoryItem>();
                if (item != null && item.itemScriptableObject is PotionSO potion)
                {
                    Debug.Log("Right-click detected on potion: " + potion.name);
                    UsePotion(potion, slot.gameObject);
                }
                else
                {
                    Debug.Log("Right-click detected, but item is not a potion.");
                }
            }
        }
        else
        {
            Debug.Log("Clicked object is not a valid inventory slot.");
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
                    CmdDropItem(draggedObject.GetComponent<InventoryItem>().itemScriptableObject.name);

                    lastItemSlot.GetComponent<InventorySlot>().HeldItem = null;
                    Destroy(draggedObject);
                }
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
    void CmdEquipItem(GameObject equippedItem)
    {
        RpcEquipItem(equippedItem);
    }

    [Command]
    void CmdUnequipItem()
    {
        Transform handParent = connectionToClient.identity.gameObject.GetComponent<InventoryManager>().handParent;
        for (int i = 0; i < handParent.childCount; i++)
        {
            handParent.GetChild(i).gameObject.SetActive(false);
            handParent.GetChild(i).gameObject.transform.SetParent(null);

        }

        RpcUnequipItem(connectionToClient.identity);
    }

    [ClientRpc]
    void RpcEquipItem(GameObject equippedItem)
    {
        equippedItem.SetActive(true);
        equippedItem.GetComponent<BoxCollider>().isTrigger = true;
        equippedItem.GetComponent<Rigidbody>().useGravity = false;
        equippedItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        equippedItem.transform.parent = handParent.transform;
        equippedItem.transform.position = handParent.transform.position;
    }

    [ClientRpc]
    void RpcUnequipItem(NetworkIdentity identity)
    {
        Transform handParent = identity.gameObject.GetComponent<InventoryManager>().handParent;
        for (int i = 0; i < handParent.childCount; i++)
        {
            handParent.GetChild(i).gameObject.SetActive(false);
            handParent.GetChild(i).gameObject.transform.SetParent(null);

        }
    }


    [Command]
    void CmdDropItem(string testItem)
    {
        Debug.Log(testItem);

        Vector3 position = gameObject.transform.position + gameObject.transform.forward * 2;

        for (int i = 0; i < playerItems.Count; i++)
        {
            GameObject playerItem = playerItems[i];

            if (playerItem.GetComponent<ItemPickable>().itemScriptableObject.name == testItem)
            {
                playerItem.transform.SetParent(null);
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
        playerItem.transform.SetParent(null);
        playerItem.SetActive(true);
        playerItem.transform.position = position;
        playerItem.GetComponent<BoxCollider>().isTrigger = false;
        playerItem.GetComponent<Rigidbody>().useGravity = true;
        playerItem.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    private void UsePotion(PotionSO potion, GameObject slot)
    {
        Debug.Log("Using potion: " + potion.name);

        // Récupération du script de vie du joueur
        PlayerMovementController player = FindObjectOfType<PlayerMovementController>();
        if (player == null)
        {
            Debug.LogError("PlayerMovementController not found!");
            return;
        }

        // Calcul correct de la nouvelle santé du joueur
        float newHealth = Mathf.Min(player.GetMaxHealth(), player.Health + potion.healAmount);
        Debug.Log("Healing player. New health: " + newHealth);
        player.SetHealth(newHealth);
        player.CmdUsePotion(potion.healAmount);

        // Suppression de la potion de l'inventaire
        Destroy(slot.GetComponent<InventorySlot>().HeldItem);
        slot.GetComponent<InventorySlot>().SetHeldItem(null);

        // Jouer les sons
        StartCoroutine(PlayPotionSounds());

        Debug.Log("Potion consumed and removed from inventory.");
        HotbarItemChanged(); // Rafraîchir la barre de raccourcis
    }

    private IEnumerator PlayPotionSounds()
    {
        // Jouer le premier son
        audioSource.PlayOneShot(sound1);
        // Attendre la fin du premier son avant de jouer le deuxième
        yield return new WaitForSeconds(sound1.length);
        // Jouer le deuxième son
        audioSource.PlayOneShot(sound2);
    }


    private void UseSelectedHotbarItem()
    {
        GameObject selectedSlot = hotbarSlots[selectedHotbarSlot];
        GameObject heldItem = selectedSlot.GetComponent<InventorySlot>().HeldItem;

        if (heldItem != null)
        {
            InventoryItem inventoryItem = heldItem.GetComponent<InventoryItem>();
            if (inventoryItem != null && inventoryItem.itemScriptableObject is PotionSO potion)
            {
                UsePotion(potion, selectedSlot);
            }
            else
            {
                Debug.Log("The selected item is not a potion.");
            }
        }
        else
        {
            Debug.Log("No item selected in the hotbar slot.");
        }
    }

}

