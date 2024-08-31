using UnityEngine;

public class MonsterQuest : MonoBehaviour
{
    public QuestManager questManager; // Assign in the Inspector
    public int questIndex; // Indice de la quête à mettre à jour

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (questManager != null && questIndex >= 0 && questIndex < questManager.quests.Count)
            {
                Quest quest = questManager.quests[questIndex];
                if (quest != null && quest.isActive)
                {
                    questManager.UpdateQuestProgress(1);
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogError("Invalid quest index!");
            }
        }
    }
}