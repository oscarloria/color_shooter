using UnityEngine;
using System.Collections;

/// <summary>
/// Boss del Escenario 1/2: "Sentinel"
///
/// Un enemigo grande que rota sobre su eje con dos escudos separados (arriba/abajo)
/// que bloquean todo. Tiene dos caras expuestas:
/// - Boca (triángulo): dispara proyectiles Y recibe daño (riesgo/recompensa)
/// - Espalda: recibe daño pero no dispara (ventana segura)
///
/// Intro: Entra blanco → pausa → vibración + coloración + giro 360° → pausa dramática → rotación ramp-up
///
/// 3 fases por HP: rotación acelera, disparos se intensifican.
///
/// Puede usarse solo (Escenario 1) o en par via DualSentinelManager (Escenario 2).
///
/// Prefab:
/// - Body: Collider2D (IsTrigger), recibe daño
/// - Hijo ShieldTop: SentinelShield + Collider2D (IsTrigger), ricochet
/// - Hijo ShieldBottom: SentinelShield + Collider2D (IsTrigger), ricochet
/// - Hijo Mouth: SentinelMouth + Collider2D (IsTrigger), recibe daño + dispara
///
/// Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SentinelBoss : MonoBehaviour
{
    [Header("═══ Posición ═══")]
    public float distanceFromPlayer = 5f;
    public float initialAngle = 90f;

    [Header("═══ Color ═══")]
    public Color bossColor = Color.red;

    [Header("═══ HP ═══")]
    public int maxHP = 35;
    public float phase2Threshold = 0.66f;
    public float phase3Threshold = 0.33f;

    [Header("═══ Rotación ═══")]
    public float rotationSpeedPhase1 = 45f;
    public float rotationSpeedPhase2 = 70f;
    public float rotationSpeedPhase3 = 100f;

    [Header("═══ Disparo ═══")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;
    [Tooltip("Transform hijo que indica dónde salen los proyectiles (la boca).")]
    public Transform mouthTransform;

    [Tooltip("Proyectiles por ráfaga en cada fase.")]
    public int burstCountPhase1 = 3;
    public int burstCountPhase2 = 5;
    public int burstCountPhase3 = 5;

    [Tooltip("Intervalo entre ráfagas (segundos).")]
    public float burstIntervalPhase1 = 2.0f;
    public float burstIntervalPhase2 = 1.5f;
    public float burstIntervalPhase3 = 1.0f;

    public float burstProjectileDelay = 0.12f;
    public float burstSpreadAngle = 15f;

    [Header("═══ Ataque Circular (Fase 3) ═══")]
    public int circularAttackEveryNBursts = 3;
    public int circularProjectileCount = 12;

    [Header("═══ Telegrafía ═══")]
    public float telegraphDuration = 0.4f;

    [Header("═══ Feedback ═══")]
    public float damageFlashDuration = 0.1f;
    public float phaseTransitionShakeDuration = 0.5f;
    public float phaseTransitionShakeMagnitude = 0.15f;
    public float phaseTransitionPause = 1.0f;

    [Header("═══ Intro (entrada) ═══")]
    [Tooltip("Duración del slide desde fuera de pantalla (blanco).")]
    public float introSlideDuration = 1.5f;
    [Tooltip("Pausa estática después de llegar a posición (blanco, quieto).")]
    public float introPauseDuration = 1.0f;
    [Tooltip("Magnitud de la vibración durante la coloración.")]
    public float introVibrateMagnitude = 0.08f;
    [Tooltip("Duración de la vibración + coloración + giro 360° (las 3 al mismo tiempo).")]
    public float introVibrateDuration = 0.6f;
    [Tooltip("Pausa dramática después de colorearse.")]
    public float introDramaticPause = 0.3f;
    [Tooltip("Duración del ramp-up de rotación (motor encendiéndose).")]
    public float introRotationRampDuration = 1.5f;

    [Header("═══ Ricochet (body mismatch) ═══")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    [Header("═══ Prefabs ═══")]
    public GameObject explosionPrefab;

    [Header("═══ Score ═══")]
    public int scoreValue = 2000;

    /*═══════════════════  ESTADO INTERNO  ═══════════════════*/

    private int currentHP;
    private int currentPhase = 1;
    private float currentRotationSpeed;
    private int currentBurstCount;
    private float currentBurstInterval;

    private Transform player;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool bossActive = false;
    private bool isDead = false;
    private bool isPaused = false;
    private bool isInIntro = false;

    private Coroutine damageFlashCoroutine;
    private int burstsSinceCircular = 0;

    // Callback para notificar al manager (DualSentinelManager)
    private System.Action onDefeated;

    // Referencias a hijos para la intro
    private SentinelMouth mouthScript;
    private SentinelShield[] shields;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        mouthScript = GetComponentInChildren<SentinelMouth>();
        shields = GetComponentsInChildren<SentinelShield>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("SentinelBoss: No se encontró el Player.");
            return;
        }

        currentHP = maxHP;
        ConfigurePhase(1);

        // Todo empieza blanco
        if (sr != null) sr.color = Color.white;

        StartCoroutine(RunBoss());
    }

    void Update()
    {
        if (isDead) return;

        // Durante la intro, todo se maneja en la coroutine
        if (isInIntro) return;

        if (!bossActive || isPaused) return;

        transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
        KeepPosition();
    }

    /*═══════════════════  CALLBACK PARA MANAGER  ═══════════════════*/

    /// <summary>
    /// Registra un callback que se invoca cuando este Sentinel es derrotado.
    /// Usado por DualSentinelManager para coordinar el encuentro.
    /// </summary>
    public void SetOnDefeated(System.Action callback)
    {
        onDefeated = callback;
    }

    /*═══════════════════  POSICIONAMIENTO  ═══════════════════*/

    void KeepPosition()
    {
        if (player == null) return;
        float angleRad = initialAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * distanceFromPlayer;
        transform.position = (Vector2)player.position + offset;
    }

    Vector2 GetBasePosition()
    {
        if (player == null) return transform.position;
        float angleRad = initialAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * distanceFromPlayer;
        return (Vector2)player.position + offset;
    }

    /*═══════════════════  INTRO (presentación)  ═══════════════════*/

    IEnumerator RunBoss()
    {
        isInIntro = true;

        float angleRad = initialAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        Vector2 targetPos = (Vector2)player.position + direction * distanceFromPlayer;
        Vector2 startPos = (Vector2)player.position + direction * (distanceFromPlayer + 15f);

        transform.position = startPos;

        // 1. Slide blanco — entra desde fuera de pantalla
        Debug.Log("SentinelBoss: Intro — Slide blanco...");
        float elapsed = 0f;
        while (elapsed < introSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introSlideDuration;
            t = 1f - (1f - t) * (1f - t); // ease out
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;

        // 2. Pausa estática — blanco, quieto, imponente
        Debug.Log("SentinelBoss: Intro — Pausa estática...");
        yield return new WaitForSeconds(introPauseDuration);

        // 3. Vibración + coloración + giro 360° (simultáneos)
        Debug.Log("SentinelBoss: Intro — Vibración + coloración + giro 360°...");

        // Colorear body → blanco a rojo/azul
        if (sr != null) sr.color = bossColor;

        // Colorear mouth
        if (mouthScript != null) mouthScript.Colorize();

        // Flash en los escudos
        foreach (var shield in shields)
        {
            if (shield != null) shield.FlashActivation();
        }

        // Loop simultáneo: vibración + rotación 360°
        float spinDirection = (rotationSpeedPhase1 < 0f) ? -1f : 1f;
        float spinSpeed = (360f / introVibrateDuration) * spinDirection;
        elapsed = 0f;

        while (elapsed < introVibrateDuration)
        {
            elapsed += Time.deltaTime;

            // Rotación 360°
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            // Vibración en posición
            Vector2 basePos = GetBasePosition();
            Vector2 vibration = new Vector2(
                Random.Range(-1f, 1f) * introVibrateMagnitude,
                Random.Range(-1f, 1f) * introVibrateMagnitude
            );
            transform.position = basePos + vibration;

            yield return null;
        }

        KeepPosition(); // posición limpia sin vibración

        // 4. Micro-pausa dramática — todo coloreado, quieto, tensión máxima
        Debug.Log("SentinelBoss: Intro — Pausa dramática...");
        yield return new WaitForSeconds(introDramaticPause);

        // 5. Rotación ramp-up — el motor se enciende lentamente
        Debug.Log("SentinelBoss: Intro — Motor encendiéndose...");
        isInIntro = false;
        bossActive = true;

        elapsed = 0f;
        while (elapsed < introRotationRampDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introRotationRampDuration;
            // Ease in — empieza lento, acelera
            t = t * t;
            float rampSpeed = Mathf.Lerp(5f, currentRotationSpeed, t);
            transform.Rotate(0f, 0f, rampSpeed * Time.deltaTime);
            KeepPosition();
            yield return null;
        }

        // 6. ¡Combate!
        Debug.Log("SentinelBoss: ¡Intro completa! Comienza el combate.");
        StartCoroutine(ShootingLoop());
    }

    /*═══════════════════  FASES  ═══════════════════*/

    void ConfigurePhase(int phase)
    {
        currentPhase = phase;

        switch (phase)
        {
            case 1:
                currentRotationSpeed = rotationSpeedPhase1;
                currentBurstCount = burstCountPhase1;
                currentBurstInterval = burstIntervalPhase1;
                break;
            case 2:
                currentRotationSpeed = rotationSpeedPhase2;
                currentBurstCount = burstCountPhase2;
                currentBurstInterval = burstIntervalPhase2;
                break;
            case 3:
                currentRotationSpeed = rotationSpeedPhase3;
                currentBurstCount = burstCountPhase3;
                currentBurstInterval = burstIntervalPhase3;
                break;
        }
        Debug.Log($"SentinelBoss: Fase {phase}. Rotación: {currentRotationSpeed}°/s");
    }

        /// <summary>
    /// Fuerza al boss a entrar en una fase específica inmediatamente.
    /// Usado por DualSentinelManager para enrage.
    /// </summary>
    public void ForcePhase(int phase)
    {
        ConfigurePhase(phase);
        currentPhase = phase;
        Debug.Log($"SentinelBoss: ¡Forzado a Fase {phase}!");
    }

    void CheckPhaseTransition()
    {
        float hpRatio = (float)currentHP / maxHP;

        int targetPhase = 1;
        if (hpRatio <= phase3Threshold) targetPhase = 3;
        else if (hpRatio <= phase2Threshold) targetPhase = 2;

        if (targetPhase > currentPhase)
        {
            StartCoroutine(DoPhaseTransition(targetPhase));
        }
    }

    IEnumerator DoPhaseTransition(int newPhase)
    {
        isPaused = true;
        Debug.Log($"SentinelBoss: ¡Transición a Fase {newPhase}!");

        float elapsed = 0f;
        Vector3 basePos = transform.position;
        while (elapsed < phaseTransitionShakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * phaseTransitionShakeMagnitude,
                Random.Range(-1f, 1f) * phaseTransitionShakeMagnitude, 0f);
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = basePos;

        SpawnCircularAttack();

        yield return new WaitForSeconds(phaseTransitionPause);

        ConfigurePhase(newPhase);
        isPaused = false;
    }

    /*═══════════════════  DISPARO  ═══════════════════*/

    IEnumerator ShootingLoop()
    {
        while (!isDead)
        {
            if (isPaused) { yield return null; continue; }

            yield return StartCoroutine(Telegraph());
            if (isDead || isPaused) continue;

            yield return StartCoroutine(FireBurst());
            burstsSinceCircular++;

            if (currentPhase >= 3 && burstsSinceCircular >= circularAttackEveryNBursts)
            {
                burstsSinceCircular = 0;
                yield return StartCoroutine(Telegraph());
                if (!isDead && !isPaused) SpawnCircularAttack();
            }

            float waitTime = 0f;
            while (waitTime < currentBurstInterval)
            {
                if (!isPaused) waitTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator Telegraph()
    {
        if (sr == null || isDead) yield break;
        Color originalColor = sr.color;
        float elapsed = 0f;

        while (elapsed < telegraphDuration)
        {
            if (isDead) yield break;
            float pulse = Mathf.PingPong(elapsed * 12f, 1f);
            sr.color = Color.Lerp(originalColor, Color.white, pulse * 0.6f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (sr != null && !isDead) sr.color = bossColor;
    }

    IEnumerator FireBurst()
    {
        if (mouthTransform == null || isDead) yield break;

        for (int i = 0; i < currentBurstCount; i++)
        {
            if (isDead) yield break;

            Vector2 baseDir = mouthTransform.right;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            float offsetAngle = 0f;
            if (currentBurstCount > 1)
            {
                float t = (float)i / (currentBurstCount - 1);
                offsetAngle = Mathf.Lerp(-burstSpreadAngle / 2f, burstSpreadAngle / 2f, t);
            }

            SpawnProjectile(baseAngle + offsetAngle, mouthTransform.position);
            yield return new WaitForSeconds(burstProjectileDelay);
        }
    }

    void SpawnProjectile(float angleDegrees, Vector3 spawnPos)
    {
        if (projectilePrefab == null) return;

        float angleRad = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees - 90f);
        GameObject projObj = Instantiate(projectilePrefab, spawnPos, rotation);

        if (projObj.TryGetComponent(out SpriteRenderer projSR))
            projSR.color = bossColor;

        if (projObj.TryGetComponent(out EnemyProjectile ep))
            ep.bulletColor = bossColor;

        if (projObj.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = direction * projectileSpeed;
    }

    void SpawnCircularAttack()
    {
        Vector3 center = transform.position;
        for (int i = 0; i < circularProjectileCount; i++)
        {
            float angle = (360f / circularProjectileCount) * i;
            SpawnProjectile(angle, center);
        }
        Debug.Log("SentinelBoss: ¡Ataque circular!");
    }

    /*═══════════════════  COLISIONES (BODY)  ═══════════════════*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();
            return;
        }

        if (other.CompareTag("Projectile"))
        {
            Projectile playerBullet = other.GetComponent<Projectile>();
            if (playerBullet == null) return;

            // Durante intro: ricochet siempre
            if (isInIntro)
            {
                DoRicochet(playerBullet, other);
                return;
            }

            if (playerBullet.projectileColor == bossColor)
            {
                Destroy(other.gameObject);
                TakeDamage(1);
            }
            else
            {
                DoRicochet(playerBullet, other);
            }
        }
    }

    /*═══════════════════  RICOCHET  ═══════════════════*/

    void DoRicochet(Projectile playerBullet, Collider2D other)
    {
        Rigidbody2D rbPlayer = other.attachedRigidbody;
        if (rbPlayer == null) return;

        Vector2 contactNormal = Vector2.zero;
        if (col != null)
        {
            ColliderDistance2D d = Physics2D.Distance(other, col);
            if (d.isOverlapped) contactNormal = d.normal;
        }
        if (contactNormal.sqrMagnitude < 1e-6f)
            contactNormal = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

        Collider2D playerCol = playerBullet.GetComponent<Collider2D>();
        Vector2 n = contactNormal;

        if (playerCol != null && col != null)
        {
            ColliderDistance2D d = Physics2D.Distance(playerCol, col);
            if (d.isOverlapped)
            {
                n = d.normal;
                float pushOut = (-d.distance) + 0.01f;
                rbPlayer.position += n * pushOut;
            }
        }

        if (n.sqrMagnitude < 1e-6f)
            n = (rbPlayer.position - (Vector2)transform.position).normalized;

        Vector2 inVel = rbPlayer.linearVelocity;
        Vector2 outVel = Vector2.Reflect(inVel, n);

        float wantedMin = Mathf.Max(minRicochetSpeed, playerBullet.minSpeed * 1.25f);
        if (outVel.sqrMagnitude < wantedMin * wantedMin)
        {
            outVel = (outVel.sqrMagnitude < 1e-6f) ? n * wantedMin : outVel.normalized * wantedMin;
        }

        rbPlayer.linearVelocity = outVel;
        rbPlayer.position += n * postRicochetSeparation;

        if (playerCol != null && col != null)
            StartCoroutine(TemporaryIgnoreCollision(playerCol, col, postRicochetIgnoreTime));
    }

    private IEnumerator TemporaryIgnoreCollision(Collider2D a, Collider2D b, float time)
    {
        if (a == null || b == null) yield break;
        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(time);
        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }

    /*═══════════════════  DAÑO  ═══════════════════*/

    /// <summary>
    /// Recibe daño. Llamado desde el body (OnTriggerEnter2D) y desde
    /// SentinelMouth cuando un proyectil match impacta la boca.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"SentinelBoss: Daño! HP: {currentHP}/{maxHP}");

        if (damageFlashCoroutine != null)
            StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(DamageFlash());

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        CheckPhaseTransition();
    }

    IEnumerator DamageFlash()
    {
        if (sr == null) yield break;

        sr.color = Color.white;
        yield return new WaitForSeconds(damageFlashDuration);

        if (sr != null && !isDead)
            sr.color = bossColor;

        damageFlashCoroutine = null;
    }

    /*═══════════════════  MUERTE  ═══════════════════*/

    void Die()
    {
        if (isDead) return;
        isDead = true;
        bossActive = false;

        Debug.Log("SentinelBoss: ═══ ¡BOSS DERROTADO! ═══");
        StopAllCoroutines();

        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();

        if (explosionPrefab != null)
        {
            GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (boom.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = bossColor;
            }
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj?.GetComponent<SlowMotion>()?.AddSlowMotionCharge();

        onDefeated?.Invoke();

        Destroy(gameObject, 0.2f);
    }
}