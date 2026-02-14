using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Cabeza del Zuma Boss (sprite triangular).
///
/// Estados:
/// - INVULNERABLE: mientras haya orbes en el cuerpo. Color gris/blanco.
///   Proyectiles del jugador rebotan (ricochet).
/// - VULNERABLE: cuando todos los orbes son destruidos.
///   Alterna color cada N segundos (con feedback shake/flash).
///   Color match = daño. Mismatch = ricochet.
///
/// Si toca al jugador → Game Over inmediato.
///
/// Requiere:
/// - Collider2D (IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - SpriteRenderer
/// - Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ZumaBossHead : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Color de la cabeza cuando es invulnerable.")]
    public Color invulnerableColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [Tooltip("Prefab de explosión al morir.")]
    public GameObject explosionPrefab;

    [Header("Feedback de Cambio de Color")]
    public float colorChangeShakeDuration = 0.2f;
    public float colorChangeShakeMagnitude = 0.15f;

    [Header("Feedback de Daño")]
    public float damageFlashDuration = 0.1f;

    [Header("Ricochet (mismatch e invulnerable)")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    // --- Estado interno ---
    private ZumaBossController controller;
    private SpriteRenderer sr;
    private Collider2D col;
    private int currentHP;
    private int maxHP;
    private bool isVulnerable = false;
    private bool isDead = false;

    // Color alternation
    private Color[] availableColors;
    private float colorChangeInterval;
    private int currentColorIndex = 0;
    private Color currentColor;
    private Coroutine colorCycleCoroutine;

    // Feedback
    private bool isFeedbackActive = false;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Initialize(ZumaBossController bossController, int hp, Color[] headColors, float colorInterval)
    {
        controller = bossController;
        maxHP = hp;
        currentHP = hp;
        availableColors = headColors;
        colorChangeInterval = colorInterval;

        isVulnerable = false;
        if (sr != null) sr.color = invulnerableColor;
    }

    /*═══════════════════  VULNERABILIDAD  ═══════════════════*/

    public void SetVulnerable(bool vulnerable)
    {
        isVulnerable = vulnerable;

        if (vulnerable)
        {
            currentColorIndex = 0;
            currentColor = availableColors[0];
            if (sr != null) sr.color = currentColor;

            if (colorCycleCoroutine != null) StopCoroutine(colorCycleCoroutine);
            colorCycleCoroutine = StartCoroutine(ColorCycleLoop());

            Debug.Log($"ZumaBossHead: ¡Ahora vulnerable! HP: {currentHP}. Color: {ColorToName(currentColor)}");
        }
        else
        {
            if (colorCycleCoroutine != null) StopCoroutine(colorCycleCoroutine);
            if (sr != null) sr.color = invulnerableColor;
        }
    }

    /*═══════════════════  CICLO DE COLORES  ═══════════════════*/

    IEnumerator ColorCycleLoop()
    {
        while (isVulnerable && !isDead)
        {
            yield return new WaitForSeconds(colorChangeInterval);

            if (isDead || !isVulnerable) yield break;

            currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
            currentColor = availableColors[currentColorIndex];

            StartCoroutine(ColorChangeShake());

            if (sr != null) sr.color = currentColor;

            Debug.Log($"ZumaBossHead: Color cambiado a {ColorToName(currentColor)}");
        }
    }

    IEnumerator ColorChangeShake()
    {
        float elapsed = 0f;
        Vector3 basePos = transform.position;

        while (elapsed < colorChangeShakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * colorChangeShakeMagnitude,
                Random.Range(-1f, 1f) * colorChangeShakeMagnitude,
                0f);
            transform.position = basePos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /*═══════════════════  COLISIONES  ═══════════════════*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // === JUGADOR: Game Over inmediato ===
        if (other.CompareTag("Player"))
        {
            if (controller != null)
            {
                controller.TriggerGameOver();
            }
            return;
        }

        // === PROYECTIL DEL JUGADOR ===
        if (other.CompareTag("Projectile"))
        {
            Projectile playerBullet = other.GetComponent<Projectile>();
            if (playerBullet == null) return;

            // Invulnerable: siempre ricochet
            if (!isVulnerable)
            {
                DoRicochet(playerBullet, other);
                return;
            }

            // Vulnerable + color match: daño
            if (playerBullet.projectileColor == currentColor)
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

    /// <summary>
    /// Rebota el proyectil del jugador. Misma lógica que EnemyProjectile.
    /// </summary>
    void DoRicochet(Projectile playerBullet, Collider2D other)
    {
        Rigidbody2D rbPlayer = other.attachedRigidbody;
        if (rbPlayer == null) return;

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
        Debug.Log($"ZumaBossHead: Daño recibido. HP: {currentHP}/{maxHP}");

        if (!isFeedbackActive)
        {
            StartCoroutine(DamageFlash());
        }

        if (controller != null)
        {
            controller.OnHeadDamaged();
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        if (sr == null) yield break;
        isFeedbackActive = true;

        Color previousColor = sr.color;
        sr.color = Color.white;

        yield return new WaitForSeconds(damageFlashDuration);

        if (sr != null && !isDead)
        {
            sr.color = isVulnerable ? currentColor : invulnerableColor;
        }

        isFeedbackActive = false;
    }

    /*═══════════════════  MUERTE  ═══════════════════*/

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("ZumaBossHead: ¡Cabeza destruida!");

        if (colorCycleCoroutine != null) StopCoroutine(colorCycleCoroutine);

        SpawnExplosion();

        if (controller != null)
        {
            controller.OnHeadDefeated();
        }

        Destroy(gameObject);
    }

    void SpawnExplosion()
    {
        if (explosionPrefab == null) return;

        GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (boom.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = currentColor;
        }
    }

    /*═══════════════════  HELPERS  ═══════════════════*/

    string ColorToName(Color c)
    {
        if (c == Color.red) return "ROJO";
        if (c == Color.blue) return "AZUL";
        if (c == Color.green) return "VERDE";
        if (c == Color.yellow) return "AMARILLO";
        return "DESCONOCIDO";
    }
}