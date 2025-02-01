using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración de Proyectil Enemigo")]
    public Color bulletColor = Color.white; // Asignado por ShooterEnemy
    public float lifeTime = 3f;            // Se destruye tras X segundos
    [Tooltip("Si quieres un prefab de explosión o efecto de impacto, asignarlo aquí.")]
    public GameObject impactEffect;        

    private float timer;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = bulletColor; // visual
        }

        rb = GetComponent<Rigidbody2D>();

        timer = lifeTime; // Contador para autodestrucción
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            DestroyEnemyProjectile();
        }
    }

    /// <summary>
    /// Manejo de colisiones físicas (EnemyProjectile vs Player o vs PlayerProjectile).
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1) Si colisiona con el jugador => dañarlo
        if (collision.collider.CompareTag("Player"))
        {
            // Dañar al jugador
            PlayerHealth pHealth = collision.collider.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(); 
            }

            // (Opcional) crear efecto de impacto
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, Quaternion.identity);
            }

            // Destruir el proyectil
            DestroyEnemyProjectile();
        }
        // 2) Si choca con un proyectil del jugador => color matching
        else if (collision.collider.CompareTag("Projectile"))
        {
            // Por ejemplo, el proyectil del jugador se llama "Projectile.cs"
            Projectile playerBullet = collision.collider.GetComponent<Projectile>();
            if (playerBullet != null)
            {
                // Comprobar color => si coincide => destruir ambos
                if (playerBullet.projectileColor == bulletColor)
                {
                    // Efecto de impacto (opcional)
                    if (impactEffect != null)
                    {
                        Instantiate(impactEffect, transform.position, Quaternion.identity);
                    }

                    // Destruir el proyectil del jugador
                    Destroy(collision.gameObject);

                    // Destruir esta bala
                    DestroyEnemyProjectile();
                }
                else
                {
                    // No coincide => la bala del jugador rebotará?
                    // Depende de tu lógica: si "Projectile.cs" maneja rebotes, 
                    // no destruyas nada aquí.
                }
            }
        }
        else
        {
            // (Opcional) destruye la bala si toca cualquier pared/objeto
            // DestroyEnemyProjectile();
            // O si quieres que rebote, ajusta bounciness en su PhysicsMaterial2D
        }
    }

    private void DestroyEnemyProjectile()
    {
        // (Opcional) instanciar efecto extra
        // if (impactEffect != null) Instantiate(impactEffect, transform.position, Quaternion.identity);

        // Finalmente, destruir
        Destroy(gameObject);
    }
}
