using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    private GameObject titlePanel;
    private bool started;

    private string[] taglines = {
        "58 Peaks. Countless Critters. One Camera.",
        "The goats are waiting.",
        "Altitude: High. Standards: Low.",
        "Gerald the Marmot sends his regards.",
        "Now with 73% more marmot.",
        "Warning: May cause spontaneous hiking.",
        "The pikas believe in you.",
        "Thin air, thick vibes."
    };

    public System.Action OnStart;

    public void Show()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        titlePanel = new GameObject("TitleScreen");
        titlePanel.transform.SetParent(canvas.transform, false);
        var rt = titlePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // full background
        var bg = titlePanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.12f, 0.18f);

        // mountain silhouette decoration
        var mtnObj = new GameObject("Mountains");
        mtnObj.transform.SetParent(titlePanel.transform, false);
        var mrt = mtnObj.AddComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0, 0); mrt.anchorMax = new Vector2(1, 0.4f);
        mrt.sizeDelta = Vector2.zero;
        var mtnImg = mtnObj.AddComponent<Image>();
        mtnImg.color = new Color(0.12f, 0.18f, 0.25f);

        // title
        var title = MakeText(titlePanel.transform, "14er\nCritter Quest", 52,
            new Vector2(600, 140), new Vector2(0, 100));
        title.color = new Color(1f, 0.92f, 0.7f);
        title.fontStyle = FontStyles.Bold;

        // tagline
        var tagline = MakeText(titlePanel.transform, taglines[Random.Range(0, taglines.Length)], 18,
            new Vector2(500, 30), new Vector2(0, 15));
        tagline.color = new Color(0.7f, 0.75f, 0.85f);
        tagline.fontStyle = FontStyles.Italic;

        // critter parade (pixel art sprites in a row)
        SpawnCritterParade(titlePanel.transform);

        // start prompt
        var prompt = MakeText(titlePanel.transform, "Press SPACE or ENTER to start", 20,
            new Vector2(500, 30), new Vector2(0, -120));
        prompt.color = new Color(0.8f, 0.85f, 0.9f);
        StartCoroutine(PulseText(prompt));

        // version/credit
        var credit = MakeText(titlePanel.transform, "A silly game about hiking and wildlife photography", 12,
            new Vector2(500, 20), new Vector2(0, -180));
        credit.color = new Color(0.4f, 0.45f, 0.5f);

        // controls preview
        var controls = MakeText(titlePanel.transform,
            "WASD Move  |  SHIFT Jog  |  TAB Camera  |  CLICK Snap  |  G Field Guide", 11,
            new Vector2(700, 18), new Vector2(0, -210));
        controls.color = new Color(0.35f, 0.4f, 0.45f);

        StartCoroutine(WaitForStart());
    }

    private void SpawnCritterParade(Transform parent)
    {
        string[] critterNames = { "Gerald", "Beatrice", "Reginald", "Steve", "Gremlin" };
        float startX = -180;

        for (int i = 0; i < critterNames.Length; i++)
        {
            var sprite = PixelArt.GetCritterSprite(critterNames[i], 0);
            if (sprite == null) continue;

            var obj = new GameObject($"Critter_{critterNames[i]}");
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * 90, -55);
            rt.sizeDelta = new Vector2(50, 50);
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;

            // little bounce animation
            StartCoroutine(BounceSprite(rt, i * 0.3f));
        }
    }

    private IEnumerator BounceSprite(RectTransform rt, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        float baseY = rt.anchoredPosition.y;
        while (!started && rt != null)
        {
            float t = Time.unscaledTime * 2f + delay;
            float bounce = Mathf.Abs(Mathf.Sin(t)) * 8f;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, baseY + bounce);
            yield return new WaitForSecondsRealtime(0.016f);
        }
    }

    private IEnumerator PulseText(TextMeshProUGUI text)
    {
        while (!started && text != null)
        {
            float a = Mathf.Lerp(0.4f, 1f, (Mathf.Sin(Time.unscaledTime * 3f) + 1f) * 0.5f);
            text.color = new Color(text.color.r, text.color.g, text.color.b, a);
            yield return new WaitForSecondsRealtime(0.016f);
        }
    }

    private IEnumerator WaitForStart()
    {
        // freeze game but keep input working
        Time.timeScale = 0f;

        // wait a frame before accepting input (avoid instant skip)
        yield return null;
        yield return null;

        while (!started)
        {
            // check input every frame using realtime wait
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame))
            {
                started = true;
            }
            yield return new WaitForSecondsRealtime(0.016f); // ~60fps polling
        }

        AudioManager.Instance?.PlaySFX("discovery");

        // fade out
        var cg = titlePanel.AddComponent<CanvasGroup>();
        float t = 0;
        while (t < 0.8f)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - t / 0.8f;
            yield return new WaitForSecondsRealtime(0.016f);
        }

        Time.timeScale = 1f;
        Destroy(titlePanel);
        OnStart?.Invoke();
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
