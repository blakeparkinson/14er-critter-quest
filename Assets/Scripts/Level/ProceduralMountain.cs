using UnityEngine;
using System.Collections.Generic;

public class ProceduralMountain : MonoBehaviour
{
    [Header("Mountain Shape")]
    [SerializeField] private float mountainWidth = 40f;
    [SerializeField] private float mountainHeight = 60f;
    [SerializeField] private float platformMinWidth = 2f;
    [SerializeField] private float platformMaxWidth = 6f;
    [SerializeField] private float verticalSpacing = 2.5f;
    [SerializeField] private float horizontalVariance = 3f;

    [Header("Visuals")]
    [SerializeField] private Color dirtColor = new Color(0.45f, 0.35f, 0.2f);
    [SerializeField] private Color rockColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color snowColor = new Color(0.95f, 0.97f, 1f);
    [SerializeField] private Color grassColor = new Color(0.3f, 0.55f, 0.2f);

    [Header("Details")]
    [SerializeField] private float treeChance = 0.3f;
    [SerializeField] private float rockDetailChance = 0.2f;
    [SerializeField] private float snowLinePercent = 0.7f;

    private List<Transform> spawnPoints = new List<Transform>();

    public Transform[] SpawnPoints => spawnPoints.ToArray();
    public float Height => mountainHeight;

    public void Generate(int seed = -1)
    {
        if (seed >= 0) Random.InitState(seed);

        float currentY = 0;
        float currentX = 0;
        int platformIndex = 0;

        // trailhead - wide flat start
        CreatePlatform(new Vector2(0, -1), 15f, 1.5f, dirtColor, "Trailhead");
        AddTrailSign(new Vector2(-5, 0.5f));

        while (currentY < mountainHeight)
        {
            float progress = currentY / mountainHeight;
            float narrowing = Mathf.Lerp(mountainWidth * 0.5f, mountainWidth * 0.15f, progress);

            currentX += Random.Range(-horizontalVariance, horizontalVariance);
            currentX = Mathf.Clamp(currentX, -narrowing, narrowing);

            float width = Mathf.Lerp(platformMaxWidth, platformMinWidth, progress);
            width += Random.Range(-1f, 1f);
            width = Mathf.Max(width, platformMinWidth);

            Color platColor;
            if (progress > snowLinePercent)
                platColor = Color.Lerp(rockColor, snowColor, (progress - snowLinePercent) / (1f - snowLinePercent));
            else if (progress > 0.4f)
                platColor = Color.Lerp(dirtColor, rockColor, (progress - 0.4f) / 0.3f);
            else
                platColor = dirtColor;

            Vector2 pos = new Vector2(currentX, currentY);
            CreatePlatform(pos, width, 0.5f + Random.Range(0, 0.3f), platColor, $"Platform_{platformIndex}");

            // add critter spawn point on some platforms
            if (Random.value < 0.4f)
            {
                var spawnObj = new GameObject($"SpawnPoint_{platformIndex}");
                spawnObj.transform.parent = transform;
                spawnObj.transform.position = new Vector3(pos.x + Random.Range(-width * 0.3f, width * 0.3f), pos.y + 1f, 0);
                spawnPoints.Add(spawnObj.transform);
            }

            // decorations
            if (progress < 0.5f && Random.value < treeChance)
                AddTree(new Vector2(pos.x + Random.Range(-width * 0.4f, width * 0.4f), pos.y + 0.5f), progress);
            if (progress > 0.3f && Random.value < rockDetailChance)
                AddRockDetail(new Vector2(pos.x + Random.Range(-width * 0.3f, width * 0.3f), pos.y + 0.3f));

            // occasional wall sections for wall-jumping
            if (Random.value < 0.2f && progress > 0.2f)
            {
                float wallSide = Random.value > 0.5f ? 1 : -1;
                float wallX = currentX + wallSide * (width * 0.5f + 0.5f);
                CreateWall(new Vector2(wallX, currentY), 0.5f, verticalSpacing * 1.5f, rockColor);
            }

            currentY += verticalSpacing + Random.Range(-0.5f, 0.5f);
            platformIndex++;

            // switchback: occasionally shift X dramatically
            if (Random.value < 0.15f)
                currentX *= -0.8f;
        }

        // summit platform
        CreatePlatform(new Vector2(currentX, mountainHeight), 4f, 0.8f, snowColor, "Summit");
        AddSummitMarker(new Vector2(currentX, mountainHeight + 1f));
    }

    private GameObject CreatePlatform(Vector2 pos, float width, float height, Color color, string name)
    {
        var obj = new GameObject(name);
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        obj.layer = LayerMask.NameToLayer("Ground");
        if (obj.layer == -1) obj.layer = 0;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateRect($"plat_{width}_{height}_{color}", (int)(width * 16), (int)(height * 16), color);
        sr.sortingOrder = 1;
        obj.transform.localScale = new Vector3(1, 1, 1);

        var col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, height);

