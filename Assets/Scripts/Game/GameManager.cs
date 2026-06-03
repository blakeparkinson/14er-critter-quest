using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Peak Data")]
    [SerializeField] private PeakData[] peaks;
    [SerializeField] private int currentPeakIndex;

    [Header("Player Stats")]
    [SerializeField] private int totalScore;
    [SerializeField] private int peaksSummited;

    public int TotalScore => totalScore;
    public int PeaksSummited => peaksSummited;
    public PeakData CurrentPeak => peaks != null && currentPeakIndex < peaks.Length ? peaks[currentPeakIndex] : null;

    public System.Action<PeakData> OnPeakSummited;
    public System.Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AddScore(int points)
    {
        totalScore += points;
        OnScoreChanged?.Invoke(totalScore);
        Save();
    }

    public void SummitReached()
    {
        if (CurrentPeak == null) return;

        var peak = CurrentPeak;
        if (!peak.hasBeenSummited)
        {
            peak.hasBeenSummited = true;
            peaksSummited++;
            AddScore(peak.summitBonusPoints);
            OnPeakSummited?.Invoke(peak);
        }
    }

    public void LoadPeak(int index)
    {
        if (index < 0 || index >= peaks.Length) return;
        currentPeakIndex = index;
        SceneManager.LoadScene(peaks[index].sceneName);
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("PeakSelect");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.SetInt("PeaksSummited", peaksSummited);
        PlayerPrefs.SetInt("CurrentPeak", currentPeakIndex);

        if (peaks != null)
        {
            for (int i = 0; i < peaks.Length; i++)
            {
                PlayerPrefs.SetInt($"Peak_{i}_summited", peaks[i].hasBeenSummited ? 1 : 0);
            }
        }

        PlayerPrefs.Save();
    }

    private void Load()
    {
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        peaksSummited = PlayerPrefs.GetInt("PeaksSummited", 0);
        currentPeakIndex = PlayerPrefs.GetInt("CurrentPeak", 0);

        if (peaks != null)
        {
            for (int i = 0; i < peaks.Length; i++)
            {
                peaks[i].hasBeenSummited = PlayerPrefs.GetInt($"Peak_{i}_summited", 0) == 1;
            }
        }
    }
}

[System.Serializable]
public class PeakData
{
    public string peakName;
    public string sceneName;
    public int elevationFeet;
    public string range;
    public string flavorText;
    public int summitBonusPoints = 500;
    public bool hasBeenSummited;
    public Sprite peakIcon;
    public CritterData[] uniqueCritters;
}
