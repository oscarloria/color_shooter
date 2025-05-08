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
    [SerializeField] private float defaultReloadTime = 6f;

    [HideInInspector] public int magazineSize;
    [HideInInspector] public bool isReloading = false;
    public float reloadTime;
    [HideInInspector] public int currentAmmo;

    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.1f;

    public Color currentColor = Color.white;

    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator; // Referencia al script del indicador

    private CameraZoom cameraZoom;
    private KeyCode lastPressedKey = KeyCode.None; // Guarda la última tecla WASD presionada

    private const string PISTOL_MAGAZINE_SIZE_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_TIME_KEY   = "PistolReloadTime";

    [Header("Animaciones en 8 direcciones (Pistola)")]
    public ShipBodyPistolIdle8Directions idleScript;
    public ShipBodyAttack8Directions attackScript;
    public float attackAnimationDuration = 0.4f;

    private bool isPlayingAttackAnim = false;
    private float nextFireTime = 0f;

    // --- NUEVO: Referencia al controlador del contorno ---
    private PlayerOutlineController outlineController;
    // --- FIN NUEVO ---

    void Start()
    {
        // --- Carga de configuración y UI (sin cambios) ---
        magazineSize = PlayerPrefs.GetInt(PISTOL_MAGAZINE_SIZE_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(PISTOL_RELOAD_TIME_KEY, defaultReloadTime);
        currentAmmo = magazineSize;
        UpdateAmmoText();
        cameraZoom = FindObjectOfType<CameraZoom>();
        Debug.Log("[PlayerShooting] Start => magSize=" + magazineSize + ", reloadTime=" + reloadTime);

        // --- NUEVO: Obtener referencia al controlador del contorno ---
        // Asume que PlayerOutlineController está en el mismo objeto que PlayerController (padre)
        outlineController = GetComponentInParent<PlayerOutlineController>();
        if (outlineController == null)
        {
            Debug.LogWarning("PlayerShooting: No se encontró PlayerOutlineController en los padres.");
        }
        // --- FIN NUEVO ---
    }

    void Update()
    {
        // Actualiza el color basado en la entrada del teclado cada frame
        UpdateCurrentColor();
    }

    // --- Lógica de Selección de Color (sin cambios) ---
    public void UpdateCurrentColor() { /* ... código existente ... */
        if (Input.GetKeyDown(KeyCode.W)) { SetCurrentColor(Color.yellow); lastPressedKey = KeyCode.W; }
        else if (Input.GetKeyDown(KeyCode.A)) { SetCurrentColor(Color.blue); lastPressedKey = KeyCode.A; }
        else if (Input.GetKeyDown(KeyCode.S)) { SetCurrentColor(Color.green); lastPressedKey = KeyCode.S; }
        else if (Input.GetKeyDown(KeyCode.D)) { SetCurrentColor(Color.red); lastPressedKey = KeyCode.D; }
        if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey)) { KeyCode currentlyPressedKey = GetCurrentlyPressedKey(); SetCurrentColorByKey(currentlyPressedKey); lastPressedKey = currentlyPressedKey; }
    }
    KeyCode GetCurrentlyPressedKey() { /* ... código existente ... */
        if (Input.GetKey(KeyCode.D)) return KeyCode.D; if (Input.GetKey(KeyCode.S)) return KeyCode.S; if (Input.GetKey(KeyCode.A)) return KeyCode.A; if (Input.GetKey(KeyCode.W)) return KeyCode.W; return KeyCode.None;
     }
    void SetCurrentColorByKey(KeyCode key) { /* ... código existente ... */
        switch (key) { case KeyCode.W: SetCurrentColor(Color.yellow); break; case KeyCode.A: SetCurrentColor(Color.blue); break; case KeyCode.S: SetCurrentColor(Color.green); break; case KeyCode.D: SetCurrentColor(Color.red); break; default: SetCurrentColor(Color.white); break; }
     }
    void SetCurrentColor(Color color) { /* ... código existente ... */
        currentColor = color; SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>(); if (spriteRenderer != null) { spriteRenderer.color = currentColor; }
     }
    // --- Fin Lógica de Selección de Color ---

    // --- Método Shoot (Modificado) ---
    public void Shoot()
    {
        // --- Comprobaciones antes de disparar (sin cambios) ---
        if (Time.time < nextFireTime) { return; }
        if (currentColor == Color.white) { return; }
        if (isReloading) { return; }
        if (currentAmmo <= 0) { if (!isReloading) StartCoroutine(Reload()); return; }

        // --- Disparo Exitoso ---
        nextFireTime = Time.time + fireRate; // Actualizar cooldown
        currentAmmo--;
        UpdateAmmoText();

        // Calcular rotación con dispersión
        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn) ? zoomedDispersionAngle : normalDispersionAngle;
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle);

        // Elegir prefab
        GameObject chosenPrefab = null;
        if (currentColor == Color.red) chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue) chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green) chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;
        if (chosenPrefab == null) { chosenPrefab = projectilePrefab; Debug.LogWarning("[PlayerShooting] Usando fallback prefab."); }

        // Instanciar y configurar proyectil
        if (chosenPrefab != null)
        {
            GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.linearVelocity = projectile.transform.up * projectileSpeed; }
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null) { proj.projectileColor = currentColor; }

            // --- Efectos ---
            StartCoroutine(PlayAttackAnimation());
            StartCoroutine(ScaleEffect());
            if (CameraShake.Instance != null) { Vector3 recoilDirection = -transform.up; CameraShake.Instance.RecoilCamera(recoilDirection); }

            // <<<--- NUEVA LLAMADA AL PULSO DEL CONTORNO ---<<<
            outlineController?.TriggerThicknessPulse();
            // <<<--- FIN NUEVA LLAMADA ---<<<
        }
        else { Debug.LogError("[PlayerShooting] ¡chosenPrefab sigue siendo null!"); }

        // Auto-recarga si se agota munición
        if (currentAmmo <= 0 && !isReloading) { StartCoroutine(Reload()); }
    }
    // --- Fin Método Shoot ---

    // --- Corutinas (PlayAttackAnimation, Reload, ScaleEffect) y UpdateAmmoText (sin cambios) ---
    IEnumerator PlayAttackAnimation() { /* ... código existente ... */
        if (isPlayingAttackAnim) yield break; isPlayingAttackAnim = true; if (idleScript != null) idleScript.enabled = false; if (attackScript != null) attackScript.enabled = true; yield return new WaitForSeconds(attackAnimationDuration); if (attackScript != null) attackScript.enabled = false; if (idleScript != null) idleScript.enabled = true; isPlayingAttackAnim = false;
     }
    public IEnumerator Reload() { /* ... código existente ... */
         if (isReloading) yield break; isReloading = true; UpdateAmmoText(); if (reloadIndicator != null) reloadIndicator.ResetIndicator(); float reloadTimer = 0f; while (reloadTimer < reloadTime) { reloadTimer += Time.deltaTime; if (reloadIndicator != null) reloadIndicator.UpdateIndicator(reloadTimer / reloadTime); yield return null; } currentAmmo = magazineSize; isReloading = false; UpdateAmmoText(); if (reloadIndicator != null) reloadIndicator.ResetIndicator();
     }
    public void UpdateAmmoText() { /* ... código existente ... */
         if (ammoText == null) return; if (isReloading) { ammoText.text = "Pistola: RELOADING"; } else { ammoText.text = $"Pistola: {currentAmmo}/{magazineSize}"; }
     }
    IEnumerator ScaleEffect() { /* ... código existente ... */
         Vector3 originalScale = transform.localScale; Vector3 targetScale = originalScale * scaleMultiplier; float elapsedTime = 0f; float halfDuration = scaleDuration / 2f; while (elapsedTime < halfDuration) { float t = elapsedTime / halfDuration; transform.localScale = Vector3.Lerp(originalScale, targetScale, t); elapsedTime += Time.deltaTime; yield return null; } transform.localScale = targetScale; elapsedTime = 0f; while (elapsedTime < halfDuration) { float t = elapsedTime / halfDuration; transform.localScale = Vector3.Lerp(targetScale, originalScale, t); elapsedTime += Time.deltaTime; yield return null; } transform.localScale = originalScale;
     }
    // --- Fin Corutinas y UI ---
}