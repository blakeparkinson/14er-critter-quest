using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopDownBootstrap : MonoBehaviour
{
    private void Start()
    {
        // input
        new GameObject("GameInput").AddComponent<GameInput>();
        // audio
        new GameObject("AudioManager").AddComponent<AudioManager>();

        // camera setup
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 8;
        Camera.main.backgroundColor = new Color(0.18f, 0.22f, 0.15f);

        // generate world
        var worldObj = new GameObject("World");
        var world = worldObj.AddComponent<TopDownWorld>();
        world.Generate();

        // player
        var player = CreatePlayer(world.PlayerStart);

        // camera follow
        var camFollow = Camera.main.gameObject.AddComponent<TopDownCamera>();
        camFollow.SetTarget(player.transform);

        // critters
        SpawnCritters(world);

        // photo camera on player
        player.AddComponent<TopDownPhotoCamera>();

        // HUD
        CreateHUD();

        // extras
        new GameObject("DiscoveryPopup").AddComponent<DiscoveryPopup>();
        new GameObject("BiomeAmbience").AddComponent<BiomeAmbience>();

        // climb progress bar
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var progress = new GameObject("ClimbProgress").AddComponent<ClimbProgress>();
            progress.Setup(canvas.transform);

            var radar = new GameObject("CritterRadar").AddComponent<CritterRadar>();
            radar.Setup(canvas.transform);
        }

        // title screen (shows on top, pauses game until dismissed)
        var titleObj = new GameObject("TitleScreen");
        var title = titleObj.AddComponent<TitleScreen>();
        title.Show();

        Debug.Log("=== 14er Critter Quest ===");
    }

    private GameObject CreatePlayer(Vector3 startPos)
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = startPos;

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 50;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 0.4f);
        col.offset = new Vector2(0, 0.1f);

        player.AddComponent<TopDownPlayer>();

        return player;
    }

    private void SpawnCritters(TopDownWorld world)
    {
        var critterDataList = new CritterData[CritterDatabase.Templates.Length];
        for (int i = 0; i < CritterDatabase.Templates.Length; i++)
        {
            var t = CritterDatabase.Templates[i];
            var data = ScriptableObject.CreateInstance<CritterData>();
            data.critterName = t.name;
            data.sillyTitle = t.sillyTitle;
            data.fieldGuideEntry = t.entry;
            data.rarity = t.rarity;
            data.personality = t.personality;
            data.sillyActions = t.sillyActions;
            data.moveSpeed = t.personality switch
            {
                CritterPersonality.Shy => 1.5f,
                CritterPersonality.Bold => 0.6f,
                CritterPersonality.Curious => 1f,
                CritterPersonality.Chaotic => 2f,
                _ => 1f
            };
            data.basePhotoScore = t.rarity switch
            {
                CritterRarity.Common => 100,
                CritterRarity.Uncommon => 200,
                CritterRarity.Rare => 350,
                CritterRarity.Legendary => 500,
                _ => 100
            };
            data.detectionRange = 5f;
            data.fleeRange = 2.5f;
            data.idleTimeMin = 1f;
            data.idleTimeMax = 4f;
            data.spawnChance = t.rarity switch
            {
                CritterRarity.Common => 0.7f,
                CritterRarity.Uncommon => 0.4f,
                CritterRarity.Rare => 0.15f,
                CritterRarity.Legendary => 0.05f,
                _ => 0.5f
            };
            critterDataList[i] = data;
        }

        // spawn critters at spawn points
        float totalWeight = 0;
        foreach (var c in critterDataList) totalWeight += c.spawnChance;

        foreach (var spawnPos in world.SpawnPoints)
        {
            // pick weighted random critter
            float roll = Random.Range(0, totalWeight);
            float cum = 0;
            CritterData chosen = critterDataList[0];
            foreach (var c in critterDataList)
            {
                cum += c.spawnChance;
                if (roll <= cum) { chosen = c; break; }
            }

            var critterObj = new GameObject($"Critter_{chosen.critterName}");
            critterObj.transform.position = spawnPos;

            var sr = critterObj.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.GetCritterSprite(chosen.critterName, 0);
            sr.sortingOrder = 50;

            var col = critterObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.5f);
            col.isTrigger = true;

            var critter = critterObj.AddComponent<TopDownCritter>();
            critter.Initialize(chosen);

            // speech bubble
            critterObj.AddComponent<TopDownSpeechBubble>();
        }

        // field guide
        var guideObj = new GameObject("FieldGuide");
        var guide = guideObj.AddComponent<FieldGuide>();

        // set field after AddComponent to avoid Awake null ref
        var field = typeof(FieldGuide).GetField("allCritters",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(guide, critterDataList);
        guide.SendMessage("TryInitialize", SendMessageOptions.DontRequireReceiver);
    }

    private void CreateHUD()
    {
        var canvas = new GameObject("HUD");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        // stamina bar bg
        var staminaBg = MakeRect(canvas.transform, "StaminaBG",
            new Vector2(200, 10), new Vector2(120, -30), new Color(0, 0, 0, 0.4f));
        Anchor(staminaBg, 0, 1, 0, 1);

        var staminaFill = MakeRect(staminaBg.transform, "Fill",
            Vector2.zero, Vector2.zero, new Color(0.35f, 0.82f, 0.3f));
        var fillRT = staminaFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(1, 1); fillRT.offsetMax = new Vector2(-1, -1);
        fillRT.pivot = new Vector2(0, 0.5f);

        MakeText(canvas.transform, "StLabel", "STAMINA", 10,
            new Vector2(200, 16), new Vector2(120, -18), 0, 1, 0, 1)
            .color = new Color(1, 1, 1, 0.5f);

        // altitude (based on Y position)
        var alt = MakeText(canvas.transform, "Alt", "11,000 ft", 20,
            new Vector2(180, 28), new Vector2(-110, -30), 1, 1, 1, 1);
        alt.alignment = TextAlignmentOptions.Right;

        // biome indicator
        var biome = MakeText(canvas.transform, "Biome", "TRAILHEAD", 14,
            new Vector2(180, 22), new Vector2(-110, -52), 1, 1, 1, 1);
        biome.alignment = TextAlignmentOptions.Right;
        biome.color = new Color(1, 1, 1, 0.6f);

        // critter count
        var critters = MakeText(canvas.transform, "Critters", "0/12", 16,
            new Vector2(120, 22), new Vector2(-110, -72), 1, 1, 1, 1);
        critters.alignment = TextAlignmentOptions.Right;

        MakeText(canvas.transform, "GL", "FIELD GUIDE [G]", 9,
            new Vector2(120, 16), new Vector2(-110, -86), 1, 1, 1, 1)
            .color = new Color(1, 1, 1, 0.35f);

        // controls
        MakeText(canvas.transform, "Hint", "WASD Walk | SHIFT Jog | Walk to the Summit!", 11,
            new Vector2(500, 18), new Vector2(0, 12), 0.5f, 0, 0.5f, 0)
            .color = new Color(1, 1, 1, 0.35f);

        // HUD updater
        canvas.AddComponent<TopDownHUD>().Init(fillRT, staminaFill.GetComponent<Image>(), alt, biome, critters);
    }

    private GameObject MakeRect(Transform parent, string name, Vector2 size, Vector2 pos, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        obj.AddComponent<Image>().color = color;
        return obj;
    }

    private TextMeshProUGUI MakeText(Transform parent, string name, string text, int size,
        Vector2 dims, Vector2 pos, float aMinX = 0.5f, float aMinY = 0.5f,
        float aMaxX = 0.5f, float aMaxY = 0.5f)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = dims; rt.anchoredPosition = pos;
        rt.anchorMin = new Vector2(aMinX, aMinY);
        rt.anchorMax = new Vector2(aMaxX, aMaxY);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private void Anchor(GameObject obj, float minX, float minY, float maxX, float maxY)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
    }
}
