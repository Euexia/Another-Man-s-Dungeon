using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MonsterControllerRanged : MonsterController
{
    public Transform player;
    public AIPath aiPath;
    public Animator animator;
    public Rigidbody rb;

    private StateRanged currentState;
    public StateRanged idleState;
    public StateRanged patrolRangedState;
    public StateRanged rangedAttackState;
    public StateRanged takingDamageState;
    public StateRanged deadState;

    //HEADER for inspector
    [Header("-- Monster Stats --")]
    [Tooltip("Distance initiale de detection...")]
    [SerializeField][Range(12f, 24f)] public float initialRadius = 15f;
    public float increasedRadius => initialRadius + 5f;

    [Tooltip("Distance d'attaque...")]
    [SerializeField][Range(12f, 24f)] public float attackDistance = 15f;

    [Tooltip("Description de la stratégie d'attaque du monstre.")]
    [TextArea(3, 5)]
    [SerializeField] private string attackStrategyDescription = "//DESCRIPTION";


    [Header("-- Ranged Attack --")]

    [Tooltip("Prefab de l'attaque simple à distance.")]
    [SerializeField] public GameObject rangedAttackPrefab1;

    [Tooltip("Prefab de l'attaque lourde à distance.")]
    [SerializeField] public GameObject rangedAttackPrefab2;

    [Tooltip("Prefab de l'attaque au corps a corps")]
    [SerializeField] public GameObject meleeAttackPrefab;

    [Tooltip("Vitesse du projectile")]
    [SerializeField] [Range(10f, 20f)] public float projectileSpeed = 15f;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        idleState = new IdleRangedState(this);
        rangedAttackState = new RangedAttackState(this);
        patrolRangedState = new PatrolRangedState(this);
        takingDamageState = new TakingDamageRangedState(this);
        //deadState = new DeadState(this);

        currentState = idleState; // Début en état Idle
        currentState.EnterState(); // Entrer dans l'état initial
    }

    void Update()
    {
        currentState.Update();
    }

    public void TransitionToState(StateRanged nextState)
    {
        currentState.ExitState();
        currentState = nextState;
        currentState.EnterState();
    }

    

}