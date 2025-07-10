using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefab de proyectil blanco (opcional, fallback)")]
    public GameObject projectilePrefab;

    [Header("Prefabs de proyectil para cada color (Pistola)")]
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;

    public float projectileSpeed = 20f;
    public float fireRate = 0.1f;

    // Dispersión
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 0f;

    [Header("Default Values (if no PlayerPrefs)")]
    [SerializeField] private int defaultMagazineSize = 4;
    [SerializeField] private float defaultReloadTime = 2f;

    [HideInInspector] public int magazineSize;
    [HideInInspector] public bool isReloading = false;
    public float reloadTime;
    [HideInInspector] public int currentAmmo;

    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.1f;

    public Color currentColor = Color.white;

    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    private CameraZoom cameraZoom;
    private KeyCode lastPressedKey = KeyCode.None;

    private const string PISTOL_MAGAZINE_SIZE_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_TIME_KEY   = "PistolReloadTime"; // Nombre correcto

    [Header("Animaciones en 8 direcciones (Pistola)")]
    public ShipBodyPistolIdle8Directions idleScript;
    public ShipBodyAttack8Directions attackScript;
    public float attackAnimationDuration = 0.4f;

    private bool isPlayingAttackAnim = false;
    private float nextFireTime = 0f;
    
    private PlayerOutlineController outlineController;

    void Start()
    {
        magazineSize = PlayerPrefs.GetInt(PISTOL_MAGAZINE_SIZE_KEY, defaultMagazineSize);
        // --- CORRECCIÓN AQUÍ ---
        reloadTime = PlayerPrefs.GetFloat(PISTOL_RELOAD_TIME_KEY, defaultReloadTime); // Usando el nombre correcto de la constante
        
        currentAmmo = magazineSize;
        UpdateAmmoText();
        cameraZoom = FindObjectOfType<CameraZoom>();
        outlineController = GetComponentInParent<PlayerOutlineController>();
    }

    void Update()
    {
        UpdateCurrentColor();
    }

    public void Shoot()
    {
        if (Time.time < nextFireTime || currentColor == Color.white || isReloading || currentAmmo <= 0)
        {
            if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
            return;
        }

        nextFireTime = Time.time + fireRate;
        currentAmmo--;
        UpdateAmmoText();

        float dispersion = (cameraZoom != null && cameraZoom.IsZoomedIn) ? zoomedDispersionAngle : normalDispersionAngle;
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-dispersion / 2f, dispersion / 2f));

        GameObject chosenPrefab = null;
        if (currentColor == Color.red) chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue) chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green) chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;
        if (chosenPrefab == null) chosenPrefab = projectilePrefab;

        if (chosenPrefab != null)
        {
            GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);
            if (projectile.TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = projectile.transform.up * projectileSpeed;
            }
            if (projectile.TryGetComponent(out Projectile proj))
            {
                proj.projectileColor = currentColor;
            }

            StartCoroutine(PlayAttackAnimation());
            StartCoroutine(ScaleEffect());
            if (CameraShake.Instance != null) CameraShake.Instance.RecoilCamera(-transform.up);
            outlineController?.TriggerThicknessPulse();
        }

        if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
    }

    public IEnumerator Reload()
    {
        if (isReloading) yield break;
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
        ammoText.text = isReloading ? "Pistola: RELOADING" : $"Pistola: {currentAmmo}/{magazineSize}";
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
            KeyCode currentlyPressedKey = GetCurrentlyPressedKey();
            SetCurrentColorByKey(currentlyPressedKey);
        }
    }

    KeyCode GetCurrentlyPressedKey()
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