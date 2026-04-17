using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Tactical Intel")]
    public TextMeshProUGUI killCounterText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI shotgunAmmoText;

    [Header("Lives")]
    public Image[] fullHeartImages;
    public Image[] brokenHeartImages;


    [Header("Coordinates")]
    public TextMeshProUGUI latText;
    public TextMeshProUGUI longText;

    [Header("Detection Alert")]
    public GameObject alertPanel;
    public TextMeshProUGUI alertText;
    private float alertTimer;

    private Transform playerTransform;

    void Start()
    {
        FindPlayer();

        if (alertPanel != null)
            alertPanel.SetActive(false);
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null)
            FindPlayer();

        UpdateLives();
        UpdateKillCounter();
        UpdateAmmo();
        UpdateCoordinates();
        UpdateAlert();
    }

    void UpdateLives()
    {
        if (fullHeartImages == null || GameManager.Instance == null) return;

        int currentLives = GameManager.Instance.Lives;
        for (int i = 0; i < fullHeartImages.Length; i++)
        {
            bool alive = i < currentLives;

            // Show full heart or broken heart
            if (fullHeartImages[i] != null)
                fullHeartImages[i].gameObject.SetActive(alive);

            if (brokenHeartImages != null && i < brokenHeartImages.Length && brokenHeartImages[i] != null)
                brokenHeartImages[i].gameObject.SetActive(!alive);
        }
    }

    void UpdateKillCounter()
    {
        if (killCounterText == null || GameManager.Instance == null) return;

        int killed = GameManager.Instance.GetEnemiesKilled();
        int total = GameManager.Instance.GetTotalEnemies();
        killCounterText.text = $"KILLS: {killed} / {total}";
    }

    void UpdateAmmo()
    {
        if (playerTransform == null) return;

        PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (ammoText != null)
            ammoText.text = $"AMMO: {stats.ammo}";

        if (shotgunAmmoText != null)
            shotgunAmmoText.text = $"SHOTGUN: {stats.shotgunAmmo}";
    }

    void UpdateCoordinates()
    {
        if (playerTransform == null) return;

        Vector3 pos = playerTransform.position;

        if (latText != null)
            latText.text = $"LAT: {pos.y:F4}";
        if (longText != null)
            longText.text = $"LONG: {pos.x:F4}";
    }

    public void ShowDetectionAlert(string enemyName)
    {
        if (alertPanel == null) return;

        alertPanel.SetActive(true);
        if (alertText != null)
            alertText.text = $"BOGIE_DETECTED_{enemyName}";

        alertTimer = 2f;
    }

    void UpdateAlert()
    {
        if (alertPanel == null || !alertPanel.activeSelf) return;

        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
        {
            alertPanel.SetActive(false);
        }
    }
}
