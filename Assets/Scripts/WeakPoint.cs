using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    private TankEnemy tankEnemy;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        tankEnemy = GetComponentInParent<TankEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Asignar color
        spriteRenderer.color = tankEnemy.enemyColor;
        
        // --- LÍNEA AÑADIDA ---
        // Asegura que el punto débil se dibuje encima del cuerpo del tanque.
        // Asume que el cuerpo está en el order 0.
        spriteRenderer.sortingOrder = 1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null && projectile.projectileColor == spriteRenderer.color)
            {
                tankEnemy.TakeDamage();
                Destroy(other.gameObject);
            }
        }
    }
}