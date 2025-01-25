using UnityEngine;
using System.Collections;
using TMPro;

public class RifleShooting : MonoBehaviour
{
    [Header("Configuración del Rifle Automático")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;        // Velocidad de los proyectiles
    public float fireRate = 0.1f;             // Disparos por segundo (0.1s = 10 disparos/seg)
    public float reloadTime = 2f;             // Tiempo de recarga
    public int magazineSize = 30;             // Balas por cargador

    [Header("Dispersion / Zoom")]
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 2f;

    // Variables internas
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;

    private CameraZoom cameraZoom;
    private bool isFiring = false;     // Si el jugador mantiene presionado "disparo"
    private bool canShoot = true;      // Control del fireRate
    private float nextFireTime = 0f;   // Para controlar la cadencia de disparo

    [Header("Efectos")]
    public float scaleMultiplier = 1.05f;
    public float scaleDuration = 0.05f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;   // Texto para munición

    // ----------------- Sistema de color -----------------
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None; // Para la lógica WASD

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();

        // Obtener referencia al Zoom (opcional)
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void Update()
    {
        // 1) Actualizar color vía WASD (u otras teclas) si quieres
        UpdateCurrentColor();

        // 2) Verificar si estamos en modo “firing” (disparo continuo)
        if (isFiring)
        {
            DisparoContinuo();
        }

        UpdateAmmoText();
    }

    /// <summary>
    /// Dispara continuamente mientras 'isFiring' sea true y se cumpla el fireRate.
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

        // Asignar color
        SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projSpriteRenderer != null)
        {
            projSpriteRenderer.color = currentColor;
        }

        // Efecto de escala
        StartCoroutine(ScaleEffect());

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
    /// Inicia la recarga.
    /// </summary>
    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize) yield break;

        Debug.Log("RifleShooting - Iniciando recarga.");

        // Forzar dejar de disparar
        StopFiring();

        isReloading = true;
        UpdateAmmoText();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoText();

        // Si deseas que retome el disparo continuo automáticamente
        // si el jugador aún mantiene presionado el botón, 
        // puedes revisar (por ejemplo) Input.GetMouseButton(0):
        if (Input.GetMouseButton(0))
        {
            Debug.Log("¡¡El mouse sigue presionado!! -> StartFiring()");
            StartFiring();
        }
        else
        {
            Debug.Log("No está presionado. Se queda sin disparar.");
        }
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
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;

        float elapsedTime = 0f;
        float halfDuration = scaleDuration / 2f;

        // Subir escala
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        elapsedTime = 0f;

        // Bajar escala
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    // -----------------------------------------------------------
    // LÓGICA DE COLOR (WASD). Sin usar InputSystem/Gamepad.
    // -----------------------------------------------------------
    public void UpdateCurrentColor()
    {
        // TECLADO (WASD)
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

        // Si se suelta la última tecla
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

    // -----------------------------------------------------------
    // MÉTODOS PÚBLICOS PARA DISPARO CONTINUO (mouse).
    // -----------------------------------------------------------
    /// <summary>
    /// Llamar cuando se presione el botón (MouseButtonDown).
    /// </summary>
    public void StartFiring()
    {
        if (isReloading)
        {
            Debug.Log("StartFiring() llamado, pero estamos recargando. Ignoramos.");
            return;
        }

        Debug.Log("StartFiring() => Comenzamos disparo automático.");
        isFiring = true;
        nextFireTime = Time.time; // El primer disparo puede ocurrir inmediatamente
    }

    /// <summary>
    /// Llamar cuando se suelte el botón (MouseButtonUp).
    /// </summary>
    public void StopFiring()
    {
        Debug.Log("StopFiring() => Se detiene el disparo automático.");
        isFiring = false;
    }
}