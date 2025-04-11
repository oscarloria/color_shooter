using UnityEngine;
using System.Collections;

/// <summary>
/// ShooterEnemy simple: 
/// 1) Se acerca al jugador hasta mantener una distancia 'safeDistance'.
/// 2) Aiming durante 3s => dispara => Dodging lateral 1s => repite.
/// No hay lógica de cámara/zoom. 
/// Modificado: La dirección del Dodging es aleatoria la primera vez, luego alterna.
/// </summary>
public class ShooterEnemy : MonoBehaviour
{
    [Header("Configuración del ShooterEnemy")]
    public Color enemyColor = Color.white;  // Color que define su vulnerabilidad y el color de sus proyectiles
    public float speed = 2f;                // Velocidad de movimiento
    public int maxHealth = 3;               // Vida total (3 impactos efectivos)
    public GameObject explosionPrefab;      // Prefab de explosión al morir

    [Header("Disparo")]
    public GameObject shooterProjectilePrefab; // Prefab del proyectil que dispara
    public float projectileSpeed = 5f;        // Velocidad de los proyectiles

    [Header("Distancia Mínima")]
    public float safeDistance = 6f; // El enemigo se mantendrá a esta distancia del jugador

    // Maquina de estados
    private enum ShooterState { Entering, Aiming, Shooting, Dodging }
    private ShooterState currentState;

    // Timers e internals
    private float stateTimer = 0f;  
    private Vector3 dodgeTarget;
    private int currentHealth;

    // ---- NUEVAS VARIABLES PARA EL DODGING ALTERNADO ----
    private bool isFirstDodge = true; // Indica si es la primera vez que va a esquivar
    private bool dodgeDirectionIsLeft; // Guarda la dirección del último (o próximo) esquive
    // ---------------------------------------------------

    // Referencias
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

        // SpriteRenderer y color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }

