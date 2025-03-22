using UnityEngine;
using System.Collections;

/// <summary>
/// ShooterEnemy se comporta siguiendo esta secuencia:
/// 1) Entra a la vista moviéndose desde fuera de la cámara hasta posicionarse a safeDistance del jugador.
/// 2) Apunta al jugador durante 3 segundos.
/// 3) Dispara un proyectil hacia el jugador.
/// 4) Se mueve lateralmente (dodging) para evitar contacto directo.
/// Luego repite los pasos 2-4 hasta recibir 3 impactos efectivos.
///
/// Además, una vez que el enemigo está dentro de la vista del jugador, se limita su posición para que no salga de la pantalla.
/// Se agrega feedback de daño (flash y shake) cuando recibe un impacto efectivo.
/// </summary>
public class ShooterEnemy : MonoBehaviour
{
    [Header("Configuración del ShooterEnemy")]
    public Color enemyColor = Color.white;  // Color que define su vulnerabilidad y el color de sus proyectiles
    public float speed = 2f;                // Velocidad de movimiento para reubicarse
    public int maxHealth = 3;               // Vida total (3 impactos efectivos)
    public GameObject explosionPrefab;      // Prefab de explosión al morir

    [Header("Disparo")]
    public GameObject shooterProjectilePrefab; // Prefab del proyectil que dispara
    public float projectileSpeed = 5f;         // Velocidad de los proyectiles

    [Header("Distancia Mínima")]
    public float safeDistance = 6f; // El enemigo se mantendrá a esta distancia del jugador

    // Estados del ShooterEnemy
    private enum ShooterState { Entering, Aiming, Shooting, Dodging }
    private ShooterState currentState;

    private float stateTimer = 0f;    // Temporizador para cada estado
    private Vector3 dodgeTarget;      // Posición objetivo para el movimiento lateral (dodging)

    // Variables internas
    private int currentHealth;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private CameraZoom cameraZoom;  // Referencia cacheada al CameraZoom