        // grass top for lower platforms
        float progress = pos.y / mountainHeight;
        if (progress < 0.5f)
        {
            var grass = new GameObject("Grass");
            grass.transform.parent = obj.transform;
            grass.transform.localPosition = new Vector3(0, height * 0.5f + 0.05f, 0);
            var grassSr = grass.AddComponent<SpriteRenderer>();
            grassSr.sprite = SpriteFactory.CreateRect($"grass_{width}", (int)(width * 16), 2, grassColor);
            grassSr.sortingOrder = 2;
        }

        return obj;
    }

    private void CreateWall(Vector2 pos, float width, float height, Color color)
    {
        var obj = new GameObject("Wall");
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(pos.x, pos.y + height * 0.5f, 0);
        obj.layer = LayerMask.NameToLayer("Ground");
        if (obj.layer == -1) obj.layer = 0;

        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateRect($"wall_{width}_{height}", (int)(width * 16), (int)(height * 16), color);
        sr.sortingOrder = 1;

        var col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, height);
    }

    private void AddTree(Vector2 pos, float progress)
    {
        var tree = new GameObject("Tree");
        tree.transform.parent = transform;
        tree.transform.position = new Vector3(pos.x, pos.y, 0);

        float treeHeight = Mathf.Lerp(2f, 1f, progress);

        // trunk
        var trunk = new GameObject("Trunk");
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, treeHeight * 0.3f, 0);
        var trunkSr = trunk.AddComponent<SpriteRenderer>();
        trunkSr.sprite = SpriteFactory.CreateRect("trunk", 3, (int)(treeHeight * 5), new Color(0.4f, 0.25f, 0.1f));
        trunkSr.sortingOrder = 0;

        // canopy
        var canopy = new GameObject("Canopy");
        canopy.transform.parent = tree.transform;
        canopy.transform.localPosition = new Vector3(0, treeHeight * 0.7f, 0);
        var canopySr = canopy.AddComponent<SpriteRenderer>();
        Color green = progress < 0.3f
            ? new Color(0.15f, 0.5f, 0.15f)
            : new Color(0.2f, 0.45f, 0.25f);
        canopySr.sprite = SpriteFactory.CreateRect($"canopy_{progress:F1}", 10, (int)(treeHeight * 10), green);
        canopySr.sortingOrder = 0;
    }

    private void AddRockDetail(Vector2 pos)
    {
        var rock = new GameObject("Rock");
        rock.transform.parent = transform;
        rock.transform.position = new Vector3(pos.x, pos.y, 0);
        var sr = rock.AddComponent<SpriteRenderer>();
        Color c = new Color(
            rockColor.r + Random.Range(-0.1f, 0.1f),
            rockColor.g + Random.Range(-0.1f, 0.1f),
            rockColor.b + Random.Range(-0.1f, 0.1f)
        );
        sr.sprite = SpriteFactory.CreateRect($"rock_{pos}", (int)Random.Range(4, 10), (int)Random.Range(3, 7), c);
        sr.sortingOrder = 0;
    }

    private void AddTrailSign(Vector2 pos)
    {
        var sign = new GameObject("TrailSign");
        sign.transform.parent = transform;
        sign.transform.position = new Vector3(pos.x, pos.y, 0);
        var sr = sign.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateRect("sign", 8, 6, new Color(0.6f, 0.4f, 0.15f));
        sr.sortingOrder = 3;
    }

    private void AddSummitMarker(Vector2 pos)
    {
        var marker = new GameObject("SummitMarker");
        marker.transform.parent = transform;
        marker.transform.position = new Vector3(pos.x, pos.y, 0);

        // summit post
        var post = new GameObject("Post");
        post.transform.parent = marker.transform;
        post.transform.localPosition = Vector3.zero;
        var postSr = post.AddComponent<SpriteRenderer>();
        postSr.sprite = SpriteFactory.CreateRect("summit_post", 2, 16, new Color(0.4f, 0.25f, 0.1f));
        postSr.sortingOrder = 3;

        // summit sign
        var signBoard = new GameObject("Sign");
        signBoard.transform.parent = marker.transform;
        signBoard.transform.localPosition = new Vector3(0, 0.6f, 0);
        var signSr = signBoard.AddComponent<SpriteRenderer>();
        signSr.sprite = SpriteFactory.CreateRect("summit_sign", 12, 6, new Color(0.7f, 0.5f, 0.15f));
        signSr.sortingOrder = 4;

        // summit trigger
        var trigger = marker.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(4, 3);
        marker.AddComponent<SummitTrigger>();
    }
}
