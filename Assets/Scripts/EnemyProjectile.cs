using UnityEngine;
using System.Collections;

/// <summary>
/// Proyectil enemigo con regla de color:
/// - MATCH (color igual al del proyectil del jugador): se destruyen ambos.
/// - MISMATCH (color distinto): el proyectil del jugador rebota (ricochet), el enemigo sigue.
///
/// Notas:
/// - Usa OnTriggerEnter2D: asegúrate de que el Collider2D del proyectil enemigo sea IsTrigger.
/// - El proyectil del jugador debe tener Rigidbody2D y Collider2D con tag "Projectile".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración de Proyectil Enemigo")]
    public Color bulletColor = Color.white;   // Asignado por el spawner (p. ej., ShooterEnemy)
    public float lifeTime = 3f;               // Autodestrucción tras X segundos
    [Tooltip("Prefab opcional de FX al impactar (se tiñe al color de la bala enemiga).")]
    public GameObject impactEffect;

    [Header("Ricochet Tuning")]
    [Tooltip("Velocidad mínima que imponemos al proyectil del jugador después del rebote.")]
    public float minRicochetSpeed = 6f;
    [Tooltip("Separación extra tras el rebote para salir del solape.")]
    public float postRicochetSeparation = 0.10f;
    [Tooltip("Tiempo que ignoramos la colisión entre esta pareja para evitar rebotes en bucle.")]
    public float postRicochetIgnoreTime = 0.08f;

    // Internos
    private float timer;
    private SpriteRenderer sr;
    private Collider2D col; // mi collider (trigger)

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (sr != null) sr.color = bulletColor; // tintado visual
        timer = lifeTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            DestroySelf();
    }

    /// <summary>
    /// Colisiones (Trigger) con Player o con proyectiles del jugador (tag "Projectile").
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Impacto con el jugador: daño + FX + destruir este proyectil
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

            // Intentamos obtener la normal de contacto estable vía Distance()
            Vector2 contactNormal = Vector2.zero;
            if (col != null)
            {
                ColliderDistance2D d = Physics2D.Distance(other, col);
                if (d.isOverlapped) contactNormal = d.normal; // normal desde 'other' hacia 'col'
            }
            if (contactNormal.sqrMagnitude < 1e-6f)
                contactNormal = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

            HandlePlayerProjectileHit(playerBullet, other.attachedRigidbody, contactNormal);
        }
    }

    /// <summary>
    /// Aplica la lógica MATCH/MISMATCH con soluciones anti-atasco en mismatch.
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

        // —— MISMATCH: ricochet del proyectil del jugador; este proyectil enemigo sigue vivo —— //
        if (rbPlayer != null)
        {
            Collider2D playerCol = playerBullet.GetComponent<Collider2D>();
            Vector2 n = contactNormal;

            // 1) Resolver solape si lo hay (sacarlo antes de reflejar)
            if (playerCol != null && col != null)
            {
                ColliderDistance2D d = Physics2D.Distance(playerCol, col);
                if (d.isOverlapped)
                {
                    n = d.normal; // normal desde playerCol hacia col
                    float pushOut = (-d.distance) + 0.01f; // salir del solape + epsilon
                    rbPlayer.position += n * pushOut;
                }
            }

            if (n.sqrMagnitude < 1e-6f)
                n = (rbPlayer.position - (Vector2)transform.position).normalized;

            // 2) Reflejo + clamp de velocidad mínima
            Vector2 inVel  = rbPlayer.linearVelocity;
            Vector2 outVel = Vector2.Reflect(inVel, n);

            float wantedMin = Mathf.Max(minRicochetSpeed, playerBullet.minSpeed * 1.25f);
            if (outVel.sqrMagnitude < wantedMin * wantedMin)
            {
                outVel = (outVel.sqrMagnitude < 1e-6f) ? n * wantedMin : outVel.normalized * wantedMin;
            }

            rbPlayer.linearVelocity = outVel;

            // 3) Separación extra para evitar re-contacto en el mismo frame
            rbPlayer.position += n * postRicochetSeparation;

            // 4) Ignorar esta pareja por unos ms para romper el bucle de rebotes
            if (playerCol != null && col != null)
                StartCoroutine(TemporaryIgnoreCollision(playerCol, col, postRicochetIgnoreTime));
        }

        // (Opcional) puedes instanciar un FX suave aquí si quieres feedback en mismatch
        // pero el proyectil enemigo NO se destruye.
    }

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
