using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Knife")]
    public float knifeRange = 1.5f;
    public LayerMask enemyLayer;
    public LayerMask wallLayer;

    [Header("Gun")]
    public float gunRange = 20f;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();

        // Default wall layer to Wall + Obstacle if not set
        if (wallLayer == 0)
            wallLayer = (1 << 10) | (1 << 11);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            KnifeAttack();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            GunAttack();
        }
    }

    void KnifeAttack()
    {
        // Spawn slash visual
        GameObject slashObj = new GameObject("KnifeSlash");
        slashObj.transform.SetParent(transform);
        slashObj.transform.localPosition = Vector3.zero;
        KnifeSlashEffect slash = slashObj.AddComponent<KnifeSlashEffect>();
        slash.radius = knifeRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, knifeRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Vector2 toEnemy = (hit.transform.position - transform.position).normalized;
            Vector2 forward = transform.up;
            float angle = Vector2.Angle(forward, toEnemy);

            if (angle <= 90f)
            {
                // Check line of sight — can't knife through walls
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                RaycastHit2D wallHit = Physics2D.Raycast(transform.position, toEnemy, dist, wallLayer);
                if (wallHit.collider != null) continue; // wall in the way

                DeathEffect.SpawnAt(hit.transform.position, new Color(1f, 0.2f, 0.4f));
                Destroy(hit.gameObject);
                stats.AddAmmo(1);
            }
        }
    }

    void GunAttack()
    {
        if (!stats.UseAmmo()) return;

        Vector2 fireDirection = transform.up;

        // Raycast against walls AND enemies combined, take the first hit
        int combinedMask = enemyLayer | wallLayer;
        RaycastHit2D[] allHits = Physics2D.RaycastAll(transform.position, fireDirection, gunRange, combinedMask);

        // Sort by distance
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        Vector2 endPoint = (Vector2)transform.position + fireDirection * gunRange;

        foreach (RaycastHit2D hit in allHits)
        {
            if (hit.collider == null) continue;

            int hitLayer = hit.collider.gameObject.layer;

            // Hit a wall first — bullet stops, no kill
            if (((1 << hitLayer) & wallLayer) != 0)
            {
                endPoint = hit.point;
                break;
            }

            // Hit an enemy
            if (((1 << hitLayer) & enemyLayer) != 0)
            {
                endPoint = hit.point;
                DeathEffect.SpawnAt(hit.collider.transform.position, new Color(1f, 0.2f, 0.4f));
                Destroy(hit.collider.gameObject);
                break;
            }
        }

        // Spawn bullet trail
        GameObject trailObj = new GameObject("BulletTrail");
        BulletTrail trail = trailObj.AddComponent<BulletTrail>();
        trail.Init(transform.position, endPoint);

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.1f, 0.15f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, knifeRange);
    }
}
