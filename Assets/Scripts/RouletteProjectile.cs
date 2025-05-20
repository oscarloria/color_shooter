using UnityEngine;

/// <summary>
/// Bala disparada por los triángulos del Enemigo Ruleta.
/// • Daña al jugador al contacto.
/// • Choca con proyectiles del jugador aplicando la misma lógica de colores
///   (mismo color → se destruyen ambos, color distinto → ricochet del disparo del jugador).
/// • Destruye-se tras lifeTime o al colisionar, instanciando un efecto opcional.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class RouletteProjectile : MonoBehaviour
{
    [Header("Ajustes")]
    public Color  bulletColor = Color.red;
    public float  lifeTime    = 4f;

    [Tooltip("Prefab de partícula / animación que se instancia al destruir la bala.")]
    public GameObject impactEffect;

    /*────────── internals ──────────*/
    float           timer;
    SpriteRenderer  sr;
    Rigidbody2D     rb;

    /*────────────────────────────────*/
    void Start()
    {
        sr    = GetComponent<SpriteRenderer>();
        rb    = GetComponent<Rigidbody2D>();
        timer = lifeTime;

        if (sr) sr.color = bulletColor;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            DestroySelf();
    }

    /*==================  COLISIONES  ==================*/
    void OnTriggerEnter2D(Collider2D other)
    {
        /*—— 1) Jugador ——*/
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();
            SpawnImpactFX();
            DestroySelf();
            return;
        }

        /*—— 2) Proyectil del jugador ——*/
        if (other.CompareTag("Projectile"))
        {
            Projectile pj = other.GetComponent<Projectile>();
            if (pj == null) return;

            /*– 2.a Mismo color → se destruyen ambos –*/
            if (pj.projectileColor == bulletColor)
            {
                SpawnImpactFX();
                Destroy(other.gameObject);
                DestroySelf();
            }
            /*– 2.b Color distinto → ricochet del proyectil del jugador –*/
            else
            {
                Rigidbody2D rbP = other.GetComponent<Rigidbody2D>();
                if (rbP)
                {
                    Vector2 normal = (other.transform.position - transform.position).normalized;
                    rbP.linearVelocity = Vector2.Reflect(rbP.linearVelocity, normal);
                }
                // La bala de la ruleta sigue su curso
            }
        }
    }

    /*==================  HELPERS  ==================*/
    void SpawnImpactFX()
    {
        if (impactEffect == null) return;

        GameObject fx = Instantiate(impactEffect, transform.position, Quaternion.identity);

        // Teñir partículas con el color de la bala
        if (fx.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            main.startColor = bulletColor;
        }
    }

    void DestroySelf() => Destroy(gameObject);
}
