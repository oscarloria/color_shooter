using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración de Proyectil Enemigo")]
    public Color bulletColor = Color.white; // Asignado por ShooterEnemy
    public float lifeTime = 3f;             // Se destruye tras X segundos
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
            sr.color = bulletColor; // Visual
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
    /// Manejo de colisiones mediante trigger (EnemyProjectile vs Player o vs PlayerProjectile).
    /// Asegúrate de que el Collider2D de este prefab esté marcado como Is Trigger.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Si colisiona con el jugador => dañarlo
        if (other.CompareTag("Player"))
        {
            // Dañar al jugador
            PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage();
            }

            // Llamar al shake de la cámara para generar el efecto de sacudida
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera();
            }

            // Crear efecto de impacto con el color del proyectil
            if (impactEffect != null)
            {
                GameObject explosion = Instantiate(impactEffect, transform.position, Quaternion.identity);
                ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = bulletColor;
                }
            }

            // Destruir el proyectil enemigo
            DestroyEnemyProjectile();
        }
        // 2) Si colisiona con un proyectil del jugador
        else if (other.CompareTag("Projectile"))
        {
            // Obtenemos el script del proyectil del jugador (suponiendo que se llama "Projectile")
            Projectile playerBullet = other.GetComponent<Projectile>();
            if (playerBullet != null)
            {
                // Si los colores coinciden, destruir ambos proyectiles y ejecutar el efecto de impacto.
                if (playerBullet.projectileColor == bulletColor)
                {
                    if (impactEffect != null)
                    {
                        GameObject explosion = Instantiate(impactEffect, transform.position, Quaternion.identity);
                        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            var main = ps.main;
                            main.startColor = bulletColor;
                        }
                    }
                    Destroy(other.gameObject);
                    DestroyEnemyProjectile();
                }
                else
                {
                    // Si los colores no coinciden, en lugar de destruir el proyectil del jugador,
                    // se le aplica un ricochet (reflejo de velocidad) para que rebote.
                    Rigidbody2D rbPlayer = other.GetComponent<Rigidbody2D>();
                    if (rbPlayer != null)
                    {
                        Vector2 collisionNormal = (other.transform.position - transform.position).normalized;
                        Vector2 reflectedVelocity = Vector2.Reflect(rbPlayer.linearVelocity, collisionNormal);
                        rbPlayer.linearVelocity = reflectedVelocity;
                    }
                    
                    // (Opcional) instanciar efecto de impacto con el color del proyectil
                    if (impactEffect != null)
                    {
                       /* GameObject explosion = Instantiate(impactEffect, transform.position, Quaternion.identity);
                        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            var main = ps.main;
                            main.startColor = bulletColor;
                        }*/
                    }
                    // En este caso, no se destruye el proyectil enemigo; éste sigue su curso.
                }
            }
        }
        else
        {
            // (Opcional) Aquí puedes agregar comportamiento para otras colisiones (paredes, etc.)
        }
    }

    private void DestroyEnemyProjectile()
    {
        Destroy(gameObject);
    }
}
