using UnityEngine;
using System.Collections;
using TMPro;

public class RifleShooting : MonoBehaviour
{
    [Header("Configuración del Rifle Automático")]
    [Tooltip("Prefab de proyectil fallback (blanco) si no coincide color.")]
    public GameObject projectilePrefab;  

    [Header("Prefabs de proyectil para cada color (Rifle)")]
    public GameObject projectileRedPrefab;
    public GameObject projectileBluePrefab;
    public GameObject projectileGreenPrefab;
    public GameObject projectileYellowPrefab;

    public float projectileSpeed = 20f;
    public float fireRate = 0.1f;   // 0.1 => 10 disparos/seg
    public float reloadTime = 2f;
    public int magazineSize = 30;   

    [Header("Dispersion / Zoom")]
    public float normalDispersionAngle = 5f;
    public float zoomedDispersionAngle = 2f;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;

    private CameraZoom cameraZoom;
    private bool isFiring = false;   
    private bool canShoot = true;    
    private float nextFireTime = 0f; 

    [Header("Efectos")]
    public float scaleMultiplier = 1.05f;
    public float scaleDuration = 0.05f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;   
    public WeaponReloadIndicator reloadIndicator;

    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None;

    private Coroutine scaleEffectCoroutine;

    [Header("Animaciones en 8 direcciones (Rifle)")]
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyRifleAttack8Directions rifleAttackScript;
    private bool rifleAttackActive = false;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();

        cameraZoom = FindObjectOfType<CameraZoom>();

        Debug.Log("[RifleShooting] Start => magazineSize="+magazineSize+", reloadTime="+reloadTime);
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

    private void DisparoContinuo()
    {
        if (Time.time >= nextFireTime && canShoot)
        {
            ShootOneBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootOneBullet()
    {
        // Verificación de color, recarga, munición
        if (currentColor == Color.white || isReloading || currentAmmo <= 0) return;

        currentAmmo--;

        // Dispersión (normal vs zoomed)
        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn) 
                                ? zoomedDispersionAngle 
                                : normalDispersionAngle;
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f);

        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle);

        // Seleccionar prefab según color
        GameObject chosenPrefab = null;
        if      (currentColor == Color.red)    chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue)   chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green)  chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;

        if (chosenPrefab == null)
        {
            Debug.LogWarning("[RifleShooting] chosenPrefab es null => usando fallback (projectilePrefab).");
            chosenPrefab = projectilePrefab;
        }

        // Instanciar proyectil
        GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);

        // Asignar velocidad
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = projectile.transform.up * projectileSpeed;
        }

        // Asignar color lógico en Projectile
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.projectileColor = currentColor;
        }

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

    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize) yield break;

        Debug.Log("[RifleShooting] => Iniciando recarga.");

        StopFiring(); // Forzar detener disparo continuo

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

    private void UpdateAmmoText()
    {
        if (ammoText == null) return;

        if (isReloading)
            ammoText.text = "Rifle: RELOADING";
        else
            ammoText.text = $"Rifle: {currentAmmo}/{magazineSize}";
    }

    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = Vector3.one;
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

    // NUEVO => Si color es blanco, no iniciamos Attack
    public void StartFiring()
    {
        if (isReloading)
        {
            Debug.Log("[RifleShooting] StartFiring => recargando, ignoramos.");
            return;
        }
        // Chequeamos color
        if (currentColor == Color.white)
        {
            Debug.Log("[RifleShooting] StartFiring => color=WHITE => no activa Attack ni disparo.");
            return;
        }

        Debug.Log("[RifleShooting] StartFiring => Disparo automático rifle ON");
        isFiring = true;
        nextFireTime = Time.time;

        if (!rifleAttackActive)
        {
            rifleAttackActive = true;
            if (rifleIdleScript != null)  rifleIdleScript.enabled = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = true;
        }
    }

    public void StopFiring()
    {
        Debug.Log("[RifleShooting] StopFiring => Disparo automático rifle OFF");
        isFiring = false;

        if (rifleAttackActive)
        {
            rifleAttackActive = false;
            if (rifleAttackScript != null) rifleAttackScript.enabled = false;
            if (rifleIdleScript != null)   rifleIdleScript.enabled = true;
        }
    }
}
