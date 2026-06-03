using UnityEngine;
using System.Collections.Generic;

public class CritterSpawner : MonoBehaviour
{
    [SerializeField] private CritterData[] possibleCritters;
    [SerializeField] private GameObject critterPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxCritters = 8;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float currentAltitude;

    private List<GameObject> activeCritters = new List<GameObject>();
    private float spawnTimer;

    private void Start()
    {
        for (int i = 0; i < Mathf.Min(maxCritters / 2, spawnPoints.Length); i++)
        {
            TrySpawnCritter();
        }
    }

    private void Update()
    {
        activeCritters.RemoveAll(c => c == null);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && activeCritters.Count < maxCritters)
        {
            TrySpawnCritter();
            spawnTimer = spawnInterval;
        }
    }

    private void TrySpawnCritter()
    {
        if (spawnPoints.Length == 0 || possibleCritters.Length == 0) return;

        CritterData chosen = PickCritter();
        if (chosen == null) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject critterObj = Instantiate(critterPrefab, spawnPoint.position, Quaternion.identity);

        var critter = critterObj.GetComponent<Critter>();
        if (critter != null)
        {
            var dataField = typeof(Critter).GetField("data",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dataField?.SetValue(critter, chosen);
        }

        activeCritters.Add(critterObj);
    }

    private CritterData PickCritter()
    {
        List<CritterData> eligible = new List<CritterData>();
        foreach (var critter in possibleCritters)
        {
            if (currentAltitude >= critter.minAltitude && currentAltitude <= critter.maxAltitude)
            {
                eligible.Add(critter);
            }
        }

        if (eligible.Count == 0) return null;

        float totalWeight = 0;
        foreach (var c in eligible)
            totalWeight += c.spawnChance;

        float roll = Random.Range(0, totalWeight);
        float cumulative = 0;
        foreach (var c in eligible)
        {
            cumulative += c.spawnChance;
            if (roll <= cumulative) return c;
        }

        return eligible[0];
    }

    public void SetAltitude(float alt)
    {
        currentAltitude = alt;
    }
}
