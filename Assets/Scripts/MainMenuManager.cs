using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnStartGame()
    {
        // Load the slot selection scene (or directly the game scene)
        SceneManager.LoadScene("SlotSelectionScene");
    }

    public void OnShowScoreboard()
    {
        SceneManager.LoadScene("ScoreboardScene");
    }

    public void OnShowOptions()
    {
        SceneManager.LoadScene("OptionsScene");
    }


    public void OnShowCredits()
    {
        SceneManager.LoadScene("CreditsScene");
    }

    public void OnQuitGame()
    {
        Application.Quit();
    }
}
