using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewfinderUI : MonoBehaviour
{
    private GameObject viewfinderOverlay;
    private Image[] cornerBrackets;
    private TextMeshProUGUI zoomText;
    private Image crosshairDot;
    private PhotoCamera photoCamera;

    private void Start()
    {
        photoCamera = FindFirstObjectByType<PhotoCamera>();
        if (photoCamera == null) return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        viewfinderOverlay = new GameObject("ViewfinderOverlay");
        viewfinderOverlay.transform.SetParent(canvas.transform, false);
        var rt = viewfinderOverlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // vignette darkening on edges
        CreateVignette(viewfinderOverlay.transform);

        // corner brackets
        CreateCornerBrackets(viewfinderOverlay.transform);

        // center crosshair
        CreateCrosshair(viewfinderOverlay.transform);

        // zoom indicator
        var zoomObj = new GameObject("ZoomText");
        zoomObj.transform.SetParent(viewfinderOverlay.transform, false);
        var zoomRT = zoomObj.AddComponent<RectTransform>();
        zoomRT.anchorMin = new Vector2(1, 0);
        zoomRT.anchorMax = new Vector2(1, 0);
        zoomRT.anchoredPosition = new Vector2(-80, 40);
        zoomRT.sizeDelta = new Vector2(150, 30);
        zoomText = zoomObj.AddComponent<TextMeshProUGUI>();
        zoomText.text = "1.0x";
        zoomText.fontSize = 18;
        zoomText.color = Color.white;
        zoomText.alignment = TextAlignmentOptions.Right;

        // "REC" style indicator
        var recObj = new GameObject("RecIndicator");
        recObj.transform.SetParent(viewfinderOverlay.transform, false);
        var recRT = recObj.AddComponent<RectTransform>();
        recRT.anchorMin = new Vector2(0, 1);
        recRT.anchorMax = new Vector2(0, 1);
        recRT.anchoredPosition = new Vector2(80, -60);
        recRT.sizeDelta = new Vector2(150, 30);
        var recText = recObj.AddComponent<TextMeshProUGUI>();
        recText.text = "PHOTO MODE";
        recText.fontSize = 16;
        recText.color = new Color(1f, 0.3f, 0.3f);
        recText.alignment = TextAlignmentOptions.Left;

        viewfinderOverlay.SetActive(false);
    }

    private void Update()
    {
        if (photoCamera == null) return;

        bool shouldShow = photoCamera.IsActive;
        if (viewfinderOverlay != null && viewfinderOverlay.activeSelf != shouldShow)
            viewfinderOverlay.SetActive(shouldShow);
    }

    private void CreateVignette(Transform parent)
    {
        // top bar
        CreateBar(parent, "TopBar", new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, -25), new Vector2(0, 50));
        // bottom bar
        CreateBar(parent, "BottomBar", new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 25), new Vector2(0, 50));
        // left bar
        CreateBar(parent, "LeftBar", new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(25, 0), new Vector2(50, 0));
        // right bar
        CreateBar(parent, "RightBar", new Vector2(1, 0), new Vector2(1, 1),
            new Vector2(-25, 0), new Vector2(50, 0));
    }

    private void CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = obj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.4f);
        img.raycastTarget = false;
    }

    private void CreateCornerBrackets(Transform parent)
    {
        Color bracketColor = new Color(1, 1, 1, 0.8f);
        float offset = 100;
        float size = 40;
        float thickness = 3;

        // top-left
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(-offset, offset), new Vector2(size, thickness), bracketColor);
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(-offset, offset), new Vector2(thickness, size), bracketColor);
        // top-right
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(offset, offset), new Vector2(size, thickness), bracketColor);
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(offset, offset), new Vector2(thickness, size), bracketColor);
        // bottom-left
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(-offset, -offset), new Vector2(size, thickness), bracketColor);
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(-offset, -offset), new Vector2(thickness, size), bracketColor);
        // bottom-right
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(offset, -offset), new Vector2(size, thickness), bracketColor);
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(offset, -offset), new Vector2(thickness, size), bracketColor);
    }

    private void CreateBracketLine(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
    {
        var obj = new GameObject("BracketLine");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private void CreateCrosshair(Transform parent)
    {
        // center dot
        var dot = new GameObject("CrosshairDot");
        dot.transform.SetParent(parent, false);
        var rt = dot.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(6, 6);
        crosshairDot = dot.AddComponent<Image>();
        crosshairDot.color = new Color(1, 1, 1, 0.9f);
        crosshairDot.raycastTarget = false;

        // horizontal line
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(-15, 0), new Vector2(20, 1), new Color(1, 1, 1, 0.5f));
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(15, 0), new Vector2(20, 1), new Color(1, 1, 1, 0.5f));
        // vertical line
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(1, 20), new Color(1, 1, 1, 0.5f));
        CreateBracketLine(parent, new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(1, 20), new Color(1, 1, 1, 0.5f));
    }
}
