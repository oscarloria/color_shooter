using UnityEngine;
using System.Collections;
using TMPro;

public class DefenseOrbShooting : MonoBehaviour
{
    [Header("Configuración del Orbe de Defensa")]
    public GameObject defenseOrbPrefab; // Prefab del orbe de defensa (debe tener el script DefenseOrb)
    public int magazineSize = 4;       // Número de orbes disponibles por carga
    public float reloadTime = 2f;      // Tiempo de recarga de los orbes
    public int orbDurability = 3;      // Durabilidad inicial de cada orbe
    public float orbitRadius = 2f;     // Radio de la órbita
    public float orbitSpeed = 90f;     // Velocidad angular en grados/segundo

    [Header("Disparo")]
    public float fireRate = 0.2f;      // Tiempo mínimo entre disparos de orbes

    [Header("Sistema de Color")]
    public Color currentColor = Color.white; // Color del orbe (se actualiza con WASD)

    [Header("UI")]
    public TextMeshProUGUI ammoText;   // Texto que muestra la munición
    public WeaponReloadIndicator reloadIndicator; // Indicador radial de recarga

    // Variables internas
    public int currentAmmo;
    public bool isReloading = false;
    private float nextFireTime = 0f;
    private float lastShotTime = 0f;

    // ----------------- NUEVO: Manejo de Idle y Attack (Orbs) en 8 direcciones -----------------
    [Header("Animaciones en 8 direcciones (Orbes)")]
    public ShipBodyOrbsIdle8Directions orbsIdleScript;       // Script Idle (orbes)
    public ShipBodyOrbsAttack8Directions orbsAttackScript;   // Script Attack (orbes)
    public float orbsAttackAnimationDuration = 0.5f;         // Duración del ataque animado
    private bool isPlayingOrbsAttackAnim = false;

    void Start()
    {
        currentAmmo = magazineSize;
        lastShotTime = Time.time;
        UpdateAmmoText();

        // No habilitamos orbsIdleScript ni orbsAttackScript aquí,
        // PlayerController se encarga de la Idle si currentWeapon=4,
        // y la corrutina PlayOrbsAttackAnimation() activará Attack.
    }

    /// <summary>
    /// Dispara un orbe de defensa. No dispara si currentColor es blanco.
    /// </summary>
    public void ShootOrb()
    {
        // No disparar si no hay color, se está recargando, sin munición, o cooldown
        if (currentColor == Color.white) return;
        if (isReloading || currentAmmo <= 0 || Time.time < nextFireTime) return;

        // Calcular la posición de spawn basándonos en el mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 direction = (mouseWorldPos - transform.position).normalized;
        float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lastShotTime = Time.time;

        // Posición de spawn en la circunferencia
        Vector3 spawnDirection = Quaternion.Euler(0, 0, newAngle) * Vector3.up;
        Vector3 spawnPosition = transform.position + spawnDirection.normalized * orbitRadius;

        // Instanciar orbe
        GameObject orbObj = Instantiate(defenseOrbPrefab, spawnPosition, Quaternion.identity);
        DefenseOrb newOrb = orbObj.GetComponent<DefenseOrb>();
        if (newOrb != null)
        {
            newOrb.currentAngle = newAngle;
            newOrb.orbitRadius = orbitRadius;
            newOrb.orbitSpeed = -orbitSpeed; // clockwise
            newOrb.durability = orbDurability;
            newOrb.orbColor = currentColor;
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoText();

        // NUEVO: Activar la animación de ataque
        StartCoroutine(PlayOrbsAttackAnimation());
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
    /// Corrutina que desactiva orbsIdleScript y activa orbsAttackScript 
    /// durante "orbsAttackAnimationDuration", luego vuelve al idle.
    /// </summary>
    IEnumerator PlayOrbsAttackAnimation()
    {
        if (isPlayingOrbsAttackAnim) yield break; // Evitar solapar animaciones

        isPlayingOrbsAttackAnim = true;
        Debug.Log("[DefenseOrbShooting] Orbs Attack => Activando anim de ataque.");

        // Desactivar Idle
        if (orbsIdleScript != null)
        {
            orbsIdleScript.enabled = false;
            Debug.Log("[DefenseOrbShooting] Idle Orbes DESACTIVADO.");
        }

        // Activar Attack
        if (orbsAttackScript != null)
        {
            orbsAttackScript.enabled = true;
            Debug.Log("[DefenseOrbShooting] Attack Orbes ACTIVADO.");
        }

        // Esperar la duración
        yield return new WaitForSeconds(orbsAttackAnimationDuration);

        // Desactivar Attack
        if (orbsAttackScript != null)
        {
            orbsAttackScript.enabled = false;
            Debug.Log("[DefenseOrbShooting] Attack Orbes DESACTIVADO.");
        }

        // Reactivar Idle
        if (orbsIdleScript != null)
        {
            orbsIdleScript.enabled = true;
            Debug.Log("[DefenseOrbShooting] Idle Orbes REACTIVADO.");
        }

        isPlayingOrbsAttackAnim = false;
        Debug.Log("[DefenseOrbShooting] Orbs Attack => Anim finalizada.");
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
