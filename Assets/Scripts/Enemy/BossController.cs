using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Patrol")]
    public float moveSpeed = 1.5f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 3f;
    public LayerMask obstacleMask;

    [Header("Smoothing")]
    public float rotationSpeed = 6f;
    public float steerSmoothing = 5f;

    [Header("Boss Stats")]
    public int maxBossLives = 3;

    private int bossLives;
    private Vector2 currentDirection;
    private float currentAngle;
    private Rigidbody2D rb;
    private Vector2 roamTarget;
    private float retargetTimer;
    private float stuckTimer;
    private Vector2 lastPosition;
    private Vector3 spawnPosition;
    private bool isInvincible;

    // World-space lives display
    private GameObject livesContainer;
    private SpriteRenderer[] heartRenderers;

    private const int DIR_COUNT = 24;
    private float[] interest = new float[DIR_COUNT];
    private float[] danger = new float[DIR_COUNT];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossLives = maxBossLives;
        spawnPosition = transform.position;

        if (obstacleMask == 0)
            obstacleMask = (1 << 10) | (1 << 11);

        currentDirection = Random.insideUnitCircle.normalized;
        lastPosition = transform.position;
        PickNewRoamTarget();

        CreateWorldSpaceHearts();
    }

    void CreateWorldSpaceHearts()
    {
        livesContainer = new GameObject("BossLives");
        livesContainer.transform.SetParent(transform);
        livesContainer.transform.localPosition = new Vector3(0f, 1.6f, 0f);

        // Create heart-shaped sprite
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float leftCX = 0.3f, leftCY = 0.62f, rad = 0.22f;
        float rightCX = 0.7f, rightCY = 0.62f;
        float tipX = 0.5f, tipY = 0.15f;
        for (int px = 0; px < size; px++)
        {
            for (int py = 0; py < size; py++)
            {
                float x = px / (float)size;
                float y = py / (float)size;
                bool inside = false;
                if ((x-leftCX)*(x-leftCX)+(y-leftCY)*(y-leftCY) <= rad*rad) inside = true;
                if ((x-rightCX)*(x-rightCX)+(y-rightCY)*(y-rightCY) <= rad*rad) inside = true;
                if (y <= leftCY && y >= tipY)
                {
                    float t = (y - tipY) / (leftCY - tipY);
                    if (x >= Mathf.Lerp(tipX, leftCX-rad, t) && x <= Mathf.Lerp(tipX, rightCX+rad, t)) inside = true;
                }
                if (y >= leftCY - rad*0.2f && y <= leftCY + rad && x >= leftCX && x <= rightCX)
                {
                    float midDip = leftCY + rad * 0.55f;
                    float xt = (x - leftCX) / (rightCX - leftCX);
                    if (y <= midDip + rad * 0.45f * Mathf.Sin(xt * Mathf.PI)) inside = true;
                }
                tex.SetPixel(px, py, inside ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        Sprite heartSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);

        // Label: BOSS_OMEGA
        GameObject labelObj = new GameObject("BossLabel");
        labelObj.transform.SetParent(livesContainer.transform);
        labelObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        labelObj.transform.localScale = new Vector3(0.02f, 0.02f, 1f);
        TextMesh labelMesh = labelObj.AddComponent<TextMesh>();
        labelMesh.text = "BOSS_OMEGA";
        labelMesh.fontSize = 80;
        labelMesh.characterSize = 1f;
        labelMesh.anchor = TextAnchor.MiddleCenter;
        labelMesh.alignment = TextAlignment.Center;
        labelMesh.color = new Color(0.8f, 0f, 1f, 0.9f);
        MeshRenderer labelRenderer = labelObj.GetComponent<MeshRenderer>();
        labelRenderer.sortingOrder = 15;

        // Hearts
        heartRenderers = new SpriteRenderer[maxBossLives];
        float spacing = 0.5f;
        float startX = -(maxBossLives - 1) * spacing / 2f;

        for (int i = 0; i < maxBossLives; i++)
        {
            GameObject heart = new GameObject($"Heart_{i}");
            heart.transform.SetParent(livesContainer.transform);
            heart.transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
            heart.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            SpriteRenderer sr = heart.AddComponent<SpriteRenderer>();
            sr.sprite = heartSprite;
            sr.color = new Color(0.8f, 0f, 1f, 1f);
            sr.sortingOrder = 15;
            heartRenderers[i] = sr;
        }
    }

    void Update()
    {
        // Keep lives display upright regardless of boss rotation
        if (livesContainer != null)
            livesContainer.transform.rotation = Quaternion.identity;

        UpdateHeartsDisplay();
    }

    void UpdateHeartsDisplay()
    {
        if (heartRenderers == null) return;
        for (int i = 0; i < heartRenderers.Length; i++)
        {
            if (heartRenderers[i] == null) continue;
            if (i < bossLives)
                heartRenderers[i].color = new Color(0.8f, 0f, 1f, 1f);
            else
                heartRenderers[i].color = new Color(0.3f, 0f, 0.4f, 0.3f);
        }
    }

    void FixedUpdate()
    {
        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f || Vector2.Distance(transform.position, roamTarget) < 1.5f)
        {
            PickNewRoamTarget();
        }

        Vector2 bestDir = GetBestDirection(roamTarget);

        currentDirection = Vector2.Lerp(currentDirection, bestDir, steerSmoothing * Time.fixedDeltaTime);
        currentDirection.Normalize();

        float targetAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        rb.velocity = currentDirection * moveSpeed;

        stuckTimer += Time.fixedDeltaTime;
        if (stuckTimer >= 0.8f)
        {
            float moved = Vector2.Distance(transform.position, lastPosition);
            if (moved < 0.1f)
                PickNewRoamTarget();
            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }

    void PickNewRoamTarget()
    {
        roamTarget = new Vector2(
            Random.Range(-12f, 12f),
            Random.Range(-7f, 7f)
        );
        retargetTimer = Random.Range(3f, 6f);
    }

    Vector2 GetBestDirection(Vector2 target)
    {
        Vector2 toTarget = (target - (Vector2)transform.position).normalized;

        for (int i = 0; i < DIR_COUNT; i++)
        {
            float angle = (360f / DIR_COUNT) * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            interest[i] = Mathf.Max(0f, Vector2.Dot(dir, toTarget));

            float dangerVal = 0f;
            for (int r = 0; r < 3; r++)
            {
                float spread = (r - 1) * 5f * Mathf.Deg2Rad;
                float rayAngle = angle + spread;
                Vector2 rayDir = new Vector2(Mathf.Cos(rayAngle), Mathf.Sin(rayAngle));

                RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, avoidDistance, obstacleMask);
                if (hit.collider != null)
                {
                    float d = 1f - (hit.distance / avoidDistance);
                    dangerVal = Mathf.Max(dangerVal, d);
                }
            }
            danger[i] = dangerVal;
        }

        Vector2 chosenDir = Vector2.zero;
        float bestScore = -999f;

        for (int i = 0; i < DIR_COUNT; i++)
        {
            float score = interest[i] - danger[i] * 2f;
            if (score > bestScore)
            {
                bestScore = score;
                float angle = (360f / DIR_COUNT) * i * Mathf.Deg2Rad;
                chosenDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }

        if (bestScore <= -0.5f)
        {
            float leastDanger = float.MaxValue;
            for (int i = 0; i < DIR_COUNT; i++)
            {
                if (danger[i] < leastDanger)
                {
                    leastDanger = danger[i];
                    float angle = (360f / DIR_COUNT) * i * Mathf.Deg2Rad;
                    chosenDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
            }
        }

        return chosenDir.normalized;
    }

    public bool TakeDamage(int amount = 1)
    {
        if (isInvincible) return false;

        bossLives -= amount;
        if (bossLives < 0) bossLives = 0;

        DeathEffect.SpawnAt(transform.position, new Color(0.8f, 0f, 1f));

        if (bossLives <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnEnemyKilled();
            Destroy(gameObject);
            return true;
        }

        // Become invincible and respawn
        isInvincible = true;
        StartCoroutine(RespawnBoss());
        return false;
    }

    System.Collections.IEnumerator RespawnBoss()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        VisionCone vc = GetComponent<VisionCone>();
        CircleCollider2D col = GetComponent<CircleCollider2D>();

        // Immediately disable everything
        rb.velocity = Vector2.zero;
        if (sr != null) sr.enabled = false;
        if (livesContainer != null) livesContainer.SetActive(false);
        if (vc != null) vc.enabled = false;
        if (col != null) col.enabled = false;

        // Teleport to spawn instantly
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;

        // Brief pause at spawn before reappearing
        yield return new WaitForSeconds(0.5f);

        // Show boss again
        currentDirection = Random.insideUnitCircle.normalized;
        PickNewRoamTarget();
        if (sr != null) sr.enabled = true;
        if (livesContainer != null) livesContainer.SetActive(true);
        if (vc != null) vc.enabled = true;
        if (col != null) col.enabled = true;

        // Stay invincible for a brief moment after reappearing
        yield return new WaitForSeconds(0.5f);
        isInvincible = false;
    }

    public int GetBossLives() => bossLives;
}
