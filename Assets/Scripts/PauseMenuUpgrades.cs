using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja mejoras combinadas de la pistola:
///   - Sube en +2 el cargador (4 → 28 en 12 pasos),
///   - Baja en -0.45s la recarga (6.0s → 0.6s en 12 pasos),
/// siguiendo una secuencia de costos Fibonacci en 12 escalones.
/// </summary>
public class PauseMenuUpgrades : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texto para mostrar 'Magazine: X/28'.")]
    public TextMeshProUGUI pistolMagazineInfoText;
    [Tooltip("Texto para mostrar 'Reload Time: X seg.'")]
    public TextMeshProUGUI pistolReloadInfoText;

    [Tooltip("Botón para la mejora combinada (aumentar magazine + disminuir reload).")]
    public Button upgradeBothButton;

    // Valores finales deseados (12 mejoras)
    [Header("Upgrade Limits")]
    [Tooltip("Máximo de balas al completar las 12 mejoras (4 + 12*2 = 28).")]
    public int maxPistolMagazine = 28;
    [Tooltip("Mínimo de recarga al completar las 12 mejoras (6.0 - 12*0.45 = 0.6).")]
    public float minReloadTime = 0.6f;

    // Incrementos por mejora
    private const int MAG_INCREMENT = 2;         // +2 balas
    private const float RELOAD_DECREMENT = 0.45f;// -0.45s recarga

    // Cantidad de mejoras totales
    private const int MAX_IMPROVEMENTS = 12;   

    // Secuencia de costos Fibonacci para las 12 mejoras
    private int[] fibCosts = new int[] { 
        1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144
    };

    // Claves para PlayerPrefs
    private const string PISTOL_MAG_KEY  = "PistolMagazineSize";
    private const string PISTOL_RELOAD_KEY = "PistolReloadTime";
    private const string PISTOL_BOTH_LEVEL_KEY = "PistolBothLevel"; 
    // ↑ Guardamos el número de mejoras realizadas hasta ahora

    // Referencia al PlayerShooting para actualizar en tiempo real
    private PlayerShooting playerShooting;

    void Start()
    {
        playerShooting = FindObjectOfType<PlayerShooting>();

        // Actualizar la UI al inicio
        UpdatePistolUI();
    }

    /// <summary>
    /// Llamado al pulsar un botón que sube +2 proyectiles y baja -0.45s de recarga,
    /// usando la secuencia Fibonacci (fibCosts) para cada mejora del 1 al 12.
    /// </summary>
    public void OnUpgradePistolBothClicked()
    {
        // Leer cuántas mejoras se han hecho ya
        int currentImprovementIndex = PlayerPrefs.GetInt(PISTOL_BOTH_LEVEL_KEY, 0);

        // Verificar si ya se hicieron las 12 mejoras
        if (currentImprovementIndex >= MAX_IMPROVEMENTS)
        {
            Debug.Log("Ya se alcanzaron las 12 mejoras máximas. No se puede mejorar más.");
            return;
        }

        // Costo de la siguiente mejora
        int cost = fibCosts[currentImprovementIndex];

        // Verificar si hay suficientes monedas
        if (CoinManager.CurrentCoins < cost)
        {
            Debug.Log($"No tienes suficientes Lumi-Coins. Se requieren {cost} coins para la mejora #{currentImprovementIndex + 1}.");
            return;
        }

        // Descontar el costo
        CoinManager.AddCoins(-cost);

        // Leer magazine y reloadTime actuales (fallback 4 y 6 si no existen)
        int currentMag = PlayerPrefs.GetInt(PISTOL_MAG_KEY, 4);
        float currentReload = PlayerPrefs.GetFloat(PISTOL_RELOAD_KEY, 6f);

        // Subir +2 balas (sin pasar de 28)
        currentMag += MAG_INCREMENT;
        if (currentMag > maxPistolMagazine)
            currentMag = maxPistolMagazine;

        // Bajar -0.45s recarga (sin pasar de 0.6)
        currentReload -= RELOAD_DECREMENT;
        if (currentReload < minReloadTime)
            currentReload = minReloadTime;

        // Guardar nuevos valores
        PlayerPrefs.SetInt(PISTOL_MAG_KEY, currentMag);
        PlayerPrefs.SetFloat(PISTOL_RELOAD_KEY, currentReload);

        // Incrementar el nivel de mejora en PlayerPrefs
        currentImprovementIndex++;
        PlayerPrefs.SetInt(PISTOL_BOTH_LEVEL_KEY, currentImprovementIndex);
        
        PlayerPrefs.Save();

        Debug.Log($"Mejora #{currentImprovementIndex} aplicada. Costo: {cost}, Magazine: {currentMag}, Reload: {currentReload}s");

        // Actualizar PlayerShooting en tiempo real
        if (playerShooting != null)
        {
            playerShooting.magazineSize = currentMag;
            playerShooting.currentAmmo = currentMag; // Ajustar la munición actual
            playerShooting.reloadTime = currentReload;
            playerShooting.UpdateAmmoText();
            
            Debug.Log($"PlayerShooting => magSize={currentMag}, reloadTime={currentReload}s, total improvements={currentImprovementIndex}.");
        }

        // Refrescar UI
        UpdatePistolUI();
    }

    /// <summary>
    /// Actualiza la parte de la UI que muestra magazine y reloadTime.
    /// </summary>
    private void UpdatePistolUI()
    {
        if (pistolMagazineInfoText != null)
        {
            int currentMag = PlayerPrefs.GetInt(PISTOL_MAG_KEY, 4);
            pistolMagazineInfoText.text = $"Magazine: {currentMag}/{maxPistolMagazine}";
        }

        if (pistolReloadInfoText != null)
        {
            float currentReload = PlayerPrefs.GetFloat(PISTOL_RELOAD_KEY, 6f);
            pistolReloadInfoText.text = $"Reload Time: {currentReload:0.00}s";
        }
    }
}
