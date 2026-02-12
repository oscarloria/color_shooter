using UnityEngine;
using System.Collections;

/// <summary>
/// Escopeta: disparo en abanico con múltiples perdigones.
/// </summary>
public class ShotgunShooting : WeaponBase
{
    [Header("Escopeta — Disparo")]
    public float projectileSpeed = 20f;

    [SerializeField] private int defaultPelletsPerShot = 4;
    [SerializeField] private float defaultSpreadAngle = 80f;
    public float zoomedSpreadAngle = 50f;

    [Header("Escopeta — Valores por Defecto")]
    [SerializeField] private int defaultMagazineSize = 4;
    [SerializeField] private float defaultReloadTime = 3f;

    [Header("Escopeta — Animaciones 8 Direcciones")]
    public ShipBodyShotgunIdle8Directions shotgunIdleScript;
    public ShipBodyShotgunAttack8Directions shotgunAttackScript;
    public float shotgunAttackAnimationDuration = 0.5f;

    /*───────────────────  CLAVES PLAYERPREFS  ───────────────────*/

    private const string SHOTGUN_PELLETS_KEY = "Shotgun_Pellets";
    private const string SHOTGUN_MAG_KEY = "Shotgun_Magazine";
    private const string SHOTGUN_RELOAD_KEY = "Shotgun_ReloadTime";

    /*───────────────────  ESTADO  ───────────────────*/

    [HideInInspector] public int pelletsPerShot;
    private float spreadAngle;
    private bool canShootCooldown = true;
    private bool isPlayingAttackAnim = false;

    protected override string WeaponName => "Escopeta";

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void LoadUpgrades()
    {
        pelletsPerShot = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, defaultPelletsPerShot);
        magazineSize = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, defaultReloadTime);
        spreadAngle = defaultSpreadAngle;
    }

    void Update()
    {
        UpdateCurrentColor();
    }

    /*───────────────────  DISPARO  ───────────────────*/

    public void Shoot()
    {
        if (!CanShoot() || !canShootCooldown)
        {
            if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
            return;
        }

        float totalSpread = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedSpreadAngle : spreadAngle;
        float angleStep = (pelletsPerShot > 1) ? totalSpread / (pelletsPerShot - 1) : 0f;
        float startAngle = -totalSpread * 0.5f;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, angle);
            SpawnProjectile(rotation, projectileSpeed);
        }

        ConsumeAmmo();
        StartCoroutine(ScaleEffect());
        StartCoroutine(FireRateCooldown());
        StartCoroutine(PlayAttackAnimation());
        CameraShake.Instance?.RecoilCamera(-transform.up);
    }

    /*───────────────────  COOLDOWN  ───────────────────*/

    IEnumerator FireRateCooldown()
    {
        canShootCooldown = false;
        yield return new WaitForSeconds(fireRate);
        canShootCooldown = true;
    }

    /*───────────────────  ANIMACIÓN  ───────────────────*/

    IEnumerator PlayAttackAnimation()
    {
        if (isPlayingAttackAnim) yield break;
        isPlayingAttackAnim = true;
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = false;
        if (shotgunAttackScript != null) shotgunAttackScript.enabled = true;
        yield return new WaitForSeconds(shotgunAttackAnimationDuration);
        if (shotgunAttackScript != null) shotgunAttackScript.enabled = false;
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = true;
        isPlayingAttackAnim = false;
    }
}
