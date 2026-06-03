using UnityEngine;
using TMPro;
using System.Collections;

public class SummitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject summitUI;
    [SerializeField] private TextMeshProUGUI peakNameText;
    [SerializeField] private TextMeshProUGUI elevationText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private TextMeshProUGUI congratsText;
    [SerializeField] private ParticleSystem summitParticles;

    private bool triggered;

    private string[] congratMessages = {
        "You did it! Your lungs hate you but your Field Guide thanks you!",
        "SUMMIT! The mountain goats are judging you. You passed. Barely.",
        "Peak bagged! That's {0} down, {1} to go. No big deal.",
        "You've reached the top! Oxygen levels: concerning. Vibes: immaculate.",
        "SUMMITED! Quick, take a selfie before the weather turns!",
        "The summit register says 'Gerald the Marmot was here first.' Rude.",
        "You made it! Altitude sickness is just your body celebrating.",
        "TOP OF THE WORLD! Well, top of Colorado. Close enough."
    };

    private void Start()
    {
        if (summitUI != null) summitUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        if (GameManager.Instance != null)
            GameManager.Instance.SummitReached();

        StartCoroutine(ShowSummitCelebration());
    }

    private IEnumerator ShowSummitCelebration()
    {
        if (summitParticles != null)
            summitParticles.Play();

        yield return new WaitForSeconds(0.5f);

        if (summitUI != null)
        {
            summitUI.SetActive(true);

            var peak = GameManager.Instance?.CurrentPeak;
            if (peak != null)
            {
                if (peakNameText != null) peakNameText.text = peak.peakName;
                if (elevationText != null) elevationText.text = $"{peak.elevationFeet:N0} ft";
                if (flavorText != null) flavorText.text = peak.flavorText;
            }

            if (congratsText != null)
            {
                string msg = congratMessages[Random.Range(0, congratMessages.Length)];
                int summited = GameManager.Instance?.PeaksSummited ?? 1;
                int total = 58;
                congratsText.text = string.Format(msg, summited, total - summited);
            }
        }
    }
}
