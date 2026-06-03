using UnityEngine;

public class Critter : MonoBehaviour
{
    [SerializeField] private CritterData data;

    private enum State { Idle, Wandering, Fleeing, SillyAction, Posing }

    private State currentState = State.Idle;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;

    private float stateTimer;
    private float wanderDirection;
    private bool facingRight = true;
    private string currentSillyAction;
    private bool hasBeenPhotographed;

    public CritterData Data => data;
    public bool IsDoingSillyAction => currentState == State.SillyAction;
    public string CurrentSillyAction => currentSillyAction;
    public bool HasBeenPhotographed => hasBeenPhotographed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (data != null && data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        EnterState(State.Idle);
    }

    private void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(distToPlayer);
                break;
            case State.Wandering:
                HandleWandering(distToPlayer);
                break;
            case State.Fleeing:
                HandleFleeing(distToPlayer);
                break;
            case State.SillyAction:
                HandleSillyAction(distToPlayer);
                break;
            case State.Posing:
                HandlePosing(distToPlayer);
                break;
        }

        UpdateAnimator();
    }

    private void HandleIdle(float distToPlayer)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (ShouldFlee(distToPlayer))
        {
            EnterState(State.Fleeing);
            return;
        }

        if (stateTimer <= 0)
        {
            if (Random.value < 0.15f && data.sillyActions != null && data.sillyActions.Length > 0)
                EnterState(State.SillyAction);
            else
                EnterState(State.Wandering);
        }
    }

    private void HandleWandering(float distToPlayer)
    {
        rb.linearVelocity = new Vector2(wanderDirection * data.moveSpeed, rb.linearVelocity.y);

        if (ShouldFlee(distToPlayer))
        {
            EnterState(State.Fleeing);
            return;
        }

        if (data.personality == CritterPersonality.Curious && distToPlayer < data.detectionRange)
        {
            wanderDirection = Mathf.Sign(player.position.x - transform.position.x);
        }

        if (stateTimer <= 0)
            EnterState(State.Idle);

        UpdateFacing(wanderDirection);
    }

    private void HandleFleeing(float distToPlayer)
    {
        if (data.personality == CritterPersonality.Bold)
        {
            EnterState(State.Posing);
            return;
        }

        float fleeDir = Mathf.Sign(transform.position.x - player.position.x);
        rb.linearVelocity = new Vector2(fleeDir * data.moveSpeed * 2f, rb.linearVelocity.y);
        UpdateFacing(fleeDir);

        if (distToPlayer > data.detectionRange * 1.5f || stateTimer <= 0)
            EnterState(State.Idle);
    }

    private void HandleSillyAction(float distToPlayer)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (ShouldFlee(distToPlayer) && data.personality == CritterPersonality.Shy)
        {
            EnterState(State.Fleeing);
            return;
        }

        if (stateTimer <= 0)
            EnterState(State.Idle);
    }

    private void HandlePosing(float distToPlayer)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        UpdateFacing(dirToPlayer);

        if (distToPlayer > data.detectionRange * 2f || stateTimer <= 0)
            EnterState(State.Wandering);
    }

    private bool ShouldFlee(float distToPlayer)
    {
        if (data.personality == CritterPersonality.Bold) return false;
        if (data.personality == CritterPersonality.Chaotic) return Random.value < 0.3f;
        return distToPlayer < data.fleeRange;
    }

    private void EnterState(State newState)
    {
        currentState = newState;
        currentSillyAction = null;

        switch (newState)
        {
            case State.Idle:
                stateTimer = Random.Range(data.idleTimeMin, data.idleTimeMax);
                break;
            case State.Wandering:
                stateTimer = Random.Range(2f, 5f);
                wanderDirection = Random.value > 0.5f ? 1f : -1f;
                break;
            case State.Fleeing:
                stateTimer = 3f;
                break;
            case State.SillyAction:
                stateTimer = Random.Range(2f, 4f);
                if (data.sillyActions.Length > 0)
                    currentSillyAction = data.sillyActions[Random.Range(0, data.sillyActions.Length)];
                break;
            case State.Posing:
                stateTimer = Random.Range(3f, 6f);
                break;
        }
    }

    private void UpdateFacing(float direction)
    {
        if (direction > 0 && !facingRight || direction < 0 && facingRight)
        {
            facingRight = !facingRight;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsFleeing", currentState == State.Fleeing);
        animator.SetBool("IsSillyAction", currentState == State.SillyAction);
        animator.SetBool("IsPosing", currentState == State.Posing);
    }

    public void OnPhotographed()
    {
        hasBeenPhotographed = true;

        if (data.personality == CritterPersonality.Bold)
            EnterState(State.Posing);
        else if (data.personality == CritterPersonality.Shy)
            EnterState(State.Fleeing);
        else if (data.personality == CritterPersonality.Chaotic)
            EnterState(Random.value > 0.5f ? State.SillyAction : State.Fleeing);
    }
}
