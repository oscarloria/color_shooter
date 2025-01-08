using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem; // Para usar Gamepad.current

public class PlayerShooting : MonoBehaviour
{
    // Variables de disparo
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float fireRate = 0.01f;

    // Variables de dispersión
    public float normalDispersionAngle = 5f; // Ángulo de dispersión en modo normal
    public float zoomedDispersionAngle = 0f; // Ángulo de dispersión en modo Zoom

    // Referencia al estado de Zoom
    private CameraZoom cameraZoom;

    // Variables de munición
    public int magazineSize = 6;
    public float reloadTime = 1f;
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;

    // Variables de efecto de escala
    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.1f;

    // Variables de color
    public Color currentColor = Color.white;

    // UI
    public TextMeshProUGUI ammoText;

    // Pilas para gestionar el orden de las teclas presionadas
    private KeyCode lastPressedKey = KeyCode.None;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();
        cameraZoom = FindObjectOfType<CameraZoom>(); // Obtener referencia al script de Zoom
    }

    void Update()
    {
        // Actualiza el color en función de la última tecla presionada o soltada
        UpdateCurrentColor();
    }

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

        // Si se suelta la última tecla, retrocede al color previo según el estado de la tecla
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

            // Comprobamos primero si el stick está prácticamente en neutro:
            if (Mathf.Abs(leftStick.x) < threshold && Mathf.Abs(leftStick.y) < threshold)
            {
                // => color blanco (no se puede disparar un color)
                // Solo si NO se está pulsando ninguna WASD
                // (Para que el teclado no se sobreescriba si el usuario está usando W,A,S,D)
                if (!AnyWASDPressed())
                {
                    SetCurrentColor(Color.white);
                }
            }
            else
            {
                // Si no está en neutro, asignamos color según dirección
                if (leftStick.y > threshold)
                {
                    // Arriba => Amarillo
                    SetCurrentColor(Color.yellow);
                }
                else if (leftStick.y < -threshold)
                {
                    // Abajo => Verde
                    SetCurrentColor(Color.green);
                }
                else if (leftStick.x > threshold)
                {
                    // Derecha => Rojo
                    SetCurrentColor(Color.red);
                }
                else if (leftStick.x < -threshold)
                {
                    // Izquierda => Azul
                    SetCurrentColor(Color.blue);
                }
            }
        }
    }

    // Comprueba si se mantiene pulsada alguna de las teclas W, A, S, D
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

    public void Shoot()
    {
        if (currentColor == Color.white || isReloading || currentAmmo <= 0) return;

        // Reducir la munición y actualizar la UI
        currentAmmo--;
        UpdateAmmoText();

        // Calcular el ángulo de dispersión según el estado de Zoom
        float dispersionAngle = normalDispersionAngle;
        if (cameraZoom != null && cameraZoom.IsZoomedIn)
        {
            dispersionAngle = zoomedDispersionAngle;
        }

        // Calcular un ángulo aleatorio dentro del rango de dispersión
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);

        // Crear el proyectil con dispersión
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

        StartCoroutine(ScaleEffect());

        // Llamar al efecto de retroceso de cámara
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up; // Dirección opuesta a la dirección de disparo
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        UpdateAmmoText();
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoText();
    }

    void UpdateAmmoText()
    {
        if (isReloading)
        {
            ammoText.text = "RELOADING";
        }
        else
        {
            ammoText.text = currentAmmo.ToString();
        }
    }

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