using UnityEngine;
using System.Collections.Generic;

public class PhotoCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float viewfinderSize = 4f;
    [SerializeField] private float maxPhotoDistance = 10f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 6f;
    [SerializeField] private KeyCode cameraToggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode snapKey = KeyCode.Mouse0;

    [Header("References")]
    [SerializeField] private GameObject viewfinderUI;
    [SerializeField] private GameObject crosshair;

    private PlayerController playerController;
    private UnityEngine.Camera mainCamera;
    private bool isActive;
    private float currentZoom;
    private float originalCameraSize;

    public bool IsActive => isActive;

    public System.Action<PhotoResult> OnPhotoTaken;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        mainCamera = UnityEngine.Camera.main;
        currentZoom = viewfinderSize;

        if (mainCamera != null)
            originalCameraSize = mainCamera.orthographicSize;

        if (viewfinderUI != null)
            viewfinderUI.SetActive(false);
        if (crosshair != null)
            crosshair.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(cameraToggleKey))
        {
            ToggleCameraMode();
        }

        if (!isActive) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
            if (mainCamera != null)
                mainCamera.orthographicSize = currentZoom;
        }

        if (Input.GetKeyDown(snapKey))
        {
            TakePhoto();
        }
    }

    private void ToggleCameraMode()
    {
        isActive = !isActive;
        playerController.SetCameraMode(isActive);

        if (viewfinderUI != null)
            viewfinderUI.SetActive(isActive);
        if (crosshair != null)
            crosshair.SetActive(isActive);

        if (mainCamera != null)
        {
            if (isActive)
            {
                originalCameraSize = mainCamera.orthographicSize;
                mainCamera.orthographicSize = currentZoom;
            }
            else
            {
                mainCamera.orthographicSize = originalCameraSize;
            }
        }
    }

    private void TakePhoto()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        List<CritterInFrame> crittersInFrame = new List<CritterInFrame>();

        Collider2D[] hits = Physics2D.OverlapCircleAll(mouseWorld, viewfinderSize * 0.5f);
        foreach (var hit in hits)
        {
            var critter = hit.GetComponent<Critter>();
            if (critter == null) continue;

            float dist = Vector2.Distance(mouseWorld, critter.transform.position);
            if (dist > maxPhotoDistance) continue;

            crittersInFrame.Add(new CritterInFrame
            {
                critter = critter,
                distance = dist,
                isDoingSillyAction = critter.IsDoingSillyAction,
                sillyAction = critter.CurrentSillyAction
            });

            critter.OnPhotographed();
        }

        if (crittersInFrame.Count > 0)
        {
            var result = ScorePhoto(crittersInFrame);
            OnPhotoTaken?.Invoke(result);
        }
        else
        {
            OnPhotoTaken?.Invoke(new PhotoResult
            {
                rating = PhotoRating.NoCritter,
                ratingText = PickNoCritterText(),
                score = 0
            });
        }
    }

    private PhotoResult ScorePhoto(List<CritterInFrame> critters)
    {
        float totalScore = 0;
        CritterData bestCritter = null;
        float bestContribution = 0;

        foreach (var c in critters)
        {
            float distScore = Mathf.Lerp(1.5f, 0.3f, c.distance / maxPhotoDistance);
            float rarityBonus = c.critter.Data.rarity switch
            {
                CritterRarity.Common => 1f,
                CritterRarity.Uncommon => 1.5f,
                CritterRarity.Rare => 2.5f,
                CritterRarity.Legendary => 5f,
                _ => 1f
            };
            float sillyBonus = c.isDoingSillyAction ? 2f : 1f;
            float contribution = c.critter.Data.basePhotoScore * distScore * rarityBonus * sillyBonus;
            totalScore += contribution;

            if (contribution > bestContribution)
            {
                bestContribution = contribution;
                bestCritter = c.critter.Data;
            }
        }

        int multiBonus = critters.Count > 1 ? critters.Count * 50 : 0;
        totalScore += multiBonus;

        PhotoRating rating;
        string ratingText;

        if (totalScore >= 500)
        {
            rating = PhotoRating.NationalGeographic;
            ratingText = "NATIONAL GEOGRAPHIC!";
        }
        else if (totalScore >= 300)
        {
            rating = PhotoRating.Majestic;
            ratingText = "Majestic!";
        }
        else if (totalScore >= 150)
        {
            rating = PhotoRating.Decent;
            ratingText = "Decent Shot!";
        }
        else
        {
            rating = PhotoRating.Blurry;
            ratingText = "Blurry but Valid!";
        }

        if (critters.Count >= 3)
            ratingText += " GROUP PHOTO!";

        bool anySilly = critters.Exists(c => c.isDoingSillyAction);
        if (anySilly)
            ratingText += " Caught in the act!";

        return new PhotoResult
        {
            rating = rating,
            ratingText = ratingText,
            score = Mathf.RoundToInt(totalScore),
            primaryCritter = bestCritter,
            critterCount = critters.Count,
            hadSillyAction = anySilly
        };
    }

    private string PickNoCritterText()
    {
        string[] texts = {
            "Nice rock, I guess.",
            "That's just a cloud.",
            "A scenic landscape... but where's the critter?",
            "Bold artistic choice. Zero points.",
            "You photographed nothing. Impressive commitment.",
            "The mountain doesn't count as a critter. Yet."
        };
        return texts[Random.Range(0, texts.Length)];
    }

    private struct CritterInFrame
    {
        public Critter critter;
        public float distance;
        public bool isDoingSillyAction;
        public string sillyAction;
    }
}

public enum PhotoRating
{
    NoCritter,
    Blurry,
    Decent,
    Majestic,
    NationalGeographic
}

public class PhotoResult
{
    public PhotoRating rating;
    public string ratingText;
    public int score;
    public CritterData primaryCritter;
    public int critterCount;
    public bool hadSillyAction;
}
