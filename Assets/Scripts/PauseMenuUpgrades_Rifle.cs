using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUpgrades_Rifle : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI fireRateInfoText;
    public TextMeshProUGUI magazineInfoText;
    public TextMeshProUGUI reloadInfoText;
    public Button upgradeRifleButton;

    // --- CAMBIOS DE BALANCEO ---
    [Header("Parámetros de Mejora")]
    [SerializeField] private float fireRateReductionPerUpgrade = 0.004f;
    [SerializeField] private int magazinePerUpgrade = 3;
    [SerializeField] private float reloadReductionPerUpgrade = 0.1f;

    // Nuevos Valores Base
    private const float BASE_FIRERATE = 0.08f;
    private const int BASE_MAGAZINE = 8;
    private const float BASE_RELOAD_TIME = 2f;
    
    // Límites
    private const int MAX_LEVEL = 10;
    // --- FIN DE CAMBIOS ---
    
    private float MIN_FIRERATE;
    private int MAX_MAGAZINE;
    private float MIN_RELOAD_TIME;

    // Claves de PlayerPrefs
    private const string RIFLE_FIRERATE_KEY = "Rifle_FireRate";
    private const string RIFLE_MAG_KEY = "Rifle_Magazine";
    private const string RIFLE_RELOAD_KEY = "Rifle_ReloadTime";
    private const string RIFLE_LEVEL_KEY = "Rifle_CombinedLevel";

    private int[] fibCosts = new int[] { 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

    private RifleShooting rifleShooting;

    void Awake()
    {
        MIN_FIRERATE = BASE_FIRERATE - (MAX_LEVEL * fireRateReductionPerUpgrade);
        MAX_MAGAZINE = BASE_MAGAZINE + (MAX_LEVEL * magazinePerUpgrade);
        MIN_RELOAD_TIME = BASE_RELOAD_TIME - (MAX_LEVEL * reloadReductionPerUpgrade);

        rifleShooting = FindObjectOfType<RifleShooting>();
    }
    
    void OnEnable()
    {
        UpdateUI();
    }

    public void OnUpgradeRifleClicked()
    {
        int currentLevel = PlayerPrefs.GetInt(RIFLE_LEVEL_KEY, 0);
        if (currentLevel >= MAX_LEVEL) return;

        int cost = fibCosts[currentLevel];
        if (CoinManager.CurrentCoins < cost) return;

        CoinManager.AddCoins(-cost);
        
        // Calcular nuevos valores
        float newFireRate = PlayerPrefs.GetFloat(RIFLE_FIRERATE_KEY, BASE_FIRERATE) - fireRateReductionPerUpgrade;
        int newMagazine = PlayerPrefs.GetInt(RIFLE_MAG_KEY, BASE_MAGAZINE) + magazinePerUpgrade;
        float newReload = PlayerPrefs.GetFloat(RIFLE_RELOAD_KEY, BASE_RELOAD_TIME) - reloadReductionPerUpgrade;

        // --- LÓGICA DE LÍMITES AÑADIDA ---
        if (newFireRate < MIN_FIRERATE) newFireRate = MIN_FIRERATE;
        if (newMagazine > MAX_MAGAZINE) newMagazine = MAX_MAGAZINE;
        if (newReload < MIN_RELOAD_TIME) newReload = MIN_RELOAD_TIME;

        // Guardar valores ya limitados
        PlayerPrefs.SetFloat(RIFLE_FIRERATE_KEY, newFireRate);
        PlayerPrefs.SetInt(RIFLE_MAG_KEY, newMagazine);
        PlayerPrefs.SetFloat(RIFLE_RELOAD_KEY, newReload);
        
        PlayerPrefs.SetInt(RIFLE_LEVEL_KEY, currentLevel + 1);
        PlayerPrefs.Save();

        ApplyChangesToWeapon(newFireRate, newMagazine, newReload);
        UpdateUI();
    }
    
    // --- MÉTODO REFACTORIZADO ---
    private void ApplyChangesToWeapon(float fireRate, int mag, float reload)
    {
        if (rifleShooting == null) return;
        
        rifleShooting.fireRate = fireRate;
        rifleShooting.magazineSize = mag;
        rifleShooting.reloadTime = reload;
        
        rifleShooting.currentAmmo = rifleShooting.magazineSize;
    }
    
    private void UpdateUI()
    {
        int currentLevel = PlayerPrefs.GetInt(RIFLE_LEVEL_KEY, 0);
        
        float fireRate = PlayerPrefs.GetFloat(RIFLE_FIRERATE_KEY, BASE_FIRERATE);
        int mag = PlayerPrefs.GetInt(RIFLE_MAG_KEY, BASE_MAGAZINE);
        float reload = PlayerPrefs.GetFloat(RIFLE_RELOAD_KEY, BASE_RELOAD_TIME);

        if (fireRateInfoText != null) fireRateInfoText.text = $"Fire Rate: {fireRate:F3}s";
        if (magazineInfoText != null) magazineInfoText.text = $"Magazine: {mag} / {MAX_MAGAZINE}";
        if (reloadInfoText != null) reloadInfoText.text = $"Reload: {reload:F1}s";
        
        if (upgradeRifleButton == null) return;

        if (currentLevel >= MAX_LEVEL)
        {
            upgradeRifleButton.GetComponentInChildren<TextMeshProUGUI>().text = "MAX LEVEL";
            upgradeRifleButton.interactable = false;
        }
        else
        {
            int cost = fibCosts[currentLevel];
            upgradeRifleButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade ({cost} coins)";
            upgradeRifleButton.interactable = true;
        }
    }
}