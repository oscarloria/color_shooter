using UnityEngine;
using System.Collections;
using TMPro;

public class RifleShooting : MonoBehaviour
{
    [Header("Prefabs y Parámetros de Disparo")]
    public GameObject projectilePrefab;
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;
    public float projectileSpeed = 20f;

    [Header("Dispersión")]
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 2f;

    [Header("Valores por Defecto (si no hay mejoras)")]
    [SerializeField] private float defaultFireRate = 0.1f;
    [SerializeField] private int defaultMagazineSize = 30;
    [SerializeField] private float defaultReloadTime = 2f;

    // Variables que serán modificadas por las mejoras
    [HideInInspector] public float fireRate;
    [HideInInspector] public float reloadTime;
    [HideInInspector] public int magazineSize;
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;

    // Claves de PlayerPrefs
    private const string RIFLE_FIRERATE_KEY = "Rifle_FireRate";
    private const string RIFLE_MAG_KEY = "Rifle_Magazine";
    private const string RIFLE_RELOAD_KEY = "Rifle_ReloadTime";

    [Header("Efectos y UI")]
    public float scaleMultiplier = 1.05f;
    public float scaleDuration = 0.05f;
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    // Variables internas
    private CameraZoom cameraZoom;
    private bool isFiring = false;
    private float nextFireTime = 0f;
    private Coroutine scaleEffectCoroutine;
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None;

    [Header("Animaciones")]
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyRifleAttack8Directions rifleAttackScript;
    private bool rifleAttackActive = false;

    void Start()
    {
        // Cargar mejoras desde PlayerPrefs o usar los valores por defecto
        fireRate = PlayerPrefs.GetFloat(RIFLE_FIRERATE_KEY, defaultFireRate);
        magazineSize = PlayerPrefs.GetInt(RIFLE_MAG_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(RIFLE_RELOAD_KEY, defaultReloadTime);

        currentAmmo = magazineSize;
        UpdateAmmoText();
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void Update()
    {
        UpdateCurrentColor();
        if (isFiring)
        {
            TryContinuousShoot();
        }
    }

    private void TryContinuousShoot()
    {
        if (Time.time >= nextFireTime && !isReloading)
        {
            ShootOneBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootOneBullet()
    {
        if (currentColor == Color.white || currentAmmo <= 0)
        {
            if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
            return;
        }

        currentAmmo--;
        UpdateAmmoText();

        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn) ? zoomedDispersionAngle : normalDispersionAngle;
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f));

        GameObject chosenPrefab = null;
        if (currentColor == Color.red) chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue) chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green) chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;
        if (chosenPrefab == null) chosenPrefab = projectilePrefab;

        if (chosenPrefab != null)
        {
            GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);
            if (projectile.TryGetComponent(out Rigidbody2D rb)) rb.linearVelocity = projectile.transform.up * projectileSpeed;
            if (projectile.TryGetComponent(out Projectile proj)) proj.projectileColor = currentColor;

            if (scaleEffectCoroutine != null)
            {
                StopCoroutine(scaleEffectCoroutine);
                transform.localScale = Vector3.one;
            }
            scaleEffectCoroutine = StartCoroutine(ScaleEffect());

            if (CameraShake.Instance != null) CameraShake.Instance.RecoilCamera(-transform.up);
        }

        if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
    }

    public IEnumerator Reload()
    {
        if (isReloading || currentAmmo == magazineSize) yield break;
        StopFiring();
        isReloading = true;
        UpdateAmmoText();
        if (reloadIndicator != null) reloadIndicator.ResetIndicator();

        float reloadTimer = 0f;
        while (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
            if (reloadIndicator != null) reloadIndicator.UpdateIndicator(reloadTimer / reloadTime);
            yield return null;
        }

        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoText();
        if (reloadIndicator != null) reloadIndicator.ResetIndicator();
    }

    public void UpdateAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isReloading ? "Rifle: RELOADING" : $"Rifle: {currentAmmo}/{magazineSize}";
    }

    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsedTime = 0f, halfDuration = scaleDuration / 2f;
        while (elapsedTime < halfDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        scaleEffectCoroutine = null;
    }

    public void UpdateCurrentColor()
    {
        if (Input.GetKeyDown(KeyCode.W)) { SetCurrentColor(Color.yellow); lastPressedKey = KeyCode.W; }
        else if (Input.GetKeyDown(KeyCode.A)) { SetCurrentColor(Color.blue); lastPressedKey = KeyCode.A; }
        else if (Input.GetKeyDown(KeyCode.S)) { SetCurrentColor(Color.green); lastPressedKey = KeyCode.S; }
        else if (Input.GetKeyDown(KeyCode.D)) { SetCurrentColor(Color.red); lastPressedKey = KeyCode.D; }

        if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey))
        {
            SetCurrentColorByKey(GetLastKeyPressed());
        }
    }
    
    KeyCode GetLastKeyPressed()
    {
        if (Input.GetKey(KeyCode.D)) return KeyCode.D;
        if (Input.GetKey(KeyCode.S)) return KeyCode.S;
        if (Input.GetKey(KeyCode.A)) return KeyCode.A;
        if (Input.GetKey(KeyCode.W)) return KeyCode.W;
        return KeyCode.None;
    }

    void SetCurrentColorByKey(KeyCode key)
    {
        lastPressedKey = key;
        switch (key)
        {
            case KeyCode.W: SetCurrentColor(Color.yellow); break;
            case KeyCode.A: SetCurrentColor(Color.blue); break;
            case KeyCode.S: SetCurrentColor(Color.green); break;
            case KeyCode.D: SetCurrentColor(Color.red); break;
            default: SetCurrentColor(Color.white); break;
        }
    }

    void SetCurrentColor(Color color)
    {
        currentColor = color;
        if (GetComponent<SpriteRenderer>() is SpriteRenderer sr) sr.color = currentColor;
    }
    
    public void StartFiring()
    {
        if (isReloading || currentColor == Color.white || isFiring) return;
        isFiring = true;
        if (!rifleAttackActive)
        {
            rifleAttackActive = true;
            if (rifleIdleScript != null) rifleIdleScript.enabled = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = true;
        }
    }
    
    public void StopFiring()
    {
        if (!isFiring) return;
        isFiring = false;
        if (rifleAttackActive)
        {
            rifleAttackActive = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = false;
            if (rifleIdleScript != null) rifleIdleScript.enabled = true;
        }
    }
}