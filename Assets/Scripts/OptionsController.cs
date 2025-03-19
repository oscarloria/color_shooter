using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    /// <summary>
    /// Se llama cuando el jugador presiona "Reset High Score".
    /// </summary>
    public void OnResetHighScoreClicked()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
        Debug.Log("High Score borrado desde OptionsScene.");
    }

    /// <summary>
    /// Se llama cuando el jugador presiona el botón para regresar al menú principal.
    /// </summary>
    public void OnBackToMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Se llama cuando el jugador presiona "Reset Lumi Coins".
    /// Llama a CoinManager para reiniciar la cantidad.
    /// </summary>
    public void OnResetLumiCoinsClicked()
    {
        CoinManager.ResetCoins();
        Debug.Log("Lumi-Coins borradas desde OptionsScene.");
    }
}
