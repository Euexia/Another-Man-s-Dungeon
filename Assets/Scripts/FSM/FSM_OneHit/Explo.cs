using Pathfinding;
using UnityEngine;
using System.Collections;

public class ExploState : StateHit
{
    private AIPath aiPath;
    private Transform player;
    private float attackDistance = 4f;
    private SkinnedMeshRenderer monsterRenderer;

    public ExploState(MonsterControllerHit monsterController) : base(monsterController)
    {
        aiPath = monsterController.GetComponent<AIPath>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        monsterRenderer = monsterController.GetComponentInChildren<SkinnedMeshRenderer>(); // Trouver le SkinnedMeshRenderer dans les enfants
    }

    public override void EnterState()
    {
        aiPath.canMove = false;
        Debug.Log("Monster in Explo STATE");
        SetAnimationTrigger("isExplo");
        PerformExplo(); // Initialiser la première attaque
    }

    public override void UpdateState()
    {
        RotateTowardsPlayer();
    }

    private void PerformExplo()
    {
        // Démarrer la coroutine pour attendre la fin de l'animation
        monsterController.StartCoroutine(WaitForAnimationAndExplode());
    }

    private IEnumerator WaitForAnimationAndExplode()
    {
        Animator animator = monsterController.GetComponent<Animator>();
        if (animator != null)
        {
            // Obtenir les informations sur l'état actuel de l'animation
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Attendre la durée de l'animation
            yield return new WaitForSeconds(stateInfo.length);

            // Vérifier si le monstre n'a pas été détruit avant d'instancier l'explosion
            if (monsterController != null && monsterRenderer != null)
            {
                // Désactiver le rendu du monstre
                monsterRenderer.enabled = false;

                // Instancier l'explosion à l'emplacement exact du monstre avec la même rotation
                GameObject explosion = GameObject.Instantiate(monsterController.explosionVFX, monsterController.transform.position, monsterController.transform.rotation);

                // Attendre que l'explosion se termine
                yield return new WaitForSeconds(1f);

                // Détruire le monstre et l'explosion
                GameObject.Destroy(monsterController.gameObject, 1f);
            }
        }
    }

    public override void ExitState()
    {
        // Arrêter toutes les coroutines en cours lorsque l'état est quitté
        monsterController.StopAllCoroutines();
    }

    private void RotateTowardsPlayer()
    {
        // Utiliser Lerp pour la rotation
        Vector3 direction = player.position - monsterController.transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction);
        monsterController.transform.rotation = Quaternion.Lerp(monsterController.transform.rotation, toRotation, 10 * Time.deltaTime);
    }
}


