using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

[System.Serializable]
public class Quest
{
    public string questName;
    public string description;
    public int targetCount; // Nombre total d'objectifs � atteindre
    public int currentCount; // Nombre actuel d'objectifs atteints
    public bool isActive; // Indique si la qu�te est active
    public bool isComplete; // Indique si la qu�te est compl�te

    public Quest() { }

    public Quest(string name, string desc, int target)
    {
        questName = name;
        description = desc;
        targetCount = target;
        currentCount = 0;
        isActive = false;
        isComplete = false;
    }

    public bool IsComplete()
    {
        return currentCount >= targetCount;
    }
}

public class QuestManager : NetworkBehaviour
{
    public List<Quest> quests = new List<Quest>();
    public Quest currentQuest; // La qu�te en cours

    [SyncVar(hook = nameof(OnCurrentQuestIndexChanged))]
    private int currentQuestIndex = -1; // L'indice de la qu�te en cours

    private Text questDescriptionText;
    private Text questStatusText;

    private void Start()
    {
        if (!isLocalPlayer) return; // Seulement pour l'instance locale du joueur

        // Assurez-vous que le QuestCanvas est d�sactiv� au d�part
        GameObject playerCanva = GameObject.Find("QuestCanva");
        if (playerCanva != null)
        {
            playerCanva.SetActive(false);
        }

        // Trouver et assigner les composants Text du canvas QuestSteps
        GameObject questStepsCanva = GameObject.Find("QuestSteps");
        if (questStepsCanva != null)
        {
            questDescriptionText = questStepsCanva.transform.Find("QuestDescriptionText").GetComponent<Text>();
            questStatusText = questStepsCanva.transform.Find("QuestStatusText").GetComponent<Text>();
        }

        if (quests.Count == 0)
        {
            Debug.LogError("Aucune qu�te disponible. Merci d'assigner des qu�tes dans l'inspecteur.");
        }

        if (quests.Count == 0)
        {
            // Ajouter des qu�tes pour les tests si elles ne sont pas pr�sentes
            quests.Add(new Quest("Ouvrir 1 coffre", "Ouvrir 1 coffre pour valider la qu�te.", 1));
            quests.Add(new Quest("Ouvrir 2 coffres", "Ouvrir 2 coffres pour valider la qu�te.", 2));
            quests.Add(new Quest("Ouvrir 3 coffres", "Ouvrir 3 coffres pour valider la qu�te.", 3));
            quests.Add(new Quest("Vaincre 30 monstres", "Vaincre 30 monstres pour valider la qu�te.", 30));
            quests.Add(new Quest("Vaincre 20 monstres", "Vaincre 20 monstres pour valider la qu�te.", 20));
            quests.Add(new Quest("Vaincre 10 monstres", "Vaincre 10 monstres pour valider la qu�te.", 10));
            quests.Add(new Quest("Envahir le camp adverse", "Envahir le camp adverse en passant le portail.", 1));
        }

        UpdateQuestUI();
    }

    public void StartQuest(int questIndex)
    {
        if (!isLocalPlayer) return; // Seul le joueur local peut d�marrer une qu�te

        CmdStartQuest(questIndex);
    }

    public void UpdateQuestProgress(int amount)
    {
        if (!isLocalPlayer) return; // Only the local player can update quest progress

        CmdUpdateQuestProgress(amount);
    }

    [Command]
    private void CmdStartQuest(int questIndex)
    {
        Debug.Log("CmdStartQuest appel� avec questIndex: " + questIndex);

        if (questIndex < 0 || questIndex >= quests.Count)
        {
            Debug.LogError("Index de qu�te invalide! Quest index: " + questIndex);
            return;
        }

        currentQuestIndex = questIndex;
        quests[currentQuestIndex].isActive = true;

        // Appelle la mise � jour de l'UI c�t� client
        RpcUpdateQuestUI();

        // Active le canvas de la qu�te c�t� client
        RpcActivateQuestUI();
    }

    private void CmdUpdateQuestProgress(int amount)
    {
        if (currentQuestIndex == -1) return;

        Quest currentQuest = quests[currentQuestIndex];
        if (currentQuest.isActive)
        {
            currentQuest.currentCount += amount;
            if (currentQuest.IsComplete())
            {
                currentQuest.isComplete = true;
                currentQuest.isActive = false;
                Debug.Log("Quest completed: " + currentQuest.questName);
            }

            UpdateQuestUI();
        }
    }

    [ClientRpc]
    private void RpcActivateQuestUI()
    {
        GameObject playerCanva = GameObject.Find("QuestCanva");
        if (playerCanva != null)
        {
            playerCanva.SetActive(true); // Active le canvas de la qu�te lorsqu'une qu�te est accept�e
        }

        GameObject RewardCanva = GameObject.Find("RewardDialogue");
        if (RewardCanva != null)
        {
            RewardCanva.SetActive(false); // D�sactive le dialogue de r�compense au d�but
        }

        GameObject questStepsCanva = GameObject.Find("QuestSteps");
        if (questStepsCanva != null)
        {
            questStepsCanva.SetActive(true); // Active le canvas des �tapes de la qu�te
        }
    }

    public void CompleteQuest()
    {
        if (!isLocalPlayer) return; // Seul le joueur local peut terminer une qu�te

        CmdCompleteQuest();
    }

    [Command]
    private void CmdCompleteQuest()
    {
        if (currentQuestIndex == -1) return;

        Quest currentQuest = quests[currentQuestIndex];
        currentQuest.isActive = false;
        currentQuest.isComplete = true;

        // Mise � jour de l'UI et activation du dialogue de r�compense
        RpcCompleteQuestUI();
    }

    [ClientRpc]
    private void RpcCompleteQuestUI()
    {
        UpdateQuestUI(); // Mise � jour de l'UI

        GameObject RewardCanva = GameObject.Find("RewardDialogue");
        if (RewardCanva != null)
        {
            RewardCanva.SetActive(true); // Active le dialogue de r�compense � la fin de la qu�te
        }
    }

    // Cette m�thode met � jour l'interface utilisateur de la qu�te en cours
    [ClientRpc]
    private void RpcUpdateQuestUI()
    {
        UpdateQuestUI();
    }

    private void OnCurrentQuestIndexChanged(int oldIndex, int newIndex)
    {
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        if (questDescriptionText != null && questStatusText != null && currentQuestIndex != -1)
        {
            Quest currentQuest = quests[currentQuestIndex];
            if (currentQuest.isActive)
            {
                questDescriptionText.text = currentQuest.description;
                questStatusText.text = "Quest in progress: " + currentQuest.currentCount + "/" + currentQuest.targetCount;
            }
            else if (currentQuest.isComplete)
            {
                questDescriptionText.text = currentQuest.description;
                questStatusText.text = "Quest complete";
            }
            else
            {
                questDescriptionText.text = " ";
                questStatusText.text = "No active quest";
            }
        }
    }
}
