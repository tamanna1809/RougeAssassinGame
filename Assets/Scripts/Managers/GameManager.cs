using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float respawnDelay = 1.5f;

    private int totalEnemies;
    private int enemiesKilled;
    private bool isRespawning;
    private Vector3 playerSpawnPoint;

    private static int lives = 3;
    private static bool livesInitialized = false;

    public int Lives => lives;

    public static void RefillLives()
    {
        lives = 3;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!livesInitialized)
        {
            lives = 3;
            livesInitialized = true;
        }
    }

    void Start()
    {
        CountEnemies();

        // Record where the player starts
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerSpawnPoint = player.transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isRespawning)
        {
            // Full restart — reload scene, reset lives
            lives = 3;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void CountEnemies()
    {
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        enemiesKilled = 0;
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;

        if (enemiesKilled >= totalEnemies)
        {
            WinScreen winScreen = Object.FindObjectOfType<WinScreen>();
            if (winScreen != null) winScreen.Show();
        }
    }

    public void OnPlayerDeath()
    {
        if (isRespawning) return;
        isRespawning = true;

        lives--;

        if (lives <= 0)
        {
            Invoke(nameof(ShowGameOver), respawnDelay);
        }
        else
        {
            Invoke(nameof(RespawnPlayer), respawnDelay);
        }
    }

    void RespawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerSpawnPoint;
            player.transform.rotation = Quaternion.identity;

            // Re-enable sprite
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;

            // Reset player state
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null) stats.Respawn();

            // Re-enable movement and combat
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = true;

            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null) combat.enabled = true;
        }

        isRespawning = false;
    }

    void ShowGameOver()
    {
        GameOverScreen gameOver = Object.FindObjectOfType<GameOverScreen>();
        if (gameOver != null)
            gameOver.Show();
    }

    public int GetTotalEnemies() => totalEnemies;
    public int GetEnemiesKilled() => enemiesKilled;
}
