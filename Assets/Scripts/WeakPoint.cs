using UnityEngine;

/// <summary>
/// Maneja las colisiones en el punto débil del enemigo tipo tanque.
/// </summary>
public class WeakPoint : MonoBehaviour
{
    private TankEnemy tankEnemy;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        tankEnemy = GetComponentInParent<TankEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Antes: Elegíamos un color aleatorio en GetRandomColor().
        // Ahora: Tomamos el color que el TankEnemy ya tiene asignado (tankEnemy.enemyColor),
        // lo cual permite que el punto débil tenga el color de vulnerabilidad correcto.

        spriteRenderer.color = tankEnemy.enemyColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Si el proyectil coincide en color con el punto débil (spriteRenderer.color),
                // entonces el TankEnemy recibe daño.
                if (projectile.projectileColor == spriteRenderer.color)
                {
                    tankEnemy.TakeDamage();
                    Destroy(other.gameObject);
                }
                else
                {
                    // Si no coincide el color, el proyectil rebota.
                    // La lógica de rebote ya está manejada por el proyectil.
                }
            }
        }
    }
}