using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;

public class ChaseHitState : StateHit
{
    private AIPath aiPath;
    private Transform player;
    private bool playerDetected;

    public ChaseHitState(MonsterControllerHit monsterController) : base(monsterController)
    {
        aiPath = monsterController.GetComponent<AIPath>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Assurez-vous que le joueur a ce tag
        playerDetected = false;
    }

    public override void EnterState()
    {
        float distanceToPlayer = Vector3.Distance(monsterController.transform.position, player.position);
        Debug.Log("Monster in Chase STATE");
        //aiPath.canMove = true;
        
    }

    public override void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(monsterController.transform.position, player.position);

        if (distanceToPlayer < monsterController.initialChaseRadius)
        {
            if (!playerDetected)
            {
                playerDetected = true;
                monsterController.initialChaseRadius = monsterController.increasedChaseRadius;
            }
            aiPath.destination = player.position;
            SetAnimationTrigger("isChasing");
            //aiPath.canMove = true;

            if (distanceToPlayer <= monsterController.attackDistance)
            {
                monsterController.TransitionToState(monsterController.exploState);
            }
        }
        else
        {
            if (playerDetected && distanceToPlayer >= monsterController.increasedChaseRadius)
            {
                playerDetected = false;
            }
            aiPath.canMove = false;
            monsterController.TransitionToState(monsterController.idleState);
        }
    }

    public override void ExitState()
    {
        // Logique de nettoyage de l'état de Chase
    }
}
