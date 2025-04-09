using UnityEngine;
using System.Collections;
using TMPro;

public class DefenseOrbShooting : MonoBehaviour
{
    [Header("Orbe de Defensa (Fallback Blanco)")]
    public GameObject defenseOrbPrefab; // Prefab fallback (blanco) con DefenseOrb

    [Header("Prefabs de Orbes de Color (cada uno debe tener 'DefenseOrb')")]
    public GameObject orbRedPrefab;
    public GameObject orbBluePrefab;
    public GameObject orbGreenPrefab;
    public GameObject orbYellowPrefab;

    [Header("Configuración del Orbe")]
    public int magazineSize = 4;
    public float reloadTime = 2f;
    public int orbDurability = 3;
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;

    [Header("Disparo")]
    public float fireRate = 0.2f; 

    [Header("Color del Orbe (WASD)")]
    public Color currentColor = Color.white;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    // Internas
    public int currentAmmo;
    public bool isReloading = false;
    private float nextFireTime = 0f;
    private float lastShotTime = 0f;

    // ----------------- Animaciones (Idle/Attack) en 8 direcciones (Orbes) -----------------
    [Header("Animaciones en 8 direcciones (Orbes)")]
    public ShipBodyOrbsIdle8Directions orbsIdleScript;     
    public ShipBodyOrbsAttack8Directions orbsAttackScript;
    public float orbsAttackAnimationDuration = 0.5f;
    private bool isPlayingOrbsAttackAnim = false;

    void Start()
    {
        currentAmmo = magazineSize;
        lastShotTime = Time.time;
        UpdateAmmoText();
    }

    /// <summary>
    /// Dispara un orbe de defensa. Usa un prefab distinto según currentColor.
    /// El prefab debe tener 'DefenseOrb' para orbitar alrededor del jugador.
    /// </summary>
    public void ShootOrb()
    {
        // 1) Chequeos
        if (currentColor == Color.white) 
        {
            Debug.LogWarning("[DefenseOrbShooting] currentColor=WHITE => no dispara orbe.");
            return;
        }
        if (isReloading || currentAmmo <= 0) 
        {
            Debug.LogWarning("[DefenseOrbShooting] Reloading o sin munición => no dispara.");
            return;
        }
        if (Time.time < nextFireTime)
        {
            Debug.LogWarning("[DefenseOrbShooting] FireRate => Bloqueado. nextFireTime="+nextFireTime);
            return;
        }

        // 2) Calcular posición
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 direction = (mouseWorldPos - transform.position).normalized;
        float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lastShotTime = Time.time;

        Vector3 spawnDirection = Quaternion.Euler(0, 0, newAngle) * Vector3.up;
        Vector3 spawnPosition = transform.position + spawnDirection.normalized * orbitRadius;

        // 3) Elegir prefab según color
        GameObject chosenPrefab = null;
        if      (currentColor == Color.red)    chosenPrefab = orbRedPrefab;
        else if (currentColor == Color.blue)   chosenPrefab = orbBluePrefab;
        else if (currentColor == Color.green)  chosenPrefab = orbGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = orbYellowPrefab;

        if (chosenPrefab == null)
        {
            Debug.LogWarning("[DefenseOrbShooting] chosenPrefab es null => usando fallback 'defenseOrbPrefab'.");
            chosenPrefab = defenseOrbPrefab;
        }

        Debug.Log("[DefenseOrbShooting] Instanciando Orbe color="+currentColor+" => "+chosenPrefab.name);

        // 4) Instanciar orbe
        GameObject orbObj = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);

        // 5) Configurar 'DefenseOrb'
        DefenseOrb newOrb = orbObj.GetComponent<DefenseOrb>();
        if (newOrb != null)
        {
            newOrb.currentAngle = newAngle;
            newOrb.orbitRadius = orbitRadius;
            // El orbe gira en sentido horario => -orbitSpeed
            newOrb.orbitSpeed = -orbitSpeed;
            newOrb.durability = orbDurability;
            newOrb.orbColor = currentColor;

            Debug.Log("[DefenseOrbShooting] Orbe => DefenseOrb asignado con color="+currentColor);
        }
        else
        {
            Debug.LogWarning("[DefenseOrbShooting] El prefab no tiene 'DefenseOrb'. No orbitará.");
        }

        // 6) Consumir munición y cooldown
        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoText();

        // 7) Activar anim de ataque
        StartCoroutine(PlayOrbsAttackAnimation());
    }

    /// <summary>
    /// Recarga orbes de defensa, con indicador radial.
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

    IEnumerator PlayOrbsAttackAnimation()
    {
        if (isPlayingOrbsAttackAnim) yield break;

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

    private void UpdateAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isReloading
            ? "Orbe: RELOADING"
            : $"Orbe: {currentAmmo}/{magazineSize}";
    }

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

        // Si no se presiona W/A/S/D => color.white
        if (!Input.GetKey(KeyCode.W) &&
            !Input.GetKey(KeyCode.A) &&
            !Input.GetKey(KeyCode.S) &&
            !Input.GetKey(KeyCode.D))
        {
            SetCurrentColor(Color.white);
        }
    }

    void SetCurrentColor(Color color)
    {
        currentColor = color;
    }
}
