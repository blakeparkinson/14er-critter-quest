using UnityEngine;

public class TopDownCritter : MonoBehaviour
{
    private CritterData data;
    private SpriteRenderer sr;
    private Vector3 homePos;
    private Vector3 targetPos;
    private Transform player;
    private float stateTimer;
    private float animTimer;
    private bool isSilly;
    private string currentSillyAction;
    private float wanderRadius = 3f;
    private TMPro.TextMeshPro nameLabel;

    private enum State { Idle, Wander, Flee, Silly }
    private State state = State.Idle;

    public CritterData Data => data;
    public bool IsDoingSillyAction => state == State.Silly;
    public string CurrentSillyAction => currentSillyAction;

    public void Initialize(CritterData critterData)
    {
        data = critterData;
        homePos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // make critters more visible — slightly larger
        float scale = data.critterName switch
        {
            "Steve" or "Karen" => 1.8f,
            "Reginald" or "Big Tony" => 1.5f,
            "Gerald" or "Professor Whiskers" => 1.2f,
            _ => 1.1f
        };
        transform.localScale = new Vector3(scale, scale, 1);

        // name label
        CreateNameLabel();
        EnterState(State.Idle);
    }

    private void Update()
    {
        if (data == null || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        stateTimer -= Time.deltaTime;
        animTimer += Time.deltaTime;

        // update sprite animation
        int frame = Mathf.FloorToInt(animTimer * 3f) % 4;
        var sprite = PixelArt.GetCritterSprite(data.critterName, state == State.Idle ? 0 : frame);
        if (sprite != null) sr.sprite = sprite;

        switch (state)
        {
            case State.Idle:
                if (dist < data.fleeRange && data.personality != CritterPersonality.Bold)
                    EnterState(State.Flee);
                else if (stateTimer <= 0)
                {
                    if (Random.value < 0.15f && data.sillyActions != null && data.sillyActions.Length > 0)
                        EnterState(State.Silly);
                    else
                        EnterState(State.Wander);
                }
                break;

            case State.Wander:
                Vector3 dir = (targetPos - transform.position);
                if (dir.magnitude > 0.2f)
                {
                    transform.position += dir.normalized * data.moveSpeed * Time.deltaTime;
                    sr.flipX = dir.x < 0;
                }
                else
                    EnterState(State.Idle);

                if (dist < data.fleeRange && data.personality != CritterPersonality.Bold)
                    EnterState(State.Flee);
                if (stateTimer <= 0)
                    EnterState(State.Idle);
                break;

            case State.Flee:
                Vector3 fleeDir = (transform.position - player.position).normalized;
                transform.position += fleeDir * data.moveSpeed * 2f * Time.deltaTime;
                sr.flipX = fleeDir.x < 0;

                if (dist > data.detectionRange * 1.5f || stateTimer <= 0)
                    EnterState(State.Idle);
                break;

            case State.Silly:
                // stay still, do the silly thing
                if (stateTimer <= 0)
                    EnterState(State.Idle);
                break;
        }

        // sort by Y position
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10) + 5;

        // show name label when player is close
        if (nameLabel != null)
        {
            bool showName = dist < 4f;
            nameLabel.gameObject.SetActive(showName);
            if (showName)
            {
                Color rc = data.rarity switch
                {
                    CritterRarity.Common => Color.white,
                    CritterRarity.Uncommon => new Color(0.3f, 1f, 0.3f),
                    CritterRarity.Rare => new Color(0.4f, 0.6f, 1f),
                    CritterRarity.Legendary => new Color(1f, 0.84f, 0f),
                    _ => Color.white
                };
                nameLabel.color = rc;
            }
        }
    }

    private void EnterState(State newState)
    {
        state = newState;
        currentSillyAction = null;

        switch (newState)
        {
            case State.Idle:
                stateTimer = Random.Range(data.idleTimeMin, data.idleTimeMax);
                break;
            case State.Wander:
                stateTimer = Random.Range(3f, 6f);
                targetPos = homePos + (Vector3)(Random.insideUnitCircle * wanderRadius);
                break;
            case State.Flee:
                stateTimer = 3f;
                break;
            case State.Silly:
                stateTimer = Random.Range(2f, 5f);
                if (data.sillyActions.Length > 0)
                    currentSillyAction = data.sillyActions[Random.Range(0, data.sillyActions.Length)];
                break;
        }
    }

    public void OnPhotographed()
    {
        if (data.personality == CritterPersonality.Shy)
            EnterState(State.Flee);
        else if (data.personality == CritterPersonality.Chaotic)
            EnterState(Random.value > 0.5f ? State.Silly : State.Flee);
    }

    private void CreateNameLabel()
    {
        var labelObj = new GameObject("Name");
        labelObj.transform.parent = transform;
        labelObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        nameLabel = labelObj.AddComponent<TMPro.TextMeshPro>();
        nameLabel.text = data.critterName;
        nameLabel.fontSize = 3;
        nameLabel.alignment = TMPro.TextAlignmentOptions.Center;
        nameLabel.sortingOrder = 200;
        nameLabel.rectTransform.sizeDelta = new Vector2(3, 0.6f);
        labelObj.SetActive(false);
    }
}
