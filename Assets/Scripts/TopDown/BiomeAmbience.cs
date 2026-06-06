using UnityEngine;

public class BiomeAmbience : MonoBehaviour
{
    private Transform player;
    private ParticleSystem leafParticles;
    private ParticleSystem snowParticles;
    private float mapHeight = 100f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        CreateLeafParticles();
        CreateSnowParticles();
    }

    private void Update()
    {
        if (player == null) return;

        float progress = Mathf.Clamp01(player.position.y / mapHeight);

        // leaves fall in forest biome
        var leafEmit = leafParticles.emission;
        leafEmit.rateOverTime = (progress > 0.12f && progress < 0.4f) ? 8 : 0;
        leafParticles.transform.position = (Vector2)player.position + Vector2.up * 6f;

        // snow in summit area
        var snowEmit = snowParticles.emission;
        snowEmit.rateOverTime = progress > 0.75f ? Mathf.Lerp(0, 30, (progress - 0.75f) / 0.25f) : 0;
        snowParticles.transform.position = (Vector2)player.position + Vector2.up * 6f;
    }

    private void CreateLeafParticles()
    {
        var obj = new GameObject("Leaves");
        obj.transform.parent = transform;
        leafParticles = obj.AddComponent<ParticleSystem>();

        var main = leafParticles.main;
        main.startLifetime = 4f;
        main.startSpeed = 0.3f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.4f, 0.55f, 0.15f),
            new Color(0.6f, 0.5f, 0.15f));
        main.gravityModifier = 0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;

        var shape = leafParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(14, 0.5f, 1);

        var velocity = leafParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        var rotation = leafParticles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

        var emission = leafParticles.emission;
        emission.rateOverTime = 0;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 200;
    }

    private void CreateSnowParticles()
    {
        var obj = new GameObject("Snow");
        obj.transform.parent = transform;
        snowParticles = obj.AddComponent<ParticleSystem>();

        var main = snowParticles.main;
        main.startLifetime = 5f;
        main.startSpeed = 0.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor = new Color(1f, 1f, 1f, 0.8f);
        main.gravityModifier = 0.1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;

        var shape = snowParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(16, 0.5f, 1);

        var velocity = snowParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

        var emission = snowParticles.emission;
        emission.rateOverTime = 0;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 200;
    }
}
