using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem; // Para usar Gamepad.current

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
    public WeaponReloadIndicator reloadIndicator;

    private CameraZoom cameraZoom;
    private KeyCode lastPressedKey = KeyCode.None;

    private const string PISTOL_MAGAZINE_SIZE_KEY = "PistolMagazineSize";
    private const string PISTOL_RELOAD_TIME_KEY   = "PistolReloadTime";

    [Header("Animaciones en 8 direcciones (Pistola)")]
    public ShipBodyPistolIdle8Directions idleScript;
    public ShipBodyAttack8Directions attackScript;
    public float attackAnimationDuration = 0.4f;

    private bool isPlayingAttackAnim = false;
    private float nextFireTime = 0f;

    void Start()
    {
        magazineSize = PlayerPrefs.GetInt(PISTOL_MAGAZINE_SIZE_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(PISTOL_RELOAD_TIME_KEY, defaultReloadTime);

        currentAmmo = magazineSize;
        UpdateAmmoText();

        cameraZoom = FindObjectOfType<CameraZoom>();
        Debug.Log("[PlayerShooting] Start => magSize=" + magazineSize + ", reloadTime=" + reloadTime);
    }

    void Update()
    {
        UpdateCurrentColor();
    }

    // ----------------------------------------------------------------------------
    // Actualizar color (teclado + gamepad)
    // ----------------------------------------------------------------------------
    public void UpdateCurrentColor()
    {
        // Teclado WASD
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

        // Gamepad
        Gamepad gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 leftStick = gp.leftStick.ReadValue();
            float threshold = 0.5f;

            if (Mathf.Abs(leftStick.x) < threshold && Mathf.Abs(leftStick.y) < threshold)
            {
                if (!AnyWASDPressed()) SetCurrentColor(Color.white);
            }
            else
            {
                if (leftStick.y > threshold)       SetCurrentColor(Color.yellow);
                else if (leftStick.y < -threshold)SetCurrentColor(Color.green);
                else if (leftStick.x > threshold) SetCurrentColor(Color.red);
                else if (leftStick.x < -threshold)SetCurrentColor(Color.blue);
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
            case KeyCode.W: SetCurrentColor(Color.yellow); break;
            case KeyCode.A: SetCurrentColor(Color.blue);   break;
            case KeyCode.S: SetCurrentColor(Color.green);  break;
            case KeyCode.D: SetCurrentColor(Color.red);    break;
            default:        SetCurrentColor(Color.white);  break;
        }
    }

    void SetCurrentColor(Color color)
    {
        currentColor = color;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = currentColor;
    }

    // ----------------------------------------------------------------------------
    // Disparo
    // ----------------------------------------------------------------------------
    public void Shoot()
    {
        Debug.Log("[PlayerShooting] Shoot() => Intentando disparar. currentColor=" + currentColor);



        if (Time.time < nextFireTime)
        {
            Debug.LogWarning("[PlayerShooting] FireRate => Bloqueado, nextFireTime=" + nextFireTime + ", currentTime=" + Time.time);
            return;
        }
        nextFireTime = Time.time + fireRate;

        if (currentColor == Color.white)
        {
            Debug.LogWarning("[PlayerShooting] currentColor es WHITE => no se dispara.");
            return;
        }
        if (isReloading)
        {
            Debug.LogWarning("[PlayerShooting] isReloading => no se dispara.");
            return;
        }
        if (currentAmmo <= 0)
        {
            Debug.LogWarning("[PlayerShooting] Sin munición => no se dispara.");
            return;
        }

        currentAmmo--;
        UpdateAmmoText();

        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn) ? zoomedDispersionAngle : normalDispersionAngle;
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle);

        // Elegir prefab
        GameObject chosenPrefab = null;
        if      (currentColor == Color.red)    chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue)   chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green)  chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;

        if (chosenPrefab == null)
        {
            Debug.LogWarning("[PlayerShooting] chosenPrefab es null => usando fallback (projectilePrefab).");
            chosenPrefab = projectilePrefab;
        }

        Debug.Log("[PlayerShooting] Disparo => Instanciando '" + chosenPrefab.name + "' con color=" + currentColor);

        // Instanciar proyectil
        GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);

        // IMPORTANTE: Asignar velocidad al proyectil
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = projectile.transform.up * projectileSpeed;
            Debug.Log("[PlayerShooting] => Velocidad asignada: " + rb.linearVelocity);
        }
        else
        {
            Debug.LogWarning("[PlayerShooting] => El prefab no tiene Rigidbody2D, no se mueve.");
        }


Projectile proj = projectile.GetComponent<Projectile>();
if (proj != null)
{
    proj.projectileColor = currentColor;  // Aseguras que la lógica de color coincida
    // (Opcional) proj.spriteRenderer.color = currentColor;
}



        // Anim ataque
        StartCoroutine(PlayAttackAnimation());

        // Efecto de escala
        StartCoroutine(ScaleEffect());

        // Retroceso de cámara
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up;
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }
    }

    IEnumerator PlayAttackAnimation()
    {
        if (isPlayingAttackAnim) yield break;

        isPlayingAttackAnim = true;
        Debug.Log("[PlayerShooting] Activando anim de ataque (Pistol).");

        if (idleScript != null)
        {
            idleScript.enabled = false;
            Debug.Log("[PlayerShooting] Idle Pistola DESACTIVADA.");
        }
        if (attackScript != null)
        {
            attackScript.enabled = true;
            Debug.Log("[PlayerShooting] Attack Pistola ACTIVADA.");
        }

        yield return new WaitForSeconds(attackAnimationDuration);

        if (attackScript != null)
        {
            attackScript.enabled = false;
            Debug.Log("[PlayerShooting] Attack Pistola DESACTIVADA.");
        }
        if (idleScript != null)
        {
            idleScript.enabled = true;
            Debug.Log("[PlayerShooting] Idle Pistola REACTIVADA.");
        }

        isPlayingAttackAnim = false;
    }

    // ----------------------------------------------------------------------------
    // Recarga
    // ----------------------------------------------------------------------------
    public IEnumerator Reload()
    {
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

    // ----------------------------------------------------------------------------
    // UI 
    // ----------------------------------------------------------------------------
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

    // ----------------------------------------------------------------------------
    // Efecto de escala al disparar
    // ----------------------------------------------------------------------------
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;

        float elapsedTime = 0f;
        float halfDuration = scaleDuration / 2f;

        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        elapsedTime = 0f;
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
