using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo "Comet": entra rápido, orbita 360° al jugador dejando proyectiles,
/// y sale en espiral. No ataca directamente — su amenaza son los proyectiles
/// que deja atrás. 1 HP, rápido, recompensa la reacción rápida del jugador.
/// 
/// Flujo: Entrada recta → Órbita 360° (suelta 3 CometProjectiles) → Salida en espiral → Destroy
/// 
/// Requiere:
/// - Collider2D (para ser impactado por proyectiles del jugador)
/// - Rigidbody2D (Kinematic, para colisiones)
/// - SpriteRenderer (para color visual)
/// - Tag "Enemy"
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CometEnemy : EnemyBase
{
    [Header("Comet — Órbita")]
    [Tooltip("Margen en unidades respecto al borde de cámara para calcular el radio orbital.")]
    public float screenMargin = 1f;
    [Tooltip("Tiempo en segundos para completar la órbita de 360°.")]
    public float orbitDuration = 2f;

    [Header("Comet — Entrada/Salida")]
    [Tooltip("Velocidad de entrada en línea recta hacia el punto orbital.")]
    public float entrySpeed = 15f;
    [Tooltip("Velocidad a la que el radio de la espiral se expande durante la salida.")]
    public float exitSpiralExpansionRate = 5f;
    [Tooltip("Distancia fuera de pantalla a la que se autodestruye.")]
    public float destroyOffscreenDistance = 3f;

    [Header("Comet — Proyectiles")]
    [Tooltip("Prefab del CometProjectile que deja durante la órbita.")]
    public GameObject cometProjectilePrefab;
    [Tooltip("Cantidad de proyectiles a soltar durante la órbita.")]
    public int projectileCount = 3;

    // --- Estado interno ---
    private enum CometState { Entering, Orbiting, Exiting }
    private CometState state = CometState.Entering;

    private float orbitRadius;
    private float orbitAngle;        // ángulo actual en la órbita (radianes)
    private float orbitDirection;    // +1 = CCW, -1 = CW
    private float orbitStartAngle;   // ángulo donde comienza la órbita
    private float totalOrbitAngle;   // acumulado de ángulo recorrido

    private float nextProjectileAngle; // próximo ángulo al que soltar proyectil
    private float projectileAngleInterval; // cada cuántos grados soltar uno
    private int projectilesDropped;

    private Vector3 entryTarget;     // punto en la circunferencia orbital al que se dirige
    private float currentExitRadius; // radio creciente durante la salida

    private Rigidbody2D rb;

    /*═══════════════════  CICLO DE VIDA  ═══════════════════*/

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected override void Start()
    {
        base.Start();

        // Calcular radio orbital máximo que quepa en pantalla
        orbitRadius = CalculateMaxOrbitRadius();

        // Decidir dirección aleatoria: CW o CCW
        orbitDirection = Random.value > 0.5f ? 1f : -1f;

        // Calcular el punto de entrada en la circunferencia orbital
        // (el punto más cercano al spawn del Comet)
        CalculateEntryPoint();

        // Preparar intervalos de drop de proyectiles
        projectileAngleInterval = (2f * Mathf.PI) / projectileCount;
        nextProjectileAngle = projectileAngleInterval; // el primero se suelta tras recorrer 1 intervalo
        projectilesDropped = 0;
        totalOrbitAngle = 0f;

        state = CometState.Entering;
    }

    void Update()
    {
        if (isDead || player == null) return;

        switch (state)
        {
            case CometState.Entering:
                UpdateEntering();
                break;
            case CometState.Orbiting:
                UpdateOrbiting();
                break;
            case CometState.Exiting:
                UpdateExiting();
                break;
        }
    }

    /*═══════════════════  ESTADOS  ═══════════════════*/

    /// <summary>
    /// Entrada: moverse en línea recta hacia el punto orbital de inicio.
    /// </summary>
    private void UpdateEntering()
    {
        Vector3 dir = (entryTarget - transform.position).normalized;
        transform.position += dir * entrySpeed * Time.deltaTime;

        // Rotar el sprite para que "mire" hacia donde se mueve
        RotateTowardsMovement(dir);

        // ¿Llegamos al punto de entrada?
        if (Vector3.Distance(transform.position, entryTarget) < 0.3f)
        {
            state = CometState.Orbiting;
            // Fijar el ángulo orbital actual
            orbitAngle = orbitStartAngle;
            totalOrbitAngle = 0f;
        }
    }

    /// <summary>
    /// Órbita: girar 360° alrededor del jugador soltando proyectiles.
    /// </summary>
    private void UpdateOrbiting()
    {
        // Velocidad angular: 2π / orbitDuration
        float angularSpeed = (2f * Mathf.PI) / orbitDuration;
        float angleDelta = orbitDirection * angularSpeed * Time.deltaTime;

        orbitAngle += angleDelta;
        totalOrbitAngle += Mathf.Abs(angleDelta);

        // Posicionar en la circunferencia
        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle) * orbitRadius,
            Mathf.Sin(orbitAngle) * orbitRadius
        );
        transform.position = player.position + (Vector3)offset;

        // Rotar sprite para que apunte en la dirección tangente
        Vector2 tangent = new Vector2(
            -Mathf.Sin(orbitAngle) * orbitDirection,
             Mathf.Cos(orbitAngle) * orbitDirection
        );
        RotateTowardsMovement(tangent);

        // ¿Toca soltar proyectil?
        if (projectilesDropped < projectileCount && totalOrbitAngle >= nextProjectileAngle)
        {
            DropProjectile();
            projectilesDropped++;
            nextProjectileAngle += projectileAngleInterval;
        }

        // ¿Completamos 360°?
        if (totalOrbitAngle >= 2f * Mathf.PI)
        {
            state = CometState.Exiting;
            currentExitRadius = orbitRadius;
        }
    }

    /// <summary>
    /// Salida: espiral que se abre progresivamente hasta salir de pantalla.
    /// </summary>
    private void UpdateExiting()
    {
        // Seguir girando pero expandiendo el radio
        float angularSpeed = (2f * Mathf.PI) / orbitDuration;
        orbitAngle += orbitDirection * angularSpeed * Time.deltaTime;

        currentExitRadius += exitSpiralExpansionRate * Time.deltaTime;

        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle) * currentExitRadius,
            Mathf.Sin(orbitAngle) * currentExitRadius
        );
        transform.position = player.position + (Vector3)offset;

        // Rotar sprite
        Vector2 tangent = new Vector2(
            -Mathf.Sin(orbitAngle) * orbitDirection,
             Mathf.Cos(orbitAngle) * orbitDirection
        );
        RotateTowardsMovement(tangent);

        // ¿Ya estamos suficientemente lejos de la pantalla?
        if (IsOffScreen())
        {
            Destroy(gameObject);
        }
    }

    /*═══════════════════  PROYECTILES  ═══════════════════*/

    /// <summary>
    /// Suelta un CometProjectile en la posición actual del Comet.
    /// </summary>
    private void DropProjectile()
    {
        if (cometProjectilePrefab == null) return;

        GameObject proj = Instantiate(cometProjectilePrefab, transform.position, Quaternion.identity);

        CometProjectile cp = proj.GetComponent<CometProjectile>();
        if (cp != null)
        {
            cp.bulletColor = enemyColor;
        }
    }

    /*═══════════════════  CÁLCULOS  ═══════════════════*/

    /// <summary>
    /// Calcula el mayor radio orbital posible que quepa en la pantalla.
    /// Usa el tamaño ortográfico de la cámara.
    /// </summary>
    private float CalculateMaxOrbitRadius()
    {
        Camera cam = Camera.main;
        if (cam == null) return 5f; // fallback

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Usar el menor de los dos ejes para que el círculo quepa completo
        float maxRadius = Mathf.Min(camWidth, camHeight) - screenMargin;
        return Mathf.Max(maxRadius, 2f); // mínimo de seguridad
    }

    /// <summary>
    /// Determina el punto de la circunferencia orbital más cercano
    /// al spawn (posición actual) del Comet.
    /// </summary>
    private void CalculateEntryPoint()
    {
        if (player == null) return;

        // Dirección desde el jugador hacia el spawn del Comet
        Vector2 dirFromPlayer = ((Vector2)transform.position - (Vector2)player.position).normalized;

        // El punto en la circunferencia orbital más cercano a nuestro spawn
        orbitStartAngle = Mathf.Atan2(dirFromPlayer.y, dirFromPlayer.x);

        entryTarget = player.position + new Vector3(
            Mathf.Cos(orbitStartAngle) * orbitRadius,
            Mathf.Sin(orbitStartAngle) * orbitRadius,
            0f
        );
    }

    /// <summary>
    /// Rota el sprite para que mire en la dirección de movimiento.
    /// </summary>
    private void RotateTowardsMovement(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 porque "up" es el frente
    }

    /// <summary>
    /// Verifica si el Comet está suficientemente fuera de pantalla para destruirse.
    /// </summary>
    private bool IsOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        float margin = destroyOffscreenDistance / (cam.orthographicSize * 2f);

        return vp.x < -margin || vp.x > 1f + margin ||
               vp.y < -margin || vp.y > 1f + margin;
    }

    /*═══════════════════  OVERRIDES DE ENEMYBASE  ═══════════════════*/

    /// <summary>
    /// Al morir (disparo del jugador), no deja proyectiles pendientes.
    /// La muerte cancela el vuelo — recompensa al jugador rápido.
    /// </summary>
    protected override void Die()
    {
        // La muerte base maneja: score, coins, explosión, slow motion, destroy
        base.Die();
    }

    /// <summary>
    /// Override para usar Trigger en vez de Collision, ya que el Comet
    /// es Kinematic y se mueve programáticamente.
    /// </summary>
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // No usamos Collision2D — ver OnTriggerEnter2D
    }

    /// <summary>
    /// Usa Trigger porque el Comet es Kinematic y se mueve por código.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Proyectil del jugador
        if (other.CompareTag("Projectile"))
        {
            Projectile p = other.GetComponent<Projectile>();
            if (p != null && p.projectileColor == enemyColor)
            {
                Destroy(other.gameObject);
                TakeDamage(1);
            }
            // Mismatch: el proyectil del jugador simplemente lo atraviesa
            // (el Comet es demasiado rápido para que un rebote tenga sentido)
            return;
        }

        // Colisión con jugador: daño + morir
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();

            if (!isDead)
            {
                isDead = true;
                Die();
            }
        }
    }
}
