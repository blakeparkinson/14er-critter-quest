using UnityEngine;
using UnityEditor;

public class CritterCreator : EditorWindow
{
    [MenuItem("14er Critter Quest/Create All Critter Data")]
    public static void CreateAllCritters()
    {
        string path = "Assets/ScriptableObjects/Critters";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Critters");
        }

        foreach (var template in CritterDatabase.Templates)
        {
            string assetPath = $"{path}/{template.name.Replace(" ", "_")}.asset";
            if (AssetDatabase.LoadAssetAtPath<CritterData>(assetPath) != null)
                continue;

            var data = ScriptableObject.CreateInstance<CritterData>();
            data.critterName = template.name;
            data.sillyTitle = template.sillyTitle;
            data.fieldGuideEntry = template.entry;
            data.rarity = template.rarity;
            data.personality = template.personality;
            data.sillyActions = template.sillyActions;

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

            data.spawnChance = template.rarity switch
            {
                CritterRarity.Common => 0.7f,
                CritterRarity.Uncommon => 0.4f,
                CritterRarity.Rare => 0.15f,
                CritterRarity.Legendary => 0.05f,
                _ => 0.5f
            };

            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {CritterDatabase.Templates.Length} critter ScriptableObjects!");
    }

    [MenuItem("14er Critter Quest/Quick Scene Setup")]
    public static void QuickSceneSetup()
    {
        // Create player
        var playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.layer = LayerMask.NameToLayer("Default");
        var sr = playerObj.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 0.6f, 0.9f);
        var rb = playerObj.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerObj.AddComponent<BoxCollider2D>();
        playerObj.AddComponent<PlayerController>();
        playerObj.AddComponent<PhotoCamera>();

        // Ground check
        var groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = playerObj.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.5f, 0);

        // Wall check
        var wallCheck = new GameObject("WallCheck");
        wallCheck.transform.parent = playerObj.transform;
        wallCheck.transform.localPosition = new Vector3(0.3f, 0, 0);

        // Create ground
        var ground = new GameObject("Ground");
        ground.layer = LayerMask.NameToLayer("Default");
        var groundSr = ground.AddComponent<SpriteRenderer>();
        groundSr.color = new Color(0.4f, 0.3f, 0.2f);
        ground.transform.position = new Vector3(0, -2, 0);
        ground.transform.localScale = new Vector3(30, 1, 1);
        ground.AddComponent<BoxCollider2D>();

        // Camera setup
        var cam = Camera.main;
        if (cam != null)
        {
            cam.gameObject.AddComponent<CameraFollow>();
        }

        Debug.Log("Basic scene setup complete! Assign Ground layer, add sprites, and configure serialized fields.");
    }
}
