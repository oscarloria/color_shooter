using UnityEngine;

/// <summary>
/// Se adjunta a cada triángulo hijo.
/// Gestiona colisiones con proyectiles del jugador
/// y dispara balas enemigas de su color.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class TriangleGun : MonoBehaviour
{
    [Header("Color lógico del triángulo")]
    public Color gunColor = Color.red;

    [Header("Disparo")]
    public GameObject projectilePrefab;     // se tiñe con gunColor
    public float projectileSpeed = 7f;

    /*── Internas ──*/
    RouletteEnemy owner;
    SpriteRenderer sr; // Referencia al SpriteRenderer para cambiar su color

    void Awake() // Cambiado de Start a Awake para asegurar que sr esté disponible
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisualColor(); // Aplicar color inicial
    }

    public void SetOwner(RouletteEnemy o) => owner = o;

    /// <summary>
    /// NUEVO: Método para actualizar el color del sprite del cañón.
    /// Se llamará desde RouletteEnemy cuando cambie la fase.
    /// </summary>
    public void UpdateVisualColor()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>(); // Seguridad extra
        if (sr != null) sr.color = gunColor;
    }

    /*-------------- Shoot --------------*/
    public void Shoot()
    {
        if (!projectilePrefab) return;

        Vector3 spawnPos = transform.position + transform.up * 0.4f;
        GameObject bullet = Instantiate(projectilePrefab, spawnPos, transform.rotation);
       
        // Aplicar el gunColor al sprite del proyectil si tiene SpriteRenderer
        if (bullet.TryGetComponent(out SpriteRenderer srBullet))
            srBullet.color = gunColor;

        if (bullet.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = transform.up * projectileSpeed;

        // Asignar color lógico al script de bala
        if (bullet.TryGetComponent(out RouletteProjectile rp))
            rp.bulletColor = gunColor;
        else if (bullet.TryGetComponent(out EnemyProjectile ep))
            ep.bulletColor = gunColor;      // por si reutilizas EnemyProjectile
    }

    /*-------------- Colisiones con disparos del jugador --------------*/
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Projectile")) return;

        Projectile p = col.GetComponent<Projectile>();
        if (p == null) return;

        // —— 1) Mismo color -> daño al boss ——
        if (p.projectileColor == gunColor)
        {
            owner?.ApplyDamage(1); // El daño podría ser configurable
            Destroy(col.gameObject);
            return;
        }

        // —— 2) Color distinto -> ricochet ——
        Rigidbody2D rbPlayerProjectile = col.GetComponent<Rigidbody2D>();
        if (rbPlayerProjectile)
        {
            Vector2 inDir  = rbPlayerProjectile.linearVelocity.normalized;
            // Usar la normal de la superficie del TriangleGun para el reflejo.
            // Asumiendo que 'transform.up' del TriangleGun es su "cara" exterior.
            Vector2 surfaceNormal = transform.up; 
            Vector2 outDir = Vector2.Reflect(inDir, surfaceNormal).normalized;
            rbPlayerProjectile.linearVelocity = outDir * rbPlayerProjectile.linearVelocity.magnitude;
        }
    }
}