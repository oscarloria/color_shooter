using UnityEngine;

/// <summary>
/// Punto débil de un enemigo (hijo con trigger collider).
/// Recibe proyectiles del jugador y aplica daño al EnemyBase padre
/// si el color del proyectil coincide con el del punto débil.
/// Actualmente usado por TankEnemy.
/// </summary>
public class WeakPoint : MonoBehaviour
{
    EnemyBase parentEnemy;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        parentEnemy = GetComponentInParent<EnemyBase>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (parentEnemy != null && spriteRenderer != null)
        {
            spriteRenderer.color = parentEnemy.enemyColor;
            spriteRenderer.sortingOrder = 1;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Projectile")) return;

        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null && spriteRenderer != null &&
            projectile.projectileColor == spriteRenderer.color)
        {
            parentEnemy?.TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
