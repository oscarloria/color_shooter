using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss del Escenario 3: "Canvas"
///
/// Un lienzo dividido en 3 secciones que el jugador debe "pintar" con los colores correctos.
/// Cada ronda presenta un patrón de colores. El jugador tiene una ventana de tiempo para
/// pintar las 3 secciones. Éxito = daño al boss. Fallo/timeout = contraataque.
///
/// 3 fases: colores disponibles aumentan, ventana se reduce, contraataque se intensifica.
/// Cada fase requiere 3 rondas exitosas para avanzar.
///
/// Movimiento: arco horizontal lento en la parte superior de la pantalla.
///
/// Setup:
/// - SpriteRenderer (fondo del boss)
/// - Rigidbody2D (Kinematic)
/// - Tag: "Enemy", Layer: "Enemy"
/// - 3 hijos con CanvasBossSection
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CanvasBoss : MonoBehaviour
{
    [Header("═══ Posición ═══")]
    public float heightAbovePlayer = 4f;

    [Header("═══ Movimiento (arco) ═══")]
    public float arcRangePhase1 = 2f;
    public float arcRangePhase2 = 3f;
    public float arcRangePhase3 = 4f;
    public float arcSpeedPhase1 = 0.3f;
    public float arcSpeedPhase2 = 0.4f;
    public float arcSpeedPhase3 = 0.5f;

    [Header("═══ Colores por Fase ═══")]
    [Tooltip("Fase 1: solo rojo y azul.")]
    public Color[] phase1Colors = new[] { Color.red, Color.blue };
    [Tooltip("Fase 2: rojo, azul, verde.")]
    public Color[] phase2Colors = new[] { Color.red, Color.blue, Color.green };
    [Tooltip("Fase 3: los 3 con posibles repeticiones.")]
    public Color[] phase3Colors = new[] { Color.red, Color.blue, Color.green };

    [Header("═══ Ventana de Puzzle ═══")]
    public float puzzleWindowPhase1 = 8f;
    public float puzzleWindowPhase2 = 6f;
    public float puzzleWindowPhase3 = 4f;

    [Header("═══ Rondas por Fase ═══")]
    public int roundsPerPhase = 3;

    [Header("═══ Contraataque ═══")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;

    [Tooltip("Proyectiles por ráfaga de contraataque en cada fase.")]
    public int counterBurstPhase1 = 6;
    public int counterBurstPhase2 = 10;
    public int counterBurstPhase3 = 12;

    [Tooltip("En Fase 3, añade onda 360° al contraataque.")]
    public int counterWave360Count = 10;

    [Tooltip("Duración del contraataque (tiempo antes de la siguiente ronda).")]
    public float counterAttackDuration = 2.5f;
    public float counterBurstDelay = 0.15f;

    [Header("═══ Feedback ═══")]
    public float successShakeDuration = 0.4f;
    public float successShakeMagnitude = 0.12f;
    public float failShakeDuration = 0.3f;
    public float failShakeMagnitude = 0.08f;

    [Header("═══ Intro ═══")]
    public float introSlideDuration = 1.5f;
    public float introPauseDuration = 0.8f;
    public float introVibrateDuration = 0.6f;
    public float introVibrateMagnitude = 0.08f;
    public float introColorTestDuration = 0.8f;
    public float introDramaticPause = 0.5f;

    [Header("═══ Colores Visuales ═══")]
    public Color emptyColor = new Color(0.16f, 0.16f, 0.23f, 1f);
    public Color dirtyColor = new Color(0.29f, 0.23f, 0.16f, 1f);
    public Color bossBodyColor = new Color(0.07f, 0.07f, 0.17f, 1f);

    [Header("═══ Prefabs ═══")]
    public GameObject explosionPrefab;

    [Header("═══ Score ═══")]
    public int scoreValue = 3000;
    public int scorePerRound = 200;

    /*═══════════════════  ESTADO INTERNO  ═══════════════════*/

    private int currentPhase = 1;
    private int roundsWon = 0;
    private float currentArcRange;
    private float currentArcSpeed;
    private float currentPuzzleWindow;
    private int currentCounterBurst;
    private Color[] currentColors;

    private Transform player;
    private SpriteRenderer sr;
    private bool bossActive = false;
    private bool isDead = false;
    private bool isPaused = false;
    private bool isInIntro = false;
    private bool isInPuzzle = false;

    private float arcTimer = 0f;

    private CanvasBossSection[] sections;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        sections = GetComponentsInChildren<CanvasBossSection>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("CanvasBoss: No se encontró el Player.");
            return;
        }

        ConfigurePhase(1);

        if (sr != null) sr.color = Color.white;

        // Esconder secciones al inicio (se revelan en la intro)
        foreach (var section in sections)
        {
            section.gameObject.SetActive(false);
        }

        StartCoroutine(RunBoss());
    }

    void Update()
    {
        if (isDead) return;
        if (isInIntro) return;
        if (!bossActive || isPaused) return;

        UpdateArc();
    }

    /*═══════════════════  MOVIMIENTO  ═══════════════════*/

    void UpdateArc()
    {
        if (player == null) return;

        arcTimer += Time.deltaTime * currentArcSpeed;

        float xOffset = Mathf.Sin(arcTimer * Mathf.PI * 2f) * currentArcRange;
        float yPos = player.position.y + heightAbovePlayer;

        transform.position = new Vector3(player.position.x + xOffset, yPos, 0f);
    }

    Vector3 GetBasePosition()
    {
        if (player == null) return transform.position;
        return new Vector3(player.position.x, player.position.y + heightAbovePlayer, 0f);
    }

    /*═══════════════════  INTRO  ═══════════════════*/

    IEnumerator RunBoss()
    {
        isInIntro = true;

        Vector3 targetPos = GetBasePosition();
        Vector3 startPos = new Vector3(targetPos.x, targetPos.y + 12f, 0f);

        transform.position = startPos;

        // 1. Slide blanco — baja desde arriba
        Debug.Log("CanvasBoss: Intro — Slide blanco...");
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

        // 2. Pausa estática
        Debug.Log("CanvasBoss: Intro — Pausa estática...");
        yield return new WaitForSeconds(introPauseDuration);

        // 3. Vibración + secciones aparecen (se "divide")
        Debug.Log("CanvasBoss: Intro — Vibración + división...");
        elapsed = 0f;
        Vector3 vibrateBasePos = transform.position;

        // Activar secciones como grises
        foreach (var section in sections)
        {
            section.gameObject.SetActive(true);
            section.SetEmpty();
            section.HideIndicator();
        }

        // Cambiar body a color de fondo
        if (sr != null) sr.color = bossBodyColor;

        while (elapsed < introVibrateDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * introVibrateMagnitude,
                Random.Range(-1f, 1f) * introVibrateMagnitude, 0f);
            transform.position = vibrateBasePos + offset;
            yield return null;
        }
        transform.position = vibrateBasePos;

        // 4. Test de colores — las secciones flashean colores aleatorios brevemente
        Debug.Log("CanvasBoss: Intro — Test de colores...");
        Color[] testColors = new[] { Color.red, Color.blue, Color.green };
        for (int i = 0; i < sections.Length && i < testColors.Length; i++)
        {
            sections[i].FlashColor(testColors[i], introColorTestDuration);
        }
        yield return new WaitForSeconds(introColorTestDuration);

        // Todo vuelve a gris
        foreach (var section in sections)
        {
            section.SetEmpty();
        }

        // 5. Pausa dramática
        Debug.Log("CanvasBoss: Intro — Pausa dramática...");
        yield return new WaitForSeconds(introDramaticPause);

        // 6. ¡Arranca!
        isInIntro = false;
        bossActive = true;
        arcTimer = 0f;

        Debug.Log("CanvasBoss: ¡Intro completa! Comienza el combate.");
        StartCoroutine(GameLoop());
    }

    /*═══════════════════  GAME LOOP  ═══════════════════*/

    IEnumerator GameLoop()
    {
        while (!isDead)
        {
            if (isPaused) { yield return null; continue; }

            // === NUEVA RONDA ===
            Debug.Log($"CanvasBoss: Ronda {roundsWon + 1}/{roundsPerPhase} — Fase {currentPhase}");

            // Generar patrón
            Color[] pattern = GeneratePattern();
            PresentPattern(pattern);

            // Breve pausa para que el jugador lea el patrón
            yield return new WaitForSeconds(1.0f);

            // Abrir ventana de puzzle
            isInPuzzle = true;
            foreach (var section in sections)
            {
                section.SetAcceptingInput(true);
            }

            // Esperar a que complete o se acabe el tiempo
            float puzzleTimer = 0f;
            bool puzzleComplete = false;

            while (puzzleTimer < currentPuzzleWindow && !puzzleComplete && !isDead)
            {
                puzzleTimer += Time.deltaTime;
                puzzleComplete = CheckAllSectionsPainted();
                yield return null;
            }

            isInPuzzle = false;
            foreach (var section in sections)
            {
                section.SetAcceptingInput(false);
            }

            if (isDead) yield break;

            // === EVALUAR RESULTADO ===
            if (puzzleComplete)
            {
                // ¡Éxito!
                Debug.Log("CanvasBoss: ¡Puzzle completado!");
                yield return StartCoroutine(OnRoundSuccess());

                roundsWon++;

                if (roundsWon >= roundsPerPhase)
                {
                    // Avanzar de fase
                    int nextPhase = currentPhase + 1;
                    if (nextPhase > 3)
                    {
                        // ¡Boss derrotado!
                        Die();
                        yield break;
                    }

                    roundsWon = 0;
                    yield return StartCoroutine(DoPhaseTransition(nextPhase));
                }
            }
            else
            {
                // Fallo — contraataque
                Debug.Log("CanvasBoss: Puzzle fallido. ¡Contraataque!");
                yield return StartCoroutine(OnRoundFail());
            }

            // Limpiar secciones para la siguiente ronda
            ResetSections();

            // Breve pausa entre rondas
            yield return new WaitForSeconds(0.8f);
        }
    }

    /*═══════════════════  PATRÓN  ═══════════════════*/

    Color[] GeneratePattern()
    {
        Color[] pattern = new Color[sections.Length];

        for (int i = 0; i < sections.Length; i++)
        {
            pattern[i] = currentColors[Random.Range(0, currentColors.Length)];
        }

        return pattern;
    }

    void PresentPattern(Color[] pattern)
    {
        for (int i = 0; i < sections.Length && i < pattern.Length; i++)
        {
            sections[i].SetRequired(pattern[i]);
            sections[i].SetEmpty();
            sections[i].ShowIndicator();
        }
    }

    void ResetSections()
    {
        foreach (var section in sections)
        {
            section.SetEmpty();
            section.HideIndicator();
        }
    }

    bool CheckAllSectionsPainted()
    {
        foreach (var section in sections)
        {
            if (!section.IsCorrectlyPainted()) return false;
        }
        return true;
    }

    /*═══════════════════  ÉXITO / FALLO  ═══════════════════*/

    IEnumerator OnRoundSuccess()
    {
        ScoreManager.Instance?.AddScore(scorePerRound);

        // Shake de éxito
        float elapsed = 0f;
        Vector3 basePos = transform.position;
        while (elapsed < successShakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * successShakeMagnitude,
                Random.Range(-1f, 1f) * successShakeMagnitude, 0f);
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = basePos;

        // Flash blanco en todas las secciones
        foreach (var section in sections)
        {
            section.FlashColor(Color.white, 0.3f);
        }
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator OnRoundFail()
    {
        // Shake de fallo
        float elapsed = 0f;
        Vector3 basePos = transform.position;
        while (elapsed < failShakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * failShakeMagnitude,
                Random.Range(-1f, 1f) * failShakeMagnitude, 0f);
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = basePos;

        // Contraataque
        yield return StartCoroutine(DoCounterAttack());
    }

    /*═══════════════════  CONTRAATAQUE  ═══════════════════*/

    IEnumerator DoCounterAttack()
    {
        if (player == null || isDead) yield break;

        // Ráfaga dirigida al jugador
        for (int i = 0; i < currentCounterBurst; i++)
        {
            if (isDead) yield break;

            Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
            float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            float spread = Random.Range(-20f, 20f);

            Color projColor = currentColors[Random.Range(0, currentColors.Length)];
            SpawnProjectile(baseAngle + spread, projColor);

            yield return new WaitForSeconds(counterBurstDelay);
        }

        // Fase 3: onda 360° adicional
        if (currentPhase >= 3)
        {
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < counterWave360Count; i++)
            {
                float angle = (360f / counterWave360Count) * i;
                Color projColor = currentColors[Random.Range(0, currentColors.Length)];
                SpawnProjectile(angle, projColor);
            }
            Debug.Log("CanvasBoss: ¡Onda 360° de contraataque!");
        }

        yield return new WaitForSeconds(counterAttackDuration);
    }

    void SpawnProjectile(float angleDegrees, Color color)
    {
        if (projectilePrefab == null) return;

        float angleRad = angleDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.8f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, angleDegrees - 90f);

        GameObject projObj = Instantiate(projectilePrefab, spawnPos, rotation);

        if (projObj.TryGetComponent(out SpriteRenderer projSR))
            projSR.color = color;

        if (projObj.TryGetComponent(out EnemyProjectile ep))
            ep.bulletColor = color;

        if (projObj.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = direction * projectileSpeed;
    }

    /*═══════════════════  FASES  ═══════════════════*/

    void ConfigurePhase(int phase)
    {
        currentPhase = phase;

        switch (phase)
        {
            case 1:
                currentArcRange = arcRangePhase1;
                currentArcSpeed = arcSpeedPhase1;
                currentPuzzleWindow = puzzleWindowPhase1;
                currentCounterBurst = counterBurstPhase1;
                currentColors = phase1Colors;
                break;
            case 2:
                currentArcRange = arcRangePhase2;
                currentArcSpeed = arcSpeedPhase2;
                currentPuzzleWindow = puzzleWindowPhase2;
                currentCounterBurst = counterBurstPhase2;
                currentColors = phase2Colors;
                break;
            case 3:
                currentArcRange = arcRangePhase3;
                currentArcSpeed = arcSpeedPhase3;
                currentPuzzleWindow = puzzleWindowPhase3;
                currentCounterBurst = counterBurstPhase3;
                currentColors = phase3Colors;
                break;
        }

        Debug.Log($"CanvasBoss: Fase {phase}. Ventana: {currentPuzzleWindow}s. Colores: {currentColors.Length}.");
    }

    IEnumerator DoPhaseTransition(int newPhase)
    {
        isPaused = true;
        Debug.Log($"CanvasBoss: ¡Transición a Fase {newPhase}!");

        // Shake
        float elapsed = 0f;
        Vector3 basePos = transform.position;
        while (elapsed < 0.5f)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * 0.15f,
                Random.Range(-1f, 1f) * 0.15f, 0f);
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = basePos;

        // Onda de transición
        for (int i = 0; i < 8; i++)
        {
            float angle = (360f / 8) * i;
            Color projColor = currentColors[Random.Range(0, currentColors.Length)];
            SpawnProjectile(angle, projColor);
        }

        yield return new WaitForSeconds(1.5f);

        ConfigurePhase(newPhase);
        isPaused = false;
    }

    /*═══════════════════  COLISIONES (body)  ═══════════════════*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();
        }
    }

    /*═══════════════════  MUERTE  ═══════════════════*/

    void Die()
    {
        if (isDead) return;
        isDead = true;
        bossActive = false;

        Debug.Log("CanvasBoss: ═══ ¡BOSS DERROTADO! ═══");
        StopAllCoroutines();

        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();

        if (explosionPrefab != null)
        {
            GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (boom.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = Color.white;
            }
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj?.GetComponent<SlowMotion>()?.AddSlowMotionCharge();

        Destroy(gameObject, 0.2f);
    }
}