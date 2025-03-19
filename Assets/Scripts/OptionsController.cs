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

    /// <summary>
    /// Se llama cuando el jugador presiona "Reset Pistol".
    /// Restablece la pistola a magazineSize = 6 y actualiza PlayerShooting (si está presente).
    /// </summary>
    public void OnResetPistolClicked()
    {
        // Borrar la clave de PistolMagazineSize
        PlayerPrefs.DeleteKey("PistolMagazineSize");
        PlayerPrefs.Save();
        Debug.Log("PistolMagazineSize borrada. Valor reseteado a 6 por defecto.");

        // Si PlayerShooting está presente en la escena (por ejemplo, en SampleScene),
        // actualizamos en tiempo real su magazineSize y la UI.
        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.magazineSize = 6; 
            playerShooting.currentAmmo = 6;
            playerShooting.UpdateAmmoText();
            Debug.Log("Pistola reiniciada a 6 balas en PlayerShooting.");
        }
    }
}