        // Iniciar en "Entering": se mueve hacia el jugador hasta safeDistance
        currentState = ShooterState.Entering;
        isFirstDodge = true; // Asegurarse de que la primera vez sea aleatorio al iniciar
    }

    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterShooterEnemy(this);
        }
        // Resetear el estado del dodge si el enemigo se reutiliza (por ejemplo, desde un pool)
        isFirstDodge = true; 
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

        // Controlar la lógica según el estado
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
    }

    // 1) Estado Entering: se acerca al jugador hasta safeDistance
    private void ProcessEnteringState()
    {
        // Calcular la distancia actual
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si todavía está más lejos que safeDistance, moverse
        if (distanceToPlayer > safeDistance)
        {
            // Moverse hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            AimAtPlayer();
        }
        else
        {
            // Si ya está a safeDistance (o más cerca), pasa a Aiming
            currentState = ShooterState.Aiming;
            stateTimer = 3f;  
        }
    }

    // 2) Estado Aiming: se queda apuntando al jugador durante 3s
    private void ProcessAimingState()
    {
        AimAtPlayer();
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = ShooterState.Shooting;
        }
    }

    // 3) Estado Shooting: dispara y pasa a Dodging (Lógica de dirección modificada)
    private void ProcessShootingState()
    {
        ShootProjectile();

        // Calcular vector perpendicular a la dirección del jugador
        Vector3 toPlayer = (player.position - transform.position).normalized;
        // Vector perpendicular base (por ejemplo, hacia la izquierda relativa)
        Vector3 perpendicularLeft = new Vector3(-toPlayer.y, toPlayer.x, 0f); 

        // ---- LÓGICA DE DIRECCIÓN MODIFICADA ----
        if (isFirstDodge)
        {
            // La primera vez, elegir aleatoriamente
            dodgeDirectionIsLeft = (Random.value < 0.5f); // 50% de probabilidad de ir a la izquierda
            isFirstDodge = false; // Ya no será la primera vez en los próximos ciclos
        }
        else
        {
            // Las siguientes veces, alternar la dirección
            dodgeDirectionIsLeft = !dodgeDirectionIsLeft; 
        }
        // -----------------------------------------

        // Determinar el vector de dirección final basado en la decisión
        Vector3 dodgeDirectionVector = dodgeDirectionIsLeft ? perpendicularLeft : -perpendicularLeft;

        // Calcular el punto objetivo para el esquive
        dodgeTarget = transform.position + dodgeDirectionVector * 2f; // Puedes ajustar el '2f' si quieres que esquive más o menos distancia

        // Cambiar al estado de esquivar
        currentState = ShooterState.Dodging;
        stateTimer = 1f; // Moverse lateralmente por 1s
    }

    // 4) Estado Dodging: moverse a dodgeTarget 1s, luego volver a Aiming
    private void ProcessDodgingState()
    {
        transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, speed * Time.deltaTime);
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            // Después de 1s, regresa a Aiming 3s
            currentState = ShooterState.Aiming;
            stateTimer = 3f;
        }
    }

    // Apunta al jugador
    private void AimAtPlayer()
    {
        if (player == null) return; // Añadir chequeo por si el jugador es destruido
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Dispara un proyectil
    private void ShootProjectile()
    {
        if (shooterProjectilePrefab == null) return;
        if (player == null) return; // No disparar si no hay jugador

        Debug.Log($"[ShooterEnemy] Disparando proyectil color={enemyColor} t={Time.time}");

        // Asegurarse de que apunta al jugador justo antes de disparar por si se movió durante Aiming
        AimAtPlayer(); 

        Vector3 spawnPos = transform.position + transform.up * 0.5f; // Sale un poco adelante del enemigo
        GameObject projObj = Instantiate(shooterProjectilePrefab, spawnPos, transform.rotation);

        // Asignar color visual
        SpriteRenderer sr = projObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = enemyColor;
        }

        // Asignar color lógico
        EnemyProjectile enemyProj = projObj.GetComponent<EnemyProjectile>();
        if (enemyProj != null)
        {
            enemyProj.bulletColor = enemyColor;
        }

        // Aplicar velocidad
        Rigidbody2D rb = projObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.up * projectileSpeed; // Usar velocity para movimiento constante
        } else {
            // Fallback si no hay Rigidbody2D (menos recomendado para proyectiles)
            StartCoroutine(MoveProjectile(projObj.transform, transform.up, projectileSpeed));
        }
    }

    // Corutina simple para mover el proyectil si no tiene Rigidbody2D
    private IEnumerator MoveProjectile(Transform projectileTransform, Vector3 direction, float speed) {
        while (projectileTransform != null) {
            projectileTransform.position += direction * speed * Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }
    }


    // Colisiones con proyectiles / jugador
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            Projectile projectile = collision.collider.GetComponent<Projectile>();
            // Asegurarse que el proyectil existe y tiene el color correcto
            if (projectile != null && projectile.projectileColor == enemyColor) 
            {
                currentHealth--;
                Destroy(collision.gameObject); // Destruir el proyectil del jugador

                if (currentHealth <= 0)
                {
                    DestroyShooterEnemy();
                }
                else
                {
                    // Solo iniciar feedback si aún está vivo
                    StartCoroutine(DamageFeedback());
                }
            }
            // Opcional: Podrías añadir lógica si choca con un proyectil de color incorrecto (ej. rebota?)
        }
        else if (collision.collider.CompareTag("Player"))
        {
            // Dañar al jugador
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(); 
            }

            // Efecto de cámara, si existe
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera();
            }

            // El enemigo se destruye al chocar con el jugador
            DestroyShooterEnemy(); 
        }
    }

    // Efecto visual de daño: flash y pequeño "shake"
    private IEnumerator DamageFeedback()
    {
        if (spriteRenderer == null) yield break; // Salir si no hay renderer

        Color originalColor = spriteRenderer.color; // Usar el color actual como base
        Color flashColor = Color.white; // Flash blanco simple
        
        // Flash rápido
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.05f); 
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = originalColor; // Volver al color original

        // Pequeño shake (opcional, puedes quitarlo si interfiere mucho)
        Vector3 originalPosition = transform.position;
        float shakeDuration = 0.15f;
        float elapsedTime = 0f;
        float magnitude = 0.1f; // Reducir magnitud para que sea sutil

        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            // Asegurarse de no moverse en Z si es 2D
            transform.position = originalPosition + new Vector3(x, y, 0f); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que vuelve exactamente a su posición
        transform.position = originalPosition; 
    }

    // Destruir al ShooterEnemy
    private void DestroyShooterEnemy()
    {
        // Evitar doble destrucción si ya se está destruyendo
        if (!gameObject.activeSelf) return; 

        // Sumar score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(150);

        // Soltar Lumi-Coins
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
                // Usar el color original del enemigo para la explosión
                main.startColor = new ParticleSystem.MinMaxGradient(enemyColor); 
            }
            // Opcional: Destruir el objeto de explosión después de un tiempo si no se autodestruye
             // Destroy(explosion, ps != null ? ps.main.duration : 2f); 
        }

        // Añadir slow motion (si existe el componente en el jugador)
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            SlowMotion slow = pObj.GetComponent<SlowMotion>();
            if (slow != null)
                slow.AddSlowMotionCharge();
        }

        // Desactivar el objeto inmediatamente para que deje de interactuar
        gameObject.SetActive(false); 
        // Destruir el objeto después de un pequeño delay para asegurar que todo se ejecute
        Destroy(gameObject, 0.1f); 
    }
}