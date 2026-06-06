using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TopDownPhotoCamera : MonoBehaviour
{
    private bool isActive;
    private Camera cam;
    private TopDownPlayer player;
    private GameObject viewfinder;
    private GameObject feedbackPanel;
    private TextMeshProUGUI ratingText, scoreText, critterNameText;
    private Image flashOverlay;
    private float originalSize;
    private float currentZoom;
    private float targetZoom;

    private InputAction zoomAction;

    public bool IsActive => isActive;

    private void Start()
    {
        cam = Camera.main;
        player = GetComponent<TopDownPlayer>();
        originalSize = cam != null ? cam.orthographicSize : 8;
        currentZoom = originalSize;
        targetZoom = originalSize;

        zoomAction = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");
        zoomAction.Enable();

        CreateViewfinderUI();
        CreateFeedbackUI();
        CreateFlashOverlay();
    }

    private void Update()
    {
        var input = GameInput.Instance;
        if (input == null) return;

        if (input.ToggleCameraAction.WasPressedThisFrame())
            ToggleCamera();

        if (!isActive) return;

        // zoom
        float scroll = zoomAction.ReadValue<float>();
        if (scroll != 0)
        {
            targetZoom = Mathf.Clamp(targetZoom - scroll * 0.003f, 3f, originalSize);
        }
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, 8f * Time.deltaTime);
        if (cam != null) cam.orthographicSize = currentZoom;

        // snap photo
        if (input.SnapPhotoAction.WasPressedThisFrame())
            TakePhoto();
    }

    private void ToggleCamera()
    {
        isActive = !isActive;
        player.SetCameraMode(isActive);

        if (viewfinder != null) viewfinder.SetActive(isActive);

        if (!isActive && cam != null)
        {
            targetZoom = originalSize;
            currentZoom = originalSize;
            cam.orthographicSize = originalSize;
        }
    }

    private void TakePhoto()
    {
        // find critters in view
        var critters = new List<TopDownCritter>();
        float viewRadius = currentZoom * 0.8f;
        Vector2 playerPos = transform.position;

        foreach (var critter in FindObjectsByType<TopDownCritter>(FindObjectsSortMode.None))
        {
            float dist = Vector2.Distance(playerPos, critter.transform.position);
            if (dist < viewRadius)
                critters.Add(critter);
        }

        AudioManager.Instance?.PlaySFX("shutter");
        StartCoroutine(FlashEffect());

        if (critters.Count > 0)
        {
            var result = ScorePhoto(critters);
            ShowFeedback(result);

            // photo effects (screen shake + sparkles)
            var fx = GetComponent<PhotoEffects>();
            if (fx != null) fx.TriggerPhotoEffect(transform.position, result.rating);

            foreach (var c in critters)
                c.OnPhotographed();

            if (FieldGuide.Instance != null)
                FieldGuide.Instance.RecordPhoto(result);
        }
        else
        {
            string[] misses = {
                "Nice rock, I guess.",
                "A scenic landscape... but where's the critter?",
                "You photographed nothing. Bold choice.",
                "The trail doesn't count as wildlife."
            };
            ShowFeedback(new PhotoResult
            {
                rating = PhotoRating.NoCritter,
                ratingText = misses[Random.Range(0, misses.Length)],
                score = 0
            });
        }
    }

    private PhotoResult ScorePhoto(List<TopDownCritter> critters)
    {
        float totalScore = 0;
        CritterData bestCritter = null;
        float bestScore = 0;

        foreach (var c in critters)
        {
            float dist = Vector2.Distance(transform.position, c.transform.position);
            float distScore = Mathf.Lerp(1.5f, 0.5f, dist / (currentZoom * 0.8f));
            float zoomBonus = Mathf.Lerp(1f, 2.5f, 1f - (currentZoom - 3f) / (originalSize - 3f));
            float rarityBonus = c.Data.rarity switch
            {
                CritterRarity.Common => 1f,
                CritterRarity.Uncommon => 1.5f,
                CritterRarity.Rare => 2.5f,
                CritterRarity.Legendary => 5f,
                _ => 1f
            };
            float sillyBonus = c.IsDoingSillyAction ? 2f : 1f;

            float score = c.Data.basePhotoScore * distScore * zoomBonus * rarityBonus * sillyBonus;
            totalScore += score;

            if (score > bestScore) { bestScore = score; bestCritter = c.Data; }
        }

        if (critters.Count > 1) totalScore += critters.Count * 50;

        PhotoRating rating;
        string text;

        if (totalScore >= 500) { rating = PhotoRating.NationalGeographic; text = "NATIONAL GEOGRAPHIC!"; }
        else if (totalScore >= 300) { rating = PhotoRating.Majestic; text = "Majestic!"; }
        else if (totalScore >= 150) { rating = PhotoRating.Decent; text = "Decent Shot!"; }
        else { rating = PhotoRating.Blurry; text = "Blurry but Valid!"; }

        if (critters.Count >= 3) text += " GROUP PHOTO!";
        if (critters.Exists(c => c.IsDoingSillyAction)) text += " Caught in the act!";

        return new PhotoResult
        {
            rating = rating,
            ratingText = text,
            score = Mathf.RoundToInt(totalScore),
            primaryCritter = bestCritter,
            critterCount = critters.Count,
            hadSillyAction = critters.Exists(c => c.IsDoingSillyAction)
        };
    }

    private void ShowFeedback(PhotoResult result)
    {
        if (feedbackPanel == null) return;
        StopCoroutine("HideFeedback");
        feedbackPanel.SetActive(true);

        if (ratingText != null)
        {
            ratingText.text = result.ratingText;
            ratingText.color = result.rating switch
            {
                PhotoRating.NationalGeographic => new Color(1f, 0.84f, 0f),
                PhotoRating.Majestic => new Color(0.7f, 0.3f, 1f),
                PhotoRating.Decent => new Color(0.3f, 0.85f, 0.9f),
                _ => new Color(0.7f, 0.7f, 0.7f)
            };
        }
        if (scoreText != null)
            scoreText.text = result.score > 0 ? $"+{result.score}" : "";
        if (critterNameText != null)
            critterNameText.text = result.primaryCritter != null
                ? $"{result.primaryCritter.critterName} — \"{result.primaryCritter.sillyTitle}\""
                : "";

        StartCoroutine(HideFeedback());
    }

    private IEnumerator HideFeedback()
    {
        yield return new WaitForSeconds(3f);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    private IEnumerator FlashEffect()
    {
        if (flashOverlay == null) yield break;
        flashOverlay.color = new Color(1, 1, 1, 0.6f);
        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            flashOverlay.color = new Color(1, 1, 1, 0.6f * (1f - t / 0.15f));
            yield return null;
        }
        flashOverlay.color = Color.clear;
    }

    private void CreateViewfinderUI()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        viewfinder = new GameObject("Viewfinder");
        viewfinder.transform.SetParent(canvas.transform, false);
        var rt = viewfinder.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // corner brackets
        Color bc = new Color(1, 1, 1, 0.7f);
        float off = 80, sz = 25, th = 2;
        MakeLine(viewfinder.transform, -off, off, sz, th, bc);
        MakeLine(viewfinder.transform, -off, off, th, sz, bc);
        MakeLine(viewfinder.transform, off, off, sz, th, bc);
        MakeLine(viewfinder.transform, off, off, th, sz, bc);
        MakeLine(viewfinder.transform, -off, -off, sz, th, bc);
        MakeLine(viewfinder.transform, -off, -off, th, sz, bc);
        MakeLine(viewfinder.transform, off, -off, sz, th, bc);
        MakeLine(viewfinder.transform, off, -off, th, sz, bc);

        // center dot
        MakeLine(viewfinder.transform, 0, 0, 4, 4, new Color(1, 1, 1, 0.5f));

        // label
        var label = new GameObject("Label");
        label.transform.SetParent(viewfinder.transform, false);
        var lrt = label.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 1); lrt.anchorMax = new Vector2(0.5f, 1);
        lrt.anchoredPosition = new Vector2(0, -30); lrt.sizeDelta = new Vector2(300, 25);
        var tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = "CAMERA MODE — Click to Snap | Scroll to Zoom | TAB to Exit";
        tmp.fontSize = 12; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1, 0.9f, 0.7f, 0.8f);

        viewfinder.SetActive(false);
    }

    private void CreateFeedbackUI()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        feedbackPanel = new GameObject("PhotoFeedback");
        feedbackPanel.transform.SetParent(canvas.transform, false);
        var rt = feedbackPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 80); rt.sizeDelta = new Vector2(450, 85);
        var img = feedbackPanel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.6f);

        ratingText = MakeTextChild(feedbackPanel.transform, "Rating", "", 26,
            new Vector2(430, 30), new Vector2(0, 18));
        scoreText = MakeTextChild(feedbackPanel.transform, "Score", "", 18,
            new Vector2(430, 22), new Vector2(0, -5));
        critterNameText = MakeTextChild(feedbackPanel.transform, "Name", "", 13,
            new Vector2(430, 18), new Vector2(0, -24));
        critterNameText.fontStyle = FontStyles.Italic;
        critterNameText.color = new Color(1, 1, 1, 0.7f);

        feedbackPanel.SetActive(false);
    }

    private void CreateFlashOverlay()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var flash = new GameObject("Flash");
        flash.transform.SetParent(canvas.transform, false);
        var rt = flash.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        flashOverlay = flash.AddComponent<Image>();
        flashOverlay.color = Color.clear;
        flashOverlay.raycastTarget = false;
    }

    private void MakeLine(Transform parent, float x, float y, float w, float h, Color c)
    {
        var obj = new GameObject("L");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h);
        obj.AddComponent<Image>().color = c;
    }

    private TextMeshProUGUI MakeTextChild(Transform parent, string name, string text, int size,
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
        zoomAction?.Dispose();
    }
}
