using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    // Méthode pour vérifier si la quête est complète
    public bool IsComplete()
    {
        return currentCount >= targetCount;
    }
}

public class QuestManager : MonoBehaviour
{
    public Text questDescriptionText; // Assign in the Inspector
    public Text questStatusText; // Assign in the Inspector

    public List<Quest> quests = new List<Quest>();
    public Quest currentQuest; // La quête en cours
    private int currentQuestIndex = -1; // L'indice de la quête en cours

    private void Start()
    {
        if (questDescriptionText == null || questStatusText == null)
        {
            Debug.LogError("Please assign the Text components in the Inspector.");
            return;
        }

        if (quests.Count == 0)
        {
            Debug.LogError("No quests available. Please assign quests in the Inspector.");
        }

        // Ajoutez quelques quêtes à la liste pour les tests si elle est vide
        if (quests.Count == 0)
        {
            quests.Add(new Quest("Collect 10 herbs", "Collect 10 herbs for the village healer.", 10));
            quests.Add(new Quest("Defeat 5 goblins", "Defeat 5 goblins that are terrorizing the village.", 5));
        }

        UpdateQuestUI();
    }

    public void StartQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Count)
        {
            Debug.LogError("Invalid quest index! Quest index: " + questIndex);
            return;
        }

        currentQuestIndex = questIndex;
        currentQuest = quests[questIndex];
        currentQuest.isActive = true;
        UpdateQuestUI();
    }

    public void CompleteQuest()
    {
        if (currentQuest != null)
        {
            currentQuest.isActive = false;
            currentQuest.isComplete = true;
            UpdateQuestUI();
            Debug.Log("Quest completed: " + currentQuest.questName);
        }
    }

    public void UpdateQuestProgress(int amount)
    {
        if (currentQuest != null && currentQuest.isActive)
        {
            currentQuest.currentCount += amount;
            if (currentQuest.IsComplete())
            {
                CompleteQuest();
            }
            UpdateQuestUI();
        }
    }

    public void UpdateQuestUI()
    {
        if (currentQuest != null && currentQuest.isActive)
        {
            questDescriptionText.text = currentQuest.description;
            questStatusText.text = "Quest in progress: " + currentQuest.currentCount + "/" + currentQuest.targetCount;
        }
        else if (currentQuest != null && currentQuest.isComplete)
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
