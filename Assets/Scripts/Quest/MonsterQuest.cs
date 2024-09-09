using UnityEngine;
using Mirror;

public class MonsterQuest : NetworkBehaviour
{
    public QuestManager questManager; // Assignez dans l'inspecteur
    public int questIndex; // Indice de la quête à mettre à jour

    private void OnDestroy()
    {
        if (!isServer) return; // Seul le serveur gère les mises à jour de quête

        // Vérifier que l'indice de quête est valide et que le gestionnaire de quêtes est défini
        if (questManager != null && questIndex >= 0 && questIndex < questManager.quests.Count)
        {
            Quest quest = questManager.quests[questIndex];
            if (quest != null && quest.isActive)
            {
                // Commande pour mettre à jour la progression de la quête sur le serveur
                CmdUpdateQuestProgress(1);
            }
        }
        else
        {
            Debug.LogError("Invalid quest index or QuestManager not set!");
        }
    }

    [Command]
    private void CmdUpdateQuestProgress(int amount)
    {
        // Vérifier à nouveau que l'indice de quête est valide avant de mettre à jour
        if (questIndex >= 0 && questIndex < questManager.quests.Count)
        {
            questManager.UpdateQuestProgress(amount);
        }
        else
        {
            Debug.LogError("Invalid quest index in CmdUpdateQuestProgress!");
        }
    }
}
