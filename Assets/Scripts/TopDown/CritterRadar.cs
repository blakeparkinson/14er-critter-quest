using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CritterRadar : MonoBehaviour
{
    private Transform player;
    private Image radarDot;
    private TextMeshProUGUI nearbyText;
    private float detectRange = 8f;
    private float pulseTimer;

    public void Setup(Transform canvas)
    {
        // small indicator at bottom-left
        var container = new GameObject("CritterRadar");
        container.transform.SetParent(canvas, false);
        var rt = container.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(30, 40); rt.sizeDelta = new Vector2(20, 20);

        radarDot = container.AddComponent<Image>();
        radarDot.color = Color.clear;

        // text
        var textObj = new GameObject("NearbyText");
        textObj.transform.SetParent(container.transform, false);
        var trt = textObj.AddComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(50, 0); trt.sizeDelta = new Vector2(120, 20);
        nearbyText = textObj.AddComponent<TextMeshProUGUI>();
        nearbyText.fontSize = 11;
        nearbyText.alignment = TextAlignmentOptions.Left;
        nearbyText.color = Color.clear;
    }

    private void Update()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        // find closest critter
        TopDownCritter closest = null;
        float closestDist = float.MaxValue;
        int nearbyCount = 0;

        foreach (var critter in FindObjectsByType<TopDownCritter>(FindObjectsSortMode.None))
        {
            float dist = Vector2.Distance(player.position, critter.transform.position);
            if (dist < detectRange)
            {
                nearbyCount++;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = critter;
                }
            }
        }

        if (nearbyCount > 0 && closest != null)
        {
            pulseTimer += Time.deltaTime * 4f;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;

            // closer = faster pulse, brighter
            float intensity = Mathf.Lerp(0.3f, 1f, 1f - closestDist / detectRange);

            Color rarityColor = closest.Data.rarity switch
            {
                CritterRarity.Common => Color.white,
                CritterRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f),
                CritterRarity.Rare => new Color(0.3f, 0.5f, 1f),
                CritterRarity.Legendary => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };

            radarDot.color = rarityColor * new Color(1, 1, 1, intensity * pulse);
            nearbyText.text = nearbyCount > 1 ? $"Critters nearby: {nearbyCount}" : "Critter nearby!";
            nearbyText.color = new Color(1, 1, 1, intensity * 0.7f);

            // play subtle sound on first detection
            if (nearbyCount > 0 && closestDist < 4f && Random.value < 0.005f)
                AudioManager.Instance?.PlaySFX("squeak");
        }
        else
        {
            radarDot.color = Color.clear;
            nearbyText.color = Color.clear;
            pulseTimer = 0;
        }
    }
}
