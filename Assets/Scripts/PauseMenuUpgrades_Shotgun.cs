using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUpgrades_Shotgun : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI pelletsInfoText;
    public TextMeshProUGUI magazineInfoText;
    public TextMeshProUGUI reloadInfoText;
    public Button upgradeShotgunButton;

    [Header("Parámetros de Mejora")]
    [SerializeField] private int pelletsPerUpgrade = 1;
    [SerializeField] private int magazinePerUpgrade = 1;
    // --- CORRECCIÓN FINAL DEL VALOR POR DEFECTO ---
    [SerializeField] private float reloadReductionPerUpgrade = 0.2f;

    // Nuevos Valores Base
    private const int BASE_PELLETS = 4;
    private const int BASE_MAGAZINE = 4;
    private const float BASE_RELOAD_TIME = 3f;
    
    private const int MAX_LEVEL = 8;

    private int MAX_PELLETS;
    private int MAX_MAGAZINE;
    private float MIN_RELOAD_TIME;

    // Claves de PlayerPrefs, etc...
    // (El resto del script es idéntico al anterior y ya es correcto)
    private const string SHOTGUN_PELLETS_KEY = "Shotgun_Pellets";
    private const string SHOTGUN_MAG_KEY = "Shotgun_Magazine";
    private const string SHOTGUN_RELOAD_KEY = "Shotgun_ReloadTime";
    private const string SHOTGUN_LEVEL_KEY = "Shotgun_CombinedLevel";
    private int[] fibCosts = new int[] { 3, 5, 8, 13, 21, 34, 55, 89 };
    private ShotgunShooting shotgunShooting;

    void Awake()
    {
        MAX_PELLETS = BASE_PELLETS + (MAX_LEVEL * pelletsPerUpgrade);
        MAX_MAGAZINE = BASE_MAGAZINE + (MAX_LEVEL * magazinePerUpgrade);
        MIN_RELOAD_TIME = BASE_RELOAD_TIME - (MAX_LEVEL * reloadReductionPerUpgrade);
        
        shotgunShooting = FindObjectOfType<ShotgunShooting>();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    public void OnUpgradeShotgunClicked()
    {
        int currentLevel = PlayerPrefs.GetInt(SHOTGUN_LEVEL_KEY, 0);
        if (currentLevel >= MAX_LEVEL) return;

        int cost = fibCosts[currentLevel];
        if (CoinManager.CurrentCoins < cost) return;
        
        CoinManager.AddCoins(-cost);

        int newPellets = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, BASE_PELLETS) + pelletsPerUpgrade;
        int newMagazine = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, BASE_MAGAZINE) + magazinePerUpgrade;
        float newReload = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, BASE_RELOAD_TIME) - reloadReductionPerUpgrade;

        if (newPellets > MAX_PELLETS) newPellets = MAX_PELLETS;
        if (newMagazine > MAX_MAGAZINE) newMagazine = MAX_MAGAZINE;
        if (newReload < MIN_RELOAD_TIME) newReload = MIN_RELOAD_TIME;
        
        PlayerPrefs.SetInt(SHOTGUN_PELLETS_KEY, newPellets);
        PlayerPrefs.SetInt(SHOTGUN_MAG_KEY, newMagazine);
        PlayerPrefs.SetFloat(SHOTGUN_RELOAD_KEY, newReload);
        PlayerPrefs.SetInt(SHOTGUN_LEVEL_KEY, currentLevel + 1);
        PlayerPrefs.Save();

        ApplyChangesToWeapon(newPellets, newMagazine, newReload);
        UpdateUI();
    }
    
    private void ApplyChangesToWeapon(int pellets, int mag, float reload)
    {
        if (shotgunShooting == null) return;
        shotgunShooting.pelletsPerShot = pellets;
        shotgunShooting.magazineSize = mag;
        shotgunShooting.reloadTime = reload;
        shotgunShooting.currentAmmo = shotgunShooting.magazineSize;
    }
    
    private void UpdateUI()
    {
        int currentLevel = PlayerPrefs.GetInt(SHOTGUN_LEVEL_KEY, 0);
        int pellets = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, BASE_PELLETS);
        int mag = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, BASE_MAGAZINE);
        float reload = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, BASE_RELOAD_TIME);

        if (pelletsInfoText != null) pelletsInfoText.text = $"Pellets: {pellets} / {MAX_PELLETS}";
        if (magazineInfoText != null) magazineInfoText.text = $"Magazine: {mag} / {MAX_MAGAZINE}";
        if (reloadInfoText != null) reloadInfoText.text = $"Reload: {reload:F1}s";
        
        if (upgradeShotgunButton == null) return;

        if (currentLevel >= MAX_LEVEL)
        {
            upgradeShotgunButton.GetComponentInChildren<TextMeshProUGUI>().text = "MAX LEVEL";
            upgradeShotgunButton.interactable = false;
        }
        else
        {
            int cost = fibCosts[currentLevel];
            upgradeShotgunButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade ({cost} coins)";
            upgradeShotgunButton.interactable = true;
        }
    }
}