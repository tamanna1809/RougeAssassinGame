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
    public TextMeshProUGUI briefingText;

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

        // Check if next level is the boss fight (Level3 = index 3)
        bool nextIsBoss = hasNextLevel && nextIndex == 3;

        // Make background fully opaque for boss briefing
        Image panelBg = winPanel.GetComponent<Image>();
        if (panelBg != null)
            panelBg.color = nextIsBoss
                ? new Color(0.04f, 0.03f, 0.06f, 1f)
                : new Color(0.051f, 0.059f, 0.071f, 0.9f);

        if (winText != null)
        {
            string levelDisplay = sceneName.Replace("Level", "LEVEL ");
            if (nextIsBoss)
            {
                winText.fontSize = 22;
                winText.color = new Color(0.5f, 0.6f, 0.7f, 0.6f);
                winText.text = $"{levelDisplay} CLEARED";
                RectTransform winRect = winText.GetComponent<RectTransform>();
                if (winRect != null)
                    winRect.anchoredPosition = new Vector2(0, 260);
            }
            else
            {
                winText.text = $"{levelDisplay} CLEARED";
            }
        }

        if (buttonText != null)
        {
            if (nextIsBoss)
                buttonText.text = "START BOSS FIGHT";
            else if (hasNextLevel)
                buttonText.text = $"START LEVEL {nextIndex}";
            else
                buttonText.text = "MISSION COMPLETE";
        }

        if (briefingText != null)
        {
            if (nextIsBoss)
            {
                briefingText.gameObject.SetActive(true);
                briefingText.text =
                    "<size=38><b><color=#FF3366>TIME FOR FINAL BOSS FIGHT NOW</color></b></size>\n\n" +
                    "<size=20><color=#FF3366>BOSS_OMEGA</color> has <color=#FF3366>3 LIVES</color> and patrols the entire map.\n" +
                    "Extended vision range. Stay behind cover.</size>\n\n" +
                    "<size=24><color=#00FF88>WEAPON CHAIN</color></size>\n\n" +
                    "<size=20>Press SPACE   Knife kill   \u2192   +1 Gun ammo\n" +
                    "Press S   Gun kill   \u2192   +1 Shotgun ammo\n" +
                    "Press A   Shotgun blast   \u2192   takes <color=#FF3366>2 BOSS LIVES</color></size>\n\n" +
                    "<size=17><color=#AABBCC>Shotgun pellets pierce through enemies.\n" +
                    "Kill all 4 enemies + the boss to win.</color></size>";
            }
            else
            {
                briefingText.gameObject.SetActive(false);
            }
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
            RectTransform btnRect = nextLevelButton.GetComponent<RectTransform>();
            Image btnImg = nextLevelButton.GetComponent<Image>();
            Outline btnOutline = nextLevelButton.GetComponent<Outline>();

            if (nextIsBoss)
            {
                // Purple boss fight button
                if (btnRect != null)
                {
                    btnRect.anchoredPosition = new Vector2(0, -340f);
                    btnRect.sizeDelta = new Vector2(380, 65);
                }
                if (btnImg != null)
                    btnImg.color = new Color(0.25f, 0.05f, 0.4f, 0.9f);
                if (btnOutline != null)
                    btnOutline.effectColor = new Color(0.6f, 0.2f, 0.9f, 0.6f);
                if (buttonText != null)
                {
                    buttonText.color = Color.white;
                    buttonText.fontSize = 26;
                    buttonText.fontStyle = FontStyles.Bold;
                }
                ColorBlock cb = nextLevelButton.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.4f, 0.1f, 0.7f, 0.6f);
                cb.pressedColor = new Color(0.5f, 0.15f, 0.8f, 0.8f);
                nextLevelButton.colors = cb;
            }
            else
            {
                if (btnRect != null)
                    btnRect.anchoredPosition = new Vector2(0, -40f);
            }
        }
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
