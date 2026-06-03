using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FieldGuideUI : MonoBehaviour
{
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private KeyCode toggleKey = KeyCode.G;

    [Header("Detail View")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailSprite;
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailTitle;
    [SerializeField] private TextMeshProUGUI detailEntry;
    [SerializeField] private TextMeshProUGUI detailStats;
    [SerializeField] private Image detailRarityBorder;

    private bool isOpen;

    private void Start()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleGuide();
        }
    }

    private void ToggleGuide()
    {
        isOpen = !isOpen;
        if (guidePanel != null)
            guidePanel.SetActive(isOpen);

        if (isOpen)
            RefreshEntries();

        Time.timeScale = isOpen ? 0f : 1f;
    }

    private void RefreshEntries()
    {
        if (FieldGuide.Instance == null || entryContainer == null) return;

        foreach (Transform child in entryContainer)
            Destroy(child.gameObject);

        var entries = FieldGuide.Instance.GetAllEntries();
        foreach (var entry in entries)
        {
            if (entryPrefab == null) continue;

            GameObject obj = Instantiate(entryPrefab, entryContainer);
            var nameText = obj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            var iconImage = obj.transform.Find("Icon")?.GetComponent<Image>();
            var rarityText = obj.transform.Find("Rarity")?.GetComponent<TextMeshProUGUI>();
            var button = obj.GetComponent<Button>();

            if (entry.discovered)
            {
                if (nameText != null) nameText.text = entry.critterData.critterName;
                if (iconImage != null && entry.critterData.sprite != null)
                    iconImage.sprite = entry.critterData.sprite;
                if (rarityText != null)
                    rarityText.text = entry.critterData.rarity.ToString();

                var capturedEntry = entry;
                if (button != null)
                    button.onClick.AddListener(() => ShowDetail(capturedEntry));
            }
            else
            {
                if (nameText != null) nameText.text = "???";
                if (iconImage != null) iconImage.color = Color.black;
                if (rarityText != null) rarityText.text = "";
                if (button != null) button.interactable = false;
            }
        }
    }

    private void ShowDetail(FieldGuideEntry entry)
    {
        if (detailPanel == null) return;
        detailPanel.SetActive(true);

        if (detailSprite != null && entry.critterData.sprite != null)
            detailSprite.sprite = entry.critterData.sprite;
        if (detailName != null)
            detailName.text = entry.critterData.critterName;
        if (detailTitle != null)
            detailTitle.text = $"\"{entry.critterData.sillyTitle}\"";
        if (detailEntry != null)
            detailEntry.text = entry.critterData.fieldGuideEntry;
        if (detailStats != null)
        {
            detailStats.text = $"Times Seen: {entry.timesSeen}\n" +
                               $"Best Photo: {entry.bestPhotoScore} pts ({entry.bestRating})\n" +
                               $"Rarity: {entry.critterData.rarity}";
        }
        if (detailRarityBorder != null)
        {
            detailRarityBorder.color = entry.critterData.rarity switch
            {
                CritterRarity.Common => Color.white,
                CritterRarity.Uncommon => Color.green,
                CritterRarity.Rare => new Color(0.2f, 0.4f, 1f),
                CritterRarity.Legendary => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };
        }
    }
}
