using UnityEngine;
using Pathfinding;

public class MonsterControllerHit : MonsterController
{
    public Transform player;
    public AIPath aiPath;
    public Animator animator;
    public Rigidbody rb;
    public GameObject explosionVFX;

    private StateHit currentState;
    public StateHit idleState;
    public StateHit chaseState;
    public StateHit exploState;
    public StateHit takingDamageState;
    public StateHit deadState;

    //HEADER for inspector
    [Header("-- Monster Stats --")]
    [Tooltip("Distance initiale de detection...")]
    [SerializeField][Range(8f, 20f)] public float initialChaseRadius = 13f;
    public float increasedChaseRadius => initialChaseRadius + 8f;

    [Tooltip("Distance d'attaque...")]
    [SerializeField][Range(2f, 12f)] public float attackDistance = 4f;

    [Tooltip("Description de la stratégie d'attaque du monstre.")]
    [TextArea(3, 5)]
    [SerializeField] private string attackStrategyDescription = "Le monstre explose a portee du joueur";

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        idleState = new IdleHitState(this);
        chaseState = new ChaseHitState(this);
        exploState = new ExploState(this);
        //takingDamageState = new TakingDamageHitState(this);

        currentState = idleState; // Début en état Idle
        currentState.EnterState(); // Entrer dans l'état initial
    }

    void Update()
    {
        if (!isServer) return;

        currentState.UpdateState();
    }

    public void TransitionToState(StateHit nextState)
    {
        currentState.ExitState();
        currentState = nextState;
        currentState.EnterState();
    }

}