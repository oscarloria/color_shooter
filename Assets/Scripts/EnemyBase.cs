using UnityEngine;
using System.Collections;

/// <summary>
/// Clase base abstracta para todos los enemigos de Luminity.
/// Centraliza: color, velocidad, vida, muerte, registro en EnemyManager,
/// colisiones con proyectiles/jugador, explosión y carga de SlowMotion.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Configuración Base del Enemigo")]
    public Color enemyColor = Color.white;
    public float speed = 2f;
    public int maxHealth = 1;
    public int scoreValue = 100;
    public GameObject explosionPrefab;

    // --- Estado interno ---
    public int CurrentHealth { get; protected set; }
    protected Transform player;
    protected SpriteRenderer sr;
    protected bool isDead = false;

    /*═══════════════════  CICLO DE VIDA  ═══════════════════*/

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        CurrentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        sr = GetComponent<SpriteRenderer>();
        ApplyVisualColor();
    }

    /*═══════════════════  REGISTRO EN ENEMYMANAGER  ═══════════════════*/

    protected virtual void OnEnable()
    {
        EnemyManager.Instance?.RegisterEnemy(this);
    }

    protected virtual void OnDisable()
    {
        EnemyManager.Instance?.UnregisterEnemy(this);
    }

    /*═══════════════════  APARIENCIA  ═══════════════════*/

    /// <summary>
    /// Aplica el color visual al sprite. Override si tu enemigo
    /// necesita lógica diferente (ej: TankEnemy cuyo cuerpo es blanco).
    /// </summary>
    public virtual void ApplyVisualColor()
    {
        if (sr != null) sr.color = enemyColor;
    }

    /*═══════════════════  SISTEMA DE DAÑO  ═══════════════════*/

    /// <summary>
    /// Recibe daño. Reduce HP y llama a Die() si llega a 0.
    /// Override para agregar lógica extra (fases del boss, feedback, etc).
    /// </summary>
    public virtual void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            isDead = true;
            Die();
        }
        else
        {
            OnDamageTaken();
        }
    }

    /// <summary>
    /// Llamado cuando el enemigo recibe daño pero NO muere.
    /// Override para agregar feedback visual (flash blanco, shake, etc).
    /// </summary>
    protected virtual void OnDamageTaken() { }

    /// <summary>
    /// Lógica de muerte: score, coins, explosión, slow motion charge, destroy.
    /// Override si necesitas lógica adicional (ej: desactivar antes de destruir).
    /// </summary>
    protected virtual void Die()
    {
        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();
        SpawnExplosion(enemyColor);
        GiveSlowMotionCharge();
        Destroy(gameObject);
    }

    /*═══════════════════  COLISIONES  ═══════════════════*/

    /// <summary>
    /// Manejo base de colisiones sólidas (Collision2D).
    /// Override completo si tu enemigo maneja colisiones de forma diferente.
    /// </summary>
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            HandleProjectileHit(collision);
            return;
        }

        if (collision.collider.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
    }

    /// <summary>
    /// Lógica cuando un proyectil del jugador impacta.
    /// Comportamiento base: si el color coincide → TakeDamage y destruir proyectil.
    /// Si no coincide, la física se encarga del rebote.
    /// </summary>
    protected virtual void HandleProjectileHit(Collision2D collision)
    {
        Projectile p = collision.collider.GetComponent<Projectile>();
        if (p != null && p.projectileColor == enemyColor)
        {
            Destroy(collision.gameObject);
            TakeDamage(1);
        }
    }

    /// <summary>
    /// Lógica cuando el jugador colisiona directamente con el enemigo.
    /// Comportamiento base: dañar jugador + camera shake + morir.
    /// </summary>
    protected virtual void HandlePlayerCollision(Collision2D collision)
    {
        collision.collider.GetComponent<PlayerHealth>()?.TakeDamage();
        CameraShake.Instance?.ShakeCamera();

        if (!isDead)
        {
            isDead = true;
            Die();
        }
    }

    /*═══════════════════  HELPERS COMPARTIDOS  ═══════════════════*/

    /// <summary>Mueve al enemigo hacia el jugador a la velocidad actual.</summary>
    protected void MoveTowardsPlayer()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    /// <summary>Instancia la explosión con el color indicado.</summary>
    protected void SpawnExplosion(Color color)
    {
        if (explosionPrefab == null) return;
        GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (boom.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = color;
        }
    }

    /// <summary>Otorga carga de slow motion al jugador.</summary>
    protected void GiveSlowMotionCharge()
    {
        GameObject.FindGameObjectWithTag("Player")
                  ?.GetComponent<SlowMotion>()
                  ?.AddSlowMotionCharge();
    }

    /// <summary>
    /// Coroutine genérica de flash blanco para feedback de daño.
    /// Puede ser usada por cualquier hijo que la necesite.
    /// </summary>
    protected IEnumerator FlashWhite(float duration = 0.05f)
    {
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(duration);
        if (sr != null) sr.color = original;
    }
}
