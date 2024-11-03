using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Color projectileColor; // Color del proyectil
    public float lifetime = 5f; // Tiempo antes de que el proyectil se destruya

    private SpriteRenderer spriteRenderer;
    private float lifeTimer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        projectileColor = spriteRenderer.color;
        lifeTimer = lifetime;
    }

    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}