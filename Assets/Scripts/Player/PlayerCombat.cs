using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Knife")]
    public float knifeRange = 1.5f;
    public LayerMask enemyLayer;
    public LayerMask wallLayer;

    [Header("Gun")]
    public float gunRange = 20f;

    [Header("Shotgun")]
    public float shotgunRange = 8f;
    public int shotgunPellets = 5;
    public float shotgunSpread = 25f;
    public bool shotgunEnabled = false;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();

        if (wallLayer == 0)
            wallLayer = (1 << 10) | (1 << 11);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            KnifeAttack();

        if (Input.GetKeyDown(KeyCode.S))
            GunAttack();

        if (Input.GetKeyDown(KeyCode.A) && shotgunEnabled)
            ShotgunAttack();
    }

    void KnifeAttack()
    {
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
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                RaycastHit2D wallHit = Physics2D.Raycast(transform.position, toEnemy, dist, wallLayer);
                if (wallHit.collider != null) continue;

                BossController boss = hit.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage();
                    stats.AddAmmo(1);
                }
                else
                {
                    DeathEffect.SpawnAt(hit.transform.position, new Color(1f, 0.2f, 0.4f));
                    Destroy(hit.gameObject);
                    stats.AddAmmo(1);
                }
            }
        }
    }

    void GunAttack()
    {
        if (!stats.UseAmmo()) return;

        Vector2 fireDirection = transform.up;
        int combinedMask = enemyLayer | wallLayer;
        RaycastHit2D[] allHits = Physics2D.RaycastAll(transform.position, fireDirection, gunRange, combinedMask);
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        Vector2 endPoint = (Vector2)transform.position + fireDirection * gunRange;

        foreach (RaycastHit2D hit in allHits)
        {
            if (hit.collider == null) continue;
            int hitLayer = hit.collider.gameObject.layer;

            if (((1 << hitLayer) & wallLayer) != 0)
            {
                endPoint = hit.point;
                break;
            }

            if (((1 << hitLayer) & enemyLayer) != 0)
            {
                endPoint = hit.point;
                BossController boss = hit.collider.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage();
                    // Gun kill on boss gives shotgun ammo
                    if (shotgunEnabled) stats.AddShotgunAmmo(1);
                }
                else
                {
                    DeathEffect.SpawnAt(hit.collider.transform.position, new Color(1f, 0.2f, 0.4f));
                    Destroy(hit.collider.gameObject);
                    // Gun kill gives shotgun ammo
                    if (shotgunEnabled) stats.AddShotgunAmmo(1);
                }
                break;
            }
        }

        GameObject trailObj = new GameObject("BulletTrail");
        BulletTrail trail = trailObj.AddComponent<BulletTrail>();
        trail.Init(transform.position, endPoint);

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.1f, 0.15f);
    }

    void ShotgunAttack()
    {
        if (!stats.UseShotgunAmmo()) return;

        Vector2 baseDirection = transform.up;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        int combinedMask = enemyLayer | wallLayer;

        for (int p = 0; p < shotgunPellets; p++)
        {
            // Spread pellets in a fan
            float offset = -shotgunSpread / 2f + (shotgunSpread / (shotgunPellets - 1)) * p;
            float pelletAngle = (baseAngle + offset) * Mathf.Deg2Rad;
            Vector2 pelletDir = new Vector2(Mathf.Cos(pelletAngle), Mathf.Sin(pelletAngle));

            RaycastHit2D[] allHits = Physics2D.RaycastAll(transform.position, pelletDir, shotgunRange, combinedMask);
            System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

            Vector2 endPoint = (Vector2)transform.position + pelletDir * shotgunRange;

            foreach (RaycastHit2D hit in allHits)
            {
                if (hit.collider == null) continue;
                int hitLayer = hit.collider.gameObject.layer;

                // Wall stops the pellet
                if (((1 << hitLayer) & wallLayer) != 0)
                {
                    endPoint = hit.point;
                    break;
                }

                // Enemy — pellet pierces through, keeps going
                if (((1 << hitLayer) & enemyLayer) != 0)
                {
                    endPoint = hit.point;
                    BossController boss = hit.collider.GetComponent<BossController>();
                    if (boss != null)
                    {
                        // Shotgun takes 2 boss lives
                        boss.TakeDamage(2);
                    }
                    else
                    {
                        DeathEffect.SpawnAt(hit.collider.transform.position, new Color(1f, 0.2f, 0.4f));
                        Destroy(hit.collider.gameObject);
                    }
                    // Don't break — pellet pierces through enemies
                }
            }

            // Each pellet gets its own trail
            GameObject trailObj = new GameObject("ShotgunTrail");
            BulletTrail trail = trailObj.AddComponent<BulletTrail>();
            trail.Init(transform.position, endPoint);
        }

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.2f, 0.25f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, knifeRange);
    }
}
