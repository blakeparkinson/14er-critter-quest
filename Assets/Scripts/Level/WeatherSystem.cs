using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private float baseWindSpeed = 1f;
    [SerializeField] private float windGustInterval = 5f;
    [SerializeField] private float gustStrength = 3f;

    [Header("Snow")]
    [SerializeField] private float snowStartAltitude = 40f;
    [SerializeField] private int snowParticleCount = 50;

    [Header("Clouds")]
    [SerializeField] private int cloudCount = 5;
    [SerializeField] private float cloudSpeed = 0.5f;
    [SerializeField] private float cloudMinY = 20f;
    [SerializeField] private float cloudMaxY = 55f;

    private ParticleSystem snowSystem;
    private Transform playerTransform;
    private float windTimer;
    private float currentWind;
    private GameObject[] clouds;

    public float CurrentWind => currentWind;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        CreateSnowSystem();
        CreateClouds();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float altitude = playerTransform.position.y;
        UpdateWind(altitude);
        UpdateSnow(altitude);
        UpdateClouds();
    }

    private void UpdateWind(float altitude)
    {
        windTimer -= Time.deltaTime;
        if (windTimer <= 0)
        {
            float altFactor = Mathf.Clamp01(altitude / 60f);
            currentWind = baseWindSpeed * altFactor + Random.Range(-gustStrength, gustStrength) * altFactor;
            windTimer = windGustInterval + Random.Range(-2f, 2f);
        }
    }

    private void UpdateSnow(float altitude)
    {
        if (snowSystem == null) return;

        var emission = snowSystem.emission;
        if (altitude > snowStartAltitude)
        {
            float intensity = Mathf.Clamp01((altitude - snowStartAltitude) / 20f);
            emission.rateOverTime = snowParticleCount * intensity;
            snowSystem.transform.position = playerTransform.position + Vector3.up * 8f;

            var velocityModule = snowSystem.velocityOverLifetime;
            velocityModule.x = currentWind * 0.5f;
        }
        else
        {
            emission.rateOverTime = 0;
        }
    }

    private void CreateSnowSystem()
    {
        var snowObj = new GameObject("SnowSystem");
        snowObj.transform.parent = transform;
        snowSystem = snowObj.AddComponent<ParticleSystem>();

        var main = snowSystem.main;
        main.startLifetime = 4f;
        main.startSpeed = 0.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(1f, 1f, 1f, 0.7f);
        main.gravityModifier = 0.3f;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = snowSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20, 0.5f, 1);

        var emission = snowSystem.emission;
        emission.rateOverTime = 0;

        var velocity = snowSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        var colorLife = snowSystem.colorOverLifetime;
        colorLife.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(0.7f, 0.2f), new GradientAlphaKey(0, 1) }
        );
        colorLife.color = gradient;

        var renderer = snowObj.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;
    }

    private void CreateClouds()
    {
        clouds = new GameObject[cloudCount];
        for (int i = 0; i < cloudCount; i++)
        {
            var cloud = new GameObject($"Cloud_{i}");
            cloud.transform.parent = transform;
            cloud.transform.position = new Vector3(
                Random.Range(-30f, 30f),
                Random.Range(cloudMinY, cloudMaxY),
                2
            );

            float scale = Random.Range(2f, 5f);
            cloud.transform.localScale = new Vector3(scale, scale * 0.4f, 1);

            var sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CreateRect($"cloud_{i}",
                (int)Random.Range(16, 32), (int)Random.Range(6, 12),
                new Color(1, 1, 1, Random.Range(0.2f, 0.5f)));
            sr.sortingOrder = -1;

            clouds[i] = cloud;
        }
    }

    private void UpdateClouds()
    {
        if (clouds == null) return;
        for (int i = 0; i < clouds.Length; i++)
        {
            if (clouds[i] == null) continue;
            clouds[i].transform.position += Vector3.right * (cloudSpeed + currentWind * 0.1f) * Time.deltaTime;

            if (clouds[i].transform.position.x > 40)
                clouds[i].transform.position = new Vector3(-40, clouds[i].transform.position.y, 2);
        }
    }
}
