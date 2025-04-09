using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Color projectileColor; // Color “lógico” del proyectil (para comparar con enemigos)
    public float lifetime = 5f;   // Tiempo antes de que el proyectil se destruya
    public float minSpeed = 1f;   // Velocidad mínima antes de destruir el proyectil

    private SpriteRenderer spriteRenderer;
    private float lifeTimer;
    private Rigidbody2D rb;

    void Start()
    {
        // Obtener el spriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // (OPCIONAL) Sincronizar apariencia con projectileColor
        // spriteRenderer.color = projectileColor;

        // Configurar el temporizador de vida
        lifeTimer = lifetime;

        // Obtener el Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Decrementar lifetime
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Verificar velocidad mínima
        if (rb != null && rb.linearVelocity.magnitude < minSpeed)
        {
            Destroy(gameObject);
        }
    }
}
