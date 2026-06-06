using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float jogSpeed = 5.5f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float jogDrain = 10f;
    [SerializeField] private float staminaRegen = 8f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float currentStamina;
    private Vector2 moveInput;
    private bool isJogging;
    private bool inCameraMode;
    private bool facingRight = true;

    // animation
    private float animTimer;
    private Sprite[] walkDown, walkUp, walkSide;
    private Sprite idleDown, idleUp, idleSide;
    private int facing; // 0=down, 1=up, 2=side

    private InputAction moveAction;
    private InputAction jogAction;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public bool IsMoving => moveInput.magnitude > 0.1f;
    public bool InCameraMode => inCameraMode;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentStamina = maxStamina;

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.Enable();

        jogAction = new InputAction("Jog", InputActionType.Button, "<Keyboard>/leftShift");
        jogAction.Enable();

        GenerateSprites();
    }

    private float footstepTimer;

    private void Update()
    {
        if (inCameraMode)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();
        isJogging = jogAction.IsPressed() && currentStamina > 5f;

        float speed = isJogging ? jogSpeed : walkSpeed;
        rb.linearVelocity = moveInput.normalized * speed;

        // stamina
        if (isJogging && IsMoving)
            currentStamina -= jogDrain * Time.deltaTime;
        else
            currentStamina += staminaRegen * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // facing direction
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            facing = 2;
            if (moveInput.x > 0) facingRight = true;
            else if (moveInput.x < 0) facingRight = false;
        }
        else if (moveInput.y > 0.1f) facing = 1;
        else if (moveInput.y < -0.1f) facing = 0;

        sr.flipX = !facingRight && facing == 2;

        // animate
        if (IsMoving)
        {
            float freq = isJogging ? 10f : 6f;
            animTimer += Time.deltaTime * freq;
            int frame = Mathf.FloorToInt(animTimer) % 4;
            var frames = facing == 0 ? walkDown : (facing == 1 ? walkUp : walkSide);
            if (frames != null && frame < frames.Length)
                sr.sprite = frames[frame];
        }
        else
        {
            animTimer = 0;
            sr.sprite = facing == 0 ? idleDown : (facing == 1 ? idleUp : idleSide);
        }

        // Y-sort so player walks behind/in front of objects
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10) + 50;

        // footstep sounds
        if (IsMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                AudioManager.Instance?.PlaySFX("footstep");
                footstepTimer = isJogging ? 0.25f : 0.4f;
            }
        }
        else footstepTimer = 0;
    }

    public void SetCameraMode(bool active) => inCameraMode = active;

    private void GenerateSprites()
    {
        idleDown = PixelArt.HikerDown(0);
        idleUp = PixelArt.HikerUp(0);
        idleSide = PixelArt.HikerSide(0);

        walkDown = new Sprite[] {
            PixelArt.HikerDown(0), PixelArt.HikerDown(1),
            PixelArt.HikerDown(2), PixelArt.HikerDown(3)
        };
        walkUp = new Sprite[] {
            PixelArt.HikerUp(0), PixelArt.HikerUp(1),
            PixelArt.HikerUp(2), PixelArt.HikerUp(3)
        };
        walkSide = new Sprite[] {
            PixelArt.HikerSide(0), PixelArt.HikerSide(1),
            PixelArt.HikerSide(2), PixelArt.HikerSide(3)
        };

        sr.sprite = idleDown;
    }

    private void OnDestroy()
    {
        moveAction?.Dispose();
        jogAction?.Dispose();
    }
}
