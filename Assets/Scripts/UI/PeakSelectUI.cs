using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PeakSelectUI : MonoBehaviour
{
    [SerializeField] private Transform peakListContainer;
    [SerializeField] private GameObject peakCardPrefab;

    [Header("Info Panel")]
    [SerializeField] private TextMeshProUGUI selectedPeakName;
    [SerializeField] private TextMeshProUGUI selectedPeakElevation;
    [SerializeField] private TextMeshProUGUI selectedPeakRange;
    [SerializeField] private TextMeshProUGUI selectedPeakFlavor;
    [SerializeField] private Image selectedPeakIcon;
    [SerializeField] private GameObject summitedBadge;
    [SerializeField] private Button startClimbButton;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI peaksSummitedText;
    [SerializeField] private TextMeshProUGUI crittersFoundText;

    private int selectedIndex = -1;

    private void Start()
    {
        RefreshPeakList();
        UpdateStats();

        if (startClimbButton != null)
            startClimbButton.onClick.AddListener(StartClimb);
    }

    private void RefreshPeakList()
    {
        if (GameManager.Instance == null || peakListContainer == null) return;

        foreach (Transform child in peakListContainer)
            Destroy(child.gameObject);

        var peak = GameManager.Instance.CurrentPeak;

        for (int i = 0; ; i++)
        {
            GameManager.Instance.GetType()
                .GetField("currentPeakIndex",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(GameManager.Instance, i);

            var p = GameManager.Instance.CurrentPeak;
            if (p == null)
            {
                GameManager.Instance.GetType()
                    .GetField("currentPeakIndex",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(GameManager.Instance, 0);
                break;
            }

            if (peakCardPrefab == null) continue;

            GameObject card = Instantiate(peakCardPrefab, peakListContainer);
            var nameText = card.transform.Find("PeakName")?.GetComponent<TextMeshProUGUI>();
            var elevText = card.transform.Find("Elevation")?.GetComponent<TextMeshProUGUI>();
            var badge = card.transform.Find("SummitedBadge")?.gameObject;
            var btn = card.GetComponent<Button>();

            if (nameText != null) nameText.text = p.peakName;
            if (elevText != null) elevText.text = $"{p.elevationFeet:N0} ft";
            if (badge != null) badge.SetActive(p.hasBeenSummited);

            int capturedIndex = i;
            if (btn != null)
                btn.onClick.AddListener(() => SelectPeak(capturedIndex));
        }
    }

    private void SelectPeak(int index)
    {
        selectedIndex = index;

        var prevIndex = GameManager.Instance.GetType()
            .GetField("currentPeakIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int original = (int)(prevIndex?.GetValue(GameManager.Instance) ?? 0);
        prevIndex?.SetValue(GameManager.Instance, index);

        var peak = GameManager.Instance.CurrentPeak;
        prevIndex?.SetValue(GameManager.Instance, original);

        if (peak == null) return;

        if (selectedPeakName != null) selectedPeakName.text = peak.peakName;
        if (selectedPeakElevation != null) selectedPeakElevation.text = $"{peak.elevationFeet:N0} ft";
        if (selectedPeakRange != null) selectedPeakRange.text = peak.range;
        if (selectedPeakFlavor != null) selectedPeakFlavor.text = peak.flavorText;
        if (selectedPeakIcon != null && peak.peakIcon != null) selectedPeakIcon.sprite = peak.peakIcon;
        if (summitedBadge != null) summitedBadge.SetActive(peak.hasBeenSummited);
    }

    private void UpdateStats()
    {
        if (GameManager.Instance == null) return;

        if (totalScoreText != null)
            totalScoreText.text = $"Score: {GameManager.Instance.TotalScore:N0}";
        if (peaksSummitedText != null)
            peaksSummitedText.text = $"Peaks: {GameManager.Instance.PeaksSummited}/58";
        if (crittersFoundText != null && FieldGuide.Instance != null)
            crittersFoundText.text = $"Critters: {FieldGuide.Instance.DiscoveredCount}/{FieldGuide.Instance.TotalCritters}";
    }

    private void StartClimb()
    {
        if (selectedIndex >= 0)
            GameManager.Instance.LoadPeak(selectedIndex);
    }
}
