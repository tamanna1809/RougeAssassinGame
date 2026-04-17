using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    public static void SpawnAt(Vector3 position, Color color)
    {
        int particleCount = 8;
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = new GameObject("DeathParticle");
            particle.transform.position = position;

            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = color;
            particle.transform.localScale = Vector3.one * 0.15f;
            sr.sortingOrder = 100;

            DeathParticle dp = particle.AddComponent<DeathParticle>();
            float angle = (360f / particleCount) * i * Mathf.Deg2Rad;
            dp.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(3f, 6f);
        }
    }

    static Sprite CreateCircleSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        float center = size / 2f;
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

public class DeathParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime = 0.5f;
    private float timer;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        velocity *= 0.95f;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            sr.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
