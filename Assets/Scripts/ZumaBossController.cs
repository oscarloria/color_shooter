using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZumaBossController : MonoBehaviour
{
    [Header("═══ Camino Espiral ═══")]
    public float maxRadius = 7f;
    public float minRadius = 0.8f;
    public int spiralTurns = 3;

    [Header("═══ Cadena ═══")]
    public float orbSpacing = 0.7f;
    public float retrocessionAmount = 0.6f;

    [Header("═══ Entrada ═══")]
    [Tooltip("Velocidad a la que la serpiente se desenrolla (blanca).")]
    public float entrySpeed = 8f;

    [Header("═══ Intro (presentación) ═══")]
    [Tooltip("Pausa después de desenrollarse (quieta, blanca).")]
    public float introPauseDuration = 1.0f;
    [Tooltip("Magnitud de la vibración durante la coloración.")]
    public float introVibrateMagnitude = 0.08f;
    [Tooltip("Segundos entre cada orbe al colorear (cola → cabeza).")]
    public float introColorWaveDelay = 0.04f;
    [Tooltip("Pausa dramática después de colorearse.")]
    public float introDramaticPause = 0.3f;
    [Tooltip("Distancia que retrocede en el recoil.")]
    public float introRecoilDistance = 1.5f;
    [Tooltip("Duración del recoil hacia atrás.")]
    public float introRecoilDuration = 0.25f;

    [Header("═══ Fase 1 ═══")]
    public int phase1OrbCount = 20;
    public float phase1BaseSpeed = 1.5f;
    public float phase1SpeedAccel = 0.5f;

    [Header("═══ Fase 2 ═══")]
    public int phase2OrbCount = 30;
    public float phase2BaseSpeed = 2.0f;
    public float phase2SpeedAccel = 0.6f;

    [Header("═══ Fase 3 ═══")]
    public int phase3OrbCount = 40;
    public float phase3BaseSpeed = 2.5f;
    public float phase3SpeedAccel = 0.7f;

    [Header("═══ Cabeza ═══")]
    public int headHP = 20;
    public float headColorChangeInterval = 3f;
    public float headStaggerDuration = 0.3f;

    [Header("═══ Prefabs ═══")]
    public GameObject orbPrefab;
    public GameObject headPrefab;
    public GameObject explosionPrefab;

    [Header("═══ Transiciones ═══")]
    public float pauseBetweenPhases = 3f;
    public int bossScoreValue = 5000;

    /*═══════════════════  DATOS DEL CAMINO  ═══════════════════*/

    private List<Vector2> rawPathPoints = new List<Vector2>();
    private float[] pathCumulativeDistances;
    private float totalPathLength;

    /*═══════════════════  ESTADO DE LA CADENA  ═══════════════════*/

    private float headDistance;
    private List<ZumaBossOrb> activeOrbs = new List<ZumaBossOrb>();
    private ZumaBossHead activeHead;
    private Transform player;

    /*═══════════════════  ESTADO DE FASE  ═══════════════════*/

    private int currentPhase = 0;
    private int phaseOrbTotal;
    private float phaseBaseSpeed;
    private float phaseSpeedAccel;
    private Color[] phaseColors;
    private Color[] phaseHeadColors;

    private bool bossActive = false;
    private bool chainPaused = false;
    private bool bossDefeated = false;
    private bool isEntering = false;
    private bool isInIntro = false;
    private bool isVibrating = false;
    private float fullEntryDistance;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("ZumaBossController: No se encontró el Player.");
            return;
        }

        GenerateSpiralPath();
        StartCoroutine(RunBossFight());
    }

    void Update()
    {
        if (!bossActive || bossDefeated) return;

        if (isEntering)
        {
            UpdateEntering();
            return;
        }

        // Durante la intro, solo posicionar (con vibración si aplica)
        if (isInIntro)
        {
            PositionEntities();
            return;
        }

        if (chainPaused) return;

        AdvanceChain();
        PositionEntities();
        CheckGameOver();
    }

    /*═══════════════════  ENTRADA (desenrollado blanco)  ═══════════════════*/

    void UpdateEntering()
    {
        headDistance += entrySpeed * Time.deltaTime;

        PositionEntities();

        if (headDistance >= fullEntryDistance)
        {
            headDistance = fullEntryDistance;
            isEntering = false;
            PositionEntities();

            Debug.Log("ZumaBoss: Desenrollado completo. Iniciando presentación...");
        }
    }

    /*═══════════════════  SECUENCIA DE INTRO  ═══════════════════*/

    /// <summary>
    /// Secuencia completa de presentación del boss:
    /// 1. Pausa estática (blanca, quieta)
    /// 2. Vibración + onda de color (cola → cabeza)
    /// 3. Micro-pausa dramática
    /// 4. Recoil (retroceso + arranque)
    /// </summary>
    IEnumerator DoIntroSequence()
    {
        isInIntro = true;

        // 1. Pausa estática — la serpiente blanca está quieta
        Debug.Log("ZumaBoss: Intro — Pausa estática...");
        yield return new WaitForSeconds(introPauseDuration);

        // 2. Vibración + onda de color (cola → cabeza)
        Debug.Log("ZumaBoss: Intro — Vibración + onda de color...");
        isVibrating = true;

        // Colorear orbes desde la cola (último) hasta la cabeza (primero)
        for (int i = activeOrbs.Count - 1; i >= 0; i--)
        {
            if (activeOrbs[i] != null)
            {
                activeOrbs[i].Colorize();
            }
            yield return new WaitForSeconds(introColorWaveDelay);
        }

        // Colorear la cabeza al final (pasa de blanco a gris invulnerable)
        if (activeHead != null)
        {
            activeHead.OnIntroComplete();
        }

        isVibrating = false;
        PositionEntities(); // posición limpia sin vibración

        // 3. Micro-pausa dramática — todo coloreado, quieto, tensión máxima
        Debug.Log("ZumaBoss: Intro — Pausa dramática...");
        yield return new WaitForSeconds(introDramaticPause);

        // 4. Recoil — retrocede ligeramente como tomando impulso
        Debug.Log("ZumaBoss: Intro — Recoil...");
        yield return StartCoroutine(DoRecoil());

        // 5. ¡Arranca el combate!
        isInIntro = false;
        Debug.Log("ZumaBoss: ¡Intro completa! Comienza el combate.");
    }

    /// <summary>
    /// La serpiente retrocede ligeramente y luego arranca.
    /// </summary>
    IEnumerator DoRecoil()
    {
        float startDist = headDistance;
        float targetDist = Mathf.Max(0f, headDistance - introRecoilDistance);

        // Retroceder
        float elapsed = 0f;
        while (elapsed < introRecoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introRecoilDuration;
            // Ease out — desacelera al final del retroceso
            t = 1f - (1f - t) * (1f - t);
            headDistance = Mathf.Lerp(startDist, targetDist, t);
            PositionEntities();
            yield return null;
        }

        headDistance = targetDist;
        PositionEntities();
    }

    /*═══════════════════  GENERACIÓN DEL CAMINO ESPIRAL  ═══════════════════*/

    void GenerateSpiralPath()
    {
        rawPathPoints.Clear();

        int totalRawPoints = spiralTurns * 200;

        for (int i = 0; i <= totalRawPoints; i++)
        {
            float t = (float)i / totalRawPoints;
            float angle = spiralTurns * 2f * Mathf.PI * t;
            float radius = Mathf.Lerp(maxRadius, minRadius, t);

            rawPathPoints.Add(new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            ));
        }

        pathCumulativeDistances = new float[rawPathPoints.Count];
        pathCumulativeDistances[0] = 0f;

        for (int i = 1; i < rawPathPoints.Count; i++)
        {
            pathCumulativeDistances[i] = pathCumulativeDistances[i - 1]
                + Vector2.Distance(rawPathPoints[i - 1], rawPathPoints[i]);
        }

        totalPathLength = pathCumulativeDistances[pathCumulativeDistances.Length - 1];
        Debug.Log($"ZumaBoss: Camino generado. Longitud total: {totalPathLength:F1} unidades, {rawPathPoints.Count} puntos.");
    }

    Vector2 GetPositionAtDistance(float distance)
    {
        distance = Mathf.Clamp(distance, 0f, totalPathLength);

        int lo = 0, hi = pathCumulativeDistances.Length - 1;
        while (lo < hi - 1)
        {
            int mid = (lo + hi) / 2;
            if (pathCumulativeDistances[mid] <= distance) lo = mid;
            else hi = mid;
        }

        float segmentLength = pathCumulativeDistances[hi] - pathCumulativeDistances[lo];
        float t = (segmentLength > 0f)
            ? (distance - pathCumulativeDistances[lo]) / segmentLength
            : 0f;

        return Vector2.Lerp(rawPathPoints[lo], rawPathPoints[hi], t);
    }

    Vector2 GetDirectionAtDistance(float distance)
    {
        Vector2 p1 = GetPositionAtDistance(distance);
        Vector2 p2 = GetPositionAtDistance(Mathf.Min(distance + 0.2f, totalPathLength));
        Vector2 dir = (p2 - p1);
        return (dir.sqrMagnitude > 0.0001f) ? dir.normalized : Vector2.down;
    }

    /*═══════════════════  FLUJO PRINCIPAL DEL BOSS  ═══════════════════*/

    IEnumerator RunBossFight()
    {
        for (int phase = 1; phase <= 3; phase++)
        {
            currentPhase = phase;
            ConfigurePhase(phase);
            SpawnChain();

            bossActive = true;

            Debug.Log($"ZumaBoss: === FASE {phase} INICIADA === Orbes: {phaseOrbTotal}, Vel: {phaseBaseSpeed}");

            // Esperar a que termine el desenrollado
            yield return new WaitUntil(() => !isEntering);

            // Ejecutar secuencia de intro (presentación)
            yield return StartCoroutine(DoIntroSequence());

            // Esperar a que la cabeza sea derrotada
            yield return new WaitUntil(() => activeHead == null);

            bossActive = false;

            if (phase < 3)
            {
                Debug.Log($"ZumaBoss: Fase {phase} completada. Pausa de {pauseBetweenPhases}s...");
                yield return new WaitForSeconds(pauseBetweenPhases);
            }
        }

        OnBossDefeated();
    }

    void ConfigurePhase(int phase)
    {
        switch (phase)
        {
            case 1:
                phaseOrbTotal = phase1OrbCount;
                phaseBaseSpeed = phase1BaseSpeed;
                phaseSpeedAccel = phase1SpeedAccel;
                phaseColors = new[] { Color.red, Color.blue };
                phaseHeadColors = new[] { Color.red, Color.blue };
                break;
            case 2:
                phaseOrbTotal = phase2OrbCount;
                phaseBaseSpeed = phase2BaseSpeed;
                phaseSpeedAccel = phase2SpeedAccel;
                phaseColors = new[] { Color.green, Color.yellow };
                phaseHeadColors = new[] { Color.green, Color.yellow };
                break;
            case 3:
                phaseOrbTotal = phase3OrbCount;
                phaseBaseSpeed = phase3BaseSpeed;
                phaseSpeedAccel = phase3SpeedAccel;
                phaseColors = new[] { Color.red, Color.blue, Color.green, Color.yellow };
                phaseHeadColors = new[] { Color.red, Color.blue, Color.green, Color.yellow };
                break;
        }
    }

    /*═══════════════════  SPAWN DE LA CADENA  ═══════════════════*/

    void SpawnChain()
    {
        CleanupChain();

        Vector2 playerPos = player.position;

        headDistance = 0f;
        fullEntryDistance = (phaseOrbTotal + 1) * orbSpacing;
        isEntering = true;
        isInIntro = false;
        isVibrating = false;

        // --- Spawn Cabeza (blanca durante intro) ---
        Vector2 headWorldPos = GetPositionAtDistance(0f) + playerPos;
        GameObject headObj = Instantiate(headPrefab, headWorldPos, Quaternion.identity, transform);
        activeHead = headObj.GetComponent<ZumaBossHead>();

        if (activeHead != null)
        {
            activeHead.Initialize(this, headHP, phaseHeadColors, headColorChangeInterval);
        }

        // --- Spawn Orbes (blancos durante intro) ---
        for (int i = 0; i < phaseOrbTotal; i++)
        {
            Vector2 orbWorldPos = GetPositionAtDistance(0f) + playerPos;

            Color orbColor = phaseColors[Random.Range(0, phaseColors.Length)];

            GameObject orbObj = Instantiate(orbPrefab, orbWorldPos, Quaternion.identity, transform);
            ZumaBossOrb orb = orbObj.GetComponent<ZumaBossOrb>();

            if (orb != null)
            {
                orb.Initialize(this, orbColor);
            }

            activeOrbs.Add(orb);
        }

        Debug.Log($"ZumaBoss: Cadena creada. {activeOrbs.Count} orbes. Desenrollándose...");
    }

    void CleanupChain()
    {
        foreach (var orb in activeOrbs)
        {
            if (orb != null) Destroy(orb.gameObject);
        }
        activeOrbs.Clear();

        if (activeHead != null)
        {
            Destroy(activeHead.gameObject);
            activeHead = null;
        }
    }

    /*═══════════════════  MOVIMIENTO DE LA CADENA  ═══════════════════*/

    void AdvanceChain()
    {
        float speed = CalculateCurrentSpeed();
        headDistance += speed * Time.deltaTime;

        headDistance = Mathf.Min(headDistance, totalPathLength);
    }

    void PositionEntities()
    {
        if (player == null) return;
        Vector2 playerPos = player.position;

        // Posicionar cabeza
        if (activeHead != null)
        {
            Vector2 headPos = GetPositionAtDistance(headDistance) + playerPos;

            // Vibración durante intro
            if (isVibrating)
            {
                headPos += new Vector2(
                    Random.Range(-1f, 1f) * introVibrateMagnitude,
                    Random.Range(-1f, 1f) * introVibrateMagnitude
                );
            }

            activeHead.transform.position = headPos;

            Vector2 headDir = GetDirectionAtDistance(headDistance);
            float headAngle = Mathf.Atan2(headDir.y, headDir.x) * Mathf.Rad2Deg;
            activeHead.transform.rotation = Quaternion.Euler(0f, 0f, headAngle - 90f);
        }

        // Posicionar orbes
        for (int i = 0; i < activeOrbs.Count; i++)
        {
            if (activeOrbs[i] == null) continue;

            float orbDist = headDistance - (i + 1) * orbSpacing;

            if (orbDist < 0.01f)
            {
                activeOrbs[i].gameObject.SetActive(false);
            }
            else
            {
                if (!activeOrbs[i].gameObject.activeSelf)
                    activeOrbs[i].gameObject.SetActive(true);

                Vector2 orbPos = GetPositionAtDistance(orbDist) + playerPos;

                // Vibración durante intro
                if (isVibrating)
                {
                    orbPos += new Vector2(
                        Random.Range(-1f, 1f) * introVibrateMagnitude,
                        Random.Range(-1f, 1f) * introVibrateMagnitude
                    );
                }

                activeOrbs[i].transform.position = orbPos;
            }
        }
    }

    float CalculateCurrentSpeed()
    {
        float orbRatio = (phaseOrbTotal > 0)
            ? (float)activeOrbs.Count / phaseOrbTotal
            : 0f;

        float multiplier = 1f + phaseSpeedAccel * (1f - orbRatio);
        return phaseBaseSpeed * multiplier;
    }

    /*═══════════════════  GAME OVER CHECK  ═══════════════════*/

    void CheckGameOver()
    {
        if (headDistance >= totalPathLength - 0.1f)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (bossDefeated) return;

        Debug.Log("ZumaBoss: ¡GAME OVER! La cabeza tocó al jugador.");
        bossActive = false;

        if (ScoreManager.Instance != null)
            PlayerPrefs.SetInt("FinalScore", ScoreManager.Instance.CurrentScore);
        else
            PlayerPrefs.SetInt("FinalScore", 0);

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }

    /*═══════════════════  CALLBACKS DE ORBES Y CABEZA  ═══════════════════*/

    public void OnOrbDestroyed(ZumaBossOrb orb)
    {
        activeOrbs.Remove(orb);

        headDistance = Mathf.Max(0f, headDistance - retrocessionAmount);

        Debug.Log($"ZumaBoss: Orbe destruido. Quedan: {activeOrbs.Count}. HeadDist: {headDistance:F1}");

        if (activeOrbs.Count == 0 && activeHead != null)
        {
            Debug.Log("ZumaBoss: ¡Cuerpo eliminado! Cabeza expuesta.");
            activeHead.SetVulnerable(true);
        }
    }

    public void OnHeadDamaged()
    {
        if (!chainPaused)
        {
            StartCoroutine(DoStagger(headStaggerDuration));
        }
    }

    public void OnHeadDefeated()
    {
        Debug.Log($"ZumaBoss: ¡Cabeza de Fase {currentPhase} derrotada!");
        activeHead = null;

        if (explosionPrefab != null && player != null)
        {
            SpawnExplosion(transform.position, Color.white);
        }
    }

    IEnumerator DoStagger(float duration)
    {
        chainPaused = true;
        yield return new WaitForSeconds(duration);
        chainPaused = false;
    }

    /*═══════════════════  BOSS DERROTADO  ═══════════════════*/

    void OnBossDefeated()
    {
        bossDefeated = true;
        Debug.Log("ZumaBoss: ═══ ¡BOSS DERROTADO! ═══");

        ScoreManager.Instance?.AddScore(bossScoreValue);
        SpawnExplosion(transform.position, Color.white);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj?.GetComponent<SlowMotion>()?.AddSlowMotionCharge();

        Destroy(gameObject, 1f);
    }

    /*═══════════════════  HELPERS  ═══════════════════*/

    void SpawnExplosion(Vector3 position, Color color)
    {
        if (explosionPrefab == null) return;
        GameObject boom = Instantiate(explosionPrefab, position, Quaternion.identity);
        if (boom.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = color;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (rawPathPoints == null || rawPathPoints.Count < 2) return;

        Vector3 offset = (player != null) ? player.position : transform.position;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < rawPathPoints.Count - 1; i += 5)
        {
            int next = Mathf.Min(i + 5, rawPathPoints.Count - 1);
            Gizmos.DrawLine(
                (Vector3)rawPathPoints[i] + offset,
                (Vector3)rawPathPoints[next] + offset
            );
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector3)rawPathPoints[rawPathPoints.Count - 1] + offset, 0.3f);
    }
#endif
}