using UnityEngine;
using System.Collections;

/// <summary>
/// Rifle: fuego continuo mientras se mantiene presionado el botón del mouse.
/// </summary>
public class RifleShooting : WeaponBase
{
    [Header("Rifle — Disparo")]
    public float projectileSpeed = 20f;
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 2f;

    [Header("Rifle — Valores por Defecto")]
    [SerializeField] private float defaultFireRate = 0.08f;
    [SerializeField] private int defaultMagazineSize = 8;
    [SerializeField] private float defaultReloadTime = 2f;

    [Header("Rifle — Animaciones 8 Direcciones")]
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyRifleAttack8Directions rifleAttackScript;

    /*───────────────────  CLAVES PLAYERPREFS  ───────────────────*/

    private const string RIFLE_FIRERATE_KEY = "Rifle_FireRate";
    private const string RIFLE_MAG_KEY = "Rifle_Magazine";
    private const string RIFLE_RELOAD_KEY = "Rifle_ReloadTime";

    /*───────────────────  ESTADO  ───────────────────*/

    private bool isFiring = false;
    private bool rifleAttackActive = false;
    private Coroutine scaleEffectCoroutine;

    protected override string WeaponName => "Rifle";

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void LoadUpgrades()
    {
        fireRate = PlayerPrefs.GetFloat(RIFLE_FIRERATE_KEY, defaultFireRate);
        magazineSize = PlayerPrefs.GetInt(RIFLE_MAG_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(RIFLE_RELOAD_KEY, defaultReloadTime);
    }

    void Update()
    {
        UpdateCurrentColor();
        if (isFiring) TryContinuousShoot();
    }

    /*───────────────────  FUEGO CONTINUO  ───────────────────*/

    public void StartFiring()
    {
        if (isReloading || currentColor == Color.white || isFiring) return;
        isFiring = true;
        SetAttackAnimation(true);
    }

    public void StopFiring()
    {
        if (!isFiring) return;
        isFiring = false;
        SetAttackAnimation(false);
    }

    void TryContinuousShoot()
    {
        if (Time.time >= nextFireTime && !isReloading)
        {
            ShootOneBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootOneBullet()
    {
        if (!CanShoot())
        {
            if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
            return;
        }

        float dispersion = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedDispersionAngle : normalDispersionAngle;
        Quaternion rotation = transform.rotation *
            Quaternion.Euler(0, 0, Random.Range(-dispersion / 2f, dispersion / 2f));

        GameObject proj = SpawnProjectile(rotation, projectileSpeed);
        if (proj != null)
        {
            ConsumeAmmo();

            // Reiniciar scale effect si ya estaba corriendo
            if (scaleEffectCoroutine != null)
            {
                StopCoroutine(scaleEffectCoroutine);
                transform.localScale = Vector3.one;
            }
            scaleEffectCoroutine = StartCoroutine(ScaleEffectTracked());

            CameraShake.Instance?.RecoilCamera(-transform.up);
        }
    }

    /*───────────────────  RECARGA (OVERRIDE)  ───────────────────*/

    /// <summary>El rifle detiene el fuego antes de recargar.</summary>
    public override IEnumerator Reload()
    {
        StopFiring();
        yield return base.Reload();
    }

    /*───────────────────  SCALE EFFECT (TRACKED)  ───────────────────*/

    /// <summary>
    /// Versión del ScaleEffect que limpia la referencia al terminar.
    /// Necesario porque el rifle dispara tan rápido que puede
    /// superponerse múltiples scale effects.
    /// </summary>
    IEnumerator ScaleEffectTracked()
    {
        Vector3 original = Vector3.one;
        Vector3 target = original * scaleMultiplier;
        float elapsed = 0f;
        float half = scaleDuration / 2f;

        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(original, target, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(target, original, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = original;
        scaleEffectCoroutine = null;
    }

    /*───────────────────  ANIMACIÓN  ───────────────────*/

    void SetAttackAnimation(bool active)
    {
        if (active == rifleAttackActive) return;
        rifleAttackActive = active;

        if (active)
        {
            if (rifleIdleScript != null) rifleIdleScript.enabled = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = true;
        }
        else
        {
            if (rifleAttackScript != null) rifleAttackScript.enabled = false;
            if (rifleIdleScript != null) rifleIdleScript.enabled = true;
        }
    }
}
