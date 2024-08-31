using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;

public class ChaseState : StateCaC
{
    private AIPath aiPath;
    private Transform player;
    private bool playerDetected;

    public ChaseState(MonsterControllerCaC monsterController) : base(monsterController)
    {
        aiPath = monsterController.GetComponent<AIPath>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Assurez-vous que le joueur a ce tag
        playerDetected = false;
    }

    public override void EnterState()
    {
        float distanceToPlayer = Vector3.Distance(monsterController.transform.position, player.position);
        if(monsterController.CanRush && distanceToPlayer <= monsterController.increasedChaseRadius && distanceToPlayer > 5)
        {
            Debug.Log("Monster Can rush, generating a random number");
            int random = Random.Range(0, 2);
            Debug.Log("Random number generated: " + random);
            if (random == 1)
            {
                monsterController.TransitionToState(monsterController.rushState);
            }
        }
        else
        {
            Debug.Log("Monster in Chase STATE");
            //aiPath.canMove = true;
        }
        
    }

    [Server]
    public override void Update()
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
                monsterController.TransitionToState(monsterController.attackState);
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
