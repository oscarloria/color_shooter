using UnityEngine;
using System.Collections;

/// <summary>
/// Orbes Defensivos: genera orbes que orbitan al jugador y destruyen
/// enemigos del mismo color. Sistema de color diferente (continuo vs. toggle).
/// </summary>
public class DefenseOrbShooting : WeaponBase
{
    [Header("Orbes — Configuración")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;

    [Header("Orbes — Valores por Defecto")]
    [SerializeField] private int defaultMagazineSize = 2;
    [SerializeField] private float defaultReloadTime = 2f;
    [SerializeField] private int defaultOrbDurability = 1;

    [Header("Orbes — Animaciones 8 Direcciones")]
    public ShipBodyOrbsIdle8Directions orbsIdleScript;
    public ShipBodyOrbsAttack8Directions orbsAttackScript;
    public float orbsAttackAnimationDuration = 0.5f;

    /*───────────────────  CLAVES PLAYERPREFS  ───────────────────*/

    private const string ORBS_DURABILITY_KEY = "Orbs_Durability";
    private const string ORBS_MAG_KEY = "Orbs_Magazine";
    private const string ORBS_RELOAD_KEY = "Orbs_ReloadTime";

    /*───────────────────  ESTADO  ───────────────────*/

    private bool isPlayingAttackAnim = false;

    protected override string WeaponName => "Orbe";

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void LoadUpgrades()
    {
        magazineSize = PlayerPrefs.GetInt(ORBS_MAG_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(ORBS_RELOAD_KEY, defaultReloadTime);
    }

    /*───────────────────  SISTEMA DE COLOR (OVERRIDE)  ───────────────────*/

    /// <summary>
    /// Los orbes usan GetKey (continuo) en lugar de GetKeyDown (toggle).
    /// Si sueltas todas las teclas → blanco → no puedes disparar.
    /// Esto permite reaccionar instantáneamente al color presionado.
    /// </summary>
    public override void UpdateCurrentColor()
    {
        if (Input.GetKey(KeyCode.W))      SetCurrentColor(Color.yellow);
        else if (Input.GetKey(KeyCode.A)) SetCurrentColor(Color.blue);
        else if (Input.GetKey(KeyCode.S)) SetCurrentColor(Color.green);
        else if (Input.GetKey(KeyCode.D)) SetCurrentColor(Color.red);
        else                              SetCurrentColor(Color.white);
    }

    /// <summary>
    /// Los orbes no cambian el color del sprite del jugador
    /// (los orbes mismos son los que se colorean al instanciarse).
    /// </summary>
    protected override void SetCurrentColor(Color color)
    {
        currentColor = color;
    }

    /*───────────────────  DISPARO  ───────────────────*/

    public void ShootOrb()
    {
        if (!CanShoot() || Time.time < nextFireTime) return;

        // Calcular posición de spawn en la dirección del mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 direction = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = transform.position + (direction * orbitRadius);

        // Seleccionar prefab por color
        GameObject prefab = GetColorPrefab();
        if (prefab == null) return;

        GameObject orbObj = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (orbObj.TryGetComponent(out DefenseOrb newOrb))
        {
            newOrb.currentAngle = angle;
            newOrb.orbitRadius = orbitRadius;
            newOrb.orbitSpeed = -orbitSpeed;
            newOrb.orbColor = currentColor;
            newOrb.durability = PlayerPrefs.GetInt(ORBS_DURABILITY_KEY, defaultOrbDurability);
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoText();
        StartCoroutine(PlayAttackAnimation());
    }

    /*───────────────────  ANIMACIÓN  ───────────────────*/

    IEnumerator PlayAttackAnimation()
    {
        if (isPlayingAttackAnim) yield break;
        isPlayingAttackAnim = true;
        if (orbsIdleScript != null) orbsIdleScript.enabled = false;
        if (orbsAttackScript != null) orbsAttackScript.enabled = true;
        yield return new WaitForSeconds(orbsAttackAnimationDuration);
        if (orbsAttackScript != null) orbsAttackScript.enabled = false;
        if (orbsIdleScript != null) orbsIdleScript.enabled = true;
        isPlayingAttackAnim = false;
    }
}
