using UnityEngine;

public enum BiomeType
{
    Trailhead,
    Forest,
    Alpine,
    Tundra,
    Summit
}

public class AltitudeZone : MonoBehaviour
{
    [SerializeField] private BiomeType biome;
    [SerializeField] private float zoneStartY;
    [SerializeField] private float zoneEndY;
    [SerializeField] private Color ambientTint = Color.white;
    [SerializeField] private AudioClip ambientSound;
    [SerializeField] private float windIntensity;
    [SerializeField] private ParticleSystem weatherParticles;

    [Header("Visual")]
    [SerializeField] private Material backgroundMaterial;
    [SerializeField] private Color fogColor = Color.white;
    [SerializeField] private float fogDensity = 0.01f;

    public BiomeType Biome => biome;
    public float WindIntensity => windIntensity;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (weatherParticles != null)
            weatherParticles.Play();

        var spawner = FindFirstObjectByType<CritterSpawner>();
        if (spawner != null)
            spawner.SetAltitude(zoneStartY);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (weatherParticles != null)
            weatherParticles.Stop();
    }
}
