using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem; // Para usar Gamepad.current

public class PlayerShooting : MonoBehaviour
{
    // -------------------- Variables de disparo --------------------
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float fireRate = 0.01f;

    // -------------------- Variables de dispersión --------------------
    public float normalDispersionAngle = 5f;    // Ángulo de dispersión en modo normal
    public float zoomedDispersionAngle = 0f;    // Ángulo de dispersión en modo Zoom

    // -------------------- Valores predeterminados (Inspector) --------------------
    [Header("Default Values (if no PlayerPrefs)")]
    [SerializeField] private int defaultMagazineSize = 4;    // Inicia con 4 balas
    [SerializeField] private float defaultReloadTime = 6f;   // Inicia con 6.0s de recarga

    // -------------------- Variables de munición y recarga --------------------
    [HideInInspector] public int magazineSize;    // Se leerá desde PlayerPrefs o defaultMagazineSize
    [HideInInspector] public bool isReloading = false;
    public float reloadTime;                      // Se leerá desde PlayerPrefs o defaultReloadTime
    [HideInInspector] public int currentAmmo;

    // -------------------- Variables de efecto de escala --------------------
    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.1f;

    // -------------------- Variables de color --------------------
    public Color currentColor = Color.white;

    // -------------------- UI --------------------
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;  // Indicador radial de recarga

    // -------------------- Referencia al estado de Zoom --------------------
    private CameraZoom cameraZoom;

    // Pilas para gestionar el orden de las teclas presionadas
    private KeyCode lastPressedKey = KeyCode.None;

    // Claves PlayerPrefs
    private const string PISTOL_MAGAZINE_SIZE_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_TIME_KEY   = "PistolReloadTime";

    void Start()
    {
        // Leer tamaño de cargador desde PlayerPrefs (o usar 4 si no existe)
        magazineSize = PlayerPrefs.GetInt(PISTOL_MAGAZINE_SIZE_KEY, defaultMagazineSize);

        // Leer tiempo de recarga desde PlayerPrefs (o usar 6f si no existe)
        reloadTime = PlayerPrefs.GetFloat(PISTOL_RELOAD_TIME_KEY, defaultReloadTime);

        currentAmmo = magazineSize;
        UpdateAmmoText();

        cameraZoom = FindObjectOfType<CameraZoom>(); // Obtener referencia al script CameraZoom
    }

    void Update()
    {
        // Actualiza el color en función de las teclas o del gamepad
        UpdateCurrentColor();
    }

    // -------------------- Actualizar color (teclado + gamepad) --------------------
    public void UpdateCurrentColor()
    {
        // -------------------- TECLADO (WASD) --------------------
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

        // Al soltar la última tecla presionada, ver si hay otra activa
        if (Input.GetKeyUp(lastPressedKey))
        {
            KeyCode newKey = GetLastKeyPressed();
            SetCurrentColorByKey(newKey);
        }

        // -------------------- GAMEPAD (STICK IZQUIERDO) --------------------
        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 leftStick = gp.leftStick.ReadValue();
            float threshold = 0.5f;

            // Stick casi en neutro => color blanco si no hay WASD presionado
            if (Mathf.Abs(leftStick.x) < threshold && Mathf.Abs(leftStick.y) < threshold)
            {
                if (!AnyWASDPressed())
                {
                    SetCurrentColor(Color.white);
                }
            }
            else
            {
                // Asignar color según la dirección del stick
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

        // Actualizar el color del sprite del jugador
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = currentColor;
        }
    }

    // -------------------- Disparo --------------------
    public void Shoot()
    {
        // No dispara si no hay color, está recargando o no queda munición
        if (currentColor == Color.white || isReloading || currentAmmo <= 0) return;

        // Reducir la munición y actualizar la UI
        currentAmmo--;
        UpdateAmmoText();

        // Calcular el ángulo de dispersión según el estado de Zoom
        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedDispersionAngle
            : normalDispersionAngle;

        // Ángulo aleatorio dentro del rango
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);

        // Instanciar el proyectil con ese ángulo
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle);
        GameObject projectile = Instantiate(projectilePrefab, transform.position, projectileRotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = projectile.transform.up * projectileSpeed;

        // Asignar el color al proyectil
        SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projSpriteRenderer != null)
        {
            projSpriteRenderer.color = currentColor;
        }

        // Efecto de escala
        StartCoroutine(ScaleEffect());

        // Efecto de retroceso de cámara
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up; // opuesto a la dirección de disparo
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }
    }

    // -------------------- Recarga --------------------
    public IEnumerator Reload()
    {
        isReloading = true;
        UpdateAmmoText();

        // Reiniciar el indicador de recarga
        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator();

        float reloadTimer = 0f;
        // Actualizar el indicador mientras se recarga
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

    // -------------------- UI --------------------
    public void UpdateAmmoText()
    {
        if (ammoText == null) return;

        if (isReloading)
        {
            ammoText.text = "Pistola: RELOADING";
        }
        else
        {
            ammoText.text = $"Pistola: {currentAmmo}/{magazineSize}";
        }
    }

    // -------------------- Efecto de escala al disparar --------------------
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
}