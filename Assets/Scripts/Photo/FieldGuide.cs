using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FieldGuideEntry
{
    public CritterData critterData;
    public bool discovered;
    public int bestPhotoScore;
    public PhotoRating bestRating;
    public int timesSeen;
    public string bestPhotoContext;
}

public class FieldGuide : MonoBehaviour
{
    [SerializeField] private CritterData[] allCritters;

    private Dictionary<string, FieldGuideEntry> entries = new Dictionary<string, FieldGuideEntry>();

    public static FieldGuide Instance { get; private set; }

    public int TotalCritters => allCritters.Length;
    public int DiscoveredCount
    {
        get
        {
            int count = 0;
            foreach (var entry in entries.Values)
                if (entry.discovered) count++;
            return count;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var critter in allCritters)
        {
            entries[critter.critterName] = new FieldGuideEntry
            {
                critterData = critter,
                discovered = false
            };
        }

        Load();
    }

    public void RecordPhoto(PhotoResult result)
    {
        if (result.primaryCritter == null) return;

        string name = result.primaryCritter.critterName;
        if (!entries.ContainsKey(name)) return;

        var entry = entries[name];
        bool newDiscovery = !entry.discovered;
        entry.discovered = true;
        entry.timesSeen++;

        if (result.score > entry.bestPhotoScore)
        {
            entry.bestPhotoScore = result.score;
            entry.bestRating = result.rating;
            entry.bestPhotoContext = result.ratingText;
        }

        Save();

        if (newDiscovery)
        {
            OnNewDiscovery?.Invoke(result.primaryCritter);
        }
    }

    public FieldGuideEntry GetEntry(string critterName)
    {
        return entries.TryGetValue(critterName, out var entry) ? entry : null;
    }

    public List<FieldGuideEntry> GetAllEntries()
    {
        return new List<FieldGuideEntry>(entries.Values);
    }

    public System.Action<CritterData> OnNewDiscovery;

    private void Save()
    {
        foreach (var kvp in entries)
        {
            string prefix = $"FieldGuide_{kvp.Key}";
            PlayerPrefs.SetInt($"{prefix}_discovered", kvp.Value.discovered ? 1 : 0);
            PlayerPrefs.SetInt($"{prefix}_bestScore", kvp.Value.bestPhotoScore);
            PlayerPrefs.SetInt($"{prefix}_bestRating", (int)kvp.Value.bestRating);
            PlayerPrefs.SetInt($"{prefix}_timesSeen", kvp.Value.timesSeen);
        }
        PlayerPrefs.Save();
    }

    private void Load()
    {
        foreach (var kvp in entries)
        {
            string prefix = $"FieldGuide_{kvp.Key}";
            if (PlayerPrefs.HasKey($"{prefix}_discovered"))
            {
                kvp.Value.discovered = PlayerPrefs.GetInt($"{prefix}_discovered") == 1;
                kvp.Value.bestPhotoScore = PlayerPrefs.GetInt($"{prefix}_bestScore");
                kvp.Value.bestRating = (PhotoRating)PlayerPrefs.GetInt($"{prefix}_bestRating");
                kvp.Value.timesSeen = PlayerPrefs.GetInt($"{prefix}_timesSeen");
            }
        }
    }
}
