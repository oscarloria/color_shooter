using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUpgrades : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI pistolMagazineInfoText;  // Texto para mostrar "Magazine: X/24"
    public Button upgradePistolButton;              // Botón para mejorar la pistola

    [Header("Pistol Upgrade Settings")]
    public int maxPistolMagazine = 24;             // Máximo de balas
    public int upgradeCost = 1;                    // Costo por cada bala adicional

    private PlayerShooting playerShooting;

    void Start()
    {
        // Buscar el script PlayerShooting en la escena
        playerShooting = FindObjectOfType<PlayerShooting>();
        // Actualizar la UI al inicio
        UpdatePistolUI();
    }

    /// <summary>
    /// Llamado al presionar el botón "Upgrade Pistol Magazine".
    /// </summary>
    public void OnUpgradePistolMagazineClicked()
    {
        // Leer el valor actual desde PlayerPrefs
        int currentMag = PlayerPrefs.GetInt("PistolMagazineSize", 6);

        // Verificar si ya está en el máximo
        if (currentMag >= maxPistolMagazine)
        {
            Debug.Log("La pistola ya alcanzó el límite de " + maxPistolMagazine);
            return;
        }

        // Verificar si tenemos suficientes Lumi-Coins
        if (CoinManager.CurrentCoins < upgradeCost)
        {
            Debug.Log("No tienes suficientes Lumi-Coins para mejorar la pistola.");
            return;
        }

        // Descontar coins
        CoinManager.AddCoins(-upgradeCost);

        // Incrementar magazine en 1
        currentMag += 1;
        // Guardar en PlayerPrefs
        PlayerPrefs.SetInt("PistolMagazineSize", currentMag);
        PlayerPrefs.Save();
        Debug.Log("Pistola mejorada a " + currentMag + " balas.");

        // Actualizar PlayerShooting en tiempo real si está presente
        if (playerShooting != null)
        {
            playerShooting.magazineSize = currentMag;
            // Ajustar la munición actual si quieres
            playerShooting.currentAmmo = currentMag;
            playerShooting.UpdateAmmoText();
        }

        // Actualizar la UI
        UpdatePistolUI();
    }

    /// <summary>
    /// Muestra la información actual del Magazine (por ejemplo "Magazine: 7/24").
    /// </summary>
    private void UpdatePistolUI()
    {
        if (pistolMagazineInfoText != null)
        {
            int currentMag = PlayerPrefs.GetInt("PistolMagazineSize", 6);
            pistolMagazineInfoText.text = $"Magazine: {currentMag}/{maxPistolMagazine}";
        }
    }
}
