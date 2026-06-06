using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClimbProgress : MonoBehaviour
{
    private RectTransform fillRT;
    private Image fillImage;
    private TextMeshProUGUI percentText;
    private Transform player;
    private float mapHeight = 100f;

    private Color[] biomeColors = {
        new Color(0.45f, 0.35f, 0.2f),  // trailhead (brown)
        new Color(0.2f, 0.5f, 0.15f),   // forest (green)
        new Color(0.4f, 0.55f, 0.25f),  // alpine (yellow-green)
        new Color(0.5f, 0.48f, 0.45f),  // tundra (grey)
        new Color(0.9f, 0.92f, 0.95f),  // summit (white)
    };

    public void Setup(Transform canvas)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // vertical progress bar on the right side
        var container = new GameObject("ClimbProgress");
        container.transform.SetParent(canvas, false);
        var crt = container.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(1, 0.15f); crt.anchorMax = new Vector2(1, 0.85f);
        crt.anchoredPosition = new Vector2(-25, 0);
        crt.sizeDelta = new Vector2(12, 0);

        // bg
        var bg = new GameObject("BG");
        bg.transform.SetParent(container.transform, false);
        var bgrt = bg.AddComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0, 0, 0, 0.35f);

        // fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(container.transform, false);
        fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1, 0);
        fillRT.sizeDelta = Vector2.zero;
        fillRT.pivot = new Vector2(0.5f, 0);
        fillImage = fill.AddComponent<Image>();
        fillImage.color = biomeColors[0];

        // player dot indicator
        var dot = new GameObject("Dot");
        dot.transform.SetParent(container.transform, false);
        var drt = dot.AddComponent<RectTransform>();
        drt.sizeDelta = new Vector2(18, 6);
        drt.anchorMin = new Vector2(0.5f, 0); drt.anchorMax = new Vector2(0.5f, 0);
        dot.AddComponent<Image>().color = new Color(1f, 0.9f, 0.3f);

        // summit icon at top
        var summit = new GameObject("SummitIcon");
        summit.transform.SetParent(container.transform, false);
        var srt = summit.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 1); srt.anchorMax = new Vector2(0.5f, 1);
        srt.anchoredPosition = new Vector2(0, 8); srt.sizeDelta = new Vector2(14, 14);
        var stmp = summit.AddComponent<TextMeshProUGUI>();
        stmp.text = "^"; stmp.fontSize = 12;
        stmp.alignment = TextAlignmentOptions.Center;
        stmp.color = new Color(1f, 0.84f, 0f);

        // percentage text
        var pctObj = new GameObject("Pct");
        pctObj.transform.SetParent(container.transform, false);
        var prt = pctObj.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0); prt.anchorMax = new Vector2(0.5f, 0);
        prt.anchoredPosition = new Vector2(-18, 0); prt.sizeDelta = new Vector2(40, 16);
        percentText = pctObj.AddComponent<TextMeshProUGUI>();
        percentText.fontSize = 9;
        percentText.alignment = TextAlignmentOptions.Right;
        percentText.color = new Color(1, 1, 1, 0.5f);
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        float progress = Mathf.Clamp01(player.position.y / mapHeight);
        fillRT.anchorMax = new Vector2(1, progress);

        // color by biome
        int biomeIdx;
        if (progress < 0.12f) biomeIdx = 0;
        else if (progress < 0.38f) biomeIdx = 1;
        else if (progress < 0.58f) biomeIdx = 2;
        else if (progress < 0.8f) biomeIdx = 3;
        else biomeIdx = 4;
        fillImage.color = biomeColors[biomeIdx];

        // move dot
        var dot = fillRT.parent.Find("Dot");
        if (dot != null)
            dot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,
                progress * fillRT.parent.GetComponent<RectTransform>().rect.height);

        percentText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        percentText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-18,
            progress * fillRT.parent.GetComponent<RectTransform>().rect.height);
    }
}
