using UnityEngine;

public enum CritterRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public enum CritterPersonality
{
    Shy,
    Curious,
    Bold,
    Chaotic
}

[CreateAssetMenu(fileName = "NewCritter", menuName = "14er Critter Quest/Critter Data")]
public class CritterData : ScriptableObject
{
    public string critterName;
    public string sillyTitle;
    [TextArea] public string fieldGuideEntry;
    public Sprite sprite;
    public Sprite photoFrameSprite;
    public CritterRarity rarity;
    public CritterPersonality personality;

    [Header("Behavior")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float fleeRange = 3f;
    public float idleTimeMin = 1f;
    public float idleTimeMax = 4f;

    [Header("Photo")]
    public int basePhotoScore = 100;
    public string[] photoReactions;
    public string[] sillyActions;

    [Header("Spawn")]
    public float minAltitude = 0f;
    public float maxAltitude = 100f;
    [Range(0f, 1f)] public float spawnChance = 0.5f;
}
