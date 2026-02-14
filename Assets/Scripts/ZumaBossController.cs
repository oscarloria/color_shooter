using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controlador principal del Zuma Boss.
/// Genera un camino en espiral, spawn la cadena (cabeza + orbes),
/// avanza la cadena hacia el jugador, maneja retrocesión y fases.
///
/// Arquitectura:
/// - El Controller posiciona todo: cabeza y orbes cada frame
/// - Los orbes y la cabeza manejan sus propias colisiones y notifican al Controller
/// - 3 fases de dificultad creciente
///
/// Colocar este script en un GameObject vacío en la escena.
/// </summary>
public class ZumaBossController : MonoBehaviour
{
    [Header("═══ Camino Espiral ═══")]
    [Tooltip("Radio máximo de la espiral (borde exterior).")]
    public float maxRadius = 7f;
    [Tooltip("Radio mínimo de la espiral (punto más cercano al jugador).")]
    public float minRadius = 0.8f;
    [Tooltip("Cantidad de vueltas de la espiral.")]
    public int spiralTurns = 3;

    [Header("═══ Cadena ═══")]
    [Tooltip("Distancia entre orbes a lo largo del camino.")]
    public float orbSpacing = 0.7f;
    [Tooltip("Cuánto retrocede la cabeza al destruir un orbe (en unidades de camino).")]
    public float retrocessionAmount = 0.6f;

    [Header("═══ Entrada ═══")]
    [Tooltip("Velocidad a la que la serpiente entra al escenario (desplegándose).")]
    public float entrySpeed = 8f;

    [Header("═══ Fase 1 ═══")]
    public int phase1OrbCount = 20;
    public float phase1BaseSpeed = 1.5f;
    [Tooltip("Multiplicador de velocidad cuando quedan 0 orbes (sobre baseSpeed).")]
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
    [Tooltip("HP de la cabeza en cada fase.")]
    public int headHP = 20;
    [Tooltip("Intervalo de cambio de color de la cabeza (segundos).")]
    public float headColorChangeInterval = 3f;
    [Tooltip("Duración del stagger al recibir daño la cabeza (segundos).")]
    public float headStaggerDuration = 0.3f;

    [Header("═══ Prefabs ═══")]
    [Tooltip("Prefab del orbe del cuerpo (círculo con ZumaBossOrb).")]
    public GameObject orbPrefab;
    [Tooltip("Prefab de la cabeza (triángulo con ZumaBossHead).")]
    public GameObject headPrefab;
    [Tooltip("Prefab de explosión (reutilizar el existente).")]
    public GameObject explosionPrefab;

    [Header("═══ Transiciones ═══")]
    [Tooltip("Pausa en segundos entre fases.")]
    public float pauseBetweenPhases = 3f;
    [Tooltip("Puntos de score al derrotar al boss completo.")]
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

        if (chainPaused) return;

        AdvanceChain();
        PositionEntities();
        CheckGameOver();
    }

    /*═══════════════════  ENTRADA DE LA SERPIENTE  ═══════════════════*/

    /// <summary>
    /// Durante la entrada, la cabeza avanza rápido y los orbes se despliegan
    /// progresivamente detrás de ella. Los orbes que aún no caben en el camino
    /// permanecen ocultos en el punto de entrada.
    /// </summary>
    void UpdateEntering()
    {
        headDistance += entrySpeed * Time.deltaTime;

        PositionEntities();

        // ¿Ya hay suficiente camino para toda la cadena?
        if (headDistance >= fullEntryDistance)
        {
            headDistance = fullEntryDistance;
            isEntering = false;
            PositionEntities();

            Debug.Log("ZumaBoss: Entrada completa. ¡Comienza el combate!");
        }
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

        // La cabeza empieza en el borde exterior del camino
        headDistance = 0f;
        fullEntryDistance = (phaseOrbTotal + 1) * orbSpacing;
        isEntering = true;

        // --- Spawn Cabeza ---
        Vector2 headWorldPos = GetPositionAtDistance(0f) + playerPos;
        GameObject headObj = Instantiate(headPrefab, headWorldPos, Quaternion.identity, transform);
        activeHead = headObj.GetComponent<ZumaBossHead>();

        if (activeHead != null)
        {
            activeHead.Initialize(this, headHP, phaseHeadColors, headColorChangeInterval);
        }

        // --- Spawn Orbes (todos de una vez, pero se posicionan por PositionEntities) ---
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

        Debug.Log($"ZumaBoss: Cadena creada. {activeOrbs.Count} orbes. Desplegándose...");
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
                // Este orbe aún no ha "entrado" al camino — ocultar
                activeOrbs[i].gameObject.SetActive(false);
            }
            else
            {
                // Orbe visible y posicionado en el camino
                if (!activeOrbs[i].gameObject.activeSelf)
                    activeOrbs[i].gameObject.SetActive(true);

                Vector2 orbPos = GetPositionAtDistance(orbDist) + playerPos;
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

    /*═══════════════════  DEBUG: DIBUJAR CAMINO EN EDITOR  ═══════════════════*/

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