using UnityEngine;

/// <summary>
/// Orbe individual del cuerpo del Zuma Boss.
/// Cada orbe tiene un color y 1 HP.
/// Al ser impactado por un proyectil del jugador con color match → se destruye
/// y notifica al Controller para retroceder la cadena.
///
/// Mismatch: el proyectil del jugador lo atraviesa (no ricochet).
/// Razón: con 20-40 orbes de colores mixtos, el ricochet sería frustrante.
/// El jugador necesita poder apuntar a orbes detrás de otros de color diferente.
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
    [Tooltip("Prefab de explosión al ser destruido (opcional, usa el del Controller si es null).")]
    public GameObject explosionPrefab;

    // --- Estado interno ---
    private Color orbColor;
    private ZumaBossController controller;
    private SpriteRenderer sr;
    private bool isDestroyed = false;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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
        }
        // Mismatch: el proyectil lo atraviesa sin efecto
        // (no hay ricochet — diseño intencional para Zuma Boss)
    }

    /*═══════════════════  DESTRUCCIÓN  ═══════════════════*/

    void DestroySelf()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Notificar al Controller
        if (controller != null)
        {
            controller.OnOrbDestroyed(this);
        }

        // Explosión visual
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
