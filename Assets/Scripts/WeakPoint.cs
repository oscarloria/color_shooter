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

        // Asignar un color aleatorio al punto débil
        spriteRenderer.color = GetRandomColor();
    }

    Color GetRandomColor()
    {
        // Lista de colores posibles
        Color[] colors = { Color.yellow, Color.blue, Color.green, Color.red };
        int randomIndex = Random.Range(0, colors.Length);
        return colors[randomIndex];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                if (projectile.projectileColor == spriteRenderer.color)
                {
                    // El proyectil es del mismo color, causa daño
                    tankEnemy.TakeDamage();
                    Destroy(other.gameObject);
                }
                else
                {
                    // El proyectil no es del mismo color, rebota
                    // La lógica de rebote ya está implementada en el proyectil
                }
            }
        }
    }
}