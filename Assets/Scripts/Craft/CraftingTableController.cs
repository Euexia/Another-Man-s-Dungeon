using UnityEngine;

public class CraftingTableController : MonoBehaviour
{
    [SerializeField] private GameObject craftingPanel; // Référence au panel de crafting
    [SerializeField] private Transform player; // Référence à la position du joueur
    [SerializeField] private float interactionDistance = 3f; // Distance maximale pour l'interaction

    private void Start()
    {
        // Désactiver le panel de crafting au début du jeu
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpenCraftingPanel();
        }
    }

    private void TryOpenCraftingPanel()
    {
        // Vérifiez si le joueur est à proximité de la table de craft
        if (Vector3.Distance(player.position, transform.position) <= interactionDistance)
        {
            ToggleCraftingPanel();
        }
    }

    private void ToggleCraftingPanel()
    {
        if (craftingPanel != null)
        {
            bool isActive = craftingPanel.activeSelf;
            craftingPanel.SetActive(!isActive);
            Debug.Log($"Crafting panel is now {(isActive ? "closed" : "open")}");
        }
        else
        {
            Debug.LogWarning("Crafting panel reference is not assigned.");
        }
    }
}
