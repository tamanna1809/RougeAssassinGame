using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRange = 5f;
    public float visionAngle = 27f;
    public LayerMask obstacleLayer;

    [Header("Visual")]
    public int coneResolution = 30;
    public Material coneMaterial;

    private Transform player;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Create vision cone mesh renderer
        GameObject coneObj = new GameObject("VisionConeMesh");
        coneObj.transform.SetParent(transform);
        coneObj.transform.localPosition = Vector3.zero;
        coneObj.transform.localRotation = Quaternion.identity;

        meshFilter = coneObj.AddComponent<MeshFilter>();
        meshRenderer = coneObj.AddComponent<MeshRenderer>();

        if (coneMaterial != null)
        {
            meshRenderer.material = coneMaterial;
        }
        else
        {
            // Fallback: create a transparent red material at runtime
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0.2f, 0.4f, 0.25f);
            meshRenderer.material = mat;
        }
    }

    void Update()
    {
        if (player == null) return;

        CheckForPlayer();
        DrawVisionCone();
    }

    void CheckForPlayer()
    {
        Vector2 dirToPlayer = (player.position - transform.position);
        float distToPlayer = dirToPlayer.magnitude;

        // Distance check
        if (distToPlayer > visionRange) return;

        // Angle check
        Vector2 forward = transform.up;
        float angle = Vector2.Angle(forward, dirToPlayer.normalized);
        if (angle > visionAngle) return;

        // Obstacle raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distToPlayer, obstacleLayer);
        if (hit.collider != null) return;

        // Player detected — kill
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.Die();
        }
    }

    void DrawVisionCone()
    {
        Mesh mesh = new Mesh();

        int segments = coneResolution;
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // origin (local space)

        float startAngle = -visionAngle;
        float angleStep = (visionAngle * 2f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            float rad = (currentAngle + 90f) * Mathf.Deg2Rad; // +90 because forward is up

            Vector2 localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Raycast to check for walls
            Vector2 worldDir = transform.TransformDirection(new Vector3(localDir.x, localDir.y, 0f));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDir, visionRange, obstacleLayer);

            float dist = hit.collider != null ? hit.distance : visionRange;
            vertices[i + 1] = new Vector3(localDir.x * dist, localDir.y * dist, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.4f, 0.3f);

        Vector3 leftBound = Quaternion.Euler(0, 0, visionAngle) * transform.up * visionRange;
        Vector3 rightBound = Quaternion.Euler(0, 0, -visionAngle) * transform.up * visionRange;

        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
    }
}
