using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

[System.Serializable]
public class Quest
{
    public string questName;
    public string description;
    public int targetCount; // Nombre total d'objectifs à atteindre
    public int currentCount; // Nombre actuel d'objectifs atteints
    public bool isActive; // Indique si la quête est active
    public bool isComplete; // Indique si la quête est complète

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
    public Text questDescriptionText; // Assign in the Inspector
    public Text questStatusText; // Assign in the Inspector

    public List<Quest> quests = new List<Quest>();
    public Quest currentQuest; // La quête en cours

    [SyncVar(hook = nameof(OnCurrentQuestIndexChanged))]
    private int currentQuestIndex = -1; // L'indice de la quête en cours

    private void Start()
    {
        if (!isLocalPlayer) return; // Only run on the local player instance

        if (questDescriptionText == null || questStatusText == null)
        {
            Debug.LogError("Please assign the Text components in the Inspector.");
            return;
        }

        if (quests.Count == 0)
        {
            Debug.LogError("No quests available. Please assign quests in the Inspector.");
        }

        if (quests.Count == 0)
        {
            quests.Add(new Quest("Ouvir 1 coffres", "Ouvrir 1 coffres pour valider la quête.", 1));
            quests.Add(new Quest("Ouvir 2 coffres", "Ouvrir 2 coffres pour valider la quête.", 2));
            quests.Add(new Quest("Ouvir 3 coffres", "Ouvrir 3 coffres pour valider la quête.", 3));
            quests.Add(new Quest("Vaincre 30 monstres", "Vaincre 30 monstres pour valider la quête.", 30));
            quests.Add(new Quest("Vaincre 20 monstres", "Vaincre 20 monstres pour valider la quête.", 20));
            quests.Add(new Quest("Vaincre 10 monstres", "Vaincre 10 monstres pour valider la quête.", 10));
            quests.Add(new Quest("Envahir le camp adverse", "Envahir le camp adverse en passant le portail.", 1));
        }

        UpdateQuestUI();
    }

    public void StartQuest(int questIndex)
    {
        if (!isLocalPlayer) return; // Only the local player can request to start a quest

        CmdStartQuest(questIndex);
    }

    [Command]
    private void CmdStartQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Count)
        {
            Debug.LogError("Invalid quest index! Quest index: " + questIndex);
            return;
        }

        currentQuestIndex = questIndex;
        quests[currentQuestIndex].isActive = true;

        UpdateQuestUI();
    }

    public void CompleteQuest()
    {
        if (!isLocalPlayer) return; // Only the local player can request to complete a quest

        CmdCompleteQuest();
    }

    [Command]
    private void CmdCompleteQuest()
    {
        if (currentQuestIndex == -1) return;

        Quest currentQuest = quests[currentQuestIndex];
        currentQuest.isActive = false;
        currentQuest.isComplete = true;

        UpdateQuestUI();
    }

    public void UpdateQuestProgress(int amount)
    {
        if (!isLocalPlayer) return; // Only the local player can update quest progress

        CmdUpdateQuestProgress(amount);
    }

    [Command]
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
        if (currentQuestIndex != -1)
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
