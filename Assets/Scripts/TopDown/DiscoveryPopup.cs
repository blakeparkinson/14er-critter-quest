using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiscoveryPopup : MonoBehaviour
{
    private GameObject panel;
    private TextMeshProUGUI titleText, nameText, descText;
    private Image critterImage, rarityBorder;

    private void Start()
    {
        CreateUI();

        if (FieldGuide.Instance != null)
            FieldGuide.Instance.OnNewDiscovery += ShowDiscovery;
    }

    private void ShowDiscovery(CritterData critter)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateDiscovery(critter));
    }

    private IEnumerator AnimateDiscovery(CritterData critter)
    {
        AudioManager.Instance?.PlaySFX("discovery");

        panel.SetActive(true);
        titleText.text = "NEW CRITTER DISCOVERED!";
        nameText.text = critter.critterName;
        descText.text = $"\"{critter.sillyTitle}\"\n\n{critter.fieldGuideEntry}";

        // rarity color
        Color rc = critter.rarity switch
        {
            CritterRarity.Common => Color.white,
            CritterRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f),
            CritterRarity.Rare => new Color(0.3f, 0.5f, 1f),
            CritterRarity.Legendary => new Color(1f, 0.84f, 0f),
            _ => Color.white
        };
        rarityBorder.color = rc;
        nameText.color = rc;

        titleText.color = new Color(1f, 0.9f, 0.3f);

        // critter sprite
        var sprite = PixelArt.GetCritterSprite(critter.critterName, 0);
        if (sprite != null) critterImage.sprite = sprite;

        // animate in (scale pop)
        float t = 0;
        var panelRT = panel.GetComponent<RectTransform>();
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(0.5f, 1f, Mathf.SmoothStep(0, 1, t / 0.3f));
            panelRT.localScale = Vector3.one * scale;
            yield return null;
        }
        panelRT.localScale = Vector3.one;

        // pause game briefly
        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(4f);

        Time.timeScale = 1f;

        // fade out
        t = 0;
        var canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panel.AddComponent<CanvasGroup>();
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - t / 0.5f;
            yield return null;
        }

        panel.SetActive(false);
        canvasGroup.alpha = 1f;
    }

    private void CreateUI()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        panel = new GameObject("DiscoveryPopup");
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(500, 280);

        // dark background
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.06f, 0.12f, 0.92f);

        // rarity border
        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var brt = border.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(-3, -3); brt.offsetMax = new Vector2(3, 3);
        brt.SetAsFirstSibling();
        rarityBorder = border.AddComponent<Image>();
        rarityBorder.color = Color.white;

        // inner bg (slightly inset)
        var inner = new GameObject("Inner");
        inner.transform.SetParent(panel.transform, false);
        var irt = inner.AddComponent<RectTransform>();
        irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(3, 3); irt.offsetMax = new Vector2(-3, -3);
        inner.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.12f, 0.95f);

        // "NEW CRITTER DISCOVERED!"
        titleText = MakeText(panel.transform, "Title", "", 22,
            new Vector2(480, 30), new Vector2(0, 110));

        // critter sprite (big)
        var spriteObj = new GameObject("CritterSprite");
        spriteObj.transform.SetParent(panel.transform, false);
        var srt = spriteObj.AddComponent<RectTransform>();
        srt.anchoredPosition = new Vector2(-150, 10); srt.sizeDelta = new Vector2(100, 100);
        critterImage = spriteObj.AddComponent<Image>();
        critterImage.preserveAspect = true;

        // name
        nameText = MakeText(panel.transform, "Name", "", 28,
            new Vector2(280, 35), new Vector2(50, 50));

        // description
        descText = MakeText(panel.transform, "Desc", "", 13,
            new Vector2(280, 120), new Vector2(50, -20));
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = new Color(0.8f, 0.8f, 0.8f);
        descText.enableWordWrapping = true;

        panel.SetActive(false);
    }

    private TextMeshProUGUI MakeText(Transform parent, string name, string text, int size,
        Vector2 dims, Vector2 pos)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = dims; rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        return tmp;
    }

    private void OnDestroy()
    {
        if (FieldGuide.Instance != null)
            FieldGuide.Instance.OnNewDiscovery -= ShowDiscovery;
    }
}
