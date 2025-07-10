using UnityEngine;
using System.Collections;
using TMPro;

public class ShotgunShooting : MonoBehaviour
{
    [Header("Prefabs de proyectil")]
    public GameObject projectilePrefab; // Fallback
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;

    [Header("Parámetros de Disparo")]
    public float projectileSpeed = 20f;
    public float fireRate = 0.5f;

    // --- CAMBIOS AQUÍ ---
    [Header("Spread Shot - Valores por Defecto")]
    [SerializeField] private int defaultPelletsPerShot = 4;
    [SerializeField] private float defaultSpreadAngle = 80f;
    public float zoomedSpreadAngle = 50f;

    [Header("Munición y Recarga - Valores por Defecto")]
    [SerializeField] private int defaultMagazineSize = 4;
    [SerializeField] private float defaultReloadTime = 3f;
    // --- FIN DE LOS CAMBIOS ---

    // Variables que serán modificadas por las mejoras
    [HideInInspector] public int pelletsPerShot;
    [HideInInspector] public int magazineSize;
    [HideInInspector] public float reloadTime;
    
    // Variables de estado
    [HideInInspector] public bool isReloading = false;
    [HideInInspector] public int currentAmmo;
    
    // Claves de PlayerPrefs para la Escopeta
    private const string SHOTGUN_PELLETS_KEY = "Shotgun_Pellets";
    private const string SHOTGUN_MAG_KEY = "Shotgun_Magazine";
    private const string SHOTGUN_RELOAD_KEY = "Shotgun_ReloadTime";
    
    [Header("Efectos")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.15f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    private CameraZoom cameraZoom;
    private bool canShoot = true;
    
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None;

    [Header("Animaciones en 8 direcciones (Shotgun)")]
    public ShipBodyShotgunIdle8Directions shotgunIdleScript;
    public ShipBodyShotgunAttack8Directions shotgunAttackScript;
    public float shotgunAttackAnimationDuration = 0.5f;
    private bool isPlayingShotgunAttackAnim = false;

    void Start()
    {
        pelletsPerShot = PlayerPrefs.GetInt(SHOTGUN_PELLETS_KEY, defaultPelletsPerShot);
        magazineSize = PlayerPrefs.GetInt(SHOTGUN_MAG_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(SHOTGUN_RELOAD_KEY, defaultReloadTime);

        currentAmmo = magazineSize;
        UpdateAmmoText();
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void Update()
    {
        UpdateCurrentColor();
    }

    public void Shoot()
    {
        if (currentColor == Color.white || isReloading || currentAmmo <= 0 || !canShoot)
        {
            if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
            return;
        }

        currentAmmo--;
        UpdateAmmoText();

        float totalSpread = (cameraZoom != null && cameraZoom.IsZoomedIn) ? zoomedSpreadAngle : defaultSpreadAngle;
        float angleStep = (pelletsPerShot > 1) ? totalSpread / (pelletsPerShot - 1) : 0f;
        float startAngle = -totalSpread * 0.5f;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Quaternion pelletRotation = transform.rotation * Quaternion.Euler(0, 0, currentAngle);
            
            GameObject chosenPrefab = null;
            if (currentColor == Color.red) chosenPrefab = projectileRedPrefab;
            else if (currentColor == Color.blue) chosenPrefab = projectileBluePrefab;
            else if (currentColor == Color.green) chosenPrefab = projectileGreenPrefab;
            else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;
            if (chosenPrefab == null) chosenPrefab = projectilePrefab;

            if (chosenPrefab != null)
            {
                GameObject projectile = Instantiate(chosenPrefab, transform.position, pelletRotation);
                if (projectile.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = projectile.transform.up * projectileSpeed;
                }
                if (projectile.TryGetComponent(out Projectile proj))
                {
                    proj.projectileColor = currentColor;
                }
            }
        }

        StartCoroutine(ScaleEffect());
        StartCoroutine(FireRateCooldown());
        StartCoroutine(PlayShotgunAttackAnimation());
        if (CameraShake.Instance != null) CameraShake.Instance.RecoilCamera(-transform.up);
        if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
    }

    public IEnumerator Reload()
    {
        if (isReloading || currentAmmo == magazineSize) yield break;

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
    
    IEnumerator FireRateCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    void UpdateAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isReloading ? "Escopeta: RELOADING" : $"Escopeta: {currentAmmo}/{magazineSize}";
    }

    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsedTime = 0f;
        float halfDuration = scaleDuration / 2f;

        while (elapsedTime < halfDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    public void UpdateCurrentColor()
    {
        if (Input.GetKeyDown(KeyCode.W)) { SetCurrentColor(Color.yellow); lastPressedKey = KeyCode.W; }
        else if (Input.GetKeyDown(KeyCode.A)) { SetCurrentColor(Color.blue); lastPressedKey = KeyCode.A; }
        else if (Input.GetKeyDown(KeyCode.S)) { SetCurrentColor(Color.green); lastPressedKey = KeyCode.S; }
        else if (Input.GetKeyDown(KeyCode.D)) { SetCurrentColor(Color.red); lastPressedKey = KeyCode.D; }

        if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey))
        {
            KeyCode currentlyPressedKey = GetLastKeyPressed();
            SetCurrentColorByKey(currentlyPressedKey);
            lastPressedKey = currentlyPressedKey;
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

    IEnumerator PlayShotgunAttackAnimation()
    {
        if (isPlayingShotgunAttackAnim) yield break;
        isPlayingShotgunAttackAnim = true;
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = false;
        if (shotgunAttackScript != null) shotgunAttackScript.enabled = true;
        
        yield return new WaitForSeconds(shotgunAttackAnimationDuration);

        if (shotgunAttackScript != null) shotgunAttackScript.enabled = false;
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = true;
        isPlayingShotgunAttackAnim = false;
    }
}