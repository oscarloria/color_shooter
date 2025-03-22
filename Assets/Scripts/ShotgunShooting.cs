using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem; // Necesario para usar Gamepad.current

/// <summary>
/// Maneja el disparo y la recarga de la escopeta (spread shot).
/// Se integra con el sistema de color y el zoom igual que la pistola.
///
/// Se ha modificado la lógica de dispersion de pellets para que
/// haya un tiro central (cuando pelletsPerShot es impar)
/// y el resto se distribuya simétricamente alrededor de ese ángulo.
/// </summary>
public class ShotgunShooting : MonoBehaviour
{
    // -------------- Variables de Configuración --------------
    [Header("Configuración de la Escopeta (Spread Shot)")]
    public GameObject projectilePrefab;       // Prefab del proyectil
    public float projectileSpeed = 20f;       // Velocidad de los proyectiles
    public float fireRate = 0.5f;             // Tiempo mínimo entre disparos

    public float normalSpreadAngle = 80f;     // Ángulo de dispersión sin zoom
    public float zoomedSpreadAngle = 50f;     // Ángulo de dispersión con zoom
    public int pelletsPerShot = 5;            // Número de proyectiles disparados simultáneamente

    public int magazineSize = 8;              // Cantidad de cartuchos
    public float reloadTime = 60f;            // Tiempo de recarga en segundos
    [HideInInspector] public bool isReloading = false; // Estado de recarga

    [Header("Efectos")]
    public float scaleMultiplier = 1.2f;      // Escala para feedback visual
    public float scaleDuration = 0.15f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;          // Texto para mostrar la munición en la UI (escopeta)
    public WeaponReloadIndicator reloadIndicator; // Indicador radial de recarga

    // -------------- Variables Internas --------------
    [HideInInspector] public int currentAmmo;  // Cartuchos actuales
    private CameraZoom cameraZoom;             // Referencia al Zoom (para saber si está activo)
    private bool canShoot = true;             // Control de fireRate

    // ----------------- Sistema de color -----------------
    public Color currentColor = Color.white;   // Color asignado a los proyectiles
    private KeyCode lastPressedKey = KeyCode.None;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();

        // Buscar el script de Zoom si está en la escena
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void Update()
    {
        // Actualizar la UI en todo momento (por si necesitamos refrescar)
        UpdateAmmoText();

        // Llamamos al método que revisa la lógica de color (teclado + gamepad)
        UpdateCurrentColor();
    }

    /// <summary>
    /// Dispara el spread shot (si hay cartuchos y no se está recargando).
    /// Ahora se distribuyen los pellets de forma simétrica.
    /// </summary>
    public void Shoot()
    {
        // No dispara si no hay color, está recargando o sin munición
        if (currentColor == Color.white || isReloading || currentAmmo <= 0 || !canShoot) return;

        // Disminuir munición
        currentAmmo--;
        UpdateAmmoText();

        // Determinar ángulo total de dispersión
        float totalSpread = (cameraZoom != null && cameraZoom.IsZoomedIn) 
            ? zoomedSpreadAngle 
            : normalSpreadAngle;

        // Calcular los ángulos equidistantes en [-totalSpread/2 .. +totalSpread/2]
        // Con pelletsPerShot = 5, se obtienen -40, -20, 0, +20, +40 (si totalSpread=80).
        // Si pelletsPerShot es par, no habrá un tiro exactamente en 0°, pero seguirán siendo simétricos.
        
        float angleStep = (pelletsPerShot > 1)
            ? totalSpread / (pelletsPerShot - 1)
            : 0f;

        float startAngle = -totalSpread * 0.5f; // ángulo inicial (izquierda)
        
        // Disparar "pelletsPerShot" proyectiles
        for (int i = 0; i < pelletsPerShot; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            
            // Rotación base de la escopeta
            Quaternion baseRotation = transform.rotation;
            // Aplicar el offset de "currentAngle" en Z
            Quaternion pelletRotation = baseRotation * Quaternion.Euler(0, 0, currentAngle);

            // Instanciar el proyectil
            GameObject projectile = Instantiate(projectilePrefab, transform.position, pelletRotation);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

            // Asignar velocidad
            rb.linearVelocity = projectile.transform.up * projectileSpeed;

            // Asignar color
            SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
            if (projSpriteRenderer != null)
            {
                projSpriteRenderer.color = currentColor;
            }
        }

        // Efecto visual (retroceso de cámara, escala, etc.)
        StartCoroutine(ScaleEffect());
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up; // opuesto a la dirección de disparo
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }

        // Control de fireRate
        StartCoroutine(FireRateCooldown());

        // Verificar recarga
        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    /// <summary>
    /// Corrutina para controlar el cooldown entre disparos (fireRate).
    /// </summary>
    IEnumerator FireRateCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    /// <summary>
    /// Corrutina para recargar la escopeta, actualizando el indicador radial.
    /// </summary>
    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize) yield break;

        isReloading = true;
        UpdateAmmoText();

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
    /// Actualiza el texto de la munición en la interfaz.
    /// </summary>
    void UpdateAmmoText()
    {
        if (ammoText == null) return;

        if (isReloading)
        {
            ammoText.text = "Escopeta: RELOADING";
        }
        else
        {
            ammoText.text = $"Escopeta: {currentAmmo}/{magazineSize}";
        }
    }

    /// <summary>
    /// Efecto visual de escala (similar a lo que tiene la pistola).
    /// </summary>
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
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
    }

    // ------------------------------------------------------------------
    // NUEVO: Lógica para la selección de color (similar a PlayerShooting)
    // ------------------------------------------------------------------
    public void UpdateCurrentColor()
    {
        // 1) TECLADO (WASD)
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

        // 2) GAMEPAD (STICK IZQUIERDO)
        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 leftStick = gp.leftStick.ReadValue();
            float threshold = 0.5f;

            // Stick en neutro => color blanco si no hay W,A,S,D pulsadas
            if (Mathf.Abs(leftStick.x) < threshold && Mathf.Abs(leftStick.y) < threshold)
            {
                if (!AnyWASDPressed())
                {
                    SetCurrentColor(Color.white);
                }
            }
            else
            {
                // Asignar color según la dirección
                if (leftStick.y > threshold)
                {
                    SetCurrentColor(Color.yellow);
                }
                else if (leftStick.y < -threshold)
                {
                    SetCurrentColor(Color.green);
                }
                else if (leftStick.x > threshold)
                {
                    SetCurrentColor(Color.red);
                }
                else if (leftStick.x < -threshold)
                {
                    SetCurrentColor(Color.blue);
                }
            }
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
        // (Opcional) cambiar color del sprite base, igual a PlayerShooting
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = currentColor;
        }
    }

    // Método público para asignar color desde fuera (opcional)
    public void SetShotgunColor(Color newColor)
    {
        currentColor = newColor;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = newColor;
        }
    }
}