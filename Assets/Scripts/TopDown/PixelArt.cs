using UnityEngine;
using System.Collections.Generic;

public static class PixelArt
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    // palette
    static Color T = Color.clear; // transparent
    static Color SK = new Color(0.91f, 0.74f, 0.56f); // skin
    static Color JK = new Color(0.2f, 0.48f, 0.82f);  // jacket blue
    static Color JD = new Color(0.14f, 0.34f, 0.62f);  // jacket dark
    static Color PN = new Color(0.55f, 0.42f, 0.28f);  // pants
    static Color PD = new Color(0.42f, 0.32f, 0.2f);   // pants dark
    static Color BT = new Color(0.35f, 0.2f, 0.1f);    // boots
    static Color HT = new Color(0.85f, 0.18f, 0.12f);  // hat red
    static Color HD = new Color(0.65f, 0.12f, 0.08f);  // hat dark
    static Color PK = new Color(0.22f, 0.52f, 0.18f);  // backpack
    static Color HR = new Color(0.4f, 0.25f, 0.12f);   // hair
    static Color BK = new Color(0.15f, 0.12f, 0.1f);   // black
    static Color WH = new Color(0.95f, 0.95f, 0.92f);  // white

    public static Sprite HikerDown(int frame)
    {
        string key = $"hiker_down_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        // 16x24 sprite, facing down (toward camera)
        int legOffset = frame switch { 1 => 1, 3 => -1, _ => 0 };

        Color[] px = new Color[16 * 24];
        Fill(px, 16, T);

        // boots
        DrawRect(px, 16, 4 + legOffset, 0, 3, 2, BT);
        DrawRect(px, 16, 9 - legOffset, 0, 3, 2, BT);

        // pants
        DrawRect(px, 16, 4 + legOffset, 2, 3, 4, PN);
        DrawRect(px, 16, 9 - legOffset, 2, 3, 4, PN);
        DrawRect(px, 16, 5, 5, 6, 2, PD); // belt area

        // jacket body
        DrawRect(px, 16, 3, 7, 10, 7, JK);
        DrawRect(px, 16, 4, 7, 8, 7, JK);
        // jacket shading
        DrawRect(px, 16, 3, 7, 2, 7, JD);
        DrawRect(px, 16, 11, 7, 2, 7, JD);
        // zipper
        SetCol(px, 16, 7, 8, 5, WH);

        // arms
        DrawRect(px, 16, 1, 8, 2, 5, JK);
        DrawRect(px, 16, 13, 8, 2, 5, JK);
        // hands
        DrawRect(px, 16, 1, 7, 2, 1, SK);
        DrawRect(px, 16, 13, 7, 2, 1, SK);

        // neck
        DrawRect(px, 16, 6, 14, 4, 1, SK);

        // head
        DrawRect(px, 16, 5, 15, 6, 5, SK);
        DrawRect(px, 16, 4, 16, 8, 3, SK);

        // eyes
        px[17 * 16 + 6] = BK;
        px[17 * 16 + 9] = BK;
        // eye whites
        px[17 * 16 + 5] = WH;
        px[17 * 16 + 10] = WH;

        // mouth
        px[16 * 16 + 7] = new Color(0.7f, 0.3f, 0.25f);
        px[16 * 16 + 8] = new Color(0.7f, 0.3f, 0.25f);

        // hat
        DrawRect(px, 16, 4, 20, 8, 3, HT);
        DrawRect(px, 16, 5, 23, 6, 1, HT);
        // hat brim
        DrawRect(px, 16, 3, 20, 10, 1, HD);

        var sprite = MakeSprite(key, 16, 24, px);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite HikerUp(int frame)
    {
        string key = $"hiker_up_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        int legOffset = frame switch { 1 => 1, 3 => -1, _ => 0 };

        Color[] px = new Color[16 * 24];
        Fill(px, 16, T);

        // boots
        DrawRect(px, 16, 4 + legOffset, 0, 3, 2, BT);
        DrawRect(px, 16, 9 - legOffset, 0, 3, 2, BT);

        // pants
        DrawRect(px, 16, 4 + legOffset, 2, 3, 4, PN);
        DrawRect(px, 16, 9 - legOffset, 2, 3, 4, PN);
        DrawRect(px, 16, 5, 5, 6, 2, PD);

        // jacket
        DrawRect(px, 16, 3, 7, 10, 7, JK);

        // backpack (visible from behind)
        DrawRect(px, 16, 4, 8, 8, 6, PK);
        DrawRect(px, 16, 5, 14, 6, 1, PK);
        // pack straps
        SetCol(px, 16, 5, 10, 4, new Color(0.15f, 0.15f, 0.15f));
        SetCol(px, 16, 10, 10, 4, new Color(0.15f, 0.15f, 0.15f));

        // arms
        DrawRect(px, 16, 1, 8, 2, 5, JK);
        DrawRect(px, 16, 13, 8, 2, 5, JK);
        DrawRect(px, 16, 1, 7, 2, 1, SK);
        DrawRect(px, 16, 13, 7, 2, 1, SK);

        // neck
        DrawRect(px, 16, 6, 14, 4, 1, SK);

        // head (back of head)
        DrawRect(px, 16, 5, 15, 6, 5, HR);
        DrawRect(px, 16, 4, 16, 8, 3, HR);

        // hat
        DrawRect(px, 16, 4, 20, 8, 3, HT);
        DrawRect(px, 16, 5, 23, 6, 1, HT);

        var sprite = MakeSprite(key, 16, 24, px);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite HikerSide(int frame)
    {
        string key = $"hiker_side_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        int step = frame switch { 1 => 2, 2 => 0, 3 => -2, _ => 0 };

        Color[] px = new Color[16 * 24];
        Fill(px, 16, T);

        // boots
        DrawRect(px, 16, 5 + step, 0, 3, 2, BT);
        DrawRect(px, 16, 7 - step, 0, 3, 2, BT);

        // legs
        DrawRect(px, 16, 5 + step, 2, 3, 4, PN);
        DrawRect(px, 16, 7 - step, 2, 3, 4, PD);

        // body
        DrawRect(px, 16, 4, 6, 8, 8, JK);
        DrawRect(px, 16, 4, 6, 3, 8, JD); // shading

        // backpack (side view)
        DrawRect(px, 16, 3, 7, 2, 6, PK);

        // arm
        DrawRect(px, 16, 10, 7, 2, 6, JK);
        DrawRect(px, 16, 10, 6, 2, 1, SK); // hand

        // neck
        DrawRect(px, 16, 7, 14, 3, 1, SK);

        // head
        DrawRect(px, 16, 5, 15, 6, 5, SK);
        DrawRect(px, 16, 4, 16, 7, 3, SK);

        // eye
        px[17 * 16 + 9] = BK;
        px[17 * 16 + 10] = WH;

        // mouth
        px[16 * 16 + 9] = new Color(0.7f, 0.3f, 0.25f);

        // hair (back)
        DrawRect(px, 16, 4, 16, 2, 4, HR);

        // hat
        DrawRect(px, 16, 4, 20, 8, 3, HT);
        DrawRect(px, 16, 5, 23, 6, 1, HT);

        var sprite = MakeSprite(key, 16, 24, px);
        cache[key] = sprite;
        return sprite;
    }

    // --- Critter sprites ---

    public static Sprite Marmot(int frame)
    {
        string key = $"marmot_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        Color B = new Color(0.58f, 0.42f, 0.24f);
        Color BL = new Color(0.75f, 0.62f, 0.42f);
        Color[] px = new Color[16 * 12];
        Fill(px, 16, T);

        int bob = (frame % 2 == 1) ? 1 : 0;

        // body
        DrawRect(px, 16, 2, 2 + bob, 12, 5, B);
        DrawRect(px, 16, 3, 2 + bob, 10, 3, BL); // belly
        // head
        DrawRect(px, 16, 11, 4 + bob, 4, 5, B);
        DrawRect(px, 16, 12, 4 + bob, 3, 3, BL); // cheek
        // eye
        px[(7 + bob) * 16 + 13] = BK;
        // nose
        px[(6 + bob) * 16 + 14] = new Color(0.3f, 0.15f, 0.1f);
        // ears
        px[(9 + bob) * 16 + 12] = B;
        px[(9 + bob) * 16 + 14] = B;
        // tail
        DrawRect(px, 16, 0, 4 + bob, 3, 2, B);
        // feet
        DrawRect(px, 16, 4, 0 + bob, 2, 2, BT);
        DrawRect(px, 16, 10, 0 + bob, 2, 2, BT);

        var sprite = MakeSprite(key, 16, 12, px);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite Goat(int frame)
    {
        string key = $"goat_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        Color W = new Color(0.92f, 0.9f, 0.85f);
        Color H = new Color(0.65f, 0.58f, 0.42f);
        Color[] px = new Color[18 * 16];
        Fill(px, 18, T);

        int bob = (frame % 2 == 1) ? 1 : 0;

        // body
        DrawRect(px, 18, 3, 4 + bob, 12, 6, W);
        // legs
        DrawRect(px, 18, 4, 0 + bob, 2, 4, W);
        DrawRect(px, 18, 8, 0 + bob, 2, 4, W);
        DrawRect(px, 18, 12, 0 + bob, 2, 4, W);
        // hooves
        DrawRect(px, 18, 4, 0, 2, 1, BK);
        DrawRect(px, 18, 8, 0, 2, 1, BK);
        DrawRect(px, 18, 12, 0, 2, 1, BK);
        // head
        DrawRect(px, 18, 13, 7 + bob, 5, 5, W);
        // eye
        px[(10 + bob) * 18 + 16] = BK;
        // horns
        DrawRect(px, 18, 14, 12 + bob, 2, 3, H);
        DrawRect(px, 18, 16, 13 + bob, 1, 2, H);
        // beard
        DrawRect(px, 18, 15, 6 + bob, 2, 2, W);

        var sprite = MakeSprite(key, 18, 16, px);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite Pika(int frame)
    {
        string key = $"pika_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        Color B = new Color(0.58f, 0.48f, 0.38f);
        Color BL = new Color(0.78f, 0.72f, 0.62f);
        Color[] px = new Color[10 * 10];
        Fill(px, 10, T);

        int bob = (frame % 2 == 1) ? 1 : 0;

        // body
        DrawRect(px, 10, 2, 1 + bob, 6, 4, B);
        DrawRect(px, 10, 3, 1 + bob, 4, 2, BL);
        // ears
        DrawRect(px, 10, 3, 6 + bob, 2, 3, B);
        DrawRect(px, 10, 6, 6 + bob, 2, 3, B);
        px[(7 + bob) * 10 + 4] = new Color(0.9f, 0.7f, 0.6f);
        px[(7 + bob) * 10 + 7] = new Color(0.9f, 0.7f, 0.6f);
        // eyes
        px[(4 + bob) * 10 + 4] = BK;
        px[(4 + bob) * 10 + 7] = BK;
        // nose
        px[(3 + bob) * 10 + 5] = new Color(0.3f, 0.15f, 0.1f);
        // feet
        DrawRect(px, 10, 2, 0 + bob, 2, 1, B);
        DrawRect(px, 10, 6, 0 + bob, 2, 1, B);

        var sprite = MakeSprite(key, 10, 10, px);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite Llama(int frame)
    {
        string key = $"llama_{frame}";
        if (cache.TryGetValue(key, out var s)) return s;

        Color B = new Color(0.9f, 0.85f, 0.78f);
        Color SW = new Color(0.82f, 0.18f, 0.25f); // sweater
        Color[] px = new Color[16 * 22];
        Fill(px, 16, T);

        int bob = (frame % 2 == 1) ? 1 : 0;

        // legs
        DrawRect(px, 16, 3, 0 + bob, 2, 7, B);
        DrawRect(px, 16, 11, 0 + bob, 2, 7, B);
        // hooves
        DrawRect(px, 16, 3, 0, 2, 1, BK);
        DrawRect(px, 16, 11, 0, 2, 1, BK);
        // body
        DrawRect(px, 16, 2, 7 + bob, 12, 5, B);
        // sweater!
        DrawRect(px, 16, 2, 8 + bob, 12, 2, SW);
        DrawRect(px, 16, 2, 11 + bob, 12, 1, SW);
        // neck
        DrawRect(px, 16, 5, 12 + bob, 4, 5, B);
        // head
        DrawRect(px, 16, 4, 17 + bob, 6, 4, B);
        // ears
        DrawRect(px, 16, 4, 20 + bob, 2, 1, B);
        DrawRect(px, 16, 8, 20 + bob, 2, 1, B);
        // eyes
        px[(19 + bob) * 16 + 5] = BK;
        px[(19 + bob) * 16 + 8] = BK;
        // mouth (goofy smile)
        px[(17 + bob) * 16 + 6] = new Color(0.3f, 0.15f, 0.12f);
        px[(17 + bob) * 16 + 7] = new Color(0.3f, 0.15f, 0.12f);

        var sprite = MakeSprite(key, 16, 22, px);
        cache[key] = sprite;
        return sprite;
    }

    // --- Tile sprites ---

    public static Sprite GrassTile()
    {
        if (cache.TryGetValue("grass", out var s)) return s;
        Color[] px = new Color[16 * 16];
        Color g1 = new Color(0.28f, 0.52f, 0.18f);
        Color g2 = new Color(0.33f, 0.58f, 0.22f);
        Color g3 = new Color(0.38f, 0.62f, 0.25f);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.4f + 10, y * 0.4f + 10);
                px[y * 16 + x] = n < 0.35f ? g1 : (n < 0.65f ? g2 : g3);
                // occasional grass blade accent
                if (n > 0.7f && Random.value > 0.7f)
                    px[y * 16 + x] = new Color(0.22f, 0.45f, 0.15f);
            }
        cache["grass"] = MakeSprite("grass", 16, 16, px);
        return cache["grass"];
    }

    public static Sprite DirtTile()
    {
        if (cache.TryGetValue("dirt", out var s)) return s;
        Color[] px = new Color[16 * 16];
        Color d1 = new Color(0.55f, 0.42f, 0.25f);
        Color d2 = new Color(0.5f, 0.38f, 0.22f);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.5f + 30, y * 0.5f + 30);
                px[y * 16 + x] = Color.Lerp(d1, d2, n);
                // small pebble dots
                if (n > 0.72f) px[y * 16 + x] = new Color(0.6f, 0.55f, 0.45f);
            }
        cache["dirt"] = MakeSprite("dirt", 16, 16, px);
        return cache["dirt"];
    }

    public static Sprite RockTile()
    {
        if (cache.TryGetValue("rock", out var s)) return s;
        Color[] px = new Color[16 * 16];
        Color r1 = new Color(0.45f, 0.43f, 0.42f);
        Color r2 = new Color(0.55f, 0.53f, 0.5f);
        Color r3 = new Color(0.4f, 0.38f, 0.38f);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.35f + 50, y * 0.35f + 50);
                float n2 = Mathf.PerlinNoise(x * 0.8f + 70, y * 0.8f + 70);
                px[y * 16 + x] = n < 0.4f ? r1 : (n < 0.7f ? r2 : r3);
                // crack lines
                if (n2 > 0.75f) px[y * 16 + x] = new Color(0.35f, 0.33f, 0.32f);
            }
        cache["rock"] = MakeSprite("rock", 16, 16, px);
        return cache["rock"];
    }

    public static Sprite SnowTile()
    {
        if (cache.TryGetValue("snow", out var s)) return s;
        Color[] px = new Color[16 * 16];
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.3f + 90, y * 0.3f + 90);
                float v = Mathf.Lerp(0.88f, 0.97f, n);
                px[y * 16 + x] = new Color(v, v + 0.01f, v + 0.03f);
                // sparkle
                if (n > 0.82f) px[y * 16 + x] = new Color(0.98f, 0.99f, 1f);
            }
        cache["snow"] = MakeSprite("snow", 16, 16, px);
        return cache["snow"];
    }

    public static Sprite TreeTop()
    {
        if (cache.TryGetValue("treetop", out var s)) return s;
        int w = 14, h = 14;
        Color[] px = new Color[w * h];
        Fill(px, w, T);
        Color g = new Color(0.1f, 0.33f, 0.08f);
        Color gl = new Color(0.16f, 0.43f, 0.12f);
        Color outline = new Color(0.06f, 0.2f, 0.05f);
        float cx = 6.5f, cy = 6.5f, r = 5.8f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < r - 0.8f)
                {
                    float n = Mathf.PerlinNoise(x * 0.5f + 20, y * 0.5f + 20);
                    px[y * w + x] = n > 0.45f ? gl : g;
                    // highlight on top-left
                    if (dx < -1 && dy < -1 && d < r - 2)
                        px[y * w + x] = new Color(0.2f, 0.5f, 0.16f);
                }
                else if (d < r)
                    px[y * w + x] = outline; // dark edge
            }
        // trunk shadow
        DrawRect(px, w, 6, 0, 2, 2, new Color(0.32f, 0.18f, 0.08f));
        cache["treetop"] = MakeSprite("treetop", w, h, px);
        return cache["treetop"];
    }

    public static Sprite PineTreeTop()
    {
        if (cache.TryGetValue("pinetop", out var s)) return s;
        int w = 12, h = 16;
        Color[] px = new Color[w * h];
        Fill(px, w, T);
        Color g = new Color(0.08f, 0.3f, 0.06f);
        Color gl = new Color(0.14f, 0.38f, 0.1f);
        Color outline = new Color(0.04f, 0.18f, 0.04f);
        // triangle with dark outline
        for (int y = 0; y < 14; y++)
        {
            int half = (14 - y) * w / 28 + 1;
            int cx2 = w / 2;
            for (int x = cx2 - half; x <= cx2 + half; x++)
            {
                if (x < 0 || x >= w) continue;
                bool isEdge = (x == cx2 - half || x == cx2 + half || y == 13);
                float n = Mathf.PerlinNoise(x * 0.4f + 40, y * 0.3f + 40);
                px[(y + 2) * w + x] = isEdge ? outline : (n > 0.4f ? gl : g);
                // snow on tips
                if (y > 10 && n > 0.6f)
                    px[(y + 2) * w + x] = new Color(0.85f, 0.9f, 0.92f, 0.7f);
            }
        }
        // trunk
        DrawRect(px, w, 5, 0, 2, 3, new Color(0.35f, 0.2f, 0.08f));
        cache["pinetop"] = MakeSprite("pinetop", w, h, px);
        return cache["pinetop"];
    }

    public static Sprite GetCritterSprite(string name, int frame)
    {
        return name switch
        {
            "Gerald" or "Professor Whiskers" => Marmot(frame),
            "Beatrice" or "The Council" => Pika(frame),
            "Reginald" => Goat(frame),
            "Steve" => Llama(frame),
            "Gremlin" => Pika(frame), // reuse for now
            _ => Marmot(frame)
        };
    }

    // --- Helpers ---

    private static void Fill(Color[] px, int w, Color c)
    {
        for (int i = 0; i < px.Length; i++) px[i] = c;
    }

    private static void DrawRect(Color[] px, int w, int x, int y, int rw, int rh, Color c)
    {
        int h = px.Length / w;
        for (int py = y; py < y + rh && py < h; py++)
            for (int px2 = x; px2 < x + rw && px2 < w; px2++)
                if (py >= 0 && px2 >= 0)
                    px[py * w + px2] = c;
    }

    private static void SetCol(Color[] px, int w, int x, int y, int h, Color c)
    {
        int maxH = px.Length / w;
        for (int py = y; py < y + h && py < maxH; py++)
            if (py >= 0 && x >= 0 && x < w)
                px[py * w + x] = c;
    }

    private static Sprite MakeSprite(string name, int w, int h, Color[] px)
    {
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);
    }
}
