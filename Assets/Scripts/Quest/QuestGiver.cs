using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public GameObject dialoguePanel; // Assign in the Inspector
    public GameObject questSteps; // Assign in the Inspector
    public GameObject rewardDialogue;
    public GameObject npc; // Assign in the Inspector
    public QuestManager questManager; // Assign in the Inspector
    public BoxCollider boxCollider; // Assign in the Inspector

    private bool playerInRange = false;
    private bool questAccepted = false;
    public int questIndex; // L'index de la quête à accepter, assigné dans l'inspecteur

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.A))
        {
            if (questAccepted)
            {
                if (questManager.currentQuest != null && questManager.currentQuest.isComplete)
                {
                    boxCollider.enabled = true;
                }
            }
            else
            {
                AcceptQuest();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !questAccepted)
        {
            Debug.Log("Player entered NPC trigger zone");
            dialoguePanel.SetActive(true);
            playerInRange = true;
        }

        // Vérifie si la quête est complète lorsque le joueur entre en collision avec le NPC
        if (other.CompareTag("Player") && questAccepted && questManager.currentQuest != null && questManager.currentQuest.isComplete)
        {
            // Activer le canvas de récompense
            rewardDialogue.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited NPC trigger zone");
            dialoguePanel.SetActive(false);
            playerInRange = false;
        }
    }

    public void AcceptQuest()
    {
        Debug.Log("Quest accepted");

        if (dialoguePanel == null)
        {
            Debug.LogError("dialoguePanel is not assigned!");
            return;
        }

        if (questSteps == null)
        {
            Debug.LogError("questSteps is not assigned!");
            return;
        }

        if (questManager == null)
        {
            Debug.LogError("questManager is not assigned!");
            return;
        }

        if (questIndex < 0 || questIndex >= questManager.quests.Count)
        {
            Debug.LogError("Invalid quest index! Quest index: " + questIndex);
            return;
        }

        dialoguePanel.SetActive(false);
        questSteps.SetActive(true);
        questManager.StartQuest(questIndex); // Passer l'index de la quête
        questAccepted = true;

        // Désactiver le BoxCollider du NPC
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
        else
        {
            Debug.LogError("boxCollider is not assigned!");
        }

        // Mettre à jour l'interface utilisateur de suivi de quête
        questManager.UpdateQuestUI();
    }

    public void ClaimReward()
    {
        Debug.Log("Reward claimed");

        // Hide all quest-related UI elements
        dialoguePanel.SetActive(false);
        questSteps.SetActive(false);
        rewardDialogue.SetActive(false);

        // Hide NPC interaction
        boxCollider.enabled = false;
        npc.SetActive(false);
    }
}
