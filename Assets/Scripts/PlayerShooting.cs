using UnityEngine;
using System.Collections;

/// <summary>
/// Pistola: disparo único con dispersión, magazine con recarga.
/// </summary>
public class PlayerShooting : WeaponBase
{
    [Header("Pistola — Disparo")]
    public float projectileSpeed = 20f;
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 0f;

    [Header("Pistola — Valores por Defecto")]
    [SerializeField] private int defaultMagazineSize = 4;
    [SerializeField] private float defaultReloadTime = 2f;

    [Header("Pistola — Animaciones 8 Direcciones")]
    public ShipBodyPistolIdle8Directions idleScript;
    public ShipBodyAttack8Directions attackScript;
    public float attackAnimationDuration = 0.4f;

    /*───────────────────  CLAVES PLAYERPREFS  ───────────────────*/

    private const string PISTOL_MAGAZINE_SIZE_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_TIME_KEY = "PistolReloadTime";

    /*───────────────────  ESTADO INTERNO  ───────────────────*/

    private PlayerOutlineController outlineController;
    private bool isPlayingAttackAnim = false;

    protected override string WeaponName => "Pistola";

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void Start()
    {
        base.Start();
        outlineController = GetComponentInParent<PlayerOutlineController>();
    }

    protected override void LoadUpgrades()
    {
        magazineSize = PlayerPrefs.GetInt(PISTOL_MAGAZINE_SIZE_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(PISTOL_RELOAD_TIME_KEY, defaultReloadTime);
    }

    void Update()
    {
        UpdateCurrentColor();
    }

    /*───────────────────  DISPARO  ───────────────────*/

    public void Shoot()
    {
        if (Time.time < nextFireTime || !CanShoot())
        {
            if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
            return;
        }

        nextFireTime = Time.time + fireRate;

        float dispersion = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedDispersionAngle : normalDispersionAngle;
        Quaternion rotation = transform.rotation *
            Quaternion.Euler(0, 0, Random.Range(-dispersion / 2f, dispersion / 2f));

        GameObject proj = SpawnProjectile(rotation, projectileSpeed);
        if (proj != null)
        {
            ConsumeAmmo();
            StartCoroutine(PlayAttackAnimation());
            StartCoroutine(ScaleEffect());
            CameraShake.Instance?.RecoilCamera(-transform.up);
            outlineController?.TriggerThicknessPulse();
        }
    }

    /*───────────────────  ANIMACIÓN  ───────────────────*/

    IEnumerator PlayAttackAnimation()
    {
        if (isPlayingAttackAnim) yield break;
        isPlayingAttackAnim = true;
        if (idleScript != null) idleScript.enabled = false;
        if (attackScript != null) attackScript.enabled = true;
        yield return new WaitForSeconds(attackAnimationDuration);
        if (attackScript != null) attackScript.enabled = false;
        if (idleScript != null) idleScript.enabled = true;
        isPlayingAttackAnim = false;
    }
}
