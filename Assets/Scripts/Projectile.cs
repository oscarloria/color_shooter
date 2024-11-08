using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Color projectileColor; // Color del proyectil
    public float lifetime = 5f;   // Tiempo antes de que el proyectil se destruya
    public float minSpeed = 1f;   // Velocidad mínima antes de destruir el proyectil

    private SpriteRenderer spriteRenderer;
    private float lifeTimer;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        projectileColor = spriteRenderer.color;
        lifeTimer = lifetime;

        rb = GetComponent<Rigidbody2D>(); // Obtener referencia al Rigidbody2D
    }

    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }

        // Verificar si la velocidad es menor que la mínima
        if (rb != null && rb.linearVelocity.magnitude < minSpeed)
        {
            Destroy(gameObject);
        }
    }
}