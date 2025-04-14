using UnityEngine;
using System.Collections;

/// <summary>
/// ShooterEnemy modificado:
/// 1) Entra hasta safeDistance.
/// 2) Realiza una secuencia 3 veces: Dispara -> Espera 1s -> Esquiva -> Espera 1s.
/// 3) Después del 3er disparo y sus pausas/esquive, entra en modo Kamikaze.
/// 4) Modo Kamikaze: Se lanza directo al jugador a 2x de velocidad.
/// </summary>
public class ShooterEnemy : MonoBehaviour
{
    [Header("Configuración del ShooterEnemy")]
    public Color enemyColor = Color.white;
    public float speed = 2f;
    public int maxHealth = 3;
    public GameObject explosionPrefab;

    [Header("Disparo")]
    public GameObject shooterProjectilePrefab;
    public float projectileSpeed = 5f;

    [Header("Distancia Mínima")]
    public float safeDistance = 6f;

    [Header("Tiempos de Pausa")] // NUEVO: Configurable si quieres
    [Tooltip("Tiempo de espera después de disparar antes de esquivar.")]
    public float waitTimeAfterShoot = 1.0f;
    [Tooltip("Tiempo de espera después de esquivar antes de disparar o atacar.")]
    public float waitTimeAfterDodge = 1.0f;
    [Tooltip("Duración del movimiento de esquive.")]
    public float dodgeDuration = 1.0f;


    // Maquina de estados MODIFICADA
    private enum ShooterState
    {
        Entering,           // Moviéndose a la posición inicial
        Shooting,           // Disparando un proyectil
        WaitingAfterShoot,  // Pausa después de disparar
        Dodging,            // Moviéndose lateralmente
        WaitingAfterDodge,  // Pausa después de esquivar
        Kamikaze            // Ataque final directo al jugador
    }
    private ShooterState currentState;

    // Timers e internals
    private float stateTimer = 0f;
    private Vector3 dodgeTarget;
    private int currentHealth;

    // ---- Variables para el control de secuencia ----
    private int shotsFiredCount = 0; // Contador de disparos realizados
    private const int MAX_SHOTS_BEFORE_KAMIKAZE = 3; // Número de disparos antes del ataque final
    // -----------------------------------------------

    // Variables para el dodging alternado (sin cambios)
    private bool isFirstDodge = true;
    private bool dodgeDirectionIsLeft;

