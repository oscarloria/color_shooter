using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    /// <summary>
    /// Se llama cuando el jugador presiona el botón "Reset High Score".
    /// Borra la clave "HighScore" en PlayerPrefs y la guarda.
    /// </summary>
    public void OnResetHighScoreClicked()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
        Debug.Log("High Score borrado desde la OptionsScene");
    }

    /// <summary>
    /// Se llama cuando el jugador presiona un botón para regresar al menú principal.
    /// Carga la escena MainMenuScene.
    /// </summary>
    public void OnBackToMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
