using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleRangedState : StateRanged
{
    private Transform player;
    public IdleRangedState(MonsterControllerRanged monsterController) : base(monsterController)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState()
    {
        Debug.Log("Monster in Idle STATE");
        // Logique d'initialisation de l'état Idle
    }

    public override void Update()
    {
        SetAnimationTrigger("isIdle");

        // Logique pour l'état Idle
        if (Vector3.Distance(monsterController.transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < monsterController.initialRadius) // distance de détection
        {
            monsterController.TransitionToState(monsterController.rangedAttackState);
        }

        //On teste toutes les 3 secondes si on doit changer d'état
        if (Time.time % 5 < 0.1)
        {
            int random = Random.Range(0, 4);
            Debug.Log("Random genere : " + random);
            if (random == 0)
            {
                monsterController.TransitionToState(monsterController.patrolRangedState);
            }
        }
    }

    public override void ExitState()
    {
        // Logique de nettoyage de l'état de Chase
    }
}
