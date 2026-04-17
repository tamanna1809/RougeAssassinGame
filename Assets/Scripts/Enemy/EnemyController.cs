using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] waypoints;
    public float moveSpeed = 1f;
    public float waitTime = 1f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2.5f;
    public LayerMask obstacleMask;

    [Header("Smoothing")]
    public float rotationSpeed = 5f;
    public float steerSmoothing = 4f;

    private int currentWaypointIndex;
    private float waitTimer;
    private bool waiting;
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private float currentAngle;
    private float stuckTimer;
    private Vector2 lastPosition;

    // Context steering: 16 directions sampled around the enemy
    private const int DIR_COUNT = 16;
    private float[] interest = new float[DIR_COUNT];
    private float[] danger = new float[DIR_COUNT];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (obstacleMask == 0)
            obstacleMask = (1 << 10) | (1 << 11);

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }

        currentDirection = transform.up;
        currentAngle = 0f;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        if (waiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                stuckTimer = 0f;
            }
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 target = waypoints[currentWaypointIndex].position;

        // Reached waypoint?
        if (Vector2.Distance(transform.position, target) < 0.5f)
        {
            waiting = true;
            waitTimer = waitTime;
            rb.velocity = Vector2.zero;
            stuckTimer = 0f;
            return;
        }

        // Context steering
        Vector2 bestDir = GetBestDirection(target);

        // Smooth the direction so there's no jitter
        currentDirection = Vector2.Lerp(currentDirection, bestDir, steerSmoothing * Time.fixedDeltaTime);
        currentDirection.Normalize();

        // Smooth rotation
        float targetAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        // Move
        rb.velocity = currentDirection * moveSpeed;

        // Stuck detection
        stuckTimer += Time.fixedDeltaTime;
        if (stuckTimer >= 1f)
        {
            float moved = Vector2.Distance(transform.position, lastPosition);
            if (moved < 0.15f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }

    Vector2 GetBestDirection(Vector2 target)
    {
        Vector2 toTarget = (target - (Vector2)transform.position).normalized;

        // Fill interest and danger maps
        for (int i = 0; i < DIR_COUNT; i++)
        {
            float angle = (360f / DIR_COUNT) * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Interest: how much this direction points toward the target
            interest[i] = Mathf.Max(0f, Vector2.Dot(dir, toTarget));

            // Danger: raycast for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, avoidDistance, obstacleMask);
            if (hit.collider != null)
            {
                danger[i] = 1f - (hit.distance / avoidDistance);
            }
            else
            {
                danger[i] = 0f;
            }
        }

        // Subtract danger from interest, pick best direction
        Vector2 chosenDir = Vector2.zero;
        float bestScore = -1f;

        for (int i = 0; i < DIR_COUNT; i++)
        {
            float score = interest[i] - danger[i] * 1.5f;
            if (score > bestScore)
            {
                bestScore = score;
                float angle = (360f / DIR_COUNT) * i * Mathf.Deg2Rad;
                chosenDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }

        // Fallback: if all directions are blocked, pick least dangerous
        if (bestScore <= 0f)
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

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled();
        }
    }
}
