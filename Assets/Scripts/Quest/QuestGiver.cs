using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class QuestGiver : NetworkBehaviour
{
    public GameObject dialoguePanel; // Assign in the Inspector
    public GameObject questSteps; // Assign in the Inspector
    public GameObject rewardDialogue; // Assign in the Inspector
    public GameObject npc; // Assign in the Inspector
    public QuestManager questManager; // Assign in the Inspector
    public BoxCollider boxCollider; // Assign in the Inspector

    [Header("Reward Settings")]
    public List<RandomSpawningItems> rewardItemsList; // Liste de ScriptableObjects à assigner dans l'inspecteur

    private bool playerInRange = false;

    [SyncVar(hook = nameof(OnQuestAcceptedChanged))]
    private bool questAccepted = false;

    private int questIndex; // L'index de la quête sera maintenant sélectionné aléatoirement

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (playerInRange && Input.GetKeyDown(KeyCode.A))
        {
            if (questAccepted)
            {
                if (questManager.currentQuest != null && questManager.currentQuest.isComplete)
                {
                    CmdEnableCollider();
                }
            }
            else
            {
                CmdAcceptQuest();
            }
        }
    }

    [Command]
    private void CmdEnableCollider()
    {
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("Player") && !questAccepted)
        {
            Debug.Log("Player entered NPC trigger zone");
            dialoguePanel.SetActive(true);
            playerInRange = true;
        }

        if (other.CompareTag("Player") && questAccepted && questManager.currentQuest != null && questManager.currentQuest.isComplete)
        {
            rewardDialogue.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited NPC trigger zone");
            dialoguePanel.SetActive(false);
            playerInRange = false;
        }
    }

    [Command]
    public void CmdAcceptQuest()
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

        if (questManager.quests.Count == 0)
        {
            Debug.LogError("No quests available in questManager!");
            return;
        }

        // Sélectionner un questIndex aléatoire
        questIndex = Random.Range(0, questManager.quests.Count);

        questAccepted = true;
        questManager.StartQuest(questIndex);
        RpcUpdateQuestUI();
    }

    private void OnQuestAcceptedChanged(bool oldQuestAccepted, bool newQuestAccepted)
    {
        if (newQuestAccepted)
        {
            dialoguePanel.SetActive(false);
            questSteps.SetActive(true);

            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }
    }

    [ClientRpc]
    private void RpcUpdateQuestUI()
    {
        questManager.UpdateQuestUI();
    }

    [Command]
    public void CmdClaimReward()
    {
        Debug.Log("Reward claimed");

        if (questManager.currentQuest != null && questManager.currentQuest.isComplete && rewardItemsList != null && rewardItemsList.Count > 0)
        {
            // Sélectionner un ScriptableObject aléatoire de la liste
            int randomSOIndex = Random.Range(0, rewardItemsList.Count);
            RandomSpawningItems selectedRewardItems = rewardItemsList[randomSOIndex];

            if (selectedRewardItems.itemsToSpawn.Count > 0)
            {
                // Sélectionner un objet aléatoire dans le ScriptableObject sélectionné
                int randomItemIndex = Random.Range(0, selectedRewardItems.itemsToSpawn.Count);
                ItemSO randomItemSO = selectedRewardItems.itemsToSpawn[randomItemIndex];

                if (randomItemSO.prefab != null)
                {
                    GameObject weapon = Instantiate(randomItemSO.prefab, transform.position + transform.forward * 2, Quaternion.identity);
                    NetworkServer.Spawn(weapon);
                }
            }
        }

        RpcHideQuestUI();

        if (boxCollider != null) boxCollider.enabled = false;
        if (npc != null) npc.SetActive(false);
    }

    [ClientRpc]
    private void RpcHideQuestUI()
    {
        dialoguePanel.SetActive(false);
        questSteps.SetActive(false);
        rewardDialogue.SetActive(false);
    }
}