    // Referencias
    private Transform player;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        shotsFiredCount = 0; // Reiniciar contador al inicio

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) { player = playerObj.transform; }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) { spriteRenderer.color = enemyColor; }

        currentState = ShooterState.Entering; // Empezar entrando
        isFirstDodge = true;
    }

    void OnEnable()
    {
        if (EnemyManager.Instance != null) { EnemyManager.Instance.RegisterShooterEnemy(this); }
        // Resetear estado si se reutiliza
        currentState = ShooterState.Entering;
        shotsFiredCount = 0;
        isFirstDodge = true;
        stateTimer = 0f; // Asegurar que los timers se reinicien
    }

    void OnDisable()
    {
        if (EnemyManager.Instance != null) { EnemyManager.Instance.UnregisterShooterEnemy(this); }
    }

    void Update()
    {
        if (player == null) return; // Si el jugador no existe, no hacer nada

        // Procesar estado actual
        switch (currentState)
        {
            case ShooterState.Entering:
                ProcessEnteringState();
                break;
            case ShooterState.Shooting:
                ProcessShootingState();
                break;
            case ShooterState.WaitingAfterShoot:
                ProcessWaitingAfterShootState();
                break;
            case ShooterState.Dodging:
                ProcessDodgingState();
                break;
            case ShooterState.WaitingAfterDodge:
                ProcessWaitingAfterDodgeState();
                break;
            case ShooterState.Kamikaze:
                ProcessKamikazeState();
                break;
        }
    }

    // 1) Estado Entering: se acerca al jugador hasta safeDistance
    private void ProcessEnteringState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > safeDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            AimAtPlayer(); // Apuntar mientras se acerca
        }
        else
        {
            // Ha llegado a la distancia, listo para iniciar la secuencia de disparo
            Debug.Log("[ShooterEnemy] Entering Complete. Transitioning to Shooting.");
            currentState = ShooterState.Shooting; // Pasa directamente a disparar el primer proyectil
        }
    }

    // 2) Estado Shooting: dispara y pasa a esperar después del disparo
    private void ProcessShootingState()
    {
        // Asegurarse de apuntar justo antes de disparar
        AimAtPlayer();
        ShootProjectile();
        shotsFiredCount++; // Incrementar contador de disparos

        Debug.Log($"[ShooterEnemy] Shot #{shotsFiredCount} fired. Transitioning to WaitingAfterShoot.");
        currentState = ShooterState.WaitingAfterShoot;
        stateTimer = waitTimeAfterShoot; // Configurar temporizador para la pausa
    }

     // 3) NUEVO Estado WaitingAfterShoot: Espera antes de esquivar
    private void ProcessWaitingAfterShootState()
    {
        AimAtPlayer(); // Mantener apuntado durante la espera
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            Debug.Log("[ShooterEnemy] WaitAfterShoot Complete. Transitioning to Dodging.");
            currentState = ShooterState.Dodging;
            // Calcular el objetivo del esquive AHORA, antes de empezar a moverse
            CalculateDodgeTarget();
            stateTimer = dodgeDuration; // Configurar temporizador para la duración del esquive
        }
    }

    // 4) Estado Dodging: se mueve lateralmente (cálculo movido a helper)
    private void ProcessDodgingState()
    {
        // Moverse hacia el objetivo calculado
        transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, speed * Time.deltaTime);

        // Contar tiempo del movimiento de esquive
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
             Debug.Log("[ShooterEnemy] Dodging Complete. Transitioning to WaitingAfterDodge.");
             currentState = ShooterState.WaitingAfterDodge;
             stateTimer = waitTimeAfterDodge; // Configurar temporizador para la pausa después de esquivar
        }
    }

     // 5) NUEVO Estado WaitingAfterDodge: Espera después de esquivar y decide siguiente acción
    private void ProcessWaitingAfterDodgeState()
    {
        AimAtPlayer(); // Mantener apuntado durante la espera
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            // Se acabó la espera, comprobar cuántos disparos van
            if (shotsFiredCount < MAX_SHOTS_BEFORE_KAMIKAZE)
            {
                // Aún no ha disparado 3 veces, volver a disparar
                Debug.Log($"[ShooterEnemy] WaitAfterDodge Complete (Shots: {shotsFiredCount}). Transitioning back to Shooting.");
                currentState = ShooterState.Shooting;
            }
            else
            {
                // Ya disparó 3 veces, iniciar ataque Kamikaze
                Debug.Log("[ShooterEnemy] WaitAfterDodge Complete (Max shots reached). Transitioning to Kamikaze!");
                currentState = ShooterState.Kamikaze;
            }
        }
    }

    // 6) NUEVO Estado Kamikaze: Se lanza hacia el jugador
    private void ProcessKamikazeState()
    {
        AimAtPlayer(); // Seguir mirando al jugador mientras carga

        // Moverse directamente hacia el jugador a doble velocidad
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * (speed * 2f) * Time.deltaTime;

        // La colisión con el jugador (manejada en OnCollisionEnter2D) lo destruirá
    }


    // --- Métodos Helper (sin cambios o con leves ajustes) ---

    /// <summary>
    /// Calcula el punto objetivo para el movimiento lateral de esquive.
    /// </summary>
    private void CalculateDodgeTarget()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 perpendicularLeft = new Vector3(-toPlayer.y, toPlayer.x, 0f);

        if (isFirstDodge) {
            dodgeDirectionIsLeft = (Random.value < 0.5f);
            isFirstDodge = false;
        } else {
            dodgeDirectionIsLeft = !dodgeDirectionIsLeft;
        }

        Vector3 dodgeDirectionVector = dodgeDirectionIsLeft ? perpendicularLeft : -perpendicularLeft;
        // Ajustar distancia de esquive si es necesario (ej: * 3f en lugar de * 2f)
        dodgeTarget = transform.position + dodgeDirectionVector * 3f;
         Debug.Log($"[ShooterEnemy] Calculating Dodge Target. Direction Left: {dodgeDirectionIsLeft}. Target: {dodgeTarget}");
    }


    private void AimAtPlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        // Evitar error si está justo encima
        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // Offset de -90 para que 'up' mire al target
            // Usar Slerp para una rotación más suave (opcional)
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // 10f es velocidad de rotación
            // O mantener la rotación instantánea:
            // transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void ShootProjectile()
    {
        if (shooterProjectilePrefab == null || player == null) return;

        // AimAtPlayer(); // Ya nos aseguramos de llamar Aim en los estados de espera

        Vector3 spawnPos = transform.position + transform.up * 0.5f;
        GameObject projObj = Instantiate(shooterProjectilePrefab, spawnPos, transform.rotation);

        SpriteRenderer sr = projObj.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.color = enemyColor; }

        EnemyProjectile enemyProj = projObj.GetComponent<EnemyProjectile>();
        if (enemyProj != null) { enemyProj.bulletColor = enemyColor; }

        Rigidbody2D rb = projObj.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.linearVelocity = transform.up * projectileSpeed;
        } else {
             Debug.LogWarning("[ShooterEnemy] Proyectil no tiene Rigidbody2D. Usando corutina de movimiento.");
             StartCoroutine(MoveProjectile(projObj.transform, transform.up, projectileSpeed)); // Fallback
        }
    }

    // Corutina de fallback (sin cambios)
    private IEnumerator MoveProjectile(Transform projectileTransform, Vector3 direction, float speed) {
         float lifetime = 5f; // Añadir un tiempo de vida a la corutina
         float timer = 0f;
        while (projectileTransform != null && timer < lifetime) {
            projectileTransform.position += direction * speed * Time.deltaTime;
             timer += Time.deltaTime;
            yield return null;
        }
         if (projectileTransform != null) Destroy(projectileTransform.gameObject); // Destruir si sigue existiendo
    }


    // Colisiones (sin cambios)
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
            if (playerHealth != null) { playerHealth.TakeDamage(); }
            if (CameraShake.Instance != null) { CameraShake.Instance.ShakeCamera(); }
            DestroyShooterEnemy();
        }
    }

    // Feedback de daño (sin cambios)
    private IEnumerator DamageFeedback()
    {
        if (spriteRenderer == null) yield break;
        Color originalColor = spriteRenderer.color;
        Color flashColor = Color.white;
        spriteRenderer.color = flashColor; yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = originalColor; yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = flashColor; yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = originalColor;
        Vector3 originalPosition = transform.position; float shakeDuration = 0.15f; float elapsedTime = 0f; float magnitude = 0.1f;
        while (elapsedTime < shakeDuration) {
            float x = Random.Range(-1f, 1f) * magnitude; float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = originalPosition + new Vector3(x, y, 0f);
            elapsedTime += Time.deltaTime; yield return null;
        }
        transform.position = originalPosition;
    }

    // Destrucción (sin cambios)
    private void DestroyShooterEnemy()
    {
        if (!gameObject.activeInHierarchy) return; // Usar activeInHierarchy es más robusto
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(150);
        EnemyCoinDrop coinDrop = GetComponent<EnemyCoinDrop>(); if (coinDrop != null) { coinDrop.TryDropCoins(); }
        if (explosionPrefab != null) {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null) { var main = ps.main; main.startColor = new ParticleSystem.MinMaxGradient(enemyColor); }
        }
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) { SlowMotion slow = pObj.GetComponent<SlowMotion>(); if (slow != null) slow.AddSlowMotionCharge(); }
        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }
}