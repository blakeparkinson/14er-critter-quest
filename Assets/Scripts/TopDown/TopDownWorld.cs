using UnityEngine;
using System.Collections.Generic;

public class TopDownWorld : MonoBehaviour
{
    private int mapWidth = 32;
    private int mapHeight = 100;

    private List<Vector3> critterSpawnPoints = new List<Vector3>();
    public Vector3 PlayerStart { get; private set; }
    public Vector3 SummitPos { get; private set; }
    public List<Vector3> SpawnPoints => critterSpawnPoints;

    public void Generate()
    {
        Random.InitState(42);

        for (int y = 0; y < mapHeight; y++)
        {
            float progress = (float)y / mapHeight;

            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 pos = new Vector3(x, y, 0);

                // winding trail — wider and more obvious
                float trailCenter = mapWidth / 2f + Mathf.Sin(y * 0.1f) * 5f + Mathf.Sin(y * 0.04f) * 2f;
                float distFromTrail = Mathf.Abs(x - trailCenter);
                bool isTrail = distFromTrail < 2.5f;
                bool isTrailEdge = distFromTrail >= 2.5f && distFromTrail < 3.2f;

                // create tile
                var tile = new GameObject($"T{x}_{y}");
                tile.transform.parent = transform;
                tile.transform.position = pos;
                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -100;

                BiomeType biome = GetBiome(progress);

                if (isTrail)
                {
                    sr.sprite = PixelArt.DirtTile();
                    // lighter center, darker edges
                    if (distFromTrail < 1f)
                        sr.color = new Color(0.95f, 0.9f, 0.82f);
                    else if (distFromTrail > 1.8f)
                        sr.color = new Color(0.85f, 0.78f, 0.68f);
                }
                else if (isTrailEdge)
                {
                    // rocky edge of trail
                    sr.sprite = PixelArt.RockTile();
                    sr.color = new Color(0.7f, 0.65f, 0.58f, 0.5f);
                }
                else
                {
                    sr.sprite = GetBiomeSprite(biome, x, y);

                    // water features
                    bool isWater = IsWaterAt(x, y, progress);
                    if (isWater)
                    {
                        sr.sprite = WaterTile();
                        sr.color = new Color(0.8f, 0.9f, 1f);
                    }
                    else
                    {
                        // decorations
                        if (Random.value < GetDecoChance(biome, distFromTrail))
                            SpawnDecoration(pos, biome, distFromTrail);

                        // flowers in meadows
                        if (biome == BiomeType.Alpine && IsFlowerMeadow(x, y) && Random.value < 0.4f)
                            SpawnFlower(pos);
                    }
                }

                // critter spawn points — near trail but not on it
                if (distFromTrail > 2.5f && distFromTrail < 9f && Random.value < 0.025f)
                    critterSpawnPoints.Add(pos);
            }
        }

        // special locations
        SpawnTrailhead(new Vector3(mapWidth / 2f, 2, 0));
        SpawnSummitMarker(new Vector3(mapWidth / 2f + Mathf.Sin(mapHeight * 0.12f) * 6f, mapHeight - 3, 0));

        // trail signs at biome transitions
        float[] signYs = { 0.15f, 0.35f, 0.55f, 0.75f, 0.92f };
        string[] signNames = { "Forest Begins", "Alpine Zone", "Above Treeline", "Tundra", "Summit Ahead!" };
        for (int i = 0; i < signYs.Length; i++)
        {
            float sy = signYs[i] * mapHeight;
            float sx = mapWidth / 2f + Mathf.Sin(sy * 0.12f) * 6f + 2.5f;
            SpawnTrailSign(new Vector3(sx, sy, 0), signNames[i]);
        }

