using UnityEngine;
using System.Collections;

/// <summary>
/// Manager del encuentro dual de Sentinel Boss (Escenario 2).
/// Coordina dos Sentinels: uno rojo a la derecha y uno azul a la izquierda.
/// El encuentro se completa cuando ambos son derrotados.
///
/// Setup:
/// - GameObject vacío en la escena
/// - Asignar los prefabs y parámetros en el Inspector
/// - Este script spawnea ambos Sentinels y coordina sus intros
/// </summary>
public class DualSentinelManager : MonoBehaviour
{
    [Header("═══ Prefab ═══")]
    [Tooltip("Prefab del SentinelBoss (se instancia dos veces).")]
    public GameObject sentinelPrefab;

    [Header("═══ Configuración — Sentinel Rojo (derecha) ═══")]
    [Tooltip("Ángulo de posición: 0 = derecha del jugador.")]
    public float redAngle = 0f;
    public float redDistance = 5f;
    public int redHP = 25;

    [Header("═══ Configuración — Sentinel Azul (izquierda) ═══")]
    [Tooltip("Ángulo de posición: 180 = izquierda del jugador.")]
    public float blueAngle = 180f;
    public float blueDistance = 5f;
    public int blueHP = 25;

    [Header("═══ Enrage ═══")]
    [Tooltip("Cuando uno muere, el otro acelera su rotación por este multiplicador.")]
    public float enrageRotationMultiplier = 1.5f;
    [Tooltip("Cuando uno muere, el otro dispara más rápido por este multiplicador.")]
    public float enrageBurstIntervalMultiplier = 0.7f;

    [Header("═══ Delay entre spawns ═══")]
    [Tooltip("Delay entre la entrada del primero y el segundo (para que no se solapen visualmente).")]
    public float spawnDelay = 0.5f;

    [Header("═══ Score ═══")]
    public int encounterBonusScore = 3000;

    /*═══════════════════  ESTADO INTERNO  ═══════════════════*/

    private SentinelBoss redSentinel;
    private SentinelBoss blueSentinel;
    private int sentinelsDefeated = 0;
    private bool encounterComplete = false;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Start()
    {
        if (sentinelPrefab == null)
        {
            Debug.LogError("DualSentinelManager: No se asignó el prefab de SentinelBoss.");
            return;
        }

        StartCoroutine(SpawnEncounter());
    }

    /*═══════════════════  SPAWN  ═══════════════════*/

    IEnumerator SpawnEncounter()
    {
        // --- Spawn Sentinel Rojo (derecha) ---
        GameObject redObj = Instantiate(sentinelPrefab, transform.position, Quaternion.identity);
        redSentinel = redObj.GetComponent<SentinelBoss>();

        if (redSentinel != null)
        {
            redSentinel.bossColor = Color.red;
            redSentinel.initialAngle = redAngle;
            redSentinel.distanceFromPlayer = redDistance;
            redSentinel.maxHP = redHP;
            redSentinel.SetOnDefeated(() => OnSentinelDefeated(redSentinel));
        }

        // Delay para que las intros no se solapen
        yield return new WaitForSeconds(spawnDelay);

        // --- Spawn Sentinel Azul (izquierda) ---
        GameObject blueObj = Instantiate(sentinelPrefab, transform.position, Quaternion.identity);
        blueObj.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        blueSentinel = blueObj.GetComponent<SentinelBoss>();

        if (blueSentinel != null)
        {
            blueSentinel.bossColor = Color.blue;
            blueSentinel.initialAngle = blueAngle;
            blueSentinel.distanceFromPlayer = blueDistance;
            blueSentinel.maxHP = blueHP;
            blueSentinel.SetOnDefeated(() => OnSentinelDefeated(blueSentinel));

            // Girar en sentido clockwise (negativo)
            blueSentinel.rotationSpeedPhase1 = -blueSentinel.rotationSpeedPhase1;
            blueSentinel.rotationSpeedPhase2 = -blueSentinel.rotationSpeedPhase2;
            blueSentinel.rotationSpeedPhase3 = -blueSentinel.rotationSpeedPhase3;
        }

        Debug.Log("DualSentinelManager: ¡Ambos Sentinels spawneados!");
    }

    /*═══════════════════  CALLBACKS  ═══════════════════*/

    void OnSentinelDefeated(SentinelBoss defeated)
    {
        sentinelsDefeated++;
        Debug.Log($"DualSentinelManager: Sentinel derrotado ({sentinelsDefeated}/2)");

        if (sentinelsDefeated >= 2)
        {
            // ¡Ambos derrotados!
            OnEncounterComplete();
            return;
        }

        // Uno sobrevive → ENRAGE
        SentinelBoss survivor = (defeated == redSentinel) ? blueSentinel : redSentinel;
        if (survivor != null)
        {
            EnrageSentinel(survivor);
        }
    }

    /// <summary>
    /// El Sentinel sobreviviente se enfurece: rota más rápido y dispara más seguido.
    /// </summary>
    void EnrageSentinel(SentinelBoss survivor)
    {
        Debug.Log("DualSentinelManager: ¡Sentinel sobreviviente entra en ENRAGE — Fase 3!");
        survivor.ForcePhase(3);
    }

    /*═══════════════════  ENCUENTRO COMPLETO  ═══════════════════*/

    void OnEncounterComplete()
    {
        if (encounterComplete) return;
        encounterComplete = true;

        Debug.Log("DualSentinelManager: ═══ ¡ENCUENTRO DUAL COMPLETADO! ═══");

        // Bonus por completar el encuentro
        ScoreManager.Instance?.AddScore(encounterBonusScore);

        Destroy(gameObject, 1f);
    }
}