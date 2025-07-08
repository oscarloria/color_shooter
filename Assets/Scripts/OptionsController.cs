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

    // --- Claves de PlayerPrefs para el Rifle ---
    private const string RIFLE_FIRERATE_KEY = "Rifle_FireRate";
    private const string RIFLE_MAG_KEY = "Rifle_Magazine";
    private const string RIFLE_RELOAD_KEY = "Rifle_ReloadTime";
    private const string RIFLE_LEVEL_KEY = "Rifle_CombinedLevel";

    // --- Claves de PlayerPrefs para el Orbes ---
    private const string ORBS_DURABILITY_KEY = "Orbs_Durability";
    private const string ORBS_MAG_KEY = "Orbs_Magazine";
    private const string ORBS_RELOAD_KEY = "Orbs_ReloadTime";
    private const string ORBS_LEVEL_KEY = "Orbs_CombinedLevel";



    // --- Métodos Generales y de Reseteo (Pistola, Escopeta, etc.) ---

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

    public void OnResetPistolClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_MAGAZINE_KEY);
        PlayerPrefs.Save();
        Debug.Log("PistolMagazineSize borrada.");
    }

    public void OnResetPistolReloadClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("PistolReloadTime borrada.");
    }

    public void OnResetPistolBothClicked()
    {
        PlayerPrefs.DeleteKey(PISTOL_BOTH_LEVEL_KEY);
        PlayerPrefs.DeleteKey(PISTOL_MAGAZINE_KEY);
        PlayerPrefs.DeleteKey(PISTOL_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("Todas las mejoras de Pistola reseteadas.");
    }

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

    // --- NUEVOS MÉTODOS PARA EL RIFLE ---

    public void OnResetRifleFireRateClicked()
    {
        PlayerPrefs.DeleteKey(RIFLE_FIRERATE_KEY);
        PlayerPrefs.Save();
        Debug.Log("Mejora de Cadencia de Tiro del Rifle reseteada.");
    }

    public void OnResetRifleMagazineClicked()
    {
        PlayerPrefs.DeleteKey(RIFLE_MAG_KEY);
        PlayerPrefs.Save();
        Debug.Log("Mejora de Cargador del Rifle reseteada.");
    }

    public void OnResetRifleReloadClicked()
    {
        PlayerPrefs.DeleteKey(RIFLE_RELOAD_KEY);
        PlayerPrefs.Save();
        Debug.Log("Mejora de Recarga del Rifle reseteada.");
    }

    public void OnResetRifleAllClicked()
    {
        PlayerPrefs.DeleteKey(RIFLE_FIRERATE_KEY);
        PlayerPrefs.DeleteKey(RIFLE_MAG_KEY);
        PlayerPrefs.DeleteKey(RIFLE_RELOAD_KEY);
        PlayerPrefs.DeleteKey(RIFLE_LEVEL_KEY);
        PlayerPrefs.Save();
        Debug.Log("TODAS las mejoras del Rifle han sido reseteadas.");
    }
    
    public void OnResetOrbsAllClicked()
{
    PlayerPrefs.DeleteKey(ORBS_DURABILITY_KEY);
    PlayerPrefs.DeleteKey(ORBS_MAG_KEY);
    PlayerPrefs.DeleteKey(ORBS_RELOAD_KEY);
    PlayerPrefs.DeleteKey(ORBS_LEVEL_KEY);
    PlayerPrefs.Save();
    Debug.Log("TODAS las mejoras de los Orbes han sido reseteadas.");
}

}