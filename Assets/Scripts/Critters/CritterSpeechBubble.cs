using UnityEngine;
using TMPro;

public class CritterSpeechBubble : MonoBehaviour
{
    private TextMeshPro bubbleText;
    private SpriteRenderer bubbleBG;
    private Critter critter;
    private float showTimer;
    private float bobOffset;

    private void Start()
    {
        critter = GetComponent<Critter>();

        // background
        var bgObj = new GameObject("BubbleBG");
        bgObj.transform.parent = transform;
        bgObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        bubbleBG = bgObj.AddComponent<SpriteRenderer>();
        bubbleBG.sprite = SpriteFactory.CreateRect("bubble", 32, 10, new Color(1, 1, 1, 0.9f));
        bubbleBG.sortingOrder = 15;
        bgObj.SetActive(false);

        // text
        var textObj = new GameObject("BubbleText");
        textObj.transform.parent = bgObj.transform;
        textObj.transform.localPosition = new Vector3(0, 0, -0.1f);
        bubbleText = textObj.AddComponent<TextMeshPro>();
        bubbleText.fontSize = 3;
        bubbleText.alignment = TextAlignmentOptions.Center;
        bubbleText.color = Color.black;
        bubbleText.sortingOrder = 16;
        bubbleText.rectTransform.sizeDelta = new Vector2(3, 1);

        bobOffset = Random.Range(0f, Mathf.PI * 2);
    }

    private void Update()
    {
        if (critter == null) return;

        if (critter.IsDoingSillyAction && !string.IsNullOrEmpty(critter.CurrentSillyAction))
        {
            ShowBubble(critter.CurrentSillyAction);
        }

        if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            // bob up and down
            if (bubbleBG != null)
            {
                float bob = Mathf.Sin((Time.time + bobOffset) * 3f) * 0.05f;
                bubbleBG.transform.localPosition = new Vector3(0, 1.2f + bob, 0);
            }

            if (showTimer <= 0)
                HideBubble();
        }
    }

    public void ShowBubble(string text, float duration = 3f)
    {
        if (bubbleBG == null) return;
        bubbleBG.gameObject.SetActive(true);
        bubbleText.text = text;
        showTimer = duration;

        // resize bg to fit text
        float textWidth = Mathf.Max(text.Length * 0.15f, 1f);
        bubbleBG.transform.localScale = new Vector3(textWidth, 0.6f, 1);
    }

    private void HideBubble()
    {
        if (bubbleBG != null)
            bubbleBG.gameObject.SetActive(false);
    }
}
