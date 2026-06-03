using UnityEngine;

public class PlayerFX : MonoBehaviour
{
    [Header("Squash & Stretch")]
    [SerializeField] private float squashAmount = 0.15f;
    [SerializeField] private float stretchAmount = 0.1f;
    [SerializeField] private float returnSpeed = 8f;

    [Header("Landing")]
    [SerializeField] private float landSquashThreshold = 5f;
    [SerializeField] private float landSquashMultiplier = 0.3f;

    [Header("Dust")]
    [SerializeField] private Color dustColor = new Color(0.7f, 0.6f, 0.5f, 0.5f);
    [SerializeField] private int dustParticles = 5;

    private Rigidbody2D rb;
    private PlayerController controller;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private bool wasGrounded;
    private float lastYVelocity;
    private ParticleSystem dustSystem;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        baseScale = transform.localScale;
        targetScale = baseScale;
        CreateDustSystem();
    }

    private void Update()
    {
        bool grounded = controller.IsGrounded;

        // landing squash
        if (grounded && !wasGrounded)
        {
            float impact = Mathf.Abs(lastYVelocity);
            if (impact > landSquashThreshold)
            {
                float squash = Mathf.Min(impact * 0.02f, landSquashMultiplier);
                targetScale = new Vector3(
                    baseScale.x * (1 + squash),
                    baseScale.y * (1 - squash),
                    baseScale.z);
                EmitDust(transform.position + Vector3.down * 0.5f);
            }
        }

        // jump stretch
        if (!grounded && rb.linearVelocity.y > 2f)
        {
            targetScale = new Vector3(
                baseScale.x * (1 - stretchAmount),
                baseScale.y * (1 + stretchAmount),
                baseScale.z);
        }

        // running dust
        if (grounded && Mathf.Abs(rb.linearVelocity.x) > 3f)
        {
            if (Random.value < 0.1f)
                EmitDust(transform.position + Vector3.down * 0.5f, 1);
        }

        // return to normal
        float signX = Mathf.Sign(transform.localScale.x);
        Vector3 current = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
        Vector3 lerped = Vector3.Lerp(current, targetScale, returnSpeed * Time.deltaTime);
        transform.localScale = new Vector3(lerped.x * signX, lerped.y, lerped.z);

        targetScale = Vector3.Lerp(targetScale, baseScale, returnSpeed * Time.deltaTime);

        wasGrounded = grounded;
        lastYVelocity = rb.linearVelocity.y;
    }

    private void CreateDustSystem()
    {
        var dustObj = new GameObject("DustFX");
        dustObj.transform.parent = transform;
        dustObj.transform.localPosition = Vector3.down * 0.5f;

        dustSystem = dustObj.AddComponent<ParticleSystem>();
        var emission = dustSystem.emission;
        emission.enabled = false;

        var main = dustSystem.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 1f;
        main.startSize = 0.15f;
        main.startColor = dustColor;
        main.gravityModifier = -0.3f;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = dustSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var colorLife = dustSystem.colorOverLifetime;
        colorLife.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(dustColor, 0), new GradientColorKey(dustColor, 1) },
            new[] { new GradientAlphaKey(0.5f, 0), new GradientAlphaKey(0, 1) }
        );
        colorLife.color = gradient;

        var sizeLife = dustSystem.sizeOverLifetime;
        sizeLife.enabled = true;
        sizeLife.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(
            new Keyframe(0, 0.5f), new Keyframe(1, 1.5f)));
    }

    private void EmitDust(Vector3 position, int count = -1)
    {
        if (dustSystem == null) return;
        dustSystem.transform.position = position;
        dustSystem.Emit(count > 0 ? count : dustParticles);
    }
}