        PlayerStart = new Vector3(mapWidth / 2f, 2, 0);
        SummitPos = new Vector3(mapWidth / 2f, mapHeight - 3, 0);
    }

    private BiomeType GetBiome(float progress)
    {
        if (progress < 0.12f) return BiomeType.Trailhead;
        if (progress < 0.38f) return BiomeType.Forest;
        if (progress < 0.58f) return BiomeType.Alpine;
        if (progress < 0.8f) return BiomeType.Tundra;
        return BiomeType.Summit;
    }

    private Sprite GetBiomeSprite(BiomeType biome, int x, int y)
    {
        float n = Mathf.PerlinNoise(x * 0.3f + 42, y * 0.3f);
        return biome switch
        {
            BiomeType.Forest => PixelArt.GrassTile(),
            BiomeType.Alpine => n > 0.45f ? PixelArt.GrassTile() : PixelArt.RockTile(),
            BiomeType.Tundra => n > 0.55f ? PixelArt.RockTile() : PixelArt.GrassTile(),
            BiomeType.Summit => n > 0.35f ? PixelArt.SnowTile() : PixelArt.RockTile(),
            _ => PixelArt.GrassTile()
        };
    }

    private bool IsWaterAt(int x, int y, float progress)
    {
        if (progress > 0.6f) return false; // no water above tundra

        // alpine lake at 45% mark
        float lakeX = mapWidth * 0.65f, lakeY = mapHeight * 0.45f;
        float ld = Mathf.Sqrt((x - lakeX) * (x - lakeX) + (y - lakeY) * (y - lakeY));
        if (ld < 3.5f) return true;

        // stream running down from lake
        if (progress > 0.2f && progress < 0.45f)
        {
            float streamX = mapWidth * 0.65f + Mathf.Sin(y * 0.2f) * 1.5f;
            if (Mathf.Abs(x - streamX) < 0.8f) return true;
        }

        return false;
    }

    private bool IsFlowerMeadow(int x, int y)
    {
        float n = Mathf.PerlinNoise(x * 0.15f + 100, y * 0.15f + 100);
        return n > 0.6f;
    }

    private Sprite WaterTile()
    {
        string key = "water";
        Color[] px = new Color[16 * 16];
        Color w1 = new Color(0.25f, 0.5f, 0.7f);
        Color w2 = new Color(0.3f, 0.55f, 0.75f);
        Color w3 = new Color(0.2f, 0.45f, 0.65f);
        for (int i = 0; i < px.Length; i++)
        {
            float n = Random.value;
            px[i] = n < 0.4f ? w1 : (n < 0.7f ? w2 : w3);
        }
        // sparkle
        for (int i = 0; i < 3; i++)
            px[Random.Range(0, px.Length)] = new Color(0.7f, 0.85f, 0.95f);

        var tex = new Texture2D(16, 16);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    private float GetDecoChance(BiomeType biome, float dist)
    {
        float c = biome switch
        {
            BiomeType.Forest => 0.35f,
            BiomeType.Alpine => 0.12f,
            BiomeType.Tundra => 0.06f,
            BiomeType.Summit => 0.02f,
            _ => 0.15f
        };
        return c * Mathf.Clamp01(dist / 4f);
    }

    private void SpawnDecoration(Vector3 pos, BiomeType biome, float distFromTrail)
    {
        var deco = new GameObject("D");
        deco.transform.parent = transform;
        deco.transform.position = pos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f), 0);
        var sr = deco.AddComponent<SpriteRenderer>();
        sr.sortingOrder = Mathf.RoundToInt(-pos.y * 10) + 5;

        switch (biome)
        {
            case BiomeType.Forest:
                sr.sprite = Random.value > 0.35f ? PixelArt.PineTreeTop() : PixelArt.TreeTop();
                break;
            case BiomeType.Alpine:
                sr.sprite = Random.value > 0.6f ? PixelArt.PineTreeTop() : null;
                if (sr.sprite == null)
                {
                    sr.sprite = PixelArt.RockTile();
                    deco.transform.localScale = new Vector3(0.25f + Random.value * 0.2f, 0.2f, 1);
                }
                break;
            case BiomeType.Tundra:
            case BiomeType.Summit:
                sr.sprite = PixelArt.RockTile();
                float s = 0.2f + Random.value * 0.35f;
                deco.transform.localScale = new Vector3(s, s * 0.7f, 1);
                break;
            default:
                sr.sprite = Random.value > 0.5f ? PixelArt.TreeTop() : PixelArt.PineTreeTop();
                break;
        }
    }

    private void SpawnFlower(Vector3 pos)
    {
        Color[] colors = {
            new Color(0.9f, 0.8f, 0.15f),
            new Color(0.75f, 0.2f, 0.7f),
            new Color(0.9f, 0.3f, 0.3f),
            new Color(0.95f, 0.95f, 0.85f),
            new Color(0.3f, 0.4f, 0.85f)
        };

        var flower = new GameObject("F");
        flower.transform.parent = transform;
        flower.transform.position = pos + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), 0);
        flower.transform.localScale = new Vector3(0.15f, 0.15f, 1);
        var sr = flower.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -5;

        Color c = colors[Random.Range(0, colors.Length)];
        int w = 4, h = 4;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        px[1 * w + 1] = c; px[1 * w + 2] = c;
        px[2 * w + 1] = c; px[2 * w + 2] = c;
        px[0 * w + 1] = new Color(0.3f, 0.5f, 0.2f); // stem

        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(px);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    private void SpawnTrailhead(Vector3 pos)
    {
        var sign = new GameObject("Trailhead");
        sign.transform.parent = transform;
        sign.transform.position = pos + new Vector3(2, 0, 0);
        var sr = sign.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.sprite = MakeSignSprite("TRAILHEAD\nMt. Elbert\n14,433 ft");
    }

    private void SpawnTrailSign(Vector3 pos, string text)
    {
        var sign = new GameObject("Sign");
        sign.transform.parent = transform;
        sign.transform.position = pos;
        var sr = sign.AddComponent<SpriteRenderer>();
        sr.sortingOrder = Mathf.RoundToInt(-pos.y * 10) + 10;

        // wooden post + sign
        int w = 12, h = 10;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        // post
        for (int y = 0; y < 8; y++) { px[y * w + 5] = BT; px[y * w + 6] = BT; }
        // sign board
        for (int y = 6; y < 10; y++)
            for (int x = 1; x < 11; x++)
                px[y * w + x] = new Color(0.6f, 0.42f, 0.18f);

        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(px);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);

        // text label
        var label = new GameObject("Label");
        label.transform.parent = sign.transform;
        label.transform.localPosition = new Vector3(0, 0.5f, 0);
        var tmp = label.AddComponent<TMPro.TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 2;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.2f, 0.15f, 0.1f);
        tmp.sortingOrder = 15;
        tmp.rectTransform.sizeDelta = new Vector2(3, 0.5f);
    }

    private void SpawnSummitMarker(Vector3 pos)
    {
        SummitPos = pos;

        var marker = new GameObject("Summit");
        marker.transform.parent = transform;
        marker.transform.position = pos;

        var col = marker.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(3, 3);
        marker.AddComponent<SummitTrigger2D>();

        var sr = marker.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 100;

        // summit flag sprite
        int w = 10, h = 16;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        // pole
        for (int y = 0; y < 14; y++) px[y * w + 4] = new Color(0.4f, 0.25f, 0.1f);
        // flag
        Color flagC = new Color(0.9f, 0.2f, 0.15f);
        for (int y = 10; y < 15; y++)
            for (int x = 5; x < 9; x++)
                px[y * w + x] = flagC;
        // star on flag
        px[12 * w + 6] = new Color(1f, 0.9f, 0.3f);
        px[12 * w + 7] = new Color(1f, 0.9f, 0.3f);

        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(px);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);

        // label
        var label = new GameObject("SummitLabel");
        label.transform.parent = marker.transform;
        label.transform.localPosition = new Vector3(0, 1.5f, 0);
        var tmp = label.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "SUMMIT\n14,433 ft";
        tmp.fontSize = 3;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.9f, 0.3f);
        tmp.sortingOrder = 101;
        tmp.rectTransform.sizeDelta = new Vector2(4, 1.5f);
    }

    private Sprite MakeSignSprite(string text)
    {
        int w = 20, h = 14;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        // posts
        for (int y = 0; y < 10; y++) { px[y * w + 3] = BT; px[y * w + 16] = BT; }
        // sign board
        Color board = new Color(0.55f, 0.38f, 0.15f);
        for (int y = 8; y < 14; y++)
            for (int x = 1; x < 19; x++)
                px[y * w + x] = board;

        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);
    }

    private static Color BT = new Color(0.38f, 0.22f, 0.1f);
}
