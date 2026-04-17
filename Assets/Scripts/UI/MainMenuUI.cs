using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneManager.LoadScene(1); // Level1 is index 1 in build settings
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
