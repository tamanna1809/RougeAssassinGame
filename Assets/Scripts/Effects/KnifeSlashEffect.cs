using UnityEngine;

public class KnifeSlashEffect : MonoBehaviour
{
    public float duration = 0.2f;
    public float arcAngle = 90f;
    public float radius = 1.5f;
    public int segments = 10;
    public Color slashColor = new Color(0f, 1f, 0.53f, 0.6f); // green

    private LineRenderer lineRenderer;
    private float timer;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = slashColor;
        lineRenderer.endColor = slashColor;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useWorldSpace = true;

        DrawArc();
        Destroy(gameObject, duration);
    }

    void DrawArc()
    {
        Transform parent = transform.parent != null ? transform.parent : transform;
        float baseAngle = parent.eulerAngles.z + 90f;
        float startAngle = baseAngle - arcAngle / 2f;
        float angleStep = arcAngle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector3 pos = parent.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            lineRenderer.SetPosition(i, pos);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(0.6f, 0f, timer / duration);
        Color c = slashColor;
        c.a = alpha;
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }
}
