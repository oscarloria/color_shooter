using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class RouletteEnemy : MonoBehaviour
{
    public enum CombatPhase { Phase1, Phase2, Phase3 }
    private CombatPhase currentPhase;

    [Header("Configuración de Fases (HP)")]
    [Tooltip("El enemigo estará en Fase 1 si HP > phase2HealthThreshold.")]
    public int phase2HealthThreshold = 60; 
    [Tooltip("El enemigo estará en Fase 2 si HP > phase3HealthThreshold Y HP <= phase2HealthThreshold.")]
    public int phase3HealthThreshold = 30;

    [Header("Feedback de Cambio de Fase")]
    public float phaseChangeSpinMultiplier = 5f;
    public float phaseChangeSpinDuration = 1f;

    [Header("Modificadores de Velocidad por Fase")]
    [Tooltip("Multiplicador para la velocidad de rotación propia en Fase 2 (ej: 1.2 para 20% más rápido).")]
    public float phase2SelfRotationMultiplier = 1.2f;
    [Tooltip("Multiplicador para la velocidad de rotación propia en Fase 3 (ej: 1.5 para 50% más rápido).")]
    public float phase3SelfRotationMultiplier = 1.5f;
    [Tooltip("Multiplicador para el período orbital en Fase 2 (ej: 0.8 para órbita 20% más rápida). Menor es más rápido.")]
    public float phase2OrbitPeriodMultiplier = 0.8f;
    [Tooltip("Multiplicador para el período orbital en Fase 3 (ej: 0.6 para órbita 40% más rápida). Menor es más rápido.")]
    public float phase3OrbitPeriodMultiplier = 0.6f;
    [Tooltip("Multiplicador para el intervalo de disparo en Fase 2 (ej: 0.8 para disparar 20% más rápido). Menor es más rápido.")]
    public float phase2ShootIntervalMultiplier = 0.8f;
    [Tooltip("Multiplicador para el intervalo de disparo en Fase 3 (ej: 0.6 para disparar 40% más rápido). Menor es más rápido.")]
    public float phase3ShootIntervalMultiplier = 0.6f;

    [Header("Movimiento orbital elíptico (Valores Base para Fase 1)")]
    public float orbitRadiusX = 7f;
    public float orbitRadiusY = 4f;
    public float orbitPeriod  = 8f; // Este será el valor para Fase 1
    public float selfRotationSpeed = 90f; // Este será el valor para Fase 1

    [Header("Ataque (Valores Base para Fase 1)")]
    public float shootInterval = 0.5f; // Este será el valor para Fase 1

    [Header("Vida")]
    public int   maxHP = 100;
    public GameObject explosionPrefab;

    [Header("Feedback de daño")]
    public float scalePopMultiplier = 1.05f;
    public float shakeDuration = 0.20f;
    public float shakeMagnitude = 0.15f;
    public float flashTime = 0.05f;

    [Header("Puntuación")]
    public int scoreValue = 500;

    int           currentHP;
    float         orbitAngle;
    float         shootTimer;
    Transform     player;
    TriangleGun[] guns; 
    SpriteRenderer sr;
    Rigidbody2D   rb;
    Vector3       baseScale;
    bool          feedbackRunning = false;
    bool          isDead = false;
    
    private float originalSelfRotationSpeed;
    private float originalOrbitPeriod;
    private float originalShootInterval;
    private bool isDoingPhaseChangeSpin = false;
    private bool hasInitializedPhases = false;
    private bool _currentOrbitIsClockwise;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        guns   = GetComponentsInChildren<TriangleGun>(true);
        if (guns.Length < 4)
        {
            Debug.LogError("RouletteEnemy espera al menos 4 TriangleGun hijos. Encontrados: " + guns.Length);
        }
        sr     = GetComponent<SpriteRenderer>();
        rb     = GetComponent<Rigidbody2D>();
        foreach (var g in guns)
        {
            if (g != null) g.SetOwner(this);
        }
    }

    void Start()
    {
        currentHP  = maxHP;
        // Guardar velocidades y tiempos originales (serán los de Fase 1)
        // Los valores públicos selfRotationSpeed, orbitPeriod y shootInterval ahora son los base.
        originalSelfRotationSpeed = selfRotationSpeed; 
        originalOrbitPeriod = orbitPeriod;
        originalShootInterval = shootInterval;
        
        shootTimer = originalShootInterval; // Inicializar con el intervalo de la Fase 1
        baseScale  = transform.localScale;
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        hasInitializedPhases = false; 
        CheckPhaseAndUpdateGuns(); 
    }

    void Update()
    {
        if (isDead || !player) return; 

        float directionMultiplier = _currentOrbitIsClockwise ? -1f : 1f; 
        
        // Usar la variable 'orbitPeriod' que ahora se actualiza por fase
        orbitAngle += directionMultiplier * (2f * Mathf.PI / orbitPeriod) * Time.deltaTime;
        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle) * orbitRadiusX,
            Mathf.Sin(orbitAngle) * orbitRadiusY);
        transform.position = player.position + (Vector3)offset;

        // Usar la variable 'selfRotationSpeed' que ahora se actualiza por fase (y por el spin temporal)
        transform.Rotate(0f, 0f, selfRotationSpeed * Time.deltaTime);

        if (shootInterval > 0f) // shootInterval también se actualiza por fase
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                foreach (var gun in guns)
                {
                    if (gun != null && gun.gameObject.activeSelf) 
                       gun.Shoot();
                }
                shootTimer += shootInterval; // Resetear timer con el intervalo actual de la fase
            }
        }
    }

    void CheckPhaseAndUpdateGuns()
    {
        CombatPhase determinedNewPhase;

        if (currentHP > phase2HealthThreshold) 
        {
            determinedNewPhase = CombatPhase.Phase1;
        }
        else if (currentHP > phase3HealthThreshold) 
        {
            determinedNewPhase = CombatPhase.Phase2;
        }
        else 
        {
            determinedNewPhase = CombatPhase.Phase3;
        }
        
        bool isActualPhaseChange = (hasInitializedPhases && currentPhase != determinedNewPhase);

        if (!hasInitializedPhases || isActualPhaseChange) 
        {
            string logPrefix;
            if (!hasInitializedPhases)
            {
                logPrefix = $"RouletteEnemy: Initializing to Phase {determinedNewPhase} at HP {currentHP}.";
            }
            else // Solo es isActualPhaseChange si hasInitializedPhases es true
            {
                 logPrefix = $"RouletteEnemy: Transitioning from Phase {currentPhase} to {determinedNewPhase} at HP {currentHP}.";
            }
            Debug.Log(logPrefix);

            currentPhase = determinedNewPhase; 
            SetGunColorsForCurrentPhase();

            if (isActualPhaseChange && !isDoingPhaseChangeSpin) 
            {
                StartCoroutine(DoPhaseChangeSpin());
            }
            
            if (!hasInitializedPhases)
            {
                hasInitializedPhases = true; 
            }
        }
    }

    void SetGunColorsForCurrentPhase()
    {
        if (guns == null || guns.Length < 4) return;

        Color red = Color.red; Color blue = Color.blue;
        Color green = Color.green; Color yellow = Color.yellow;
        string gunConfigLog = "";
        string speedConfigLog = "";

        switch (currentPhase)
        {
            case CombatPhase.Phase1:
                guns[0].gunColor = red; guns[1].gunColor = red;
                guns[2].gunColor = blue; guns[3].gunColor = blue;
                _currentOrbitIsClockwise = true; 
                gunConfigLog = "2R, 2B. Órbita: CW.";

                selfRotationSpeed = originalSelfRotationSpeed;
                orbitPeriod = originalOrbitPeriod;
                shootInterval = originalShootInterval;
                speedConfigLog = $"Rot={selfRotationSpeed:F1}, Periodo={orbitPeriod:F1}, IntDisp={shootInterval:F2}";
                break;

            case CombatPhase.Phase2:
                guns[0].gunColor = yellow; guns[1].gunColor = yellow;
                guns[2].gunColor = red; guns[3].gunColor = red;
                _currentOrbitIsClockwise = false; 
                gunConfigLog = "2Y, 2R. Órbita: CCW.";

                selfRotationSpeed = originalSelfRotationSpeed * phase2SelfRotationMultiplier;
                orbitPeriod = originalOrbitPeriod * phase2OrbitPeriodMultiplier;
                shootInterval = originalShootInterval * phase2ShootIntervalMultiplier;
                speedConfigLog = $"Rot={selfRotationSpeed:F1}, Periodo={orbitPeriod:F1}, IntDisp={shootInterval:F2}";
                break;

            case CombatPhase.Phase3:
                guns[0].gunColor = red; guns[1].gunColor = blue;
                guns[2].gunColor = green; guns[3].gunColor = yellow;
                _currentOrbitIsClockwise = true; 
                gunConfigLog = "R,B,G,Y. Órbita: CW.";

                selfRotationSpeed = originalSelfRotationSpeed * phase3SelfRotationMultiplier;
                orbitPeriod = originalOrbitPeriod * phase3OrbitPeriodMultiplier;
                shootInterval = originalShootInterval * phase3ShootIntervalMultiplier;
                speedConfigLog = $"Rot={selfRotationSpeed:F1}, Periodo={orbitPeriod:F1}, IntDisp={shootInterval:F2}";
                break;
        }
        Debug.Log($"RouletteEnemy - Config Fase {currentPhase} (HP: {currentHP}): {gunConfigLog} | {speedConfigLog}");

        foreach (TriangleGun gun in guns)
        {
            if (gun != null) gun.UpdateVisualColor();
        }
    }

    IEnumerator DoPhaseChangeSpin()
    {
        if (isDead) yield break; 
        isDoingPhaseChangeSpin = true;
        
        float fastSpinSpeed = originalSelfRotationSpeed * phaseChangeSpinMultiplier;
        Debug.Log($"RouletteEnemy: Spin por cambio de fase. Vel. rot. aumentada a: {fastSpinSpeed} deg/s.");
        selfRotationSpeed = fastSpinSpeed;

        yield return new WaitForSeconds(phaseChangeSpinDuration);

        if (!isDead && isDoingPhaseChangeSpin) 
        {
            // Restaurar a la velocidad de la fase actual, no necesariamente a originalSelfRotationSpeed
            // ya que la fase actual podría tener su propio multiplicador.
            // La lógica de SetGunColorsForCurrentPhase() ya establece la selfRotationSpeed correcta para la fase actual.
            // Así que, llamamos a esa lógica de nuevo para asegurar que la velocidad sea la correcta para la fase actual.
            // O, más simple, la fase NO ha cambiado durante el spin, así que la selfRotationSpeed ya debería estar
            // con el multiplicador de fase correcto *antes* de que este spin comenzara a modificarla.
            // El 'originalSelfRotationSpeed' es la base.
            // La velocidad de la fase actual se calcula en SetGunColorsForCurrentPhase.
            // Necesitamos restaurar a la velocidad que la fase actual dicta.

            // Re-calculamos la velocidad de rotación para la fase actual:
            switch (currentPhase)
            {
                case CombatPhase.Phase1:
                    selfRotationSpeed = originalSelfRotationSpeed;
                    break;
                case CombatPhase.Phase2:
                    selfRotationSpeed = originalSelfRotationSpeed * phase2SelfRotationMultiplier;
                    break;
                case CombatPhase.Phase3:
                    selfRotationSpeed = originalSelfRotationSpeed * phase3SelfRotationMultiplier;
                    break;
                default: // Fallback
                    selfRotationSpeed = originalSelfRotationSpeed;
                    break;
            }
            Debug.Log($"RouletteEnemy: Spin finalizado. Vel. rot. restaurada a {selfRotationSpeed} deg/s para {currentPhase}.");
        }
        isDoingPhaseChangeSpin = false;
    }

    public void ApplyDamage(int dmg)
    {
        if (isDead || dmg <= 0) return;
        currentHP = Mathf.Max(currentHP - dmg, 0);
        CheckPhaseAndUpdateGuns(); 

        if (!feedbackRunning) StartCoroutine(DamageFeedback());
        if (currentHP == 0 && !isDead)
        {
            isDead = true; 
            Die();
        }
    }
    
    IEnumerator DamageFeedback()
    {
        feedbackRunning = true;
        Color originalBodyColor = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = Color.white; 
        transform.localScale = baseScale * scalePopMultiplier;
        float elapsed = 0f;
        Vector3 positionBeforeShake = transform.position; 

        while (elapsed < shakeDuration)
        {
            Vector3 offset = new Vector3(Random.Range(-1f, 1f) * shakeMagnitude, Random.Range(-1f, 1f) * shakeMagnitude, 0f);
            transform.position = positionBeforeShake + offset; 
            yield return null;
            elapsed += Time.deltaTime;
        }
        
        if (player != null) 
        {
            Vector2 currentOrbitalOffset = new Vector2(Mathf.Cos(orbitAngle) * orbitRadiusX, Mathf.Sin(orbitAngle) * orbitRadiusY);
            transform.position = player.position + (Vector3)currentOrbitalOffset;
        } else {
            transform.position = positionBeforeShake; 
        }

        yield return new WaitForSeconds(flashTime); 
        if (sr != null) sr.color = originalBodyColor;
        transform.localScale = baseScale;
        feedbackRunning = false;
    }

    void Die()
    {
        Debug.Log("RouletteEnemy: Muriendo.");
        if (isDoingPhaseChangeSpin)
        {
            // Si muere durante el spin, asegurarse de que la corutina no interfiera más
            StopCoroutine(DoPhaseChangeSpin()); 
            isDoingPhaseChangeSpin = false;
        }
        selfRotationSpeed = originalSelfRotationSpeed; // Restaurar por si acaso

        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerObject.GetComponent<SlowMotion>()?.AddSlowMotionCharge();
        }
        Destroy(gameObject);
    }
}