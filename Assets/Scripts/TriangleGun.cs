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
    public GameObject projectilePrefab;     // se tiñe con gunColor
    public float projectileSpeed = 7f;

    /*── Internas ──*/
    RouletteEnemy owner;

    public void SetOwner(RouletteEnemy o) => owner = o;

    /*-------------- Shoot --------------*/
    public void Shoot()
    {
        if (!projectilePrefab) return;

        Vector3 spawnPos = transform.position + transform.up * 0.4f;
        GameObject bullet = Instantiate(projectilePrefab, spawnPos, transform.rotation);
        if (bullet.TryGetComponent(out SpriteRenderer srBullet))
            srBullet.color = gunColor;

        if (bullet.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = transform.up * projectileSpeed;

        // Asignar color lógico al script de bala
        if (bullet.TryGetComponent(out RouletteProjectile rp))
            rp.bulletColor = gunColor;
        else if (bullet.TryGetComponent(out EnemyProjectile ep))
            ep.bulletColor = gunColor;      // por si reutilizas EnemyProjectile
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
            owner?.ApplyDamage(1);
            Destroy(col.gameObject);
            return;
        }

        // —— 2) Color distinto -> ricochet ——
        Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
        if (rb)
        {
            Vector2 inDir  = rb.linearVelocity.normalized;
            Vector2 outDir = Vector2.Reflect(inDir, transform.up).normalized;
            rb.linearVelocity = outDir * rb.linearVelocity.magnitude;
        }
    }
}