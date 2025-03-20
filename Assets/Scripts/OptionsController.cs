using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de Opciones que maneja los reseteos de High Score, Lumi-Coins,
/// y los valores de la pistola (cargador, tiempo de recarga y combo).
/// </summary>
public class OptionsController : MonoBehaviour
{
    // Claves de PlayerPrefs
    private const string HIGH_SCORE_KEY         = "HighScore";
    private const string PISTOL_MAGAZINE_KEY    = "PistolMagazineSize";
    private const string PISTOL_RELOAD_KEY      = "PistolReloadTime";
    private const string PISTOL_BOTH_LEVEL_KEY  = "PistolBothLevel"; // Para el combo

    /// <summary>
    /// Se llama cuando el jugador presiona "Reset High Score".
    /// Elimina la clave "HighScore" en PlayerPrefs y la guarda.
    /// </summary>
    public void OnResetHighScoreClicked()
    {
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
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
    /// Llama a CoinManager para reiniciar la cantidad a 0.
    /// </summary>
    public void OnResetLumiCoinsClicked()
    {
        CoinManager.ResetCoins();
        Debug.Log("Lumi-Coins borradas desde OptionsScene.");
    }

    /// <summary>
    /// Se llama cuando el jugador presiona "Reset Pistol".
    /// Restablece la pistola a su valor por defecto de 4 proyectiles,
    /// si no existe clave en PlayerPrefs, y actualiza PlayerShooting.
    /// </summary>
    public void OnResetPistolClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_MAGAZINE_KEY);
        PlayerPrefs.Save();
        Debug.Log("PistolMagazineSize borrada. Valor reseteado a 4 por defecto (en PlayerShooting).");

        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            // Volver a 4 proyectiles
            playerShooting.magazineSize = 4; 
            playerShooting.currentAmmo = 4;
            playerShooting.UpdateAmmoText();
            Debug.Log("Pistola reiniciada a 4 balas en PlayerShooting.");
        }
    }

    /// <summary>
    /// Se llama cuando el jugador presiona "Reset Pistol Reload".
    /// Restablece el tiempo de recarga a su valor por defecto de 6s,
    /// si no existe clave en PlayerPrefs, y actualiza PlayerShooting.
    /// </summary>
    public void OnResetPistolReloadClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("PistolReloadTime borrada. Valor reseteado a 6.0s por defecto (en PlayerShooting).");

        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.reloadTime = 6f; 
            Debug.Log("ReloadTime reiniciado a 6s en PlayerShooting.");
        }
    }

    /// <summary>
    /// Se llama cuando el jugador presiona "Reset Pistol Combo".
    /// Restablece el combo de mejoras (PistolBothLevel) a 0 y 
    /// limpia las claves de PistolMagazineSize y PistolReloadTime,
    /// para volver a los valores iniciales (4 balas, 6s recarga).
    /// </summary>
    public void OnResetPistolBothClicked()
    {
        // Borrar nivel de mejoras del combo
        PlayerPrefs.DeleteKey(PISTOL_BOTH_LEVEL_KEY);

        // Borrar las claves de magazine y reload
        PlayerPrefs.DeleteKey(PISTOL_MAGAZINE_KEY);
        PlayerPrefs.DeleteKey(PISTOL_RELOAD_KEY);

        PlayerPrefs.Save();
        Debug.Log("Reset combo: PistolBothLevel, Magazine y Reload borrados. Volverá a 4 balas / 6s.");

        // Actualizar PlayerShooting
        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.magazineSize = 4;
            playerShooting.currentAmmo = 4;
            playerShooting.reloadTime = 6f;
            playerShooting.UpdateAmmoText();
            Debug.Log("Pistola (combo) reiniciada a 4 balas y 6s de recarga en PlayerShooting.");
        }
    }
}
