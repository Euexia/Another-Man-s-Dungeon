using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateHit : NetworkBehaviour
{
    protected MonsterControllerHit monsterController;

    public StateHit(MonsterControllerHit monsterController)
    {
        this.monsterController = monsterController;
    }

    // Méthodes à appeler lors de l'entrée et de la sortie d'un état
    public virtual void EnterState() { }
    public virtual void ExitState() { }

    // Méthode abstraite pour la mise à jour de l'état
    public abstract void UpdateState();

    // Méthode pour définir un trigger d'animation
    protected void SetAnimationTrigger(string triggerName)
    {
        monsterController.animator.SetTrigger(triggerName);
    }
}