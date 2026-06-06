using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SummitTrigger2D : MonoBehaviour
{
    private bool triggered;

    private string[] messages = {
        "Your lungs hate you but your Field Guide thanks you!",
        "The mountain goats are judging you. You passed. Barely.",
        "Oxygen levels: concerning. Vibes: immaculate.",
        "Quick, take a selfie before the weather turns!",
        "The summit register says 'Gerald was here first.' Rude.",
        "Altitude sickness is just your body celebrating.",
        "TOP OF COLORADO! Well, one of 58. Close enough."
    };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        AudioManager.Instance?.PlaySFX("summit");
        StartCoroutine(ShowCelebration());
    }

    private IEnumerator ShowCelebration()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) yield break;

        Time.timeScale = 0.2f;

        var panel = new GameObject("SummitPanel");
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500, 200);
        panel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

        // border
        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var brt = border.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(-3, -3); brt.offsetMax = new Vector2(3, 3);
        brt.SetAsFirstSibling();
        border.AddComponent<Image>().color = new Color(1f, 0.84f, 0f);

        // inner
        var inner = new GameObject("Inner");
        inner.transform.SetParent(panel.transform, false);
        var irt = inner.AddComponent<RectTransform>();
        irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(3, 3); irt.offsetMax = new Vector2(-3, -3);
        inner.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        var title = MakeText(panel.transform, "SUMMIT!", 32,
            new Vector2(480, 40), new Vector2(0, 60));
        title.color = new Color(1f, 0.84f, 0f);

        MakeText(panel.transform, "Mt. Elbert — 14,433 ft", 20,
            new Vector2(480, 28), new Vector2(0, 25));

        var msg = MakeText(panel.transform, messages[Random.Range(0, messages.Length)], 14,
            new Vector2(460, 50), new Vector2(0, -15));
        msg.color = new Color(0.8f, 0.8f, 0.85f);
        msg.fontStyle = FontStyles.Italic;
        msg.enableWordWrapping = true;

        int found = FieldGuide.Instance != null ? FieldGuide.Instance.DiscoveredCount : 0;
        int total = FieldGuide.Instance != null ? FieldGuide.Instance.TotalCritters : 12;
        MakeText(panel.transform, $"Critters photographed: {found}/{total}", 16,
            new Vector2(480, 25), new Vector2(0, -55));

        // animate in
        float t = 0;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            rt.localScale = Vector3.one * Mathf.SmoothStep(0, 1, t / 0.4f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(6f);
        Time.timeScale = 1f;

        // fade
        var cg = panel.AddComponent<CanvasGroup>();
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - t;
            yield return null;
        }
        Destroy(panel);
    }

    private TextMeshProUGUI MakeText(Transform parent, string text, int size, Vector2 dims, Vector2 pos)
    {
        var obj = new GameObject("T");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = dims; rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        return tmp;
    }
}
