using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private Color flashColor = new Color(1, 1, 1, 0.6f);

    private Image flashImage;
    private Transform cameraTransform;
    private Vector3 originalCamPos;

    private void Start()
    {
        cameraTransform = Camera.main?.transform;

        // create flash overlay
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            var flashObj = new GameObject("CameraFlash");
            flashObj.transform.SetParent(canvas.transform, false);
            var rt = flashObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            flashImage = flashObj.AddComponent<Image>();
            flashImage.color = Color.clear;
            flashImage.raycastTarget = false;
        }

        var photoCamera = GetComponent<PhotoCamera>();
        if (photoCamera != null)
            photoCamera.OnPhotoTaken += OnPhoto;
    }

    private void OnPhoto(PhotoResult result)
    {
        StartCoroutine(FlashAndShake(result.rating));
    }

    private IEnumerator FlashAndShake(PhotoRating rating)
    {
        // flash
        if (flashImage != null)
        {
            Color fc = rating == PhotoRating.NationalGeographic
                ? new Color(1f, 0.9f, 0.3f, 0.8f)
                : flashColor;
            flashImage.color = fc;

            float t = 0;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                flashImage.color = Color.Lerp(fc, Color.clear, t / flashDuration);
                yield return null;
            }
            flashImage.color = Color.clear;
        }

        // shake
        if (cameraTransform != null)
        {
            float mag = rating >= PhotoRating.Majestic ? shakeMagnitude * 2f : shakeMagnitude;
            originalCamPos = cameraTransform.localPosition;
            float t = 0;
            while (t < shakeDuration)
            {
                t += Time.deltaTime;
                float x = Random.Range(-mag, mag) * (1 - t / shakeDuration);
                float y = Random.Range(-mag, mag) * (1 - t / shakeDuration);
                cameraTransform.localPosition = originalCamPos + new Vector3(x, y, 0);
                yield return null;
            }
            cameraTransform.localPosition = originalCamPos;
        }
    }

    private void OnDestroy()
    {
        var photoCamera = GetComponent<PhotoCamera>();
        if (photoCamera != null)
            photoCamera.OnPhotoTaken -= OnPhoto;
    }
}
