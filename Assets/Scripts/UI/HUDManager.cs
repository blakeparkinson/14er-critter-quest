using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Color staminaFullColor = Color.green;
    [SerializeField] private Color staminaLowColor = Color.red;

    [Header("Altitude")]
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private Slider altitudeBar;

    [Header("Field Guide")]
    [SerializeField] private TextMeshProUGUI critterCountText;

    [Header("Photo Feedback")]
    [SerializeField] private GameObject photoFeedbackPanel;
    [SerializeField] private TextMeshProUGUI photoRatingText;
    [SerializeField] private TextMeshProUGUI photoScoreText;
    [SerializeField] private TextMeshProUGUI photoCritterName;
    [SerializeField] private float feedbackDisplayTime = 3f;

    [Header("Discovery")]
    [SerializeField] private GameObject discoveryPanel;
    [SerializeField] private TextMeshProUGUI discoveryNameText;
    [SerializeField] private TextMeshProUGUI discoveryTitleText;
    [SerializeField] private Image discoverySprite;
    [SerializeField] private float discoveryDisplayTime = 4f;

    [Header("Camera Mode")]
    [SerializeField] private GameObject cameraModeIndicator;
    [SerializeField] private TextMeshProUGUI cameraModeText;

    private PlayerController player;
    private PhotoCamera photoCamera;
    private Coroutine feedbackCoroutine;
    private Coroutine discoveryCoroutine;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        photoCamera = FindFirstObjectByType<PhotoCamera>();

        if (photoCamera != null)
            photoCamera.OnPhotoTaken += ShowPhotoFeedback;

        if (FieldGuide.Instance != null)
            FieldGuide.Instance.OnNewDiscovery += ShowDiscovery;

        if (photoFeedbackPanel != null) photoFeedbackPanel.SetActive(false);
        if (discoveryPanel != null) discoveryPanel.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        UpdateStamina();
        UpdateAltitude();
        UpdateCritterCount();
        UpdateCameraMode();
    }

    private void UpdateStamina()
    {
        if (staminaBar == null) return;
        float ratio = player.CurrentStamina / player.MaxStamina;
        staminaBar.value = ratio;
        if (staminaFill != null)
            staminaFill.color = Color.Lerp(staminaLowColor, staminaFullColor, ratio);
    }

    private void UpdateAltitude()
    {
        if (altitudeText == null) return;
        float altitude = player.transform.position.y;
        int feet = Mathf.RoundToInt(11000 + altitude * 100);
        altitudeText.text = $"{feet:N0} ft";
    }

    private void UpdateCritterCount()
    {
        if (critterCountText == null || FieldGuide.Instance == null) return;
        critterCountText.text = $"{FieldGuide.Instance.DiscoveredCount}/{FieldGuide.Instance.TotalCritters}";
    }

    private void UpdateCameraMode()
    {
        if (cameraModeIndicator == null || photoCamera == null) return;
        cameraModeIndicator.SetActive(photoCamera.IsActive);
    }

    private void ShowPhotoFeedback(PhotoResult result)
    {
        if (photoFeedbackPanel == null) return;

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(DisplayPhotoFeedback(result));
    }

    private IEnumerator DisplayPhotoFeedback(PhotoResult result)
    {
        photoFeedbackPanel.SetActive(true);

        if (photoRatingText != null)
        {
            photoRatingText.text = result.ratingText;
            photoRatingText.color = result.rating switch
            {
                PhotoRating.NationalGeographic => new Color(1f, 0.84f, 0f),
                PhotoRating.Majestic => new Color(0.6f, 0.2f, 1f),
                PhotoRating.Decent => Color.cyan,
                PhotoRating.Blurry => Color.gray,
                _ => Color.white
            };
        }

        if (photoScoreText != null)
            photoScoreText.text = result.score > 0 ? $"+{result.score}" : "";

        if (photoCritterName != null)
            photoCritterName.text = result.primaryCritter != null
                ? $"{result.primaryCritter.critterName} - \"{result.primaryCritter.sillyTitle}\""
                : "";

        if (FieldGuide.Instance != null && result.primaryCritter != null)
            FieldGuide.Instance.RecordPhoto(result);

        yield return new WaitForSeconds(feedbackDisplayTime);
        photoFeedbackPanel.SetActive(false);
    }

    private void ShowDiscovery(CritterData critter)
    {
        if (discoveryPanel == null) return;

        if (discoveryCoroutine != null) StopCoroutine(discoveryCoroutine);
        discoveryCoroutine = StartCoroutine(DisplayDiscovery(critter));
    }

    private IEnumerator DisplayDiscovery(CritterData critter)
    {
        discoveryPanel.SetActive(true);

        if (discoveryNameText != null)
            discoveryNameText.text = $"NEW CRITTER DISCOVERED!";
        if (discoveryTitleText != null)
            discoveryTitleText.text = $"{critter.critterName}\n\"{critter.sillyTitle}\"";
        if (discoverySprite != null && critter.sprite != null)
            discoverySprite.sprite = critter.sprite;

        yield return new WaitForSeconds(discoveryDisplayTime);
        discoveryPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (photoCamera != null)
            photoCamera.OnPhotoTaken -= ShowPhotoFeedback;
        if (FieldGuide.Instance != null)
            FieldGuide.Instance.OnNewDiscovery -= ShowDiscovery;
    }
}
