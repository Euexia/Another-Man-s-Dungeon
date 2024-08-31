using UnityEngine;
using Pathfinding;

public class RushState : StateCaC
{
    private Transform player;
    private AIPath aiPath;
    private Vector3 rushDirection;
    private float originalSpeed;
    private bool isRushing;

    public RushState(MonsterControllerCaC monsterController) : base(monsterController)
    {
        aiPath = monsterController.GetComponent<AIPath>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Assurez-vous que le joueur a ce tag
        isRushing = false;
    }

    public override void EnterState()
    {
        Debug.Log("Monster in Rush STATE");

        if (!isRushing)
        {
            SetAnimationTrigger("isRushing");
            // Initialiser le rush
            originalSpeed = aiPath.maxSpeed;
            aiPath.maxSpeed *= 1.3f; // Doubler la vitesse
            Vector3 direction = (player.position - monsterController.transform.position).normalized;
            rushDirection = direction;
            isRushing = true;
        }
    }

    public override void Update()
    {
        SetAnimationTrigger("isRushing");
        if (isRushing)
        {
            // Appliquer la direction de rush
            aiPath.destination = monsterController.transform.position + rushDirection * 50f; // Fonce tout droit
            // Si le monstre rencontre un obstacle
            if (aiPath.reachedEndOfPath)
            {
                EndRush();
            }
        }
    }

    public override void ExitState()
    {
        // Réinitialiser la vitesse lors de la sortie de l'état
        aiPath.maxSpeed = originalSpeed;
        isRushing = false;
        Debug.Log("Exiting Rush State");
    }

    private void EndRush()
    {
        aiPath.maxSpeed = originalSpeed; // Réinitialiser la vitesse
        aiPath.canMove = false;
        SetAnimationTrigger("isTakingDamage");
        //le monstre attend 2 secondes
        monsterController.TransitionToState(monsterController.chaseState);

        //monsterController.Invoke("TransitionToChaseState", 2f); // Attendre 2 secondes avant de passer à l'état de chase
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            EndRush();
        }
    }

    private void TransitionToChaseState()
    {
        monsterController.TransitionToState(monsterController.chaseState);
    }
}




