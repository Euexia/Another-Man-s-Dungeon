using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleCaCState : StateCaC
{
    public IdleCaCState(MonsterControllerCaC monsterControllerCaC) : base(monsterControllerCaC) { }

    public override void EnterState()
    {
        //Debug.Log("Monster in Idle STATE");
        // Logique d'initialisation de l'état Idle
    }
    public override void Update()
    {
        SetAnimationTrigger("isIdle");
        // Logique pour l'état Idle
        if (Vector3.Distance(monsterController.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < monsterController.initialChaseRadius) // distance de détection par exemple
        {
            monsterController.TransitionToState(monsterController.chaseState);
            //monsterController.TransitionToState(monsterController.rushState);
        }
    }

    public override void ExitState()
    {
        // Logique de nettoyage de l'état Idle
    }
}
