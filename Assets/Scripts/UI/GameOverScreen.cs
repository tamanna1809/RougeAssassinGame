using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI sassText;
    public Button restartButton;
    public Button mainMenuButton;

    private static string[] sassyMessages = {
        "GOING ROGUE DOESN'T MEAN GOING BLIND.",
        "THE ENEMIES SEND THEIR REGARDS.",
        "YOU BROUGHT A KNIFE TO A VISION CONE FIGHT.",
        "TIP: TRY NOT WALKING INTO THEIR FACE.",
        "ROGUE STATUS: REVOKED.",
        "THE ENEMIES DIDN'T EVEN BREAK A SWEAT.",
        "ROGUE DOWN. DIGNITY ALSO DOWN."
    };

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "MISSION FAILED";

        if (sassText != null)
            sassText.text = sassyMessages[Random.Range(0, sassyMessages.Length)];
    }

    void OnRestartClicked()
    {
        GameManager.RefillLives();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnMainMenuClicked()
    {
        GameManager.RefillLives();
        SceneManager.LoadScene(0);
    }
}
