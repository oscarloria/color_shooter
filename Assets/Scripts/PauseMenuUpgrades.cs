using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUpgrades : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texto para mostrar 'Magazine: X/28'.")]
    public TextMeshProUGUI pistolMagazineInfoText;
    [Tooltip("Texto para mostrar 'Reload Time: X seg.'")]
    public TextMeshProUGUI pistolReloadInfoText;

    [Tooltip("Botón para la mejora combinada (aumentar magazine + disminuir reload).")]
    public Button upgradeBothButton;

    [Header("Upgrade Limits")]
    [Tooltip("Máximo de balas al completar las 12 mejoras.")]
    public int maxPistolMagazine = 28;
    // --- CAMBIO: Nuevo valor mínimo de recarga ---
    [Tooltip("Mínimo de recarga al completar las 12 mejoras.")]
    public float minReloadTime = 0.5f;

    // --- CAMBIO: Nuevos incrementos para la progresión ---
    private const int MAG_INCREMENT = 2; // Se mantiene igual
    private const float RELOAD_DECREMENT = 0.125f; // Ajustado para la nueva progresión
    private const int MAX_IMPROVEMENTS = 12;

    private int[] fibCosts = new int[] { 
        1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144
    };

    private const string PISTOL_MAG_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_KEY = "PistolReloadTime";
    private const string PISTOL_BOTH_LEVEL_KEY = "PistolBothLevel";

    private PlayerShooting playerShooting;

    void Start()
    {
        playerShooting = FindObjectOfType<PlayerShooting>();
        UpdatePistolUI();
    }
    
    void OnEnable()
    {
        UpdatePistolUI();
    }

    public void OnUpgradePistolBothClicked()
    {
        int currentImprovementIndex = PlayerPrefs.GetInt(PISTOL_BOTH_LEVEL_KEY, 0);

        if (currentImprovementIndex >= MAX_IMPROVEMENTS)
        {
            Debug.Log("Ya se alcanzaron las 12 mejoras máximas. No se puede mejorar más.");
            return;
        }

        int cost = fibCosts[currentImprovementIndex];
        if (CoinManager.CurrentCoins < cost)
        {
            Debug.Log($"No tienes suficientes Lumi-Coins. Se requieren {cost} coins para la mejora #{currentImprovementIndex + 1}.");
            return;
        }
        
        CoinManager.AddCoins(-cost);
        
        // --- CAMBIO: Valor por defecto de recarga actualizado a 2f ---
        int currentMag = PlayerPrefs.GetInt(PISTOL_MAG_KEY, 4);
        float currentReload = PlayerPrefs.GetFloat(PISTOL_RELOAD_KEY, 2f);
        
        currentMag += MAG_INCREMENT;
        if (currentMag > maxPistolMagazine) currentMag = maxPistolMagazine;
        
        currentReload -= RELOAD_DECREMENT;
        if (currentReload < minReloadTime) currentReload = minReloadTime;

        PlayerPrefs.SetInt(PISTOL_MAG_KEY, currentMag);
        PlayerPrefs.SetFloat(PISTOL_RELOAD_KEY, currentReload);

        currentImprovementIndex++;
        PlayerPrefs.SetInt(PISTOL_BOTH_LEVEL_KEY, currentImprovementIndex);
        
        PlayerPrefs.Save();

        Debug.Log($"Mejora #{currentImprovementIndex} aplicada. Costo: {cost}, Magazine: {currentMag}, Reload: {currentReload}s");

        if (playerShooting != null)
        {
            playerShooting.magazineSize = currentMag;
            playerShooting.currentAmmo = currentMag;
            playerShooting.reloadTime = currentReload;
            playerShooting.UpdateAmmoText();
        }
        
        UpdatePistolUI();
    }

    private void UpdatePistolUI()
    {
        if (pistolMagazineInfoText != null)
        {
            int currentMag = PlayerPrefs.GetInt(PISTOL_MAG_KEY, 4);
            pistolMagazineInfoText.text = $"Magazine: {currentMag}/{maxPistolMagazine}";
        }

        if (pistolReloadInfoText != null)
        {
            // --- CAMBIO: Valor por defecto de recarga actualizado a 2f ---
            float currentReload = PlayerPrefs.GetFloat(PISTOL_RELOAD_KEY, 2f);
            pistolReloadInfoText.text = $"Reload Time: {currentReload:0.00}s";
        }
        
        if (upgradeBothButton == null) return;

        int currentLevel = PlayerPrefs.GetInt(PISTOL_BOTH_LEVEL_KEY, 0);

        if (currentLevel >= MAX_IMPROVEMENTS)
        {
            upgradeBothButton.GetComponentInChildren<TextMeshProUGUI>().text = "MAX LEVEL";
            upgradeBothButton.interactable = false;
        }
        else
        {
            int cost = fibCosts[currentLevel];
            upgradeBothButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade ({cost} coins)";
            upgradeBothButton.interactable = true;
        }
    }
}