using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    private float shakeDuration;
    private float shakeMagnitude;
    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeDuration -= Time.unscaledDeltaTime;

            if (shakeDuration <= 0f)
            {
                transform.localPosition = originalPos;
            }
        }
    }
}
