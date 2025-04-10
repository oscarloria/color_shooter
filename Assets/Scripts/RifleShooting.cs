using UnityEngine;
using System.Collections;
using TMPro;

// Nota: No se necesita 'using UnityEngine.InputSystem;' ya que no había lógica de gamepad.

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
    private bool isFiring = false;  // Indica si el botón de disparo está presionado
    // private bool canShoot = true; // No es necesario si usamos nextFireTime
    private float nextFireTime = 0f; // Controla la cadencia de tiro

    [Header("Efectos")]
    public float scaleMultiplier = 1.05f;
    public float scaleDuration = 0.05f;
    private Coroutine scaleEffectCoroutine; // Para manejar el efecto de escala

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    // --- Sistema de color ---
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None; // Última tecla WASD presionada

    [Header("Animaciones en 8 direcciones (Rifle)")]
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyRifleAttack8Directions rifleAttackScript;
    private bool rifleAttackActive = false; // Controla estado de animación de ataque

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText(); // Actualizar UI al inicio

        cameraZoom = FindObjectOfType<CameraZoom>();

        Debug.Log("[RifleShooting] Start => magazineSize="+magazineSize+", reloadTime="+reloadTime);
    }

    void Update()
    {
        // Actualizar color vía WASD
        UpdateCurrentColor();

        // Disparo continuo si isFiring está activo
        if (isFiring)
        {
            TryContinuousShoot(); // Renombrado para claridad
        }

        // Actualizar UI (Considera si es necesario cada frame o solo al cambiar)
        // UpdateAmmoText(); // Comentado: Se actualiza en Start, ShootOneBullet y Reload
    }

    // Intenta disparar continuamente si se cumplen las condiciones
    private void TryContinuousShoot()
    {
        // Disparar si ha pasado el tiempo de fireRate y no estamos recargando
        if (Time.time >= nextFireTime && !isReloading)
        {
            ShootOneBullet(); // Intenta disparar una bala
            // Establecer el tiempo para el próximo disparo posible
            nextFireTime = Time.time + fireRate;
        }
    }

    // Lógica para disparar una única bala
    private void ShootOneBullet()
    {
        // --- Comprobaciones ---
        // 1. No disparar si el color es blanco
        if (currentColor == Color.white) return;
        // 2. No disparar si no hay munición (la recarga se inicia si es necesario)
        if (currentAmmo <= 0)
        {
            // Iniciar recarga si es necesario y no se está recargando ya
            if (!isReloading) StartCoroutine(Reload());
            return;
        }
        // Nota: La comprobación de isReloading se hace en TryContinuousShoot

        // --- Disparo ---
        currentAmmo--;    // Gastar munición
        UpdateAmmoText(); // Actualizar UI

        // Calcular dispersión (depende del zoom)
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

        // Usar fallback si no hay prefab específico
        if (chosenPrefab == null)
        {
            // Debug.LogWarning("[RifleShooting] chosenPrefab es null => usando fallback (projectilePrefab).");
            chosenPrefab = projectilePrefab;
        }

        // Instanciar si tenemos prefab
        if (chosenPrefab != null)
        {
            GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);

            // Asignar velocidad
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = projectile.transform.up * projectileSpeed; // Usar velocity
            }

            // Asignar color lógico en el script Projectile
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.projectileColor = currentColor;
            }

            // --- Efectos ---
            // Detener efecto de escala anterior si aún se ejecuta y resetear escala
            if (scaleEffectCoroutine != null)
            {
                StopCoroutine(scaleEffectCoroutine);
                transform.localScale = Vector3.one; // Resetear escala a 1
            }
            // Iniciar nuevo efecto de escala
            scaleEffectCoroutine = StartCoroutine(ScaleEffect());

            // Retroceso de cámara
            if (CameraShake.Instance != null)
            {
                Vector3 recoilDirection = -transform.up;
                CameraShake.Instance.RecoilCamera(recoilDirection);
            }
        }
        else {
             Debug.LogError("[RifleShooting] ¡chosenPrefab es null incluso después del fallback!");
        }


        // Si se acaba la munición DESPUÉS de disparar, iniciar recarga
        if (currentAmmo <= 0 && !isReloading)
        {
            // Debug.Log("[RifleShooting] Munición agotada tras disparo, iniciando recarga.");
            StartCoroutine(Reload());
        }
    }

    // Corutina para manejar la recarga
    public IEnumerator Reload()
    {
        // No recargar si ya se está recargando o si la munición está llena
        if (isReloading || currentAmmo == magazineSize) yield break;

        // Debug.Log("[RifleShooting] => Iniciando recarga.");

        StopFiring(); // Importante: Asegurarse de detener el disparo si se inicia la recarga

        isReloading = true; // Marcar como recargando
        UpdateAmmoText();   // Actualizar UI

        // Reiniciar indicador de recarga (si existe)
        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator();

        float reloadTimer = 0f; // Temporizador
        // Bucle durante el tiempo de recarga
        while (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime; // Incrementar temporizador
            // Actualizar indicador de progreso (si existe)
            if (reloadIndicator != null)
                reloadIndicator.UpdateIndicator(reloadTimer / reloadTime); // Valor de 0 a 1
            yield return null; // Esperar al siguiente frame
        }

        // Finalizar recarga
        currentAmmo = magazineSize; // Rellenar munición
        isReloading = false;        // Marcar como no recargando
        UpdateAmmoText();           // Actualizar UI

        // Resetear indicador (si existe)
        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator();

        // Debug.Log("[RifleShooting] Recarga completada.");
    }

    // Actualiza el texto de la munición en la UI
    private void UpdateAmmoText()
    {
        if (ammoText == null) return; // Salir si no hay referencia

        // Mostrar "RELOADING" o la cuenta actual
        if (isReloading)
            ammoText.text = "Rifle: RELOADING";
        else
            ammoText.text = $"Rifle: {currentAmmo}/{magazineSize}"; // Formato: Rifle: 25/30
    }

    // Corutina para el efecto visual de escala al disparar
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = Vector3.one; // Asumir escala original es 1,1,1
        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsedTime = 0f;
        float halfDuration = scaleDuration / 2f;

        // Escalar hacia arriba
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            // Usar deltaTime para respetar pausa
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale; // Asegurar escala máxima

        // Escalar hacia abajo
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale; // Asegurar escala original
        scaleEffectCoroutine = null; // Marcar corutina como terminada
    }

    // -----------------------------------------------------------
    // LÓGICA DE COLOR (Solo Teclado WASD)
    // -----------------------------------------------------------
    public void UpdateCurrentColor()
    {
        // --- Lógica del Teclado ---
        // Detecta si se PRESIONA una tecla WASD en este frame
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

        // Detecta si se SUELTA la última tecla que se había presionado
        if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey))
        {
            // Verifica si alguna OTRA tecla WASD sigue presionada
            KeyCode currentlyPressedKey = GetLastKeyPressed();
            SetCurrentColorByKey(currentlyPressedKey); // Establece el color correspondiente (o blanco)

            // CORREGIDO: Actualiza lastPressedKey a la tecla que quedó presionada, o None si ninguna
            lastPressedKey = currentlyPressedKey;
        }
        // --- Fin Lógica del Teclado ---

        // No hay lógica de Gamepad aquí en el script original
    }

    // Se eliminó la función AnyWASDPressed() porque no se utilizaba.
    /*
    bool AnyWASDPressed()
    {
        return (Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D));
    }
    */

    // Devuelve la tecla WASD que está actualmente presionada (con prioridad D > S > A > W)
    // o KeyCode.None si ninguna lo está.
    KeyCode GetLastKeyPressed()
    {
        if (Input.GetKey(KeyCode.D)) return KeyCode.D;
        if (Input.GetKey(KeyCode.S)) return KeyCode.S;
        if (Input.GetKey(KeyCode.A)) return KeyCode.A;
        if (Input.GetKey(KeyCode.W)) return KeyCode.W;
        return KeyCode.None;
    }

    // Establece el color actual basado en una KeyCode.
    void SetCurrentColorByKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: SetCurrentColor(Color.yellow); break;
            case KeyCode.A: SetCurrentColor(Color.blue);   break;
            case KeyCode.S: SetCurrentColor(Color.green);  break;
            case KeyCode.D: SetCurrentColor(Color.red);    break;
            default:        SetCurrentColor(Color.white);  break; // Si key es KeyCode.None
        }
    }

    // Establece el color actual y actualiza el color del SpriteRenderer si existe.
    void SetCurrentColor(Color color)
    {
        currentColor = color;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;
        // Si el color cambia, y estábamos disparando (isFiring=true) con color blanco,
        // ahora deberíamos poder empezar a disparar de verdad.
        // Y si cambiamos a blanco mientras disparamos, deberíamos parar.
        // Esto se maneja en ShootOneBullet() que chequea el color.
        // También se chequea en StartFiring().
    }

    // Inicia el modo de disparo automático y la animación de ataque
    public void StartFiring()
    {
        // No empezar a disparar si se está recargando
        if (isReloading)
        {
            // Debug.Log("[RifleShooting] StartFiring => recargando, ignoramos.");
            return;
        }
        // No empezar a disparar si el color es blanco
        if (currentColor == Color.white)
        {
            // Debug.Log("[RifleShooting] StartFiring => color=WHITE => no activa Attack ni disparo.");
            return;
        }

        // Solo activar si no estaba ya activo
        if (!isFiring)
        {
            // Debug.Log("[RifleShooting] StartFiring => Disparo automático rifle ON");
            isFiring = true;
            // Permitir el primer disparo inmediatamente si ha pasado el cooldown inicial
            // nextFireTime = Time.time; // Esto permite disparar inmediatamente al presionar

            // Activar animación de ataque si no estaba activa
            if (!rifleAttackActive)
            {
                rifleAttackActive = true;
                if (rifleIdleScript != null)  rifleIdleScript.enabled = false;
                if (rifleAttackScript != null) rifleAttackScript.enabled = true;
            }
        }
    }

    // Detiene el modo de disparo automático y la animación de ataque
    public void StopFiring()
    {
        // Solo desactivar si estaba activo
        if (isFiring)
        {
            // Debug.Log("[RifleShooting] StopFiring => Disparo automático rifle OFF");
            isFiring = false;

            // Desactivar animación de ataque si estaba activa
            if (rifleAttackActive)
            {
                rifleAttackActive = false;
                if (rifleAttackScript != null) rifleAttackScript.enabled = false;
                if (rifleIdleScript != null)  rifleIdleScript.enabled = true;
            }
        }
    }
}
