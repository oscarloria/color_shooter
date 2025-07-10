using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUpgrades_Orbs : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI durabilityInfoText;
    public TextMeshProUGUI magazineInfoText;
    public TextMeshProUGUI reloadInfoText;
    public Button upgradeOrbsButton;

    [Header("Parámetros de Mejora")]
    [SerializeField] private int durabilityPerUpgrade = 1;
    [SerializeField] private int magazinePerUpgrade = 1;
    [SerializeField] private float reloadReductionPerUpgrade = 0.2f;

    // Nuevos Valores Base
    private const int BASE_DURABILITY = 1;
    private const int BASE_MAGAZINE = 2;
    private const float BASE_RELOAD_TIME = 2f;
    
    private const int MAX_LEVEL = 5;

    private int MAX_DURABILITY;
    private int MAX_MAGAZINE;
    private float MIN_RELOAD_TIME;

    private const string ORBS_DURABILITY_KEY = "Orbs_Durability";
    private const string ORBS_MAG_KEY = "Orbs_Magazine";
    private const string ORBS_RELOAD_KEY = "Orbs_ReloadTime";
    private const string ORBS_LEVEL_KEY = "Orbs_CombinedLevel";

    private int[] fibCosts = new int[] { 10, 20, 35, 55, 80 };
    private DefenseOrbShooting defenseOrbShooting;

    void Awake()
    {
        MAX_DURABILITY = BASE_DURABILITY + (MAX_LEVEL * durabilityPerUpgrade);
        MAX_MAGAZINE = BASE_MAGAZINE + (MAX_LEVEL * magazinePerUpgrade);
        MIN_RELOAD_TIME = BASE_RELOAD_TIME - (MAX_LEVEL * reloadReductionPerUpgrade);
        
        defenseOrbShooting = FindObjectOfType<DefenseOrbShooting>();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    public void OnUpgradeOrbsClicked()
    {
        int currentLevel = PlayerPrefs.GetInt(ORBS_LEVEL_KEY, 0);
        if (currentLevel >= MAX_LEVEL) return;

        int cost = fibCosts[currentLevel];
        if (CoinManager.CurrentCoins < cost) return;

        CoinManager.AddCoins(-cost);

        int newDurability = PlayerPrefs.GetInt(ORBS_DURABILITY_KEY, BASE_DURABILITY) + durabilityPerUpgrade;
        int newMagazine = PlayerPrefs.GetInt(ORBS_MAG_KEY, BASE_MAGAZINE) + magazinePerUpgrade;
        float newReload = PlayerPrefs.GetFloat(ORBS_RELOAD_KEY, BASE_RELOAD_TIME) - reloadReductionPerUpgrade;

        if (newDurability > MAX_DURABILITY) newDurability = MAX_DURABILITY;
        if (newMagazine > MAX_MAGAZINE) newMagazine = MAX_MAGAZINE;
        if (newReload < MIN_RELOAD_TIME) newReload = MIN_RELOAD_TIME;
        
        PlayerPrefs.SetInt(ORBS_DURABILITY_KEY, newDurability);
        PlayerPrefs.SetInt(ORBS_MAG_KEY, newMagazine);
        PlayerPrefs.SetFloat(ORBS_RELOAD_KEY, newReload);
        PlayerPrefs.SetInt(ORBS_LEVEL_KEY, currentLevel + 1);
        PlayerPrefs.Save();

        ApplyChangesToWeapon(newMagazine, newReload);
        UpdateUI();
    }
    
    private void ApplyChangesToWeapon(int mag, float reload)
    {
        if (defenseOrbShooting == null) return;
        
        defenseOrbShooting.magazineSize = mag;
        defenseOrbShooting.reloadTime = reload;
        
        defenseOrbShooting.currentAmmo = defenseOrbShooting.magazineSize;
    }
    
    private void UpdateUI()
    {
        int currentLevel = PlayerPrefs.GetInt(ORBS_LEVEL_KEY, 0);
        
        int durability = PlayerPrefs.GetInt(ORBS_DURABILITY_KEY, BASE_DURABILITY);
        int mag = PlayerPrefs.GetInt(ORBS_MAG_KEY, BASE_MAGAZINE);
        float reload = PlayerPrefs.GetFloat(ORBS_RELOAD_KEY, BASE_RELOAD_TIME);

        if (durabilityInfoText != null) durabilityInfoText.text = $"Durability: {durability} / {MAX_DURABILITY}";
        if (magazineInfoText != null) magazineInfoText.text = $"Orbs: {mag} / {MAX_MAGAZINE}";
        if (reloadInfoText != null) reloadInfoText.text = $"Reload: {reload:F1}s";
        
        if (upgradeOrbsButton == null) return;

        if (currentLevel >= MAX_LEVEL)
        {
            upgradeOrbsButton.GetComponentInChildren<TextMeshProUGUI>().text = "MAX LEVEL";
            upgradeOrbsButton.interactable = false;
        }
        else
        {
            int cost = fibCosts[currentLevel];
            // --- CORRECCIÓN FINAL AQUÍ ---
            upgradeOrbsButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade ({cost} coins)";
            upgradeOrbsButton.interactable = true;
        }
    }
}