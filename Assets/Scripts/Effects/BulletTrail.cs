using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    public float lifetime = 0.3f;
    private LineRenderer lineRenderer;

    public void Init(Vector2 start, Vector2 end)
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startColor = new Color(1f, 0.2f, 0.4f, 1f); // pink
        lineRenderer.endColor = new Color(1f, 0.2f, 0.4f, 0f);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Destroy(gameObject, lifetime);
    }
}
