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
    [SerializeField] private int magazinePerUpgrade = 2;
    [SerializeField] private float reloadReductionPerUpgrade = 0.5f;

    // Valores Base
    private const int BASE_PELLETS = 5;
    private const int BASE_MAGAZINE = 8;
    private const float BASE_RELOAD_TIME = 10f;
    
    // Límite de niveles de mejora
    private const int MAX_LEVEL = 10;

    // --- CAMBIO: Declarar las variables sin inicializarlas aquí ---
    private int MAX_PELLETS;
    private int MAX_MAGAZINE;
    private float MIN_RELOAD_TIME;

    // Claves de PlayerPrefs
    private const string SHOTGUN_PELLETS_KEY = "Shotgun_Pellets";
    private const string SHOTGUN_MAG_KEY = "Shotgun_Magazine";
    private const string SHOTGUN_RELOAD_KEY = "Shotgun_ReloadTime";
    private const string SHOTGUN_LEVEL_KEY = "Shotgun_CombinedLevel";

    // Secuencia de costos
    private int[] fibCosts = new int[] { 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

    private ShotgunShooting shotgunShooting;

    // --- CAMBIO: Añadir el método Awake() para calcular los límites ---
    void Awake()
    {
        // Calculamos los valores máximos/mínimos aquí, cuando ya conocemos los valores del Inspector.
        MAX_PELLETS = BASE_PELLETS + (MAX_LEVEL * pelletsPerUpgrade);
        MAX_MAGAZINE = BASE_MAGAZINE + (MAX_LEVEL * magazinePerUpgrade);
        MIN_RELOAD_TIME = BASE_RELOAD_TIME - (MAX_LEVEL * reloadReductionPerUpgrade);
    }

    void Start()
    {
        shotgunShooting = FindObjectOfType<ShotgunShooting>();
        UpdateUI();
    }
    
    void OnEnable()
    {
        UpdateUI();
    }

    public void OnUpgradeShotgunClicked()
    {
        int currentLevel = PlayerPrefs.GetInt(SHOTGUN_LEVEL_KEY, 0);

        if (currentLevel >= MAX_LEVEL)
        {
            Debug.Log("Escopeta: Nivel máximo de mejora alcanzado.");
            return;
        }

        int cost = fibCosts[currentLevel];
        if (CoinManager.CurrentCoins < cost)
        {
            Debug.Log($"No hay suficientes monedas para mejorar la escopeta. Costo: {cost}");
            return;
        }
        
        CoinManager.AddCoins(-cost);

        int currentPellets = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, BASE_PELLETS);
        int currentMagazine = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, BASE_MAGAZINE);
        float currentReload = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, BASE_RELOAD_TIME);

        PlayerPrefs.SetInt(SHOTGUN_PELLETS_KEY, currentPellets + pelletsPerUpgrade);
        PlayerPrefs.SetInt(SHOTGUN_MAG_KEY, currentMagazine + magazinePerUpgrade);
        PlayerPrefs.SetFloat(SHOTGUN_RELOAD_KEY, currentReload - reloadReductionPerUpgrade);
        
        PlayerPrefs.SetInt(SHOTGUN_LEVEL_KEY, currentLevel + 1);
        PlayerPrefs.Save();

        ApplyChangesToWeapon();
        UpdateUI();
    }
    
    private void ApplyChangesToWeapon()
    {
        if (shotgunShooting == null) return;
        
        shotgunShooting.pelletsPerShot = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, BASE_PELLETS);
        shotgunShooting.magazineSize = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, BASE_MAGAZINE);
        shotgunShooting.reloadTime = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, BASE_RELOAD_TIME);
        
        shotgunShooting.currentAmmo = shotgunShooting.magazineSize;
    }
    
    private void UpdateUI()
    {
        int currentLevel = PlayerPrefs.GetInt(SHOTGUN_LEVEL_KEY, 0);
        
        int pellets = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, BASE_PELLETS);
        int mag = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, BASE_MAGAZINE);
        float reload = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, BASE_RELOAD_TIME);

        if (pelletsInfoText != null) 
        {
            pelletsInfoText.text = $"Pellets: {pellets} / {MAX_PELLETS}";
        }
        if (magazineInfoText != null)
        {
            magazineInfoText.text = $"Magazine: {mag} / {MAX_MAGAZINE}";
        }
        if (reloadInfoText != null)
        {
            reloadInfoText.text = $"Reload: {reload:F1}s";
        }

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