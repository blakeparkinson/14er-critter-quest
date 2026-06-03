using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        SetupLayers();
        var player = CreatePlayer();
        SetupCamera(player.transform);
        var mountain = CreateMountain();
        SetupCritters(mountain);
        CreateHUD();
        CreateBackgrounds();
        SetupAudio();
        SetupWeather();

        // add camera flash to player
        player.AddComponent<CameraFlash>();

        Debug.Log("=== 14er Critter Quest ===");
        Debug.Log("WASD to move, SPACE to jump");
        Debug.Log("TAB to toggle camera mode, CLICK to snap photo");
        Debug.Log("G to open Field Guide");
        Debug.Log("Climb the mountain and photograph critters!");
    }

    private void SetupLayers()
    {
        // Ground layer should be set up in editor, but we work around it
        // by using Default layer and a tag-based approach as fallback
    }

    private GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, 0);

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateHiker();
        sr.sortingOrder = 10;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 3f;

        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 1.4f);
        col.offset = new Vector2(0, 0.2f);

        var pm = player.AddComponent<PhysicsMaterial2D>();
        pm.friction = 0;
        col.sharedMaterial = pm;

        // ground check
        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.5f, 0);

        // wall check
        var wallCheck = new GameObject("WallCheck");
        wallCheck.transform.parent = player.transform;
        wallCheck.transform.localPosition = new Vector3(0.4f, 0.2f, 0);

        var pc = player.AddComponent<PlayerController>();
        SetPrivateField(pc, "groundCheck", groundCheck.transform);
        SetPrivateField(pc, "wallCheck", wallCheck.transform);
        SetPrivateField(pc, "groundLayer", ~0); // all layers for now

        player.AddComponent<PhotoCamera>();
        player.AddComponent<PlayerFX>();

        return player;
    }

    private void SetupCamera(Transform target)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = 7;
        cam.backgroundColor = new Color(0.53f, 0.76f, 0.96f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        var follow = cam.gameObject.AddComponent<CameraFollow>();
        SetPrivateField(follow, "target", target);
    }

    private ProceduralMountain CreateMountain()
    {
        var mountainObj = new GameObject("Mountain");
        var mountain = mountainObj.AddComponent<ProceduralMountain>();
        mountain.Generate(42);
        return mountain;
    }

    private void SetupCritters(ProceduralMountain mountain)
    {
        var spawnerObj = new GameObject("CritterSpawner");
        var spawner = spawnerObj.AddComponent<CritterSpawner>();

        // create critter prefab
        var prefab = CreateCritterPrefab();

        // create critter data assets at runtime
        var critterDataList = new CritterData[CritterDatabase.Templates.Length];
        for (int i = 0; i < CritterDatabase.Templates.Length; i++)
        {
            var template = CritterDatabase.Templates[i];
            var data = ScriptableObject.CreateInstance<CritterData>();
            data.critterName = template.name;
            data.sillyTitle = template.sillyTitle;
            data.fieldGuideEntry = template.entry;
            data.rarity = template.rarity;
            data.personality = template.personality;
            data.sillyActions = template.sillyActions;
            data.sprite = SpriteFactory.CreateCritterSprite(template.name);

            data.moveSpeed = template.personality switch
            {
                CritterPersonality.Shy => 3f,
                CritterPersonality.Bold => 1.5f,
                CritterPersonality.Curious => 2f,
                CritterPersonality.Chaotic => 4f,
                _ => 2f
            };
            data.basePhotoScore = template.rarity switch
            {
                CritterRarity.Common => 100,
                CritterRarity.Uncommon => 200,
                CritterRarity.Rare => 350,
                CritterRarity.Legendary => 500,
                _ => 100
            };
            data.detectionRange = 6f;
            data.fleeRange = 3.5f;
            data.spawnChance = template.rarity switch
            {
                CritterRarity.Common => 0.7f,
                CritterRarity.Uncommon => 0.4f,
                CritterRarity.Rare => 0.15f,
                CritterRarity.Legendary => 0.05f,
                _ => 0.5f
            };
            data.maxAltitude = 999f;

            critterDataList[i] = data;
        }

        SetPrivateField(spawner, "possibleCritters", critterDataList);
        SetPrivateField(spawner, "critterPrefab", prefab);
        SetPrivateField(spawner, "spawnPoints", mountain.SpawnPoints);
        SetPrivateField(spawner, "maxCritters", 10);

        // field guide
        var guideObj = new GameObject("FieldGuide");
        var guide = guideObj.AddComponent<FieldGuide>();
        SetPrivateField(guide, "allCritters", critterDataList);
    }

    private GameObject CreateCritterPrefab()
    {
        var prefab = new GameObject("CritterPrefab");
        prefab.SetActive(false);

        var sr = prefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 8;

        var rb = prefab.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 2f;

        var col = prefab.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.6f);

        prefab.AddComponent<Critter>();
        prefab.AddComponent<CritterSpeechBubble>();

        // don't destroy so it can be used as template
        DontDestroyOnLoad(prefab);
        return prefab;
    }

    private void CreateHUD()
    {
        var canvas = new GameObject("HUD Canvas");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        canvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        // stamina bar
        var staminaBg = CreateUIElement(canvas.transform, "StaminaBG", new Vector2(200, 20),
            new Vector2(120, -30), new Color(0.2f, 0.2f, 0.2f, 0.8f));
        staminaBg.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        staminaBg.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);

        var staminaBar = CreateUIElement(canvas.transform, "StaminaBar", new Vector2(196, 16),
            new Vector2(120, -30), Color.green);
        staminaBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        staminaBar.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        var staminaSlider = staminaBg.AddComponent<Slider>();
        staminaSlider.fillRect = staminaBar.GetComponent<RectTransform>();
        staminaSlider.interactable = false;

        // stamina label
        CreateTextElement(canvas.transform, "StaminaLabel", "STAMINA", 14,
            new Vector2(200, 20), new Vector2(120, -10), TextAlignmentOptions.Left);

        // altitude text
        var altText = CreateTextElement(canvas.transform, "AltitudeText", "11,000 ft", 28,
            new Vector2(300, 40), new Vector2(-160, -30), TextAlignmentOptions.Right);
        altText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        altText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

        // critter count
        var critterText = CreateTextElement(canvas.transform, "CritterCount", "0/12", 22,
            new Vector2(200, 40), new Vector2(-160, -70), TextAlignmentOptions.Right);
        critterText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        critterText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

        var critterLabel = CreateTextElement(canvas.transform, "CritterLabel", "FIELD GUIDE [G]", 12,
            new Vector2(200, 20), new Vector2(-160, -90), TextAlignmentOptions.Right);
        critterLabel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        critterLabel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

        // photo feedback panel (hidden by default)
        var feedbackPanel = CreateUIElement(canvas.transform, "PhotoFeedback", new Vector2(500, 120),
            new Vector2(0, 100), new Color(0, 0, 0, 0.7f));
        feedbackPanel.SetActive(false);
        var feedbackRT = feedbackPanel.GetComponent<RectTransform>();
        feedbackRT.anchorMin = new Vector2(0.5f, 0);
        feedbackRT.anchorMax = new Vector2(0.5f, 0);

        CreateTextElement(feedbackPanel.transform, "RatingText", "", 36,
            new Vector2(480, 50), new Vector2(0, 20), TextAlignmentOptions.Center);
        CreateTextElement(feedbackPanel.transform, "ScoreText", "", 24,
            new Vector2(480, 30), new Vector2(0, -15), TextAlignmentOptions.Center);
        CreateTextElement(feedbackPanel.transform, "CritterName", "", 18,
            new Vector2(480, 25), new Vector2(0, -40), TextAlignmentOptions.Center);

        // camera mode indicator
        var camIndicator = CreateUIElement(canvas.transform, "CameraModeIndicator", new Vector2(250, 40),
            new Vector2(0, -50), new Color(0, 0, 0, 0.6f));
        var camRT = camIndicator.GetComponent<RectTransform>();
        camRT.anchorMin = new Vector2(0.5f, 1);
        camRT.anchorMax = new Vector2(0.5f, 1);
        camIndicator.SetActive(false);

        CreateTextElement(camIndicator.transform, "CamModeText", "CAMERA MODE - Click to Snap!", 18,
            new Vector2(240, 30), Vector2.zero, TextAlignmentOptions.Center);

        // controls hint
        CreateTextElement(canvas.transform, "ControlsHint", "WASD Move | SPACE Jump | TAB Camera | G Guide", 14,
            new Vector2(600, 25), new Vector2(0, 20), TextAlignmentOptions.Center)
            .GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        // fix anchor
        var hint = GameObject.Find("ControlsHint");
        if (hint != null)
        {
            var hrt = hint.GetComponent<RectTransform>();
            hrt.anchorMax = new Vector2(0.5f, 0);
        }

        // wire up HUD manager
        var hud = canvas.AddComponent<HUDManager>();
        SetPrivateField(hud, "staminaBar", staminaSlider);
        SetPrivateField(hud, "staminaFill", staminaBar.GetComponent<Image>());
        SetPrivateField(hud, "altitudeText", altText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hud, "critterCountText", critterText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hud, "photoFeedbackPanel", feedbackPanel);
        SetPrivateField(hud, "photoRatingText",
            feedbackPanel.transform.Find("RatingText")?.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hud, "photoScoreText",
            feedbackPanel.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hud, "photoCritterName",
            feedbackPanel.transform.Find("CritterName")?.GetComponent<TextMeshProUGUI>());
        SetPrivateField(hud, "cameraModeIndicator", camIndicator);

        // viewfinder overlay
        canvas.AddComponent<ViewfinderUI>();
    }

    private void CreateBackgrounds()
    {
        // far mountain silhouette
        CreateBGLayer("FarMountains", -5, 0.1f,
            new Color(0.35f, 0.4f, 0.55f), new Vector3(0, 15, 5));
        // mid mountains
        CreateBGLayer("MidMountains", -3, 0.3f,
            new Color(0.3f, 0.45f, 0.35f), new Vector3(5, 10, 3));
        // clouds
        CreateBGLayer("Clouds", -2, 0.15f,
            new Color(1f, 1f, 1f, 0.4f), new Vector3(-3, 25, 4));
    }

    private void CreateBGLayer(string name, int sortOrder, float parallax, Color color, Vector3 pos)
    {
        var obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(8, 4, 1);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateMountainBG(64, 32, color, Color.white, 0.8f);
        sr.sortingOrder = sortOrder;
        sr.color = color;

        var parallaxComp = obj.AddComponent<ParallaxBackground>();
        SetPrivateField(parallaxComp, "parallaxFactor", parallax);
    }

    private GameObject CreateUIElement(Transform parent, string name, Vector2 size, Vector2 pos, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    private GameObject CreateTextElement(Transform parent, string name, string text, int fontSize,
        Vector2 size, Vector2 pos, TextAlignmentOptions alignment)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableAutoSizing = false;
        return obj;
    }

    private void SetupAudio()
    {
        var audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<AudioManager>();
    }

    private void SetupWeather()
    {
        var weatherObj = new GameObject("WeatherSystem");
        weatherObj.AddComponent<WeatherSystem>();
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
        field?.SetValue(target, value);
    }
}
