using UnityEngine;
using System.Collections;

/// <summary>
/// Boss del Escenario 1: "Pulse"
///
/// Un corazón energético que late: se expande (vulnerable, rojo) y se contrae
/// (invulnerable, blanco). Al contraerse libera una onda 360° de proyectiles.
/// Se mueve en péndulo horizontal en la parte superior de la pantalla.
///
/// Intro: Slide blanco → pausa → vibración + coloración → pausa → latidos seguros en rojo → péndulo ramp-up
///
/// 3 fases por HP: péndulo más rápido/amplio, ciclo más rápido, Fase 3 onda doble.
///
/// Setup:
/// - SpriteRenderer (círculo)
/// - CircleCollider2D (IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PulseBoss : MonoBehaviour
{
    [Header("═══ Posición ═══")]
    [Tooltip("Altura Y respecto al jugador (parte superior de la pantalla).")]
    public float heightAbovePlayer = 4f;

    [Header("═══ Color ═══")]
    public Color bossColor = Color.red;

    [Header("═══ HP ═══")]
    public int maxHP = 28;
    public float phase2Threshold = 0.66f;
    public float phase3Threshold = 0.33f;

    [Header("═══ Péndulo ═══")]
    [Tooltip("Distancia horizontal máxima del centro en cada fase.")]
    public float pendulumRangePhase1 = 2f;
    public float pendulumRangePhase2 = 3.5f;
    public float pendulumRangePhase3 = 5f;

    [Tooltip("Velocidad del péndulo (ciclos/segundo) en cada fase.")]
    public float pendulumSpeedPhase1 = 0.4f;
    public float pendulumSpeedPhase2 = 0.6f;
    public float pendulumSpeedPhase3 = 0.9f;

    [Header("═══ Latido (ciclo) ═══")]
    [Tooltip("Duración total de un ciclo (contracción + expansión) por fase.")]
    public float beatCycleDurationPhase1 = 3.0f;
    public float beatCycleDurationPhase2 = 2.5f;
    public float beatCycleDurationPhase3 = 2.0f;

    [Tooltip("Proporción del ciclo que es vulnerable (expansión). Ej: 0.6 = 60% vulnerable.")]
    public float vulnerableRatio = 0.6f;

    [Header("═══ Escala (latido visual) ═══")]
    [Tooltip("Escala cuando está contraído (invulnerable).")]
    public float contractedScale = 0.6f;
    [Tooltip("Escala cuando está expandido (vulnerable).")]
    public float expandedScale = 1.4f;

    [Header("═══ Proyectiles (onda) ═══")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;

    [Tooltip("Proyectiles por onda en cada fase.")]
    public int waveCountPhase1 = 8;
    public int waveCountPhase2 = 12;
    public int waveCountPhase3 = 12;

    [Tooltip("Delay entre la primera y segunda onda en Fase 3.")]
    public float doubleWaveDelay = 0.3f;

    [Header("═══ Telegrafía ═══")]
    [Tooltip("Duración del brillo pre-onda.")]
    public float telegraphDuration = 0.3f;

    [Header("═══ Feedback ═══")]
    public float damageFlashDuration = 0.1f;
    public float phaseTransitionShakeDuration = 0.5f;
    public float phaseTransitionShakeMagnitude = 0.15f;
    public float phaseTransitionPause = 1.0f;
    [Tooltip("Magnitud del temblor cuando está expandido/vulnerable.")]
    public float vulnerableTrembleMagnitude = 0.03f;

    [Header("═══ Cambio de Estado ═══")]
    [Tooltip("Duración de la vibración antes de cada cambio de estado.")]
    public float stateChangeShakeDuration = 0.25f;
    [Tooltip("Magnitud de la vibración antes de cada cambio de estado.")]
    public float stateChangeShakeMagnitude = 0.06f;

    [Header("═══ Intro ═══")]
    [Tooltip("Duración del slide desde fuera de pantalla.")]
    public float introSlideDuration = 1.5f;
    [Tooltip("Pausa después de llegar a posición (blanco, quieto).")]
    public float introPauseDuration = 0.5f;
    [Tooltip("Duración de la vibración + coloración.")]
    public float introVibrateDuration = 0.6f;
    [Tooltip("Magnitud de la vibración durante la coloración.")]
    public float introVibrateMagnitude = 0.08f;
    [Tooltip("Pausa dramática después de colorearse.")]
    public float introDramaticPause = 0.5f;
    [Tooltip("Cantidad de latidos seguros en rojo (sin proyectiles).")]
    public int introSafeBeats = 2;
    [Tooltip("Duración de cada latido seguro.")]
    public float introSafeBeatDuration = 1.2f;
    [Tooltip("Duración del ramp-up del péndulo.")]
    public float introPendulumRampDuration = 1.5f;

    [Header("═══ Ricochet (invulnerable) ═══")]
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
    private float currentPendulumRange;
    private float currentPendulumSpeed;
    private float currentBeatCycleDuration;
    private int currentWaveCount;

    private Transform player;
    private SpriteRenderer sr;
    private Collider2D col;
    private Vector3 baseScale;

    private bool bossActive = false;
    private bool isDead = false;
    private bool isPaused = false;
    private bool isInIntro = false;
    private bool isVulnerable = false;

    private float pendulumTimer = 0f;
    private Coroutine damageFlashCoroutine;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        baseScale = transform.localScale;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("PulseBoss: No se encontró el Player.");
            return;
        }

        currentHP = maxHP;
        ConfigurePhase(1);

        // Empieza blanco
        if (sr != null) sr.color = Color.white;

        StartCoroutine(RunBoss());
    }

    void Update()
    {
        if (isDead) return;
        if (isInIntro) return;
        if (!bossActive || isPaused) return;

        UpdatePendulum();
    }

    /*═══════════════════  PÉNDULO  ═══════════════════*/

    void UpdatePendulum()
    {
        if (player == null) return;

        pendulumTimer += Time.deltaTime * currentPendulumSpeed;

        float xOffset = Mathf.Sin(pendulumTimer * Mathf.PI * 2f) * currentPendulumRange;
        float yPos = player.position.y + heightAbovePlayer;

        Vector3 targetPos = new Vector3(player.position.x + xOffset, yPos, 0f);

        // Temblor sutil cuando está vulnerable
        if (isVulnerable)
        {
            targetPos += new Vector3(
                Random.Range(-1f, 1f) * vulnerableTrembleMagnitude,
                Random.Range(-1f, 1f) * vulnerableTrembleMagnitude, 0f);
        }

        transform.position = targetPos;
    }

    /*═══════════════════  INTRO (presentación)  ═══════════════════*/

    IEnumerator RunBoss()
    {
         isInIntro = true;

        Vector3 targetPos = new Vector3(player.position.x, player.position.y + heightAbovePlayer, 0f);
        Vector3 startPos = new Vector3(player.position.x, player.position.y + heightAbovePlayer + 12f, 0f);

        transform.position = startPos;
        transform.localScale = baseScale * contractedScale;

        // 1. Slide blanco — baja desde arriba
        Debug.Log("PulseBoss: Intro — Slide blanco...");
        float elapsed = 0f;
        while (elapsed < introSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introSlideDuration;
            t = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;

        // 2. Latidos en blanco: grande, pequeño, grande, pequeño...
        Debug.Log("PulseBoss: Intro — Latidos en blanco...");
        for (int i = 0; i < introSafeBeats; i++)
        {
            yield return StartCoroutine(AnimateScale(contractedScale, expandedScale, introSafeBeatDuration * 0.5f));
            yield return StartCoroutine(AnimateScale(expandedScale, contractedScale, introSafeBeatDuration * 0.5f));
        }

        // 3. Se hace grande, vibra y cambia a rojo
        Debug.Log("PulseBoss: Intro — Expansión + vibración + coloración...");
        yield return StartCoroutine(AnimateScale(contractedScale, expandedScale, introSafeBeatDuration * 0.5f));

        // Vibración + coloración simultáneas (ya expandido)
        elapsed = 0f;
        Vector3 vibrateBasePos = transform.position;

        while (elapsed < introVibrateDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * introVibrateMagnitude,
                Random.Range(-1f, 1f) * introVibrateMagnitude, 0f);
            transform.position = vibrateBasePos + offset;

            float t = elapsed / introVibrateDuration;
            if (sr != null) sr.color = Color.Lerp(Color.white, bossColor, t);

            yield return null;
        }

        if (sr != null) sr.color = bossColor;
        transform.position = vibrateBasePos;

        // 4. Grande, rojo, estático — pausa dramática
        Debug.Log("PulseBoss: Intro — Pausa dramática...");
        yield return new WaitForSeconds(introDramaticPause);

        // 5. Comienza a moverse — péndulo ramp-up
        Debug.Log("PulseBoss: Intro — Péndulo arrancando...");
        isInIntro = false;
        bossActive = true;
        pendulumTimer = 0f;

        elapsed = 0f;
        float originalSpeed = currentPendulumSpeed;
        while (elapsed < introPendulumRampDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introPendulumRampDuration;
            t = t * t;
            currentPendulumSpeed = Mathf.Lerp(0.05f, originalSpeed, t);
            yield return null;
        }
        currentPendulumSpeed = originalSpeed;

        // ¡Combate!
        Debug.Log("PulseBoss: ¡Intro completa! Comienza el combate.");
        StartCoroutine(BeatLoop());
    }

    /// <summary>
    /// Vibración durante la intro (usa posición fija, no la del péndulo).
    /// </summary>
    IEnumerator IntroStateShake(Vector3 basePos)
    {
        float elapsed = 0f;

        while (elapsed < stateChangeShakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * stateChangeShakeMagnitude,
                Random.Range(-1f, 1f) * stateChangeShakeMagnitude, 0f);
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = basePos;
    }

    IEnumerator AnimateScale(float fromScale, float toScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease in-out para que se sienta orgánico
            t = t * t * (3f - 2f * t);
            float currentScale = Mathf.Lerp(fromScale, toScale, t);
            transform.localScale = baseScale * currentScale;
            yield return null;
        }
        transform.localScale = baseScale * toScale;
    }

    /*═══════════════════  CICLO DE LATIDO  ═══════════════════*/

    IEnumerator BeatLoop()
    {
        while (!isDead)
        {
            if (isPaused) { yield return null; continue; }

            float invulnerableDuration = currentBeatCycleDuration * (1f - vulnerableRatio);
            float vulnerableDuration = currentBeatCycleDuration * vulnerableRatio;

            // === VIBRACIÓN PRE-CONTRACCIÓN ===
            yield return StartCoroutine(StateChangeShake());

            if (isDead) yield break;

            // === CONTRACCIÓN (invulnerable, blanco) ===
            isVulnerable = false;
            if (sr != null) sr.color = Color.white;

            yield return StartCoroutine(AnimateScale(expandedScale, contractedScale, invulnerableDuration * 0.5f));

            if (isDead) yield break;

            // Telegrafía pre-onda
            if (telegraphDuration > 0f)
            {
                yield return StartCoroutine(Telegraph());
                if (isDead || isPaused) continue;
            }

            // ¡ONDA de proyectiles!
            SpawnWave();

            // Fase 3: onda doble
            if (currentPhase >= 3)
            {
                yield return new WaitForSeconds(doubleWaveDelay);
                if (!isDead) SpawnWave();
            }

            // Breve pausa post-onda
            yield return new WaitForSeconds(invulnerableDuration * 0.3f);

            if (isDead) yield break;

            // === VIBRACIÓN PRE-EXPANSIÓN ===
            yield return StartCoroutine(StateChangeShake());

            if (isDead) yield break;

            // === EXPANSIÓN (vulnerable, rojo) ===
            isVulnerable = true;
            if (sr != null) sr.color = bossColor;

            yield return StartCoroutine(AnimateScale(contractedScale, expandedScale, vulnerableDuration * 0.3f));

            if (isDead) yield break;

            // Mantener expandido (ventana de ataque)
            float holdTime = vulnerableDuration * 0.7f;
            float holdElapsed = 0f;
            while (holdElapsed < holdTime && !isDead && !isPaused)
            {
                holdElapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    /*═══════════════════  ONDAS DE PROYECTILES  ═══════════════════*/

    void SpawnWave()
    {
        if (projectilePrefab == null) return;

        for (int i = 0; i < currentWaveCount; i++)
        {
            float angle = (360f / currentWaveCount) * i;
            SpawnProjectile(angle);
        }

        Debug.Log($"PulseBoss: ¡Onda! {currentWaveCount} proyectiles.");
    }

    void SpawnProjectile(float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees - 90f);

        GameObject projObj = Instantiate(projectilePrefab, spawnPos, rotation);

        if (projObj.TryGetComponent(out SpriteRenderer projSR))
            projSR.color = bossColor;

        if (projObj.TryGetComponent(out EnemyProjectile ep))
            ep.bulletColor = bossColor;

        if (projObj.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = direction * projectileSpeed;
    }

    /*═══════════════════  TELEGRAFÍA  ═══════════════════*/

    IEnumerator Telegraph()
    {
        if (sr == null || isDead) yield break;

        float elapsed = 0f;
        while (elapsed < telegraphDuration)
        {
            if (isDead) yield break;
            float intensity = elapsed / telegraphDuration;
            float pulse = Mathf.PingPong(elapsed * 15f, 1f) * intensity;
            sr.color = Color.Lerp(Color.white, bossColor, pulse * 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (sr != null && !isDead) sr.color = Color.white;
    }

    /*═══════════════════  FEEDBACK  ═══════════════════*/

    /// <summary>
    /// Vibración breve antes de cambiar de estado (invulnerable ↔ vulnerable).
    /// Pausa el péndulo durante la vibración.
    /// </summary>
    IEnumerator StateChangeShake()
    {
        bool wasBossActive = bossActive;
        bossActive = false;

        float elapsed = 0f;
        Vector3 basePos = transform.position;

        while (elapsed < stateChangeShakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * stateChangeShakeMagnitude,
                Random.Range(-1f, 1f) * stateChangeShakeMagnitude, 0f);
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = basePos;
        bossActive = wasBossActive;
    }

    /*═══════════════════  FASES  ═══════════════════*/

    void ConfigurePhase(int phase)
    {
        currentPhase = phase;

        switch (phase)
        {
            case 1:
                currentPendulumRange = pendulumRangePhase1;
                currentPendulumSpeed = pendulumSpeedPhase1;
                currentBeatCycleDuration = beatCycleDurationPhase1;
                currentWaveCount = waveCountPhase1;
                break;
            case 2:
                currentPendulumRange = pendulumRangePhase2;
                currentPendulumSpeed = pendulumSpeedPhase2;
                currentBeatCycleDuration = beatCycleDurationPhase2;
                currentWaveCount = waveCountPhase2;
                break;
            case 3:
                currentPendulumRange = pendulumRangePhase3;
                currentPendulumSpeed = pendulumSpeedPhase3;
                currentBeatCycleDuration = beatCycleDurationPhase3;
                currentWaveCount = waveCountPhase3;
                break;
        }

        Debug.Log($"PulseBoss: Fase {phase}. Péndulo: {currentPendulumRange} rango, {currentPendulumSpeed} vel. Ciclo: {currentBeatCycleDuration}s.");
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
        Debug.Log($"PulseBoss: ¡Transición a Fase {newPhase}!");

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

        SpawnWave();

        yield return new WaitForSeconds(phaseTransitionPause);

        ConfigurePhase(newPhase);
        isPaused = false;
    }

    /*═══════════════════  COLISIONES  ═══════════════════*/

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

            // Intro o invulnerable: ricochet siempre
            if (isInIntro || !isVulnerable)
            {
                DoRicochet(playerBullet, other);
                return;
            }

            // Vulnerable + match: daño
            if (playerBullet.projectileColor == bossColor)
            {
                Destroy(other.gameObject);
                TakeDamage(1);
                return;
            }

            // Vulnerable + mismatch: ricochet
            DoRicochet(playerBullet, other);
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

    void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"PulseBoss: Daño! HP: {currentHP}/{maxHP}");

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
            sr.color = isVulnerable ? bossColor : Color.white;

        damageFlashCoroutine = null;
    }

    /*═══════════════════  MUERTE  ═══════════════════*/

    void Die()
    {
        if (isDead) return;
        isDead = true;
        bossActive = false;

        Debug.Log("PulseBoss: ═══ ¡BOSS DERROTADO! ═══");
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

        Destroy(gameObject, 0.2f);
    }
}