using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class FieldGuideViewer : MonoBehaviour
{
    private GameObject panel;
    private bool isOpen;
    private InputAction guideAction;

    private void Start()
    {
        guideAction = new InputAction("Guide", InputActionType.Button, "<Keyboard>/g");
        guideAction.Enable();
    }

    private void Update()
    {
        if (guideAction.WasPressedThisFrame())
            Toggle();
    }

    private void Toggle()
    {
        isOpen = !isOpen;
        if (isOpen)
            Show();
        else
            Hide();
    }

    private void Show()
    {
        if (FieldGuide.Instance == null) return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Time.timeScale = 0f;

        panel = new GameObject("FieldGuidePanel");
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // dark background
        panel.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

        // title
        MakeText(panel.transform, "FIELD GUIDE", 28,
            new Vector2(400, 35), new Vector2(0, 0), 0.5f, 0.92f)
            .color = new Color(1f, 0.9f, 0.6f);

        var subtitle = MakeText(panel.transform, "", 14,
            new Vector2(400, 22), new Vector2(0, 0), 0.5f, 0.88f);
        int found = FieldGuide.Instance.DiscoveredCount;
        int total = FieldGuide.Instance.TotalCritters;
        subtitle.text = $"{found} of {total} critters discovered";
        subtitle.color = new Color(0.6f, 0.65f, 0.7f);

        // critter grid
        var entries = FieldGuide.Instance.GetAllEntries();
        int cols = 3;
        float cardW = 180, cardH = 220;
        float startX = -((cols - 1) * cardW * 0.5f);
        float startY = 0;
        int row = 0, col = 0;

        foreach (var entry in entries)
        {
            float x = startX + col * (cardW + 10);
            float y = startY - row * (cardH + 10);

            CreateCritterCard(panel.transform, entry, new Vector2(x, y), new Vector2(cardW, cardH));

            col++;
            if (col >= cols)
            {
                col = 0;
                row++;
            }
        }

        // close hint
        MakeText(panel.transform, "Press G to close", 12,
            new Vector2(200, 20), new Vector2(0, 0), 0.5f, 0.04f)
            .color = new Color(0.5f, 0.5f, 0.55f);
    }

    private void CreateCritterCard(Transform parent, FieldGuideEntry entry, Vector2 pos, Vector2 size)
    {
        var card = new GameObject("Card");
        card.transform.SetParent(parent, false);
        var crt = card.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = pos;
        crt.sizeDelta = size;

        // card background
        Color bgColor = entry.discovered
            ? new Color(0.12f, 0.14f, 0.2f)
            : new Color(0.08f, 0.08f, 0.1f);
        card.AddComponent<Image>().color = bgColor;

        // rarity border
        if (entry.discovered)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(card.transform, false);
            var brt = border.AddComponent<RectTransform>();
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-2, -2);
            brt.offsetMax = new Vector2(2, 2);
            brt.SetAsFirstSibling();
            var bimg = border.AddComponent<Image>();
            bimg.color = entry.critterData.rarity switch
            {
                CritterRarity.Common => new Color(0.4f, 0.4f, 0.4f),
                CritterRarity.Uncommon => new Color(0.2f, 0.7f, 0.2f),
                CritterRarity.Rare => new Color(0.2f, 0.4f, 0.9f),
                CritterRarity.Legendary => new Color(0.9f, 0.75f, 0f),
                _ => Color.gray
            };
        }

        if (entry.discovered)
        {
            // critter sprite
            var spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(card.transform, false);
            var srt = spriteObj.AddComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(0, 40);
            srt.sizeDelta = new Vector2(60, 60);
            var img = spriteObj.AddComponent<Image>();
            img.sprite = PixelArt.GetCritterSprite(entry.critterData.critterName, 0);
            img.preserveAspect = true;

            // name
            MakeTextLocal(card.transform, entry.critterData.critterName, 14,
                new Vector2(size.x - 10, 20), new Vector2(0, -10))
                .color = Color.white;

            // title
            var title = MakeTextLocal(card.transform, $"\"{entry.critterData.sillyTitle}\"", 10,
                new Vector2(size.x - 10, 16), new Vector2(0, -28));
            title.color = new Color(0.7f, 0.7f, 0.75f);
            title.fontStyle = FontStyles.Italic;

            // stats
            string stats = $"Seen: {entry.timesSeen}x";
            if (entry.bestPhotoScore > 0)
                stats += $"  Best: {entry.bestPhotoScore}pts";
            var statsText = MakeTextLocal(card.transform, stats, 9,
                new Vector2(size.x - 10, 14), new Vector2(0, -48));
            statsText.color = new Color(0.5f, 0.55f, 0.6f);

            // rarity label
            var rarityText = MakeTextLocal(card.transform, entry.critterData.rarity.ToString(), 9,
                new Vector2(size.x - 10, 14), new Vector2(0, -62));
            rarityText.color = entry.critterData.rarity switch
            {
                CritterRarity.Common => Color.gray,
                CritterRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f),
                CritterRarity.Rare => new Color(0.4f, 0.6f, 1f),
                CritterRarity.Legendary => new Color(1f, 0.84f, 0f),
                _ => Color.gray
            };
        }
        else
        {
            // undiscovered
            MakeTextLocal(card.transform, "???", 24,
                new Vector2(size.x, 30), new Vector2(0, 20))
                .color = new Color(0.3f, 0.3f, 0.35f);

            MakeTextLocal(card.transform, "Not yet discovered", 10,
                new Vector2(size.x, 16), new Vector2(0, -15))
                .color = new Color(0.25f, 0.25f, 0.3f);
        }
    }

    private void Hide()
    {
        Time.timeScale = 1f;
        if (panel != null)
            Destroy(panel);
    }

    private TextMeshProUGUI MakeText(Transform parent, string text, int size,
        Vector2 dims, Vector2 pos, float anchorX, float anchorY)
    {
        var obj = new GameObject("T");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorX, anchorY);
        rt.anchorMax = new Vector2(anchorX, anchorY);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dims;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private TextMeshProUGUI MakeTextLocal(Transform parent, string text, int size,
        Vector2 dims, Vector2 pos)
    {
        var obj = new GameObject("T");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = dims;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private void OnDestroy()
    {
        guideAction?.Dispose();
    }
}
