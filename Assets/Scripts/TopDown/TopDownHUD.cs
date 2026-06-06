using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopDownHUD : MonoBehaviour
{
    private RectTransform staminaFillRT;
    private Image staminaFillImage;
    private TextMeshProUGUI altText;
    private TextMeshProUGUI biomeText;
    private TextMeshProUGUI critterText;
    private TopDownPlayer player;

    public void Init(RectTransform fillRT, Image fillImg, TextMeshProUGUI alt,
        TextMeshProUGUI biome, TextMeshProUGUI critters)
    {
        staminaFillRT = fillRT;
        staminaFillImage = fillImg;
        altText = alt;
        biomeText = biome;
        critterText = critters;
    }

    private void Update()
    {
        if (player == null)
            player = FindFirstObjectByType<TopDownPlayer>();
        if (player == null) return;

        // stamina
        float ratio = player.CurrentStamina / player.MaxStamina;
        if (staminaFillRT != null)
            staminaFillRT.anchorMax = new Vector2(ratio, 1);
        if (staminaFillImage != null)
            staminaFillImage.color = Color.Lerp(
                new Color(0.9f, 0.2f, 0.2f), new Color(0.35f, 0.82f, 0.3f), ratio);

        // altitude based on Y
        float y = player.transform.position.y;
        float progress = Mathf.Clamp01(y / 80f);
        int feet = Mathf.RoundToInt(Mathf.Lerp(11000, 14433, progress));
        if (altText != null)
            altText.text = $"{feet:N0} ft";

        // biome
        if (biomeText != null)
        {
            if (progress < 0.15f) biomeText.text = "TRAILHEAD";
            else if (progress < 0.4f) biomeText.text = "FOREST";
            else if (progress < 0.6f) biomeText.text = "ALPINE MEADOW";
            else if (progress < 0.8f) biomeText.text = "TUNDRA";
            else biomeText.text = "SUMMIT";
        }

        // critters
        if (critterText != null && FieldGuide.Instance != null)
            critterText.text = $"{FieldGuide.Instance.DiscoveredCount}/{FieldGuide.Instance.TotalCritters}";
    }
}
