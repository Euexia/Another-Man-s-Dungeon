using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakingDamageCaCState : StateCaC
{

    public TakingDamageCaCState(MonsterControllerCaC monsterController) : base(monsterController)
    {
    }


    // Start is called before the first frame update
    public override void EnterState()
    {
        Debug.Log("Monster Taking Damage");
    }

    // Update is called once per frame
    public override void Update()
    {
        //on trigger l'anim isTakingDamage
        SetAnimationTrigger("isTakingDamage");
        //on joue le son de dégats
        //on repasse en idle
        monsterController.TransitionToState(monsterController.idleState);
    }


}
