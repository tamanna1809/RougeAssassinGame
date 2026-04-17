using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayButton()
    {
        SceneManager.LoadScene(1); // Level1
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
