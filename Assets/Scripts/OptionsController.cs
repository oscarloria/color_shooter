using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    // --- Claves de PlayerPrefs para la Pistola ---
    private const string HIGH_SCORE_KEY = "HighScore";
    private const string PISTOL_MAGAZINE_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_KEY = "PistolReloadTime";
    private const string PISTOL_BOTH_LEVEL_KEY = "PistolBothLevel";
    
    // --- Claves de PlayerPrefs para la Escopeta ---
    private const string SHOTGUN_PELLETS_KEY = "Shotgun_Pellets";
    private const string SHOTGUN_MAG_KEY = "Shotgun_Magazine";
    private const string SHOTGUN_RELOAD_KEY = "Shotgun_ReloadTime";
    private const string SHOTGUN_LEVEL_KEY = "Shotgun_CombinedLevel";

    // --- Métodos Generales y de High Score ---

    public void OnResetHighScoreClicked()
    {
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        PlayerPrefs.Save();
        Debug.Log("High Score borrado.");
    }

    public void OnBackToMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void OnResetLumiCoinsClicked()
    {
        CoinManager.ResetCoins();
        Debug.Log("Lumi-Coins borradas.");
    }
    
    // --- MÉTODOS DE PISTOLA RESTAURADOS ---

    public void OnResetPistolClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_MAGAZINE_KEY);
        PlayerPrefs.Save();
        Debug.Log("PistolMagazineSize borrada. El valor se reseteará al por defecto.");

        // Este código es opcional si quieres ver el cambio en una escena de pruebas,
        // pero la lógica principal es que el valor se resetee la próxima vez que se inicie el juego.
        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            // Aquí deberíamos obtener el valor por defecto del script PlayerShooting
            // En lugar de hardcodearlo, lo ideal es que PlayerShooting se encargue al iniciar.
            Debug.Log("La pistola usará su valor de cargador por defecto la próxima vez.");
        }
    }

    public void OnResetPistolReloadClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("PistolReloadTime borrada. El valor se reseteará al por defecto.");
    }

    public void OnResetPistolBothClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_BOTH_LEVEL_KEY);
        PlayerPrefs.DeleteKey(PISTOL_MAGAZINE_KEY);
        PlayerPrefs.DeleteKey(PISTOL_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("Todas las mejoras de Pistola reseteadas.");
    }
    
    // --- MÉTODOS NUEVOS PARA LA ESCOPETA ---

    public void OnResetShotgunPelletsClicked()
    {
        PlayerPrefs.DeleteKey(SHOTGUN_PELLETS_KEY);
        PlayerPrefs.Save();
        Debug.Log("Mejora de Perdigones de Escopeta reseteada.");
    }
    
    public void OnResetShotgunMagazineClicked()
    {
        PlayerPrefs.DeleteKey(SHOTGUN_MAG_KEY);
        PlayerPrefs.Save();
        Debug.Log("Mejora de Cargador de Escopeta reseteada.");
    }

    public void OnResetShotgunReloadClicked()
    {
        PlayerPrefs.DeleteKey(SHOTGUN_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("Mejora de Recarga de Escopeta reseteada.");
    }

    public void OnResetShotgunAllClicked()
    {
        PlayerPrefs.DeleteKey(SHOTGUN_PELLETS_KEY);
        PlayerPrefs.DeleteKey(SHOTGUN_MAG_KEY);
        PlayerPrefs.DeleteKey(SHOTGUN_RELOAD_KEY);
        PlayerPrefs.DeleteKey(SHOTGUN_LEVEL_KEY);
        PlayerPrefs.Save();
        Debug.Log("TODAS las mejoras de la Escopeta han sido reseteadas.");
    }
}