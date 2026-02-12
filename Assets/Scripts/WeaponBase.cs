using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Clase base abstracta para todas las armas de Luminity.
/// Centraliza: sistema de colores (WASD), munición, recarga,
/// UI de ammo, efecto de escala y selección de prefab por color.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    /*═══════════════════  PREFABS POR COLOR  ═══════════════════*/

    [Header("Prefabs por Color")]
    [Tooltip("Prefab fallback si no hay prefab específico de color.")]
    public GameObject projectilePrefab;
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;

    /*═══════════════════  MUNICIÓN Y RECARGA  ═══════════════════*/

    [Header("Munición y Recarga")]
    public float fireRate = 0.1f;

    [HideInInspector] public int magazineSize;
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public float reloadTime;
    [HideInInspector] public bool isReloading = false;

    /*═══════════════════  EFECTOS  ═══════════════════*/

    [Header("Efecto de Escala al Disparar")]
    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.1f;

    /*═══════════════════  UI  ═══════════════════*/

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    /*═══════════════════  COLOR  ═══════════════════*/

    [HideInInspector] public Color currentColor = Color.white;

    /*═══════════════════  ESTADO INTERNO  ═══════════════════*/

    protected CameraZoom cameraZoom;
    protected float nextFireTime = 0f;
    private KeyCode lastPressedKey = KeyCode.None;

    /// <summary>Nombre del arma para mostrar en la UI (ej: "Pistola", "Escopeta").</summary>
    protected abstract string WeaponName { get; }

    /*═══════════════════  CICLO DE VIDA  ═══════════════════*/

    protected virtual void Start()
    {
        LoadUpgrades();
        currentAmmo = magazineSize;
        UpdateAmmoText();
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    /// <summary>
    /// Carga los valores de mejora desde PlayerPrefs.
    /// Cada arma define sus propias claves y defaults.
    /// </summary>
    protected abstract void LoadUpgrades();

    /*═══════════════════  SISTEMA DE COLORES (WASD)  ═══════════════════*/

    /// <summary>
    /// Sistema de selección de color con prioridad de tecla.
    /// Al soltar una tecla, revierte al color de la tecla aún presionada.
    /// Si no hay ninguna tecla presionada → blanco (no puede disparar).
    /// </summary>
    public virtual void UpdateCurrentColor()
    {
        if (Input.GetKeyDown(KeyCode.W)) { SetCurrentColor(Color.yellow); lastPressedKey = KeyCode.W; }
        else if (Input.GetKeyDown(KeyCode.A)) { SetCurrentColor(Color.blue); lastPressedKey = KeyCode.A; }
        else if (Input.GetKeyDown(KeyCode.S)) { SetCurrentColor(Color.green); lastPressedKey = KeyCode.S; }
        else if (Input.GetKeyDown(KeyCode.D)) { SetCurrentColor(Color.red); lastPressedKey = KeyCode.D; }

        if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey))
        {
            KeyCode stillPressed = GetCurrentlyPressedColorKey();
            SetCurrentColorByKey(stillPressed);
            lastPressedKey = stillPressed;
        }
    }

    KeyCode GetCurrentlyPressedColorKey()
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
            case KeyCode.A: SetCurrentColor(Color.blue); break;
            case KeyCode.S: SetCurrentColor(Color.green); break;
            case KeyCode.D: SetCurrentColor(Color.red); break;
            default:        SetCurrentColor(Color.white); break;
        }
    }

    protected virtual void SetCurrentColor(Color color)
    {
        currentColor = color;
        if (TryGetComponent(out SpriteRenderer sr))
            sr.color = currentColor;
    }

    /*═══════════════════  RECARGA  ═══════════════════*/

    /// <summary>
    /// Coroutine de recarga compartida. Actualiza UI y reload indicator.
    /// Override si necesitas lógica extra (ej: Rifle detiene el fuego antes).
    /// </summary>
    public virtual IEnumerator Reload()
    {
        if (isReloading || currentAmmo == magazineSize) yield break;

        isReloading = true;
        UpdateAmmoText();
        if (reloadIndicator != null) reloadIndicator.ResetIndicator();

        float timer = 0f;
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            if (reloadIndicator != null) reloadIndicator.UpdateIndicator(timer / reloadTime);
            yield return null;
        }

        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoText();
        if (reloadIndicator != null) reloadIndicator.ResetIndicator();
    }

    /*═══════════════════  UI DE MUNICIÓN  ═══════════════════*/

    public virtual void UpdateAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isReloading
            ? $"{WeaponName}: RELOADING"
            : $"{WeaponName}: {currentAmmo}/{magazineSize}";
    }

    /*═══════════════════  SELECCIÓN DE PREFAB POR COLOR  ═══════════════════*/

    /// <summary>
    /// Retorna el prefab correspondiente al color actual.
    /// Usa el prefab fallback si no hay uno específico.
    /// </summary>
    protected GameObject GetColorPrefab()
    {
        GameObject chosen = currentColor == Color.red    ? projectileRedPrefab
                          : currentColor == Color.blue   ? projectileBluePrefab
                          : currentColor == Color.green  ? projectileGreenPrefab
                          : currentColor == Color.yellow ? projectileYellowPrefab
                          : null;

        return chosen != null ? chosen : projectilePrefab;
    }

    /*═══════════════════  EFECTO DE ESCALA  ═══════════════════*/

    /// <summary>
    /// Efecto de "pulso" de escala al disparar.
    /// Crece y vuelve al tamaño original suavemente.
    /// </summary>
    protected IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsed = 0f;
        float half = scaleDuration / 2f;

        // Crecer
        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        // Volver
        elapsed = 0f;
        while (elapsed < half)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    /*═══════════════════  HELPERS  ═══════════════════*/

    /// <summary>Verifica si se puede disparar (color válido, munición, no recargando).</summary>
    protected bool CanShoot()
    {
        return currentColor != Color.white && !isReloading && currentAmmo > 0;
    }

    /// <summary>Consume 1 bala y actualiza UI. Inicia recarga automática si se acabó.</summary>
    protected void ConsumeAmmo()
    {
        currentAmmo--;
        UpdateAmmoText();
        if (currentAmmo <= 0 && !isReloading) StartCoroutine(Reload());
    }

    /// <summary>Instancia un proyectil con color, rotación y velocidad.</summary>
    protected GameObject SpawnProjectile(Quaternion rotation, float speed)
    {
        GameObject prefab = GetColorPrefab();
        if (prefab == null) return null;

        GameObject proj = Instantiate(prefab, transform.position, rotation);

        if (proj.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = proj.transform.up * speed;

        if (proj.TryGetComponent(out Projectile p))
            p.projectileColor = currentColor;

        return proj;
    }
}
