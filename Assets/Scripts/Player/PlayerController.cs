using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForce = 12f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 40f;
    [SerializeField] private float airAcceleration = 30f;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private float lowJumpGravityMultiplier = 3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.3f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 5f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float jumpStaminaCost = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float moveInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool facingRight = true;
    private float currentStamina;
    private bool inCameraMode;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float defaultGravityScale;
    private bool isJumping;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public bool IsGrounded => isGrounded;
    public bool InCameraMode => inCameraMode;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentStamina = maxStamina;
        defaultGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (inCameraMode)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (wallCheck != null)
        {
            float dir = facingRight ? 1f : -1f;
            isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, wallCheckDistance, groundLayer);
        }

        // coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // jump buffer
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // ground/coyote jump
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && currentStamina >= jumpStaminaCost)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            currentStamina -= jumpStaminaCost;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            isJumping = true;
            AudioManager.Instance?.PlaySFX("jump");
        }
        // wall jump
        else if (Input.GetButtonDown("Jump") && isWallSliding && currentStamina >= jumpStaminaCost)
        {
            float dir = facingRight ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir * wallJumpForce * 0.7f, wallJumpForce);
            currentStamina -= jumpStaminaCost;
            isJumping = true;
            Flip();
            AudioManager.Instance?.PlaySFX("jump");
        }

        // variable jump height
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0 && isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            isJumping = false;
        }

        // better gravity feel
        if (rb.linearVelocity.y < 0)
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.gravityScale = defaultGravityScale * lowJumpGravityMultiplier;
        else
            rb.gravityScale = defaultGravityScale;

        HandleStamina();
        HandleWallSlide();
        UpdateAnimator();

        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    private void FixedUpdate()
    {
        if (inCameraMode) return;

        float targetSpeed = moveInput * moveSpeed;
        float accel = isGrounded ? acceleration : airAcceleration;
        float decel2 = isGrounded ? deceleration : airAcceleration * 0.5f;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel2;

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    private void HandleStamina()
    {
        if (!isGrounded && rb.linearVelocity.y > 0)
            currentStamina -= staminaDrainRate * Time.deltaTime;
        else if (isGrounded)
            currentStamina += staminaRegenRate * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private void HandleWallSlide()
    {
        isWallSliding = isTouchingWall && !isGrounded && moveInput != 0;
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    public void SetCameraMode(bool active)
    {
        inCameraMode = active;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            float dir = facingRight ? 1f : -1f;
            Gizmos.DrawRay(wallCheck.position, Vector2.right * dir * wallCheckDistance);
        }
    }
}
