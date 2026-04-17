using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    public AudioClip knifeSound;
    public AudioClip gunSound;
    public AudioClip deathSound;
    public AudioClip alertSound;

    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayKnife()
    {
        if (knifeSound != null) audioSource.PlayOneShot(knifeSound);
    }

    public void PlayGun()
    {
        if (gunSound != null) audioSource.PlayOneShot(gunSound);
    }

    public void PlayDeath()
    {
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
    }

    public void PlayAlert()
    {
        if (alertSound != null) audioSource.PlayOneShot(alertSound);
    }
}
