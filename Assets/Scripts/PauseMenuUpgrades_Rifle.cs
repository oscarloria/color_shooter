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

    [Header("Parámetros de Mejora")]
    [SerializeField] private float fireRateReductionPerUpgrade = 0.005f;
    [SerializeField] private int magazinePerUpgrade = 5;
    [SerializeField] private float reloadReductionPerUpgrade = 0.1f;

    // Valores Base (de RifleShooting.cs)
    private const float BASE_FIRERATE = 0.1f;
    private const int BASE_MAGAZINE = 30;
    private const float BASE_RELOAD_TIME = 2f;
    
    // Límites
    private const int MAX_LEVEL = 10;
    
    // --- CAMBIO: Declarar las variables sin inicializarlas aquí ---
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
        // --- CAMBIO: Calcular los límites aquí ---
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
        
        float currentFireRate = PlayerPrefs.GetFloat(RIFLE_FIRERATE_KEY, BASE_FIRERATE);
        int currentMagazine = PlayerPrefs.GetInt(RIFLE_MAG_KEY, BASE_MAGAZINE);
        float currentReload = PlayerPrefs.GetFloat(RIFLE_RELOAD_KEY, BASE_RELOAD_TIME);

        PlayerPrefs.SetFloat(RIFLE_FIRERATE_KEY, currentFireRate - fireRateReductionPerUpgrade);
        PlayerPrefs.SetInt(RIFLE_MAG_KEY, currentMagazine + magazinePerUpgrade);
        PlayerPrefs.SetFloat(RIFLE_RELOAD_KEY, currentReload - reloadReductionPerUpgrade);
        
        PlayerPrefs.SetInt(RIFLE_LEVEL_KEY, currentLevel + 1);
        PlayerPrefs.Save();

        ApplyChangesToWeapon();
        UpdateUI();
    }
    
    private void ApplyChangesToWeapon()
    {
        if (rifleShooting == null) return;
        
        rifleShooting.fireRate = PlayerPrefs.GetFloat(RIFLE_FIRERATE_KEY, BASE_FIRERATE);
        rifleShooting.magazineSize = PlayerPrefs.GetInt(RIFLE_MAG_KEY, BASE_MAGAZINE);
        rifleShooting.reloadTime = PlayerPrefs.GetFloat(RIFLE_RELOAD_KEY, BASE_RELOAD_TIME);
        
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