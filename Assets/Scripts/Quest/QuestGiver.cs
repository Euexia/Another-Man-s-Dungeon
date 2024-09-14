using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public class QuestGiver : NetworkBehaviour
{
    public GameObject npc; // Assign in the Inspector
    public QuestManager questManager; // Assign in the Inspector
    public BoxCollider boxCollider; // Assign in the Inspector

    [Header("Reward Settings")]
    public List<RandomSpawningItems> rewardItemsList; // Liste de ScriptableObjects � assigner dans l'inspecteur

    private bool playerInRange = false;

    [SyncVar(hook = nameof(OnQuestAcceptedChanged))]
    private bool questAccepted = false;

    private int questIndex; // L'index de la qu�te sera maintenant s�lectionn� al�atoirement

    private Button acceptQuestButton; // R�f�rence au bouton dans le Canvas du joueur

    private void Start()
    {

    }


    private void Update()
    {
        if (!isLocalPlayer || acceptQuestButton == null) return;
        if (questAccepted)
        {
            // Si la qu�te est termin�e, activer le collider pour la r�compense
            if (questManager.currentQuest != null && questManager.currentQuest.isComplete)
                {
                    CmdEnableCollider();
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
        if (other.CompareTag("Player") && !questAccepted)
        {
            Debug.Log("Player entered NPC trigger zone");
            other.transform.Find("PlayerGui").Find("DialoguePanel").gameObject.SetActive(true);
            playerInRange = true;
            GameObject acceptQuestButton = other.transform.Find("PlayerGui").Find("DialoguePanel").Find("AcceptButton").gameObject;

            // Activer le bouton d'acceptation de qu�te
            if (acceptQuestButton != null)
            {
                acceptQuestButton.gameObject.SetActive(true);
                Debug.Log("AcceptButton activated");
                acceptQuestButton.GetComponent<Button>().onClick.AddListener(CmdAcceptQuest); // Lier la m�thode CmdAcceptQuest au clic du bouton
                Debug.Log("AcceptButton activated2");
            }
        }

        if (other.CompareTag("Player") && questAccepted && questManager.currentQuest != null && questManager.currentQuest.isComplete)
        {
            other.transform.Find("PlayerGui").Find("RewardDialogue").gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !questAccepted)
        {
            Debug.Log("Player exited NPC trigger zone");
            other.transform.Find("PlayerGui").Find("DialoguePanel").gameObject.SetActive(false);
            playerInRange = false;
            GameObject acceptQuestButton = other.transform.Find("PlayerGui").Find("DialoguePanel").Find("AcceptButton").gameObject;

            // D�sactiver le bouton d'acceptation de qu�te
            if (acceptQuestButton != null)
            {
                acceptQuestButton.gameObject.SetActive(false);
                acceptQuestButton.GetComponent<Button>().onClick.RemoveListener(CmdAcceptQuest); // Supprimer la m�thode du bouton pour �viter les duplications
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAcceptQuest()
    {
        Debug.Log("CmdAcceptQuest called");
        GameObject acceptQuestButton = connectionToClient.identity.transform.Find("PlayerGui").Find("DialoguePanel").Find("AcceptButton").gameObject;

        // Si le joueur ne poss�de pas la connexion sur cet objet
        if (connectionToClient == null)
        {
            Debug.LogError("No connection to client!");
            return;
        }

        // V�rifie si le questManager est bien assign�
        if (questManager == null)
        {
            Debug.LogError("questManager is not assigned!");
            return;
        }

        // V�rifie si des qu�tes sont disponibles
        if (questManager.quests.Count == 0)
        {
            Debug.LogError("No quests available in questManager!");
            return;
        }

        // S�lectionner un questIndex al�atoire
        questIndex = Random.Range(0, questManager.quests.Count);

        // Marquer la qu�te comme accept�e
        questAccepted = true;

        // D�marrer la qu�te dans le QuestManager
        questManager.StartQuest(questIndex);

        // Mettre � jour l'interface utilisateur de la qu�te
        RpcUpdateQuestUI(connectionToClient);

        // D�sactiver le bouton apr�s avoir accept� la qu�te
        if (acceptQuestButton != null)
        {
            acceptQuestButton.gameObject.SetActive(false);
        }
    }




    private void OnQuestAcceptedChanged(bool oldQuestAccepted, bool newQuestAccepted)
    {
        if (newQuestAccepted)
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }
    }

    [TargetRpc]
    private void RpcUpdateQuestUI(NetworkConnectionToClient player)
    {
        Debug.Log("RpcUpdateQuestUI called");
        questManager.UpdateQuestUI();
    }


    [Command]
    public void CmdClaimReward()
    {
        Debug.Log("Reward claimed");

        if (questManager.currentQuest != null && questManager.currentQuest.isComplete && rewardItemsList != null && rewardItemsList.Count > 0)
        {
            // S�lectionner un ScriptableObject al�atoire de la liste
            int randomSOIndex = Random.Range(0, rewardItemsList.Count);
            RandomSpawningItems selectedRewardItems = rewardItemsList[randomSOIndex];

            if (selectedRewardItems.itemsToSpawn.Count > 0)
            {
                // S�lectionner un objet al�atoire dans le ScriptableObject s�lectionn�
                int randomItemIndex = Random.Range(0, selectedRewardItems.itemsToSpawn.Count);
                ItemSO randomItemSO = selectedRewardItems.itemsToSpawn[randomItemIndex];

                if (randomItemSO.prefab != null)
                {
                    GameObject weapon = Instantiate(randomItemSO.prefab, transform.position + transform.forward * 2, Quaternion.identity);
                    NetworkServer.Spawn(weapon);
                }
            }
        }

        RpcHideQuestUI(connectionToClient);

        if (boxCollider != null) boxCollider.enabled = false;
        if (npc != null) npc.SetActive(false);
    }

    [TargetRpc]
    private void RpcHideQuestUI(NetworkConnectionToClient player)
    {
        GameObject playerObject = player.identity.gameObject;
        playerObject.transform.Find("PlayerGui").Find("DialoguePanel").gameObject.SetActive(false);
        playerObject.transform.Find("PlayerGui").Find("QuestSteps").gameObject.SetActive(false);
        playerObject.transform.Find("PlayerGui").Find("RewardDialogue").gameObject.SetActive(false);
    }
}
