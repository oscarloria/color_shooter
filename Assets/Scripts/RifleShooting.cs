using UnityEngine;
using System.Collections;
using TMPro;

public class RifleShooting : MonoBehaviour
{
    [Header("Configuración del Rifle Automático")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;        // Velocidad de los proyectiles
    public float fireRate = 0.1f;             // Tiempo entre disparos (0.1s = 10 disparos/seg)
    public float reloadTime = 2f;             // Tiempo de recarga
    public int magazineSize = 30;             // Balas por cargador

    [Header("Dispersion / Zoom")]
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 2f;

    // Variables internas
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;

    private CameraZoom cameraZoom;
    private bool isFiring = false;     // Disparo continuo
    private bool canShoot = true;      // Control del fireRate
    private float nextFireTime = 0f;   // Para cadencia de disparo

    [Header("Efectos")]
    public float scaleMultiplier = 1.05f;
    public float scaleDuration = 0.05f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;   // Texto para munición
    public WeaponReloadIndicator reloadIndicator; // Indicador radial de recarga

    // ----------------- Sistema de color -----------------
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None; // Para lógica WASD

    // Para asegurar que no se superpongan múltiples efectos de escala
    private Coroutine scaleEffectCoroutine;

    // NUEVO: Referencias para Idle y Attack en 8 direcciones (rifle)
    [Header("Animaciones en 8 direcciones (Rifle)")]
    // CORREGIDO: Referencia al script idle ESPECÍFICO para rifle
    public ShipBodyRifleIdle8Directions rifleIdleScript;          
    public ShipBodyRifleAttack8Directions rifleAttackScript;      
    private bool rifleAttackActive = false;                       // indica si Attack está ON

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();

        // Obtener referencia al Zoom
        cameraZoom = FindObjectOfType<CameraZoom>();

        // Idle ON, Attack OFF al inicio
      //  if (rifleIdleScript != null)  rifleIdleScript.enabled = true;
       // if (rifleAttackScript != null) rifleAttackScript.enabled = false;
    }

    void Update()
    {
        // Actualizar color vía WASD
        UpdateCurrentColor();

        // Si estamos en disparo continuo, lo gestionamos
        if (isFiring)
        {
            DisparoContinuo();
        }

        UpdateAmmoText();
    }

    /// <summary>
    /// Dispara continuamente mientras isFiring sea true y se cumpla el fireRate.
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
    /// Dispara una sola bala, considerando dispersión y color.
    /// </summary>
    private void ShootOneBullet()
    {
        if (currentColor == Color.white || isReloading || currentAmmo <= 0) return;

        currentAmmo--;

        // Dispersión según zoom
        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedDispersionAngle
            : normalDispersionAngle;

        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);

        // Instanciar proyectil
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle);
        GameObject projectile = Instantiate(projectilePrefab, transform.position, projectileRotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = projectile.transform.up * projectileSpeed;

        // Asignar color al proyectil
        SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projSpriteRenderer != null)
        {
            projSpriteRenderer.color = currentColor;
        }

        // Efecto de escala: detener el efecto en curso antes de iniciar uno nuevo.
        if (scaleEffectCoroutine != null)
        {
            StopCoroutine(scaleEffectCoroutine);
            transform.localScale = Vector3.one; // Reinicia a escala normal.
        }
        scaleEffectCoroutine = StartCoroutine(ScaleEffect());

        // Retroceso de cámara
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up;
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }

        // Si se acaba la munición, iniciar recarga
        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    /// <summary>
    /// Corrutina para recargar el rifle, actualizando el indicador radial.
    /// </summary>
    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize) yield break;

        Debug.Log("RifleShooting - Iniciando recarga.");

        // Forzar detener el disparo continuo
        StopFiring();

        isReloading = true;
        UpdateAmmoText();

        // Reiniciar el indicador de recarga
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

        // Reiniciar el indicador al finalizar
        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator();
    }

    /// <summary>
    /// Actualiza el texto de la munición en la UI.
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
    /// Pequeño efecto de escala durante el disparo.
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

    // NUEVO: Manejo de disparo continuo => Attack vs Idle
    public void StartFiring()
    {
        if (isReloading)
        {
            Debug.Log("StartFiring() => estamos recargando, ignoramos.");
            return;
        }
        Debug.Log("StartFiring() => Disparo automático rifle ON");
        isFiring = true;
        nextFireTime = Time.time;

        // Activar anim de ataque, desactivar idle
        if (!rifleAttackActive)
        {
            rifleAttackActive = true;
            if (rifleIdleScript != null)  rifleIdleScript.enabled = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = true;
        }
    }

    public void StopFiring()
    {
        Debug.Log("StopFiring() => Disparo automático rifle OFF");
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
