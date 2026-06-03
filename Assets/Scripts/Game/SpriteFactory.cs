using UnityEngine;
using System.Collections.Generic;

public static class SpriteFactory
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite CreateRect(string key, int w, int h, Color color)
    {
        if (cache.TryGetValue(key, out var cached)) return cached;

        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
        cache[key] = sprite;
        return sprite;
    }

    public static Sprite CreateHiker()
    {
        int w = 16, h = 24;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];

        // boots (brown)
        Color boots = new Color(0.45f, 0.25f, 0.1f);
        FillRect(pixels, w, 4, 0, 4, 4, boots);
        FillRect(pixels, w, 8, 0, 4, 4, boots);

        // pants (khaki)
        Color pants = new Color(0.76f, 0.69f, 0.5f);
        FillRect(pixels, w, 4, 4, 8, 6, pants);

        // jacket (blue)
        Color jacket = new Color(0.2f, 0.5f, 0.85f);
        FillRect(pixels, w, 3, 10, 10, 7, jacket);

        // arms
        FillRect(pixels, w, 1, 11, 2, 5, jacket);
        FillRect(pixels, w, 13, 11, 2, 5, jacket);

        // hands (skin)
        Color skin = new Color(0.9f, 0.75f, 0.6f);
        FillRect(pixels, w, 1, 10, 2, 1, skin);
        FillRect(pixels, w, 13, 10, 2, 1, skin);

        // head (skin)
        FillRect(pixels, w, 5, 17, 6, 5, skin);

        // eyes
        pixels[18 * w + 6] = Color.black;
        pixels[18 * w + 10] = Color.black;

        // smile
        pixels[17 * w + 7] = Color.black;
        pixels[17 * w + 8] = Color.black;
        pixels[17 * w + 9] = Color.black;

        // hat (red beanie)
        Color hat = new Color(0.85f, 0.15f, 0.15f);
        FillRect(pixels, w, 4, 22, 8, 2, hat);

        // backpack (green)
        Color pack = new Color(0.2f, 0.55f, 0.2f);
        FillRect(pixels, w, 11, 11, 3, 5, pack);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);
    }

    public static Sprite CreateMarmot()
    {
        int w = 16, h = 12;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];

        Color body = new Color(0.6f, 0.45f, 0.25f);
        Color belly = new Color(0.8f, 0.7f, 0.5f);

        // body
        FillRect(pixels, w, 2, 2, 12, 6, body);
        FillRect(pixels, w, 4, 1, 8, 2, body);
        // belly
        FillRect(pixels, w, 4, 2, 8, 3, belly);
        // head
        FillRect(pixels, w, 12, 5, 4, 5, body);
        // eyes
        pixels[8 * w + 14] = Color.black;
        // nose
        pixels[7 * w + 15] = new Color(0.3f, 0.15f, 0.1f);
        // tail
        FillRect(pixels, w, 0, 4, 3, 2, body);
        // feet
        Color feet = new Color(0.35f, 0.2f, 0.1f);
        FillRect(pixels, w, 4, 0, 2, 2, feet);
        FillRect(pixels, w, 10, 0, 2, 2, feet);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);
    }

    public static Sprite CreateGoat()
    {
        int w = 18, h = 16;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];

        Color body = new Color(0.95f, 0.95f, 0.9f);
        Color horn = new Color(0.7f, 0.65f, 0.5f);

        // body
        FillRect(pixels, w, 3, 4, 12, 7, body);
        // legs
        FillRect(pixels, w, 4, 0, 2, 4, body);
        FillRect(pixels, w, 8, 0, 2, 4, body);
        FillRect(pixels, w, 12, 0, 2, 4, body);
        // head
        FillRect(pixels, w, 13, 8, 5, 5, body);
        // beard
        FillRect(pixels, w, 15, 7, 2, 2, body);
        // horns
        FillRect(pixels, w, 14, 13, 2, 3, horn);
        FillRect(pixels, w, 16, 14, 1, 2, horn);
        // eye
        pixels[11 * w + 16] = Color.black;
        // hooves
        Color hoof = new Color(0.3f, 0.3f, 0.3f);
        FillRect(pixels, w, 4, 0, 2, 1, hoof);
        FillRect(pixels, w, 8, 0, 2, 1, hoof);
        FillRect(pixels, w, 12, 0, 2, 1, hoof);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);
    }

    public static Sprite CreatePika()
    {
        int w = 10, h = 8;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];

        Color body = new Color(0.6f, 0.5f, 0.4f);
        Color belly = new Color(0.8f, 0.75f, 0.65f);

        FillRect(pixels, w, 2, 1, 6, 4, body);
        FillRect(pixels, w, 3, 1, 4, 2, belly);
        // ears
        FillRect(pixels, w, 3, 5, 2, 3, body);
        FillRect(pixels, w, 6, 5, 2, 3, body);
        pixels[6 * w + 4] = new Color(0.9f, 0.7f, 0.6f);
        pixels[6 * w + 7] = new Color(0.9f, 0.7f, 0.6f);
        // eyes
        pixels[4 * w + 4] = Color.black;
        pixels[4 * w + 7] = Color.black;
        // nose
        pixels[3 * w + 5] = new Color(0.3f, 0.15f, 0.1f);
        // feet
        FillRect(pixels, w, 2, 0, 2, 1, body);
        FillRect(pixels, w, 6, 0, 2, 1, body);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.25f), 16);
    }

    public static Sprite CreateLlama()
    {
        int w = 16, h = 22;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[w * h];

        Color body = new Color(0.92f, 0.88f, 0.8f);
        Color sweater = new Color(0.85f, 0.2f, 0.3f);

        // legs
        FillRect(pixels, w, 3, 0, 2, 8, body);
        FillRect(pixels, w, 11, 0, 2, 8, body);
        // body
        FillRect(pixels, w, 2, 8, 12, 6, body);
        // sweater stripes!
        FillRect(pixels, w, 2, 9, 12, 2, sweater);
        FillRect(pixels, w, 2, 12, 12, 1, sweater);
        // neck
        FillRect(pixels, w, 5, 14, 4, 5, body);
        // head
        FillRect(pixels, w, 4, 19, 6, 3, body);
        // ears
        FillRect(pixels, w, 4, 21, 2, 1, body);
        FillRect(pixels, w, 8, 21, 2, 1, body);
        // eyes
        pixels[20 * w + 5] = Color.black;
        pixels[20 * w + 8] = Color.black;
        // mouth (goofy smile)
        pixels[19 * w + 6] = Color.black;
        pixels[19 * w + 7] = Color.black;
        // hooves
        Color hoof = new Color(0.3f, 0.3f, 0.3f);
        FillRect(pixels, w, 3, 0, 2, 1, hoof);
        FillRect(pixels, w, 11, 0, 2, 1, hoof);

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.2f), 16);
    }

    public static Sprite CreateCritterSprite(string critterName)
    {
        return critterName switch
        {
            "Gerald" or "Professor Whiskers" => CreateMarmot(),
            "Beatrice" or "The Council" => CreatePika(),
            "Reginald" => CreateGoat(),
            "Steve" => CreateLlama(),
            _ => CreateMarmot()
        };
    }

    public static Sprite CreateMountainBG(int w, int h, Color baseColor, Color snowColor, float snowLine)
    {
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color col = t > snowLine ? Color.Lerp(baseColor, snowColor, (t - snowLine) / (1f - snowLine)) : baseColor;
            col *= Random.Range(0.95f, 1.05f);
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = col;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    private static void FillRect(Color[] pixels, int texWidth, int x, int y, int w, int h, Color color)
    {
        for (int py = y; py < y + h && py < pixels.Length / texWidth; py++)
            for (int px = x; px < x + w && px < texWidth; px++)
                pixels[py * texWidth + px] = color;
    }
}
