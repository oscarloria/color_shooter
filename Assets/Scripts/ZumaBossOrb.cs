using UnityEngine;
using System.Collections;

/// <summary>
/// Orbe individual del cuerpo del Zuma Boss.
/// Cada orbe tiene un color y 1 HP.
/// Al ser impactado por un proyectil del jugador con color match → se destruye
/// y notifica al Controller para retroceder la cadena.
///
/// Mismatch: ricochet del proyectil del jugador (mecánica signature de Luminity).
///
/// Requiere:
/// - CircleCollider2D (IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - SpriteRenderer
/// - Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ZumaBossOrb : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Prefab de explosión al ser destruido.")]
    public GameObject explosionPrefab;

    [Header("Ricochet (mismatch)")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    // --- Estado interno ---
    private Color orbColor;
    private ZumaBossController controller;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isDestroyed = false;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// Llamado por ZumaBossController al instanciar el orbe.
    /// </summary>
    public void Initialize(ZumaBossController bossController, Color color)
    {
        controller = bossController;
        orbColor = color;

        if (sr != null) sr.color = orbColor;
    }

    /*═══════════════════  COLISIONES  ═══════════════════*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        // Solo reaccionar a proyectiles del jugador
        if (!other.CompareTag("Projectile")) return;

        Projectile playerBullet = other.GetComponent<Projectile>();
        if (playerBullet == null) return;

        // ¿Color match?
        if (playerBullet.projectileColor == orbColor)
        {
            // Match: destruir proyectil y este orbe
            Destroy(other.gameObject);
            DestroySelf();
            return;
        }

        // Mismatch: ricochet del proyectil del jugador
        Rigidbody2D rbPlayer = other.attachedRigidbody;
        if (rbPlayer != null)
        {
            // Calcular normal de contacto
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

            // Ignorar colisión temporalmente para evitar rebotes en bucle
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

    /*═══════════════════  DESTRUCCIÓN  ═══════════════════*/

    void DestroySelf()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (controller != null)
        {
            controller.OnOrbDestroyed(this);
        }

        SpawnExplosion();
        Destroy(gameObject);
    }

    void SpawnExplosion()
    {
        if (explosionPrefab == null) return;

        GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (boom.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = orbColor;
        }
    }
}