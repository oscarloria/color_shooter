using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem; // Necesario para usar Gamepad.current

/// <summary>
/// Maneja el disparo y la recarga de la escopeta (spread shot).
/// Admite distintos prefabs de proyectil según el color,
/// asigna 'projectileColor' en Projectile.cs para que la comparación con enemyColor funcione.
/// </summary>
public class ShotgunShooting : MonoBehaviour
{
    [Header("Prefab de proyectil fallback (blanco)")]
    public GameObject projectilePrefab;         

    [Header("Prefabs de proyectil para cada color (Shotgun)")]
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;

    public float projectileSpeed = 20f;       
    public float fireRate = 0.5f;            

    public float normalSpreadAngle = 80f;    
    public float zoomedSpreadAngle = 50f;    
    public int pelletsPerShot = 5;           

    public int magazineSize = 8;             
    public float reloadTime = 60f;           
    [HideInInspector] public bool isReloading = false;

    [Header("Efectos")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.15f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;          
    public WeaponReloadIndicator reloadIndicator; 

    [HideInInspector] public int currentAmmo;
    private CameraZoom cameraZoom;            
    private bool canShoot = true;            

    // ----------------- Sistema de color -----------------
    public Color currentColor = Color.white; 
    private KeyCode lastPressedKey = KeyCode.None;

    [Header("Animaciones en 8 direcciones (Shotgun)")]
    public ShipBodyShotgunIdle8Directions shotgunIdleScript;     
    public ShipBodyShotgunAttack8Directions shotgunAttackScript; 
    public float shotgunAttackAnimationDuration = 0.5f;          
    private bool isPlayingShotgunAttackAnim = false;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();

        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void Update()
    {
        UpdateAmmoText();
        UpdateCurrentColor();
    }

    /// <summary>
    /// Dispara un spread shot (pelletsPerShot proyectiles) 
    /// con distintos prefabs (rojo, azul, verde, amarillo) según currentColor.
    /// </summary>
    public void Shoot()
    {
        // No dispara si color es blanco, recargando, sin munición o cooldown
        if (currentColor == Color.white || isReloading || currentAmmo <= 0 || !canShoot) return;

        currentAmmo--;
        UpdateAmmoText();

        // Ángulo total (zoom vs normal)
        float totalSpread = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedSpreadAngle
            : normalSpreadAngle;

        float angleStep = (pelletsPerShot > 1)
            ? totalSpread / (pelletsPerShot - 1)
            : 0f;
        float startAngle = -totalSpread * 0.5f;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Quaternion baseRotation = transform.rotation;
            Quaternion pelletRotation = baseRotation * Quaternion.Euler(0, 0, currentAngle);

            // Seleccionar el prefab según el color
            GameObject chosenPrefab = null;
            if      (currentColor == Color.red)    chosenPrefab = projectileRedPrefab;
            else if (currentColor == Color.blue)   chosenPrefab = projectileBluePrefab;
            else if (currentColor == Color.green)  chosenPrefab = projectileGreenPrefab;
            else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;

            if (chosenPrefab == null)
            {
                // Fallback si no hay prefab
                chosenPrefab = projectilePrefab;
            }

            // Instanciar
            GameObject projectile = Instantiate(chosenPrefab, transform.position, pelletRotation);

            // Asignar velocidad
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = projectile.transform.up * projectileSpeed;
            }

            // MUY IMPORTANTE => Asignar projectileColor en Projectile.cs
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.projectileColor = currentColor;  // <---- CLAVE
            }
        }

        // Efecto de escala
        StartCoroutine(ScaleEffect());

        // Retroceso de cámara
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up; 
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }

        // Control de fireRate
        StartCoroutine(FireRateCooldown());

        // Animación de ataque
        StartCoroutine(PlayShotgunAttackAnimation());

        // Si se quedó sin munición, recargar
        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator FireRateCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

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

    void UpdateAmmoText()
    {
        if (ammoText == null) return;
        if (isReloading)
            ammoText.text = "Escopeta: RELOADING";
        else
            ammoText.text = $"Escopeta: {currentAmmo}/{magazineSize}";
    }

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

    // ------------------------------------------------------------------
    // Actualizar color (igual a PlayerShooting)
    // ------------------------------------------------------------------
    public void UpdateCurrentColor()
    {
        // Teclado
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
                if (!AnyWASDPressed())
                {
                    SetCurrentColor(Color.white);
                }
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
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;
    }

    IEnumerator PlayShotgunAttackAnimation()
    {
        if (isPlayingShotgunAttackAnim) yield break;

        isPlayingShotgunAttackAnim = true;
        Debug.Log("[ShotgunShooting] Attack anim => Activada.");

        if (shotgunIdleScript != null) shotgunIdleScript.enabled = false;
        if (shotgunAttackScript != null) shotgunAttackScript.enabled = true;

        yield return new WaitForSeconds(shotgunAttackAnimationDuration);

        if (shotgunAttackScript != null) shotgunAttackScript.enabled = false;
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = true;

        isPlayingShotgunAttackAnim = false;
        Debug.Log("[ShotgunShooting] Attack anim => Finalizada.");
    }
}
