using UnityEngine;
using TMPro;

public class TopDownSpeechBubble : MonoBehaviour
{
    private TopDownCritter critter;
    private GameObject bubble;
    private TextMeshPro text;
    private SpriteRenderer bgSR;

    private void Start()
    {
        critter = GetComponent<TopDownCritter>();

        bubble = new GameObject("Bubble");
        bubble.transform.parent = transform;
        bubble.transform.localPosition = new Vector3(0, 1f, 0);

        // bg
        bgSR = bubble.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(1, 1, 1, 0.85f));
        tex.Apply();
        bgSR.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        bgSR.sortingOrder = 200;

        // text
        var textObj = new GameObject("Text");
        textObj.transform.parent = bubble.transform;
        textObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        text = textObj.AddComponent<TextMeshPro>();
        text.fontSize = 2.5f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.15f, 0.12f, 0.1f);
        text.sortingOrder = 201;
        text.rectTransform.sizeDelta = new Vector2(3, 0.6f);

        bubble.SetActive(false);
    }

    private void Update()
    {
        if (critter == null) return;

        bool show = critter.IsDoingSillyAction && !string.IsNullOrEmpty(critter.CurrentSillyAction);

        if (show && !bubble.activeSelf)
        {
            bubble.SetActive(true);
            text.text = critter.CurrentSillyAction;
            float width = Mathf.Max(critter.CurrentSillyAction.Length * 0.12f, 1.2f);
            bubble.transform.localScale = new Vector3(width, 0.5f, 1);
        }
        else if (!show && bubble.activeSelf)
        {
            bubble.SetActive(false);
        }

        // bob
        if (bubble.activeSelf)
        {
            float bob = Mathf.Sin(Time.time * 3f) * 0.05f;
            bubble.transform.localPosition = new Vector3(0, 1f + bob, 0);
        }
    }
}
