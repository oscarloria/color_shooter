using UnityEngine;
using System.Collections;
using TMPro;
// Se eliminó: using UnityEngine.InputSystem; // Ya no se necesita para el gamepad

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
    public WeaponReloadIndicator reloadIndicator; // Referencia al script del indicador

    private CameraZoom cameraZoom;
    private KeyCode lastPressedKey = KeyCode.None; // Guarda la última tecla WASD presionada

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
        // Carga la configuración guardada o usa los valores por defecto
        magazineSize = PlayerPrefs.GetInt(PISTOL_MAGAZINE_SIZE_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(PISTOL_RELOAD_TIME_KEY, defaultReloadTime);

        // Inicializa la munición y la UI
        currentAmmo = magazineSize;
        UpdateAmmoText();

        // Encuentra la referencia al script de zoom de la cámara
        cameraZoom = FindObjectOfType<CameraZoom>();
        Debug.Log("[PlayerShooting] Start => magSize=" + magazineSize + ", reloadTime=" + reloadTime);
    }

    void Update()
    {
        // Actualiza el color basado en la entrada del teclado cada frame
        UpdateCurrentColor();
    }

    // ----------------------------------------------------------------------------
    // Actualizar color (solo teclado)
    // ----------------------------------------------------------------------------
    public void UpdateCurrentColor()
    {
        // --- Lógica del Teclado ---
        // Detecta si se PRESIONA una tecla WASD en este frame
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetCurrentColor(Color.yellow);
            lastPressedKey = KeyCode.W; // Guarda W como la última tecla presionada
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetCurrentColor(Color.blue);
            lastPressedKey = KeyCode.A; // Guarda A como la última tecla presionada
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetCurrentColor(Color.green);
            lastPressedKey = KeyCode.S; // Guarda S como la última tecla presionada
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SetCurrentColor(Color.red);
            lastPressedKey = KeyCode.D; // Guarda D como la última tecla presionada
        }

        // Detecta si se SUELTA la última tecla que se había presionado
        if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey))
        {
            // Verifica si alguna OTRA tecla WASD sigue presionada
            KeyCode currentlyPressedKey = GetCurrentlyPressedKey();
            SetCurrentColorByKey(currentlyPressedKey); // Establece el color correspondiente (o blanco si ninguna)

            // Actualiza lastPressedKey a la tecla que quedó presionada, o None si ninguna
            lastPressedKey = currentlyPressedKey;
        }
        // --- Fin Lógica del Teclado ---


        // --- Sección de Gamepad Eliminada ---
        // Toda la lógica que usaba Gamepad.current y gp.leftStick fue removida.
        // --- Fin Sección Eliminada ---
    }

    // Se eliminó la función AnyWASDPressed() porque solo era usada por la lógica del gamepad.

    // Devuelve la tecla WASD que está actualmente presionada (con prioridad D > S > A > W)
    // o KeyCode.None si ninguna lo está.
    KeyCode GetCurrentlyPressedKey() // Renombrada para mayor claridad
    {
        if (Input.GetKey(KeyCode.D)) return KeyCode.D; // Si D está presionada, devuelve D
        if (Input.GetKey(KeyCode.S)) return KeyCode.S; // Si no, si S está presionada, devuelve S
        if (Input.GetKey(KeyCode.A)) return KeyCode.A; // Si no, si A está presionada, devuelve A
        if (Input.GetKey(KeyCode.W)) return KeyCode.W; // Si no, si W está presionada, devuelve W
        return KeyCode.None; // Si ninguna está presionada, devuelve None
    }

    // Establece el color actual basado en una KeyCode.
    void SetCurrentColorByKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: SetCurrentColor(Color.yellow); break; // Amarillo para W
            case KeyCode.A: SetCurrentColor(Color.blue);   break; // Azul para A
            case KeyCode.S: SetCurrentColor(Color.green);  break; // Verde para S
            case KeyCode.D: SetCurrentColor(Color.red);    break; // Rojo para D
            default:        SetCurrentColor(Color.white);  break; // Blanco si key es KeyCode.None (ninguna presionada)
        }
    }

    // Establece el color actual y actualiza el color del SpriteRenderer.
    void SetCurrentColor(Color color)
    {
        currentColor = color; // Actualiza la variable de color
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>(); // Obtiene el componente SpriteRenderer
        if (spriteRenderer != null) // Si existe el componente
        {
            spriteRenderer.color = currentColor; // Aplica el color al sprite
        }
    }

    // ----------------------------------------------------------------------------
    // Disparo
    // ----------------------------------------------------------------------------
    public void Shoot()
    {
        // Log para depuración
        // Debug.Log("[PlayerShooting] Shoot() => Intentando disparar. currentColor=" + currentColor);

        // --- Comprobaciones antes de disparar ---
        // 1. Respetar la cadencia de tiro (fireRate)
        if (Time.time < nextFireTime)
        {
            // Debug.LogWarning("[PlayerShooting] FireRate => Bloqueado, nextFireTime=" + nextFireTime + ", currentTime=" + Time.time);
            return; // Salir si no ha pasado suficiente tiempo desde el último disparo
        }
        nextFireTime = Time.time + fireRate; // Actualizar el tiempo para el próximo disparo permitido

        // 2. No disparar si el color es blanco (estado neutral)
        if (currentColor == Color.white)
        {
            // Debug.LogWarning("[PlayerShooting] currentColor es WHITE => no se dispara.");
            return;
        }
        // 3. No disparar si se está recargando
        if (isReloading)
        {
            // Debug.LogWarning("[PlayerShooting] isReloading => no se dispara.");
            return;
        }
        // 4. No disparar si no hay munición
        if (currentAmmo <= 0)
        {
            // Debug.LogWarning("[PlayerShooting] Sin munición => no se dispara.");
            // Considera iniciar la recarga automáticamente aquí si no se está recargando ya
            if (!isReloading) StartCoroutine(Reload());
            return;
        }

        // --- Lógica del Disparo ---
        currentAmmo--; // Gastar una bala
        UpdateAmmoText(); // Actualizar la UI de munición

        // Calcular ángulo de dispersión (menor si se está haciendo zoom)
        float dispersionAngle = (cameraZoom != null && cameraZoom.IsZoomedIn) ? zoomedDispersionAngle : normalDispersionAngle;
        float randomAngle = Random.Range(-dispersionAngle / 2f, dispersionAngle / 2f); // Ángulo aleatorio dentro de la dispersión
        Quaternion projectileRotation = transform.rotation * Quaternion.Euler(0, 0, randomAngle); // Aplicar rotación con dispersión

        // Elegir el prefab del proyectil según el color actual
        GameObject chosenPrefab = null;
        if      (currentColor == Color.red)    chosenPrefab = projectileRedPrefab;
        else if (currentColor == Color.blue)   chosenPrefab = projectileBluePrefab;
        else if (currentColor == Color.green)  chosenPrefab = projectileGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = projectileYellowPrefab;

        // Usar el prefab por defecto si no se encontró uno específico para el color (o si algo falló)
        if (chosenPrefab == null)
        {
            Debug.LogWarning("[PlayerShooting] chosenPrefab es null para color " + currentColor + " => usando fallback (projectilePrefab).");
            chosenPrefab = projectilePrefab; // Asegúrate que projectilePrefab esté asignado en el Inspector
        }

        // Solo proceder si tenemos un prefab válido (normal o fallback)
         if (chosenPrefab != null)
         {
            // Debug.Log("[PlayerShooting] Disparo => Instanciando '" + chosenPrefab.name + "' con color=" + currentColor);

            // Instanciar el proyectil en la posición y rotación calculadas
            GameObject projectile = Instantiate(chosenPrefab, transform.position, projectileRotation);

            // Asignar velocidad al proyectil usando su Rigidbody2D
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = projectile.transform.up * projectileSpeed; // Usar velocity en lugar de linearVelocity (más común para movimiento constante)
                // Debug.Log("[PlayerShooting] => Velocidad asignada: " + rb.velocity);
            }
            else
            {
                Debug.LogWarning("[PlayerShooting] => El prefab de proyectil '" + chosenPrefab.name + "' no tiene Rigidbody2D, no se moverá.");
            }

            // Si el proyectil tiene un script "Projectile", pasarle el color
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.projectileColor = currentColor; // Asegura que el proyectil sepa su color lógico
            }

            // Iniciar corutinas para efectos visuales y de animación
            StartCoroutine(PlayAttackAnimation()); // Animación de ataque del jugador
            StartCoroutine(ScaleEffect());         // Efecto de escala del jugador
            if (CameraShake.Instance != null)      // Efecto de retroceso de la cámara
            {
                Vector3 recoilDirection = -transform.up; // Dirección opuesta a la que apunta el jugador
                CameraShake.Instance.RecoilCamera(recoilDirection);
            }
         }
         else
         {
             Debug.LogError("[PlayerShooting] ¡No se pudo instanciar el proyectil porque chosenPrefab y projectilePrefab son null!");
         }

        // Si la munición llega a 0 DESPUÉS de disparar, iniciar recarga
        if (currentAmmo <= 0 && !isReloading)
        {
             // Debug.Log("[PlayerShooting] Munición agotada tras disparo, iniciando recarga.");
             StartCoroutine(Reload());
        }
    }

    // Corutina para manejar la animación de ataque
    IEnumerator PlayAttackAnimation()
    {
        if (isPlayingAttackAnim) yield break; // Salir si ya se está ejecutando la animación

        isPlayingAttackAnim = true; // Marcar que la animación está activa
        // Debug.Log("[PlayerShooting] Activando anim de ataque (Pistol).");

        // Desactivar animación idle y activar animación de ataque
        if (idleScript != null) idleScript.enabled = false;
        if (attackScript != null) attackScript.enabled = true;

        // Esperar la duración de la animación de ataque
        yield return new WaitForSeconds(attackAnimationDuration);

        // Desactivar animación de ataque y reactivar animación idle
        // (Hacerlo en orden inverso por si acaso)
        if (attackScript != null) attackScript.enabled = false;
        if (idleScript != null) idleScript.enabled = true;


        isPlayingAttackAnim = false; // Marcar que la animación ha terminado
         // Debug.Log("[PlayerShooting] Animación de ataque terminada.");
    }


    // ----------------------------------------------------------------------------
    // Recarga (CORREGIDO para usar ResetIndicator y UpdateIndicator)
    // ----------------------------------------------------------------------------
    public IEnumerator Reload()
    {
        // Evitar iniciar una recarga si ya se está recargando
        if (isReloading) yield break;

        isReloading = true; // Marcar como recargando
        UpdateAmmoText();   // Actualizar UI para mostrar "RELOADING"
        // Debug.Log("[PlayerShooting] Iniciando Coroutine Reload...");

        // Reiniciar el indicador al inicio de la recarga (si existe)
        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator(); // CORREGIDO: Usa el método original

        float reloadTimer = 0f; // Inicializar el temporizador de recarga
        // Bucle mientras dure la recarga
        while (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime; // Incrementar el temporizador
            // Actualizar el indicador de recarga (si existe) con el progreso (0 a 1)
            if (reloadIndicator != null)
                reloadIndicator.UpdateIndicator(reloadTimer / reloadTime); // CORREGIDO: Usa el método original
            yield return null; // Esperar al siguiente frame
        }

        // Finalizar la recarga
        currentAmmo = magazineSize; // Rellenar munición
        isReloading = false;        // Marcar como no recargando
        UpdateAmmoText();           // Actualizar UI para mostrar munición completa

        // Resetear el indicador al final de la recarga (si existe)
        if (reloadIndicator != null)
            reloadIndicator.ResetIndicator(); // CORREGIDO: Usa el método original

        // Debug.Log("[PlayerShooting] Recarga completada.");
    }

    // ----------------------------------------------------------------------------
    // UI
    // ----------------------------------------------------------------------------
    public void UpdateAmmoText()
    {
        if (ammoText == null) return; // Salir si no hay referencia al texto de munición

        // Mostrar "RELOADING" o la cuenta de munición
        if (isReloading)
        {
            ammoText.text = "Pistola: RELOADING";
        }
        else
        {
            ammoText.text = $"Pistola: {currentAmmo}/{magazineSize}"; // Formato: Pistola: 3/4
        }
    }

    // ----------------------------------------------------------------------------
    // Efecto de escala al disparar
    // ----------------------------------------------------------------------------
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale; // Escala inicial
        Vector3 targetScale = originalScale * scaleMultiplier; // Escala objetivo (más grande)

        float elapsedTime = 0f;
        float halfDuration = scaleDuration / 2f; // Duración para escalar hacia arriba y hacia abajo

        // Escalar hacia arriba (Lerp de original a target)
        while (elapsedTime < halfDuration)
        {
            // Calcula el progreso (0 a 1)
            float t = elapsedTime / halfDuration;
            // Interpola linealmente la escala
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            // Incrementa el tiempo transcurrido (usando deltaTime para que respete la pausa del juego)
            elapsedTime += Time.deltaTime;
            // Espera al siguiente frame
            yield return null;
        }

        // Asegurarse de que llega exactamente a la escala objetivo
        transform.localScale = targetScale;

        // Resetear tiempo para escalar hacia abajo
        elapsedTime = 0f;

        // Escalar hacia abajo (Lerp de target a original)
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que vuelve exactamente a la escala original
        transform.localScale = originalScale;
    }
}