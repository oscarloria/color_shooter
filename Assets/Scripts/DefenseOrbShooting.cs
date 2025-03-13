using UnityEngine;
using System.Collections;
using TMPro;

public class DefenseOrbShooting : MonoBehaviour
{
    [Header("Configuración del Orbe de Defensa")]
    public GameObject defenseOrbPrefab; // Prefab del orbe de defensa (debe tener el script DefenseOrb)
    public int magazineSize = 4;          // Número de orbes disponibles por carga
    public float reloadTime = 2f;         // Tiempo de recarga para los orbes
    public int orbDurability = 3;         // Durabilidad inicial de cada orbe (golpes que soporta)
    public float orbitRadius = 2f;        // Radio de la órbita alrededor del jugador
    public float orbitSpeed = 90f;        // Velocidad angular en grados/segundo

    [Header("Disparo")]
    public float fireRate = 0.2f;         // Tiempo mínimo entre disparos de orbes

    [Header("Sistema de Color")]
    public Color currentColor = Color.white; // Color asignado al orbe (se actualiza con WASD)

    [Header("UI")]
    public TextMeshProUGUI ammoText;          // Texto que muestra la munición del orbe
    public WeaponReloadIndicator reloadIndicator;  // Indicador radial de recarga

    // Variables internas accesibles para PlayerController
    public int currentAmmo;
    public bool isReloading = false;
    private float nextFireTime = 0f;
    private float lastShotTime = 0f;

    void Start()
    {
        currentAmmo = magazineSize;
        lastShotTime = Time.time;
        UpdateAmmoText();
    }

    /// <summary>
    /// Dispara un orbe de defensa. No dispara si currentColor es blanco.
    /// </summary>
    public void ShootOrb()
    {
        // No disparar si no se ha seleccionado un color.
        if (currentColor == Color.white) return;
        if (isReloading || currentAmmo <= 0 || Time.time < nextFireTime)
            return;

        // Calcular la posición de spawn basándose en la posición del mouse.
        // Convertir la posición del mouse (screen space) a world space.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Asegurarse de que está en el plano 2D.
        Vector3 direction = (mouseWorldPos - transform.position).normalized;
        // Calcular el ángulo en grados a partir de la dirección.
        float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lastShotTime = Time.time;

        // Calcular la posición de spawn sobre la circunferencia de radio orbitRadius.
        Vector3 spawnDirection = Quaternion.Euler(0, 0, newAngle) * Vector3.up;
        Vector3 spawnPosition = transform.position + spawnDirection.normalized * orbitRadius;

        // Instanciar el orbe (como objeto independiente para evitar que la rotación del jugador lo afecte)
        GameObject orbObj = Instantiate(defenseOrbPrefab, spawnPosition, Quaternion.identity);
        DefenseOrb newOrb = orbObj.GetComponent<DefenseOrb>();
        if (newOrb != null)
        {
            newOrb.currentAngle = newAngle;
            newOrb.orbitRadius = orbitRadius;
            // Para que los orbes sigan girando en sentido clockwise, se asigna un orbitSpeed negativo.
            newOrb.orbitSpeed = -orbitSpeed;
            newOrb.durability = orbDurability;
            newOrb.orbColor = currentColor;
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoText();
    }

    /// <summary>
    /// Corrutina para recargar los orbes de defensa, actualizando el indicador radial.
    /// </summary>
    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize) yield break;

        isReloading = true;
        UpdateAmmoText();

        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator();

        float reloadTimer = 0f;
        while (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
            if (reloadIndicator != null)
                reloadIndicator.UpdateIndicator(reloadTimer / reloadTime);
            yield return null;
        }

        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoText();

        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator();
    }

    /// <summary>
    /// Actualiza el texto de la munición en la UI.
    /// </summary>
    private void UpdateAmmoText()
    {
        if (ammoText == null) return;
        if (isReloading)
            ammoText.text = "Orbe: RELOADING";
        else
            ammoText.text = $"Orbe: {currentAmmo}/{magazineSize}";
    }

    /// <summary>
    /// Actualiza el color asignado a los orbes basándose en la entrada WASD.
    /// Si no se mantiene ninguna tecla WASD, se asigna Color.white.
    /// </summary>
    public void UpdateCurrentColor()
    {
        if (Input.GetKeyDown(KeyCode.W))
            SetCurrentColor(Color.yellow);
        else if (Input.GetKeyDown(KeyCode.A))
            SetCurrentColor(Color.blue);
        else if (Input.GetKeyDown(KeyCode.S))
            SetCurrentColor(Color.green);
        else if (Input.GetKeyDown(KeyCode.D))
            SetCurrentColor(Color.red);

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            SetCurrentColor(Color.white);
        }
    }

    void SetCurrentColor(Color color)
    {
        currentColor = color;
    }
}
