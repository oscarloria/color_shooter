using UnityEngine;
using System.Collections;
using TMPro;
// Se eliminó: using UnityEngine.InputSystem; // Ya no es necesario para Gamepad.current

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

    [Header("Spread Shot")]
    public float normalSpreadAngle = 80f;
    public float zoomedSpreadAngle = 50f;
    public int pelletsPerShot = 5;

    [Header("Munición y Recarga")]
    public int magazineSize = 8;
    public float reloadTime = 10f; 
    [HideInInspector] public bool isReloading = false;
    [HideInInspector] public int currentAmmo;

    [Header("Efectos")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.15f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;

    private CameraZoom cameraZoom;
    private bool canShoot = true; // Para controlar fireRate

    // ----------------- Sistema de color -----------------
    public Color currentColor = Color.white;
    private KeyCode lastPressedKey = KeyCode.None; // Guarda la última tecla WASD presionada

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
        // Actualizar color y texto de munición cada frame
        // Nota: UpdateAmmoText() se llama también desde Shoot() y Reload(),
        // llamarlo aquí podría ser redundante si no cambia fuera de esas acciones.
        // Considera quitarlo de Update() si no es estrictamente necesario.
        // UpdateAmmoText(); // Comentado temporalmente para evaluación
        UpdateCurrentColor();
    }

    /// <summary>
    /// Dispara un spread shot (pelletsPerShot proyectiles)
    /// con distintos prefabs (rojo, azul, verde, amarillo) según currentColor.
    /// </summary>
    public void Shoot()
    {
        // --- Comprobaciones antes de disparar ---
        // 1. No disparar si el color es blanco
        if (currentColor == Color.white) return;
        // 2. No disparar si se está recargando
        if (isReloading) return;
        // 3. No disparar si no hay munición
        if (currentAmmo <= 0)
        {
            // Iniciar recarga si no hay munición y no se está recargando ya
            if (!isReloading) StartCoroutine(Reload());
            return;
        }
        // 4. No disparar si el cooldown de fireRate está activo
        if (!canShoot) return;

        // --- Lógica del Disparo ---
        currentAmmo--;    // Gastar una bala
        UpdateAmmoText(); // Actualizar UI

        // Determinar ángulo de dispersión (depende del zoom)
        float totalSpread = (cameraZoom != null && cameraZoom.IsZoomedIn)
            ? zoomedSpreadAngle
            : normalSpreadAngle;

        // Calcular ángulo entre perdigones
        float angleStep = (pelletsPerShot > 1)
            ? totalSpread / (pelletsPerShot - 1) // Evitar división por cero si solo hay 1 perdigón
            : 0f;
        // Calcular ángulo inicial para centrar la dispersión
        float startAngle = -totalSpread * 0.5f;

        // Instanciar cada perdigón
        for (int i = 0; i < pelletsPerShot; i++)
        {
            // Calcular rotación para este perdigón
            float currentAngle = startAngle + angleStep * i;
            Quaternion baseRotation = transform.rotation; // Rotación base del jugador
            Quaternion pelletRotation = baseRotation * Quaternion.Euler(0, 0, currentAngle); // Aplicar ángulo de dispersión

            // Seleccionar el prefab del proyectil según el color actual
            GameObject chosenPrefab = null;
            if      (currentColor == Color.red)    chosenPrefab = projectileRedPrefab;
            else if (currentColor == Color.blue)   chosenPrefab = projectileBluePrefab;
            else if (currentColor == Color.green)  chosenPrefab = projectileGreenPrefab;
            else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;

            // Usar prefab de fallback si no hay uno específico para el color
            if (chosenPrefab == null)
            {
                // Debug.LogWarning("[ShotgunShooting] No se encontró prefab para color " + currentColor + ". Usando fallback.");
                chosenPrefab = projectilePrefab;
            }

            // Instanciar solo si tenemos un prefab válido
            if (chosenPrefab != null)
            {
                GameObject projectile = Instantiate(chosenPrefab, transform.position, pelletRotation);

                // Asignar velocidad
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Usar velocity para movimiento constante inicial
                    rb.linearVelocity = projectile.transform.up * projectileSpeed;
                }

                // MUY IMPORTANTE => Asignar projectileColor en el script Projectile del perdigón
                Projectile proj = projectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.projectileColor = currentColor; // <---- CLAVE para la lógica de daño/interacción
                }
            }
            else {
                Debug.LogError("[ShotgunShooting] ¡chosenPrefab es null incluso después del fallback! Revisa las asignaciones en el Inspector.");
            }
        }

        // --- Efectos y Cooldowns ---
        StartCoroutine(ScaleEffect());              // Efecto visual de escala
        StartCoroutine(FireRateCooldown());         // Iniciar cooldown para el próximo disparo
        StartCoroutine(PlayShotgunAttackAnimation());// Iniciar animación de ataque

        // Retroceso de cámara si el componente existe
        if (CameraShake.Instance != null)
        {
            Vector3 recoilDirection = -transform.up; // Dirección opuesta a la que apunta
            CameraShake.Instance.RecoilCamera(recoilDirection);
        }

        // Si se quedó sin munición DESPUÉS de disparar, iniciar recarga
        if (currentAmmo <= 0 && !isReloading)
        {
            // Debug.Log("[ShotgunShooting] Munición agotada tras disparo, iniciando recarga.");
            StartCoroutine(Reload());
        }
    }

    // Corutina para manejar el cooldown de la cadencia de tiro
    IEnumerator FireRateCooldown()
    {
        canShoot = false; // Bloquear disparo
        yield return new WaitForSeconds(fireRate); // Esperar tiempo de cooldown
        canShoot = true;  // Permitir disparo de nuevo
    }

    // Corutina para manejar la recarga
    public IEnumerator Reload()
    {
        // No recargar si ya se está recargando o si la munición está llena
        if (isReloading || currentAmmo == magazineSize) yield break;

        isReloading = true; // Marcar como recargando
        UpdateAmmoText();   // Actualizar UI para mostrar "RELOADING"
        // Debug.Log("[ShotgunShooting] Iniciando recarga...");

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

        // Debug.Log("[ShotgunShooting] Recarga completada.");
    }

    // Actualiza el texto de la munición en la UI
    void UpdateAmmoText()
    {
        if (ammoText == null) return; // Salir si no hay referencia

        // Mostrar "RELOADING" o la cuenta actual
        if (isReloading)
            ammoText.text = "Escopeta: RELOADING";
        else
            ammoText.text = $"Escopeta: {currentAmmo}/{magazineSize}"; // Formato: Escopeta: 5/8
    }

    // Corutina para el efecto visual de escala al disparar
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
            // Usar deltaTime para que el efecto respete la pausa del juego
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
    }

    // ------------------------------------------------------------------
    // Actualizar color (solo teclado, lógica idéntica a PlayerShooting)
    // ------------------------------------------------------------------
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
            KeyCode currentlyPressedKey = GetLastKeyPressed(); // Usa el nombre original de tu método
            SetCurrentColorByKey(currentlyPressedKey); // Establece el color correspondiente (o blanco si ninguna)

            // CORREGIDO: Actualiza lastPressedKey a la tecla que quedó presionada, o None si ninguna
            lastPressedKey = currentlyPressedKey;
        }
        // --- Fin Lógica del Teclado ---


        // --- Sección de Gamepad Eliminada ---
        /*
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
                if (leftStick.y > threshold)      SetCurrentColor(Color.yellow);
                else if (leftStick.y < -threshold)SetCurrentColor(Color.green);
                else if (leftStick.x > threshold) SetCurrentColor(Color.red);
                else if (leftStick.x < -threshold)SetCurrentColor(Color.blue);
            }
        }
        */
        // --- Fin Sección Eliminada ---
    }

    // Se eliminó la función AnyWASDPressed() porque solo era usada por la lógica del gamepad.
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
    // o KeyCode.None si ninguna lo está. (Mantenido nombre original)
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

    // Establece el color actual y actualiza el color del SpriteRenderer.
    void SetCurrentColor(Color color)
    {
        currentColor = color;
        // Intenta obtener SpriteRenderer, puede que no esté en este mismo objeto
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = currentColor;
        // Considera si el color debe aplicarse a un objeto hijo o diferente
    }

    // Corutina para manejar la animación de ataque de la escopeta
    IEnumerator PlayShotgunAttackAnimation()
    {
        if (isPlayingShotgunAttackAnim) yield break; // Evitar solapamiento

        isPlayingShotgunAttackAnim = true;
        // Debug.Log("[ShotgunShooting] Attack anim => Activada.");

        // Desactivar idle, activar ataque
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = false;
        if (shotgunAttackScript != null) shotgunAttackScript.enabled = true;

        // Esperar duración
        yield return new WaitForSeconds(shotgunAttackAnimationDuration);

        // Desactivar ataque, reactivar idle
        if (shotgunAttackScript != null) shotgunAttackScript.enabled = false;
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = true;

        isPlayingShotgunAttackAnim = false;
        // Debug.Log("[ShotgunShooting] Attack anim => Finalizada.");
    }
}