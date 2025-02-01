using UnityEngine;

/// <summary>
/// ShooterEnemy se comporta siguiendo esta secuencia:
/// 1) Entra a la vista moviéndose desde fuera de la cámara hasta posicionarse a safeDistance del jugador.
/// 2) Apunta al jugador durante 3 segundos.
/// 3) Dispara un proyectil hacia el jugador.
/// 4) Se mueve lateralmente (dodging) para evitar contacto directo.
/// Luego repite los pasos 2-4 hasta recibir 3 impactos efectivos.
/// 
/// Además, una vez que el enemigo está dentro de la vista del jugador, se limita su posición para que no salga de la pantalla.
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
    public float projectileSpeed = 5f;           // Velocidad de los proyectiles

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
        if (player == null) return;

        // Evitar contacto directo: si se acerca demasiado, se aleja
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < safeDistance * 0.8f)
        {
            Vector3 awayDir = (transform.position - player.position).normalized;
            transform.position += awayDir * speed * Time.deltaTime;
            // Mientras se corrige la distancia, se omite la máquina de estados.
            return;
        }

        // Procesa el comportamiento según el estado actual
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

        // Una vez que el enemigo ya ha entrado en la vista (no en estado Entering),
        // aseguramos que se mantenga dentro de la pantalla.
        if (currentState != ShooterState.Entering)
        {
            transform.position = ClampToCameraView(transform.position);
        }
    }

    // Estado Entering: Se mueve hacia un punto a safeDistance del jugador hasta estar en cámara.
    private void ProcessEnteringState()
    {
        // Calcula una posición objetivo a safeDistance del jugador en la dirección contraria a su posición actual.
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

    // Estado Aiming: Apunta al jugador durante 3 segundos sin moverse.
    private void ProcessAimingState()
    {
        AimAtPlayer();
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = ShooterState.Shooting;
        }
    }

    // Estado Shooting: Dispara un proyectil y pasa a Dodging.
    private void ProcessShootingState()
    {
        ShootProjectile();

        // Calcular objetivo para movimiento lateral (dodging)
        Vector3 toPlayer = (player.position - transform.position).normalized;
        // Obtener una dirección perpendicular (aleatoria entre izquierda y derecha)
        Vector3 perpendicular = new Vector3(-toPlayer.y, toPlayer.x, 0f);
        if (Random.value > 0.5f)
            perpendicular = -perpendicular;
        // Se define un desplazamiento lateral de, por ejemplo, 2 unidades.
        dodgeTarget = transform.position + perpendicular * 2f;

        currentState = ShooterState.Dodging;
        stateTimer = 1f; // Duración del movimiento lateral
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

    // Rota el ShooterEnemy para apuntar al jugador.
    private void AimAtPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Dispara un proyectil hacia el jugador.
    private void ShootProjectile()
    {
        if (shooterProjectilePrefab == null) return;

        Debug.Log($"ShooterEnemy: disparando un proyectil de color {enemyColor} en t={Time.time} seg.");

        // Se instancia el proyectil un poco delante para evitar colisiones inmediatas
        Vector3 spawnPos = transform.position + transform.up * 0.5f;
        GameObject projObj = Instantiate(shooterProjectilePrefab, spawnPos, transform.rotation);

        // Asignar color al SpriteRenderer del proyectil
        SpriteRenderer sr = projObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = enemyColor;
        }

        // Asignar color al script Projectile (para color matching en colisiones)
        Projectile projScript = projObj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.projectileColor = enemyColor;
        }

        // Darle velocidad al proyectil en la dirección en que está apuntando el enemigo.
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
    /// Recibe una posición en mundo y la clampa para que esté dentro de la vista de la cámara.
    /// Se usan márgenes para evitar que el enemigo quede justo en el borde.
    /// </summary>
    private Vector3 ClampToCameraView(Vector3 pos)
    {
        Camera cam = Camera.main;
        if (cam == null) return pos;

        Vector3 viewportPos = cam.WorldToViewportPoint(pos);
        // Definir márgenes (puedes ajustar estos valores)
        float minX = 0.05f, maxX = 0.95f;
        float minY = 0.05f, maxY = 0.95f;

        viewportPos.x = Mathf.Clamp(viewportPos.x, minX, maxX);
        viewportPos.y = Mathf.Clamp(viewportPos.y, minY, maxY);

        Vector3 clampedPos = cam.ViewportToWorldPoint(viewportPos);
        clampedPos.z = pos.z;
        return clampedPos;
    }

    // Manejo de colisiones: verifica impactos de proyectiles del jugador o contacto con el jugador.
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si colisiona con un proyectil del jugador
        if (collision.collider.CompareTag("Projectile"))
        {
            Projectile projectile = collision.collider.GetComponent<Projectile>();
            if (projectile != null)
            {
                if (projectile.projectileColor == enemyColor)
                {
                    currentHealth--;
                    Destroy(collision.gameObject);
                    if (currentHealth <= 0)
                    {
                        DestroyShooterEnemy();
                    }
                    else
                    {
                        // Puedes agregar feedback visual o sonoro por el daño recibido.
                    }
                }
            }
        }
        // Si colisiona con el jugador
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

    // Manejo de la destrucción del ShooterEnemy.
    private void DestroyShooterEnemy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(150);
        }
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
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            SlowMotion slow = pObj.GetComponent<SlowMotion>();
            if (slow != null)
            {
                slow.AddSlowMotionCharge();
            }
        }
        Destroy(gameObject);
    }
}
