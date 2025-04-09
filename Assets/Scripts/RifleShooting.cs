using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Maneja el disparo automático (rifle). Admite distintos prefabs de proyectil
/// según el color, y asigna "projectileColor" en Projectile.cs para la mecánica
/// de coincidencia con enemigos. Incluye animaciones Idle/Attack en 8 direcciones.
/// </summary>
public class RifleShooting : MonoBehaviour
{
    [Header("Configuración del Rifle Automático")]
    [Tooltip("Prefab de proyectil blanco (opcional, fallback).")]
    public GameObject projectilePrefab;  // Fallback si no coincide color

    [Header("Prefabs de proyectil para cada color (Rifle)")]
    // NUEVO: Asignar en el Inspector un prefab de color nativo (rojo, azul, etc.)
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;

    public float projectileSpeed = 20f;
    public float fireRate = 0.1f;      // 0.1 => 10 disparos/seg
    public float reloadTime = 2f;      // Tiempo de recarga
    public int magazineSize = 30;      // Balas por cargador

    [Header("Dispersion / Zoom")]
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 2f;

    // Internas
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;

    private CameraZoom cameraZoom;
    private bool isFiring = false;     // Disparo continuo (mouseDown)
    private bool canShoot = true;      // Control de fireRate
    private float nextFireTime = 0f;   // Para cadencia de disparo

    [Header("Efectos")]
    public float scaleMultiplier = 1.05f;
    public float scaleDuration = 0.05f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;   
    public WeaponReloadIndicator reloadIndicator;

    // ----------------- Sistema de color -----------------
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None; 

    // Para asegurar que no se superpongan múltiples efectos de escala
    private Coroutine scaleEffectCoroutine;

    [Header("Animaciones en 8 direcciones (Rifle)")]
    public ShipBodyRifleIdle8Directions rifleIdleScript;          
    public ShipBodyRifleAttack8Directions rifleAttackScript;      
    private bool rifleAttackActive = false;                       // si Attack está ON

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();

        // Obtener referencia al Zoom
        cameraZoom = FindObjectOfType<CameraZoom>();

        // Dejamos que el PlayerController maneje Idle/Attack
        // No forzamos nada aquí para no chocar con PlayerController
    }

    void Update()
    {
        // Actualizar color vía WASD
        UpdateCurrentColor();

        // Disparo continuo si isFiring
        if (isFiring) 
        {
            DisparoContinuo();
        }

        // Actualizar UI
        UpdateAmmoText();
    }

    /// <summary>
    /// Llamado repetidamente mientras isFiring==true (mouseButtonDown en PlayerController).
    /// Respeta el fireRate y llama ShootOneBullet().
    /// </summary>
    private void DisparoContinuo()
    {
        if (Time.time >= nextFireTime && canShoot)
        {
            ShootOneBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Dispara una sola bala, con dispersión y color. Se llama en DisparoContinuo().
    /// </summary>
    private void ShootOneBullet()
    {
        // Si color es blanco, recargando o sin munición => no dispara
        if (currentColor == Color.white || isReloading || currentAmmo <= 0) return;

        currentAmmo--;

        // Dispersión
        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedDispersionAngle
            : normalDispersionAngle;
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);

        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle);

        // ---------------------- NUEVO: Elegir prefab según color ----------------------
        GameObject chosenPrefab = null;
        if      (currentColor == Color.red)    chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue)   chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green)  chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;

        if (chosenPrefab == null)
        {
            // fallback
            chosenPrefab = projectilePrefab;
        }

        // Instanciar el proyectil
        GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);

        // Asignar velocidad
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = projectile.transform.up * projectileSpeed;
        }

        // *** Asignar color lógico en Projectile.cs para colisiones con enemigos
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.projectileColor = currentColor; 
        }
        // -----------------------------------------------------------------------------

        // Efecto de escala
        if (scaleEffectCoroutine != null)
        {
            StopCoroutine(scaleEffectCoroutine);
            transform.localScale = Vector3.one;
        }
        scaleEffectCoroutine = StartCoroutine(ScaleEffect());

        // Retroceso de cámara
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up;
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }

        // Si se acaba la munición, recargar
        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    /// <summary>
    /// Corrutina para recargar el rifle.
    /// </summary>
    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize) yield break;

        Debug.Log("[RifleShooting] Reload => Iniciando recarga.");

        // Forzar detener el disparo continuo
        StopFiring();

        isReloading = true;
        UpdateAmmoText();

        // Reiniciar el indicador radial
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
    /// Actualiza la UI de munición.
    /// </summary>
    private void UpdateAmmoText()
    {
        if (ammoText == null) return;

        if (isReloading)
        {
            ammoText.text = "Rifle: RELOADING";
        }
        else
        {
            ammoText.text = $"Rifle: {currentAmmo}/{magazineSize}";
        }
    }

    /// <summary>
    /// Efecto de escala al disparar.
    /// </summary>
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsedTime = 0f;
        float halfDuration = scaleDuration / 2f;

        // Escalar hacia arriba
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        elapsedTime = 0f;
        // Escalar hacia abajo
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        scaleEffectCoroutine = null;
    }

    // -----------------------------------------------------------
    // LÓGICA DE COLOR (WASD).
    // -----------------------------------------------------------
    public void UpdateCurrentColor()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetCurrentColor(Color.yellow);
            lastPressedKey = KeyCode.W;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetCurrentColor(Color.blue);
            lastPressedKey = KeyCode.A;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetCurrentColor(Color.green);
            lastPressedKey = KeyCode.S;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SetCurrentColor(Color.red);
            lastPressedKey = KeyCode.D;
        }

        if (Input.GetKeyUp(lastPressedKey))
        {
            KeyCode newKey = GetLastKeyPressed();
            SetCurrentColorByKey(newKey);
        }
    }

    bool AnyWASDPressed()
    {
        return (Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D));
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
            case KeyCode.W:
                SetCurrentColor(Color.yellow);
                break;
            case KeyCode.A:
                SetCurrentColor(Color.blue);
                break;
            case KeyCode.S:
                SetCurrentColor(Color.green);
                break;
            case KeyCode.D:
                SetCurrentColor(Color.red);
                break;
            default:
                SetCurrentColor(Color.white);
                break;
        }
    }

    void SetCurrentColor(Color color)
    {
        currentColor = color;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;
    }

    /// <summary>
    /// Inicia el disparo automático (mantener mouseDown).
    /// Activa anim de ataque y desactiva la idle (rifle).
    /// </summary>
    public void StartFiring()
    {
        if (isReloading)
        {
            Debug.Log("[RifleShooting] StartFiring => recargando, ignoramos.");
            return;
        }
        Debug.Log("[RifleShooting] StartFiring => Disparo automático rifle ON");
        isFiring = true;
        nextFireTime = Time.time;

        // Activar anim de ataque
        if (!rifleAttackActive)
        {
            rifleAttackActive = true;
            if (rifleIdleScript != null)  rifleIdleScript.enabled = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = true;
        }
    }

    /// <summary>
    /// Detiene el disparo automático (mouseUp).
    /// Reactiva la idle del rifle y apaga el script Attack.
    /// </summary>
    public void StopFiring()
    {
        Debug.Log("[RifleShooting] StopFiring => Disparo automático rifle OFF");
        isFiring = false;

        // Volver a Idle
        if (rifleAttackActive)
        {
            rifleAttackActive = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = false;
            if (rifleIdleScript != null)   rifleIdleScript.enabled = true;
        }
    }
}
