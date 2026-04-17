using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinScreen : MonoBehaviour
{
    public GameObject winPanel;
    public TextMeshProUGUI winText;
    public Button nextLevelButton;
    public TextMeshProUGUI buttonText;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    public void Show()
    {
        if (winPanel == null) return;

        winPanel.SetActive(true);

        string sceneName = SceneManager.GetActiveScene().name;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        bool hasNextLevel = nextIndex < SceneManager.sceneCountInBuildSettings;

        // Show which level was cleared
        if (winText != null)
        {
            string levelDisplay = sceneName.Replace("Level", "LEVEL ");
            winText.text = $"{levelDisplay} CLEARED";
        }

        // Button text
        if (buttonText != null)
        {
            if (hasNextLevel)
            {
                string nextName = $"LEVEL {nextIndex}";
                buttonText.text = $"START {nextName}";
            }
            else
            {
                buttonText.text = "MISSION COMPLETE";
            }
        }

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(true);
    }

    void OnNextLevelClicked()
    {
        GameManager.RefillLives();

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
