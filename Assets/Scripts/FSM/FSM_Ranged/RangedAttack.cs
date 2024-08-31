using UnityEngine;
using UnityEngine.Tilemaps;

public class RangedAttackState : StateRanged
{
    private Transform player;
    private int attackCounter = 0;
    private float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    private float meleeAttackCooldown = 10f;
    private float meleeLastAttackTime = 0f;
    private float rotationSpeed = 5f; // Vitesse de rotation

    public RangedAttackState(MonsterControllerRanged monsterController) : base(monsterController)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void EnterState()
    {
        Debug.Log("Monster in Ranged Attack STATE");
    }

    public override void Update()
    {
        SetAnimationTrigger("isIdle");
        float distanceToPlayer = Vector3.Distance(monsterController.transform.position, player.position);
        RotateTowardsPlayer();

        Debug.DrawRay(GetOffset(), Vector3.down*100, Color.red);

        if (lastAttackTime > 0)
        {
            lastAttackTime = Mathf.Max(0, lastAttackTime - Time.deltaTime);
        }
        if (meleeLastAttackTime > 0)
        {
            meleeLastAttackTime = Mathf.Max(0, meleeLastAttackTime - Time.deltaTime);
        }
        if (distanceToPlayer > monsterController.increasedRadius) // distance de détection
        {
            //on recharge le cooldown
            lastAttackTime = Time.time;
            monsterController.TransitionToState(monsterController.idleState);
        }
        else if (distanceToPlayer < 5f)
        {
            MeleeAttack();
        }
        else
        {
            Attack();
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Ranged Attack State");
    }

    private void Attack()
    {
        if (lastAttackTime == 0)
        {
            attackCounter++;
            if (attackCounter <= 4)
            {
                LightAttack();
                lastAttackTime = attackCooldown;
            }
            else
            {
                attackCounter = 0;
                HeavyAttack();
                lastAttackTime = attackCooldown;
            }
        }
    }

    private void LightAttack()
    {
        SetAnimationTrigger("isAttacking1");
        //tire le prefab 1
        //on instancie le prefab 1
        GameObject rangedAttack1 = GameObject.Instantiate(monsterController.rangedAttackPrefab1, GetOffset(), Quaternion.identity);
        rangedAttack1.transform.LookAt(player);
        //on recup son rb
        Rigidbody rb = rangedAttack1.GetComponent<Rigidbody>();
        rb.AddForce(rangedAttack1.transform.forward * monsterController.projectileSpeed, ForceMode.Impulse);
    }

    private void HeavyAttack()
    {
        SetAnimationTrigger("isAttacking2");
        // tire le prefab 2
        GameObject rangedAttack2 = GameObject.Instantiate(monsterController.rangedAttackPrefab2, GetOffset(), Quaternion.identity);
        rangedAttack2.transform.LookAt(player);
        //on recup son rb
        Rigidbody rb = rangedAttack2.GetComponent<Rigidbody>();
        rb.AddForce(rangedAttack2.transform.forward * monsterController.projectileSpeed, ForceMode.Impulse);

    }

    private void MeleeAttack()
    {
        if ( meleeLastAttackTime == 0)
        {
            Debug.Log("Lancement Melee Attack");
            SetAnimationTrigger("isAttackingCaC");
            //spawn le prefab CaC et le détruit après 2 seconde
            //on raycast pour avoir la pos du sol
            RaycastHit hit;
            Debug.Log(monsterController.transform.position);
            if (Physics.Raycast(GetOffset(), Vector3.down, out hit, 100f))
            {
                Vector3 pos = hit.point;
                pos.y += 0.5200602f;
                //on instancie le prefab CaC
                GameObject meleeAttack = GameObject.Instantiate(monsterController.meleeAttackPrefab, pos, Quaternion.identity);
                //on le détruit après 2 secondes
                GameObject.Destroy(meleeAttack, 2f);
                Debug.Log("Melee Attack SPAWN");
                Debug.Log(pos);
            }
            //GameObject meleeAttack = GameObject.Instantiate(monsterController.meleeAttackPrefab, monsterController.transform.position, Quaternion.identity);
            meleeLastAttackTime = meleeAttackCooldown;
        }
    }

    private Vector3 GetOffset()
    {
        return monsterController.transform.position + monsterController.transform.up * 1.8f;
    }

    private void RotateTowardsPlayer()
    {
        //on utilise lerp pour la rotation
        Vector3 direction = player.position - monsterController.transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction);
        monsterController.transform.rotation = Quaternion.Lerp(monsterController.transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

    }
}

//0.5200602