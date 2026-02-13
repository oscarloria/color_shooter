using UnityEngine;
using System.Collections;

/// <summary>
/// Proyectil dejado por el CometEnemy.
/// 
/// Ciclo de vida:
/// 1. DORMIDO — inmóvil por dormantDuration, pulsa visualmente
/// 2. HOMING  — se mueve lentamente hacia el jugador con turn rate (curvatura)
/// 3. MUERTE  — se autodestruye tras lifeTime o al ser impactado por proyectil match
///
/// Reglas de color (mismas que EnemyProjectile):
/// - Toca al jugador → daño (sin importar color seleccionado)
/// - Proyectil del jugador con color match → ambos se destruyen
/// - Proyectil del jugador con color diferente → ricochet del proyectil del jugador
///
/// Requiere:
/// - Collider2D (IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - SpriteRenderer (para color y pulso visual)
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class CometProjectile : MonoBehaviour
{
    [Header("Color")]
    [Tooltip("Color lógico del proyectil. Asignado por CometEnemy al instanciarlo.")]
    public Color bulletColor = Color.white;

    [Header("Fase Dormida")]
    [Tooltip("Tiempo en segundos que el proyectil permanece inmóvil antes de activarse.")]
    public float dormantDuration = 1.5f;
    [Tooltip("Velocidad del pulso visual durante la fase dormida.")]
    public float pulseSpeed = 3f;
    [Tooltip("Intensidad mínima del alfa durante el pulso.")]
    [Range(0.2f, 1f)]
    public float pulseMinAlpha = 0.4f;

    [Header("Fase Homing")]
    [Tooltip("Velocidad de movimiento hacia el jugador (más lento que un enemigo Normal).")]
    public float moveSpeed = 1.2f;
    [Tooltip("Velocidad de giro en grados/segundo. Menor = curvas más amplias.")]
    public float turnRate = 90f;

    [Header("Vida")]
    [Tooltip("Tiempo total de vida antes de autodestruirse (desde el instante de creación).")]
    public float lifeTime = 8f;

    [Header("Impacto FX")]
    [Tooltip("Prefab de efecto visual al impactar (opcional).")]
    public GameObject impactEffect;

    [Header("Ricochet (para mismatch con proyectil del jugador)")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    // --- Estado interno ---
    private enum ProjectileState { Dormant, Homing }
    private ProjectileState state = ProjectileState.Dormant;

    private float dormantTimer;
    private float lifeTimer;
    private float currentAngle; // dirección actual de movimiento (radianes)

    private Transform player;
    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    /*═══════════════════  CICLO DE VIDA  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Aplicar color visual
        if (sr != null) sr.color = bulletColor;

        dormantTimer = dormantDuration;
        lifeTimer = lifeTime;
        state = ProjectileState.Dormant;

        // Dirección inicial: apuntar hacia el jugador desde el inicio
        // (pero no se moverá hasta que termine la fase dormida)
        if (player != null)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            currentAngle = Mathf.Atan2(toPlayer.y, toPlayer.x);
        }
    }

    void Update()
    {
        // Autodestrucción por tiempo de vida
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            DestroySelf();
            return;
        }

        switch (state)
        {
            case ProjectileState.Dormant:
                UpdateDormant();
                break;
            case ProjectileState.Homing:
                UpdateHoming();
                break;
        }
    }

    /*═══════════════════  FASE DORMIDA  ═══════════════════*/

    /// <summary>
    /// Inmóvil, pulsa visualmente para indicar que va a activarse.
    /// </summary>
    private void UpdateDormant()
    {
        dormantTimer -= Time.deltaTime;

        // Pulso visual: oscilar el alfa del sprite
        if (sr != null)
        {
            float alpha = Mathf.Lerp(pulseMinAlpha, 1f, (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) / 2f);
            sr.color = new Color(bulletColor.r, bulletColor.g, bulletColor.b, alpha);
        }

        // ¿Terminó la fase dormida?
        if (dormantTimer <= 0f)
        {
            ActivateHoming();
        }
    }

    /// <summary>
    /// Transición de dormido a homing.
    /// </summary>
    private void ActivateHoming()
    {
        state = ProjectileState.Homing;

        // Restaurar alfa completo
        if (sr != null)
        {
            sr.color = bulletColor;
        }

        // Recalcular dirección inicial hacia el jugador
        if (player != null)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            currentAngle = Mathf.Atan2(toPlayer.y, toPlayer.x);
        }
    }

    /*═══════════════════  FASE HOMING  ═══════════════════*/

    /// <summary>
    /// Se mueve lentamente hacia el jugador con turn rate limitado,
    /// generando trayectorias curvas naturales.
    /// </summary>
    private void UpdateHoming()
    {
        if (player == null) return;

        // Calcular el ángulo deseado (directo al jugador)
        Vector2 toPlayer = (player.position - transform.position).normalized;
        float desiredAngle = Mathf.Atan2(toPlayer.y, toPlayer.x);

        // Girar gradualmente hacia el ángulo deseado (turn rate limitado)
        float maxTurn = turnRate * Mathf.Deg2Rad * Time.deltaTime;
        float angleDiff = Mathf.DeltaAngle(currentAngle * Mathf.Rad2Deg, desiredAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        currentAngle += Mathf.Clamp(angleDiff, -maxTurn, maxTurn);

        // Mover en la dirección actual
        Vector2 moveDir = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
        transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        // Rotar sprite para que mire hacia donde se mueve
        float spriteAngle = currentAngle * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, spriteAngle - 90f);
    }

    /*═══════════════════  COLISIONES (TRIGGER)  ═══════════════════*/

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Impacto con el jugador: daño sin importar color
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();
            SpawnImpactFX();
            DestroySelf();
            return;
        }

        // 2) Impacto con proyectil del jugador: regla de color
        if (other.CompareTag("Projectile"))
        {
            Projectile playerBullet = other.GetComponent<Projectile>();
            if (playerBullet == null) return;

            // Calcular normal de contacto
            Vector2 contactNormal = Vector2.zero;
            if (col != null)
            {
                ColliderDistance2D d = Physics2D.Distance(other, col);
                if (d.isOverlapped) contactNormal = d.normal;
            }
            if (contactNormal.sqrMagnitude < 1e-6f)
                contactNormal = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

            HandlePlayerProjectileHit(playerBullet, other.attachedRigidbody, contactNormal);
        }
    }

    /// <summary>
    /// Match: ambos se destruyen. Mismatch: ricochet del proyectil del jugador.
    /// </summary>
    private void HandlePlayerProjectileHit(Projectile playerBullet, Rigidbody2D rbPlayer, Vector2 contactNormal)
    {
        if (playerBullet == null) return;

        // —— MATCH: destruir ambos —— //
        if (playerBullet.projectileColor == bulletColor)
        {
            SpawnImpactFX();
            Destroy(playerBullet.gameObject);
            DestroySelf();
            return;
        }

        // —— MISMATCH: ricochet del proyectil del jugador —— //
        if (rbPlayer != null)
        {
            Collider2D playerCol = playerBullet.GetComponent<Collider2D>();
            Vector2 n = contactNormal;

            // Resolver solape
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

            // Reflejo + velocidad mínima
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
    }

    /*═══════════════════  HELPERS  ═══════════════════*/

    private IEnumerator TemporaryIgnoreCollision(Collider2D a, Collider2D b, float time)
    {
        if (a == null || b == null) yield break;
        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(time);
        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }

    private void SpawnImpactFX()
    {
        if (impactEffect == null) return;
        GameObject fx = Instantiate(impactEffect, transform.position, Quaternion.identity);
        if (fx.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = bulletColor;
        }
    }

    private void DestroySelf() => Destroy(gameObject);
}
