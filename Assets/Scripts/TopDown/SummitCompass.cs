using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummitCompass : MonoBehaviour
{
    private RectTransform arrow;
    private TextMeshProUGUI distText;
    private Transform player;
    private Vector3 summitPos;

    public void Setup(Transform canvas, Vector3 summit)
    {
        summitPos = summit;

        // container
        var container = new GameObject("Compass");
        container.transform.SetParent(canvas, false);
        var crt = container.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 1);
        crt.anchorMax = new Vector2(0.5f, 1);
        crt.anchoredPosition = new Vector2(0, -70);
        crt.sizeDelta = new Vector2(120, 40);

        // arrow
        var arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(container.transform, false);
        arrow = arrowObj.AddComponent<RectTransform>();
        arrow.sizeDelta = new Vector2(24, 24);
        arrow.anchoredPosition = new Vector2(-30, 0);
        var arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
        arrowText.text = "▲";
        arrowText.fontSize = 20;
        arrowText.alignment = TextAlignmentOptions.Center;
        arrowText.color = new Color(1f, 0.84f, 0f);

        // distance text
        var distObj = new GameObject("Dist");
        distObj.transform.SetParent(container.transform, false);
        var drt = distObj.AddComponent<RectTransform>();
        drt.sizeDelta = new Vector2(80, 24);
        drt.anchoredPosition = new Vector2(20, 0);
        distText = distObj.AddComponent<TextMeshProUGUI>();
        distText.fontSize = 13;
        distText.alignment = TextAlignmentOptions.Left;
        distText.color = new Color(1, 1, 1, 0.6f);
    }

    private void Update()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        Vector3 dir = summitPos - player.position;
        float dist = dir.magnitude;

        // rotate arrow to point at summit
        if (arrow != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            arrow.localRotation = Quaternion.Euler(0, 0, angle);
        }

        // distance text
        if (distText != null)
        {
            if (dist < 3f)
                distText.text = "SUMMIT!";
            else
                distText.text = $"{Mathf.RoundToInt(dist * 50)}m to summit";
        }
    }
}
