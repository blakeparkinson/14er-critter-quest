using UnityEngine;
using System.Collections;

public class PhotoEffects : MonoBehaviour
{
    private Camera cam;
    private Vector3 originalCamPos;

    private void Start()
    {
        cam = Camera.main;

        var photoCam = GetComponent<TopDownPhotoCamera>();
        if (photoCam != null)
            photoCam.GetType().GetEvent("OnPhotoTaken"); // can't subscribe easily, use alternate

        // poll approach - check for photo flash overlay as signal
    }

    public void TriggerPhotoEffect(Vector3 position, PhotoRating rating)
    {
        StartCoroutine(ScreenShake(rating));
        SpawnStars(position, rating);
    }

    private IEnumerator ScreenShake(PhotoRating rating)
    {
        if (cam == null) yield break;

        float magnitude = rating switch
        {
            PhotoRating.NationalGeographic => 0.15f,
            PhotoRating.Majestic => 0.1f,
            PhotoRating.Decent => 0.05f,
            _ => 0.02f
        };
        float duration = 0.2f;

        var camScript = cam.GetComponent<TopDownCamera>();
        Vector3 basePos = cam.transform.position;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float decay = 1f - t / duration;
            float offX = Random.Range(-magnitude, magnitude) * decay;
            float offY = Random.Range(-magnitude, magnitude) * decay;
            cam.transform.position = basePos + new Vector3(offX, offY, 0);
            yield return null;
        }
    }

    private void SpawnStars(Vector3 position, PhotoRating rating)
    {
        int count = rating switch
        {
            PhotoRating.NationalGeographic => 12,
            PhotoRating.Majestic => 8,
            PhotoRating.Decent => 5,
            _ => 2
        };

        Color starColor = rating switch
        {
            PhotoRating.NationalGeographic => new Color(1f, 0.84f, 0f),
            PhotoRating.Majestic => new Color(0.7f, 0.4f, 1f),
            PhotoRating.Decent => new Color(0.4f, 0.85f, 0.9f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };

        for (int i = 0; i < count; i++)
        {
            StartCoroutine(AnimateStar(position, starColor, i));
        }
    }

    private IEnumerator AnimateStar(Vector3 pos, Color color, int index)
    {
        var star = new GameObject("Star");
        var sr = star.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 300;

        // tiny star sprite
        var tex = new Texture2D(4, 4);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.clear;
        px[1] = color; px[4] = color; px[5] = color; px[6] = color;
        px[9] = color; px[13] = color; // cross shape
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(px);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16);

        float angle = index * (360f / 12f) * Mathf.Deg2Rad;
        float speed = Random.Range(2f, 4f);
        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        star.transform.position = pos;

        float t = 0;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            star.transform.position += dir * speed * Time.deltaTime;
            speed *= 0.95f;
            sr.color = new Color(color.r, color.g, color.b, 1f - t / 0.6f);
            float s = Mathf.Lerp(0.3f, 0.05f, t / 0.6f);
            star.transform.localScale = Vector3.one * s;
            yield return null;
        }

        Destroy(star);
    }
}
