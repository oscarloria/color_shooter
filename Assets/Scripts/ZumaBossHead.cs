using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Cabeza del Zuma Boss (sprite triangular).
///
/// Estados:
/// - INVULNERABLE: mientras haya orbes en el cuerpo. Color gris/blanco.
///   Proyectiles del jugador la atraviesan sin efecto.
/// - VULNERABLE: cuando todos los orbes son destruidos.
///   Alterna color cada N segundos (con feedback shake/flash).
///   Solo recibe daño del color que muestra actualmente.
///   Stagger breve al recibir daño (pausa la cadena).
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
    public Color invulnerableColor = new Color(0.7f, 0.7f, 0.7f, 1f); // gris claro
    [Tooltip("Prefab de explosión al morir.")]
    public GameObject explosionPrefab;

    [Header("Feedback de Cambio de Color")]
    [Tooltip("Duración del shake al cambiar de color.")]
    public float colorChangeShakeDuration = 0.2f;
    [Tooltip("Magnitud del shake al cambiar de color.")]
    public float colorChangeShakeMagnitude = 0.15f;

    [Header("Feedback de Daño")]
    [Tooltip("Duración del flash blanco al recibir daño.")]
    public float damageFlashDuration = 0.1f;

    // --- Estado interno ---
    private ZumaBossController controller;
    private SpriteRenderer sr;
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
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// Llamado por ZumaBossController al instanciar la cabeza.
    /// </summary>
    public void Initialize(ZumaBossController bossController, int hp, Color[] headColors, float colorInterval)
    {
        controller = bossController;
        maxHP = hp;
        currentHP = hp;
        availableColors = headColors;
        colorChangeInterval = colorInterval;

        // Empezar invulnerable (gris)
        isVulnerable = false;
        if (sr != null) sr.color = invulnerableColor;
    }

    /*═══════════════════  VULNERABILIDAD  ═══════════════════*/

    /// <summary>
    /// Activa o desactiva el estado vulnerable de la cabeza.
    /// Cuando vulnerable: empieza a alternar colores.
    /// </summary>
    public void SetVulnerable(bool vulnerable)
    {
        isVulnerable = vulnerable;

        if (vulnerable)
        {
            // Empezar ciclo de colores
            currentColorIndex = 0;
            currentColor = availableColors[0];
            if (sr != null) sr.color = currentColor;

            if (colorCycleCoroutine != null) StopCoroutine(colorCycleCoroutine);
            colorCycleCoroutine = StartCoroutine(ColorCycleLoop());

            Debug.Log($"ZumaBossHead: ¡Ahora vulnerable! HP: {currentHP}. Color: {ColorToName(currentColor)}");
        }
        else
        {
            // Volver a invulnerable
            if (colorCycleCoroutine != null) StopCoroutine(colorCycleCoroutine);
            if (sr != null) sr.color = invulnerableColor;
        }
    }

    /*═══════════════════  CICLO DE COLORES  ═══════════════════*/

    /// <summary>
    /// Alterna el color de la cabeza cada N segundos con feedback visual.
    /// </summary>
    IEnumerator ColorCycleLoop()
    {
        while (isVulnerable && !isDead)
        {
            yield return new WaitForSeconds(colorChangeInterval);

            if (isDead || !isVulnerable) yield break;

            // Siguiente color
            currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
            currentColor = availableColors[currentColorIndex];

            // Feedback: shake + aplicar nuevo color
            StartCoroutine(ColorChangeShake());

            if (sr != null) sr.color = currentColor;

            Debug.Log($"ZumaBossHead: Color cambiado a {ColorToName(currentColor)}");
        }
    }

    /// <summary>
    /// Shake breve al cambiar de color para alertar al jugador.
    /// </summary>
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

        // El Controller reposicionará en el siguiente frame
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
            // Si es invulnerable, el proyectil la atraviesa
            if (!isVulnerable) return;

            Projectile playerBullet = other.GetComponent<Projectile>();
            if (playerBullet == null) return;

            // ¿Color match con el color actual de la cabeza?
            if (playerBullet.projectileColor == currentColor)
            {
                Destroy(other.gameObject);
                TakeDamage(1);
            }
            // Mismatch: el proyectil la atraviesa
        }
    }

    /*═══════════════════  DAÑO  ═══════════════════*/

    void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"ZumaBossHead: Daño recibido. HP: {currentHP}/{maxHP}");

        // Feedback visual: flash blanco
        if (!isFeedbackActive)
        {
            StartCoroutine(DamageFlash());
        }

        // Notificar al Controller para stagger
        if (controller != null)
        {
            controller.OnHeadDamaged();
        }

        // ¿Muerta?
        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Flash blanco breve al recibir daño.
    /// </summary>
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

        // Parar ciclo de colores
        if (colorCycleCoroutine != null) StopCoroutine(colorCycleCoroutine);

        // Explosión
        SpawnExplosion();

        // Notificar al Controller
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
