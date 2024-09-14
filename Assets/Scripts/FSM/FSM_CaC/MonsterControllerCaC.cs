using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;

public class MonsterControllerCaC : MonsterController
{
    public AIPath aiPath;
    public Animator animator;
    public Rigidbody rb;

    private StateCaC currentState;
    public StateCaC idleState;
    public StateCaC chaseState;
    public StateCaC attackState;
    public StateCaC rushState;
    public StateCaC takingDamageState;
    public StateCaC deadState;

    //HEADER for inspector
    [Header("-- Monster Stats --")]
    [Tooltip("Distance initiale de detection...")]
    [SerializeField][Range(8f, 20f)] public float initialChaseRadius = 13f;
    public float increasedChaseRadius => initialChaseRadius + 3f;

    [Tooltip("Distance d'attaque...")]
    [SerializeField][Range(2f, 12f)] public float attackDistance = 4f;

    [Tooltip("Le monstre peut-il rush ?")]
    [SerializeField] public bool CanRush = false;
    [Tooltip("Vitesse de rush...")]
    [SerializeField] public float rushSpeedMultiplier = 1.3f;

    [Tooltip("Description de la stratégie d'attaque du monstre.")]
    [TextArea(3, 5)]
    [SerializeField] private string attackStrategyDescription = "Le monstre attaque de manière agressive lorsque le joueur est à portée.";

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
        {
            players.Add(entry.Value.identity);
        }

        idleState = new IdleCaCState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        rushState = new RushState(this);
        takingDamageState = new TakingDamageCaCState(this);
        //deadState = new DeadState(this);

        currentState = idleState; // Début en état Idle
        currentState.EnterState(); // Entrer dans l'état initial
    }

    void Update()
    {
        if (!isServer) return;

        currentState.Update();
    }

    public void TransitionToState(StateCaC nextState)
    {
        currentState.ExitState();
        currentState = nextState;
        currentState.EnterState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Déléguer la gestion de collision à l'état actuel
        if (currentState is RushState rushState)
        {
            rushState.OnCollisionEnter(collision);
        }
    }

    public Transform GetClosestPlayer(Transform point)
    {
        Transform closest = null;

        foreach (NetworkIdentity player in players)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(point.position, player.transform.position);

                if (closest == null || Vector3.Distance(closest.position, point.position) > distanceToPlayer) {
                    closest = player.transform;
                }
            }
        }

        return closest;
    }

}