    void Start()
    {
        currentHealth = maxHealth;

        // Obtener referencia al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Configurar el SpriteRenderer y asignar el color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }

        // Comenzamos en estado "Entering" para movernos dentro de la cámara
        currentState = ShooterState.Entering;

        // Cachear la referencia al CameraZoom (se asume que hay uno en la escena)
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterShooterEnemy(this);
        }
    }

    void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterShooterEnemy(this);
        }
    }

    void Update()
    {
        if (player == null)
            return;

        bool isZoomActive = (cameraZoom != null && cameraZoom.IsZoomedIn);

        // Solo aplicar la corrección de distancia si el Zoom NO está activo
        if (!isZoomActive)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < safeDistance * 0.8f)
            {
                Vector3 awayDir = (transform.position - player.position).normalized;
                transform.position += awayDir * speed * Time.deltaTime;
                // Mientras se corrige la distancia, se omite la máquina de estados.
                return;
            }
        }

        // Procesar la lógica según el estado actual
        switch (currentState)
        {
            case ShooterState.Entering:
                ProcessEnteringState();
                break;
            case ShooterState.Aiming:
                ProcessAimingState();
                break;
            case ShooterState.Shooting:
                ProcessShootingState();
                break;
            case ShooterState.Dodging:
                ProcessDodgingState();
                break;
        }

        // Clampear la posición (margen) si no está en Entering y no hay Zoom
        if (currentState != ShooterState.Entering && !isZoomActive)
        {
            transform.position = ClampToCameraView(transform.position);
        }
    }

    // Estado Entering: Se mueve hacia un punto a safeDistance del jugador hasta estar en cámara.
    private void ProcessEnteringState()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 targetPos = player.position + direction * safeDistance;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        AimAtPlayer();

        // Una vez que el enemigo está dentro de la vista de la cámara, pasa a Aiming.
        if (IsInView())
        {
            currentState = ShooterState.Aiming;
            stateTimer = 3f; // 3 segundos de apuntado
        }
    }

    // Estado Aiming: Apunta al jugador durante 3 segundos.
    private void ProcessAimingState()
    {
        AimAtPlayer();
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = ShooterState.Shooting;
        }
    }

    // Estado Shooting: Dispara y pasa a Dodging.
    private void ProcessShootingState()
    {
        ShootProjectile();

        // Calcular objetivo para movimiento lateral (dodging)
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-toPlayer.y, toPlayer.x, 0f);
        if (Random.value > 0.5f)
            perpendicular = -perpendicular;

        dodgeTarget = transform.position + perpendicular * 2f;

        currentState = ShooterState.Dodging;
        stateTimer = 1f;
    }

    // Estado Dodging: Se mueve lateralmente y luego regresa a Aiming.
    private void ProcessDodgingState()
    {
        transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, speed * Time.deltaTime);
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = ShooterState.Aiming;
            stateTimer = 3f;
        }
    }

    // Rota el enemigo para apuntar al jugador.
    private void AimAtPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Dispara un proyectil hacia el jugador.
    private void ShootProjectile()
    {
        if (shooterProjectilePrefab == null)
            return;

        Debug.Log($"ShooterEnemy: disparando un proyectil de color {enemyColor} en t={Time.time} seg.");

        Vector3 spawnPos = transform.position + transform.up * 0.5f;
        GameObject projObj = Instantiate(shooterProjectilePrefab, spawnPos, transform.rotation);

        // Asignar color al SpriteRenderer
        SpriteRenderer sr = projObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = enemyColor;
        }

        // Asignar color al script EnemyProjectile (para colisiones)
        EnemyProjectile enemyProj = projObj.GetComponent<EnemyProjectile>();
        if (enemyProj != null)
        {
            enemyProj.bulletColor = enemyColor;
        }

        // Velocidad al proyectil
        Rigidbody2D rb = projObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.up * projectileSpeed;
        }
    }

    // Retorna true si el ShooterEnemy está dentro de la vista principal de la cámara.
    private bool IsInView()
    {
        if (Camera.main == null) return false;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewportPos.x >= 0 && viewportPos.x <= 1 &&
                viewportPos.y >= 0 && viewportPos.y <= 1 &&
                viewportPos.z > 0);
    }

    /// <summary>
    /// Limita la posición para que no salga de la vista de la cámara (con márgenes).
    /// </summary>
    private Vector3 ClampToCameraView(Vector3 pos)
    {
        Camera cam = Camera.main;
        if (cam == null) return pos;

        Vector3 viewportPos = cam.WorldToViewportPoint(pos);

        float horizontalMargin = 0.2f;
        float verticalMargin = 0.1f;

        viewportPos.x = Mathf.Clamp(viewportPos.x, horizontalMargin, 1f - horizontalMargin);
        viewportPos.y = Mathf.Clamp(viewportPos.y, verticalMargin, 1f - verticalMargin);

        Vector3 clampedPos = cam.ViewportToWorldPoint(viewportPos);
        clampedPos.z = pos.z;
        return clampedPos;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            Projectile projectile = collision.collider.GetComponent<Projectile>();
            if (projectile != null && projectile.projectileColor == enemyColor)
            {
                currentHealth--;
                Destroy(collision.gameObject);

                if (currentHealth <= 0)
                {
                    DestroyShooterEnemy();
                }
                else
                {
                    StartCoroutine(DamageFeedback());
                }
            }
        }
        else if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera();
            }

            DestroyShooterEnemy();
        }
    }

    /// <summary>
    /// Efecto visual de daño: flash y un pequeño shake del ShooterEnemy.
    /// </summary>
    private IEnumerator DamageFeedback()
    {
        Color originalColor = spriteRenderer.color;
        Color flashColor = Color.Lerp(enemyColor, Color.white, 0.5f);
        spriteRenderer.color = flashColor;

        Vector3 originalPosition = transform.position;
        float shakeDuration = 0.2f;
        float elapsedTime = 0f;
        float magnitude = 0.5f;

        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = originalPosition + new Vector3(x, y, 0f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        spriteRenderer.color = originalColor;
    }

    // -------------------------------------------------
    // Manejo de la destrucción del ShooterEnemy
    // -------------------------------------------------
    private void DestroyShooterEnemy()
    {
        // Sumar score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(150);

        // Intentar soltar Lumi-Coins (Script EnemyCoinDrop)
        EnemyCoinDrop coinDrop = GetComponent<EnemyCoinDrop>();
        if (coinDrop != null)
        {
            coinDrop.TryDropCoins();
        }

        // Efecto de explosión
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = enemyColor;
            }
        }

        // Añadir slow motion
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            SlowMotion slow = pObj.GetComponent<SlowMotion>();
            if (slow != null)
                slow.AddSlowMotionCharge();
        }

        // Destruir
        Destroy(gameObject);
    }
}