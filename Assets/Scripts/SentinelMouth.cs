using UnityEngine;
using System.Collections;

/// <summary>
/// Boca del Sentinel Boss. Pieza separada con collider propio.
/// Empieza blanca durante la intro, se colorea con Colorize().
/// Color match = daño al boss. Mismatch = ricochet.
/// Durante intro = ricochet siempre.
///
/// Setup:
/// - Hijo del SentinelBoss
/// - SpriteRenderer (se colorea por código)
/// - Collider2D (IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SentinelMouth : MonoBehaviour
{
    [Header("Ricochet")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    private Collider2D col;
    private SpriteRenderer sr;
    private SentinelBoss boss;
    private bool isInIntro = true;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        boss = GetComponentInParent<SentinelBoss>();
    }

    void Start()
    {
        // Empieza blanco durante la intro
        if (sr != null) sr.color = Color.white;
    }

    /// <summary>
    /// Llamado por el Controller durante la intro.
    /// Revela el color real y permite recibir daño por match.
    /// </summary>
    public void Colorize()
    {
        isInIntro = false;
        if (sr != null && boss != null)
            sr.color = boss.bossColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Projectile")) return;

        Projectile playerBullet = other.GetComponent<Projectile>();
        if (playerBullet == null) return;
        if (boss == null) return;

        // Durante intro: ricochet siempre
        if (isInIntro)
        {
            DoRicochet(playerBullet, other);
            return;
        }

        // Match: daño al boss
        if (playerBullet.projectileColor == boss.bossColor)
        {
            Destroy(other.gameObject);
            boss.TakeDamage(1);
            return;
        }

        // Mismatch: ricochet
        DoRicochet(playerBullet, other);
    }

    void DoRicochet(Projectile playerBullet, Collider2D other)
    {
        Rigidbody2D rbPlayer = other.attachedRigidbody;
        if (rbPlayer == null) return;

        Vector2 contactNormal = Vector2.zero;
        if (col != null)
        {
            ColliderDistance2D d = Physics2D.Distance(other, col);
            if (d.isOverlapped) contactNormal = d.normal;
        }
        if (contactNormal.sqrMagnitude < 1e-6f)
            contactNormal = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;

        Collider2D playerCol = playerBullet.GetComponent<Collider2D>();
        Vector2 n = contactNormal;

        if (playerCol != null && col != null)
        {
            ColliderDistance2D d = Physics2D.Distance(playerCol, col);
            if (d.isOverlapped)
            {
                n = d.normal;
                float pushOut = (-d.distance) + 0.01f;
                rbPlayer.position += n * pushOut;
            }
        }

        if (n.sqrMagnitude < 1e-6f)
            n = (rbPlayer.position - (Vector2)transform.position).normalized;

        Vector2 inVel = rbPlayer.linearVelocity;
        Vector2 outVel = Vector2.Reflect(inVel, n);

        float wantedMin = Mathf.Max(minRicochetSpeed, playerBullet.minSpeed * 1.25f);
        if (outVel.sqrMagnitude < wantedMin * wantedMin)
        {
            outVel = (outVel.sqrMagnitude < 1e-6f) ? n * wantedMin : outVel.normalized * wantedMin;
        }

        rbPlayer.linearVelocity = outVel;
        rbPlayer.position += n * postRicochetSeparation;

        if (playerCol != null && col != null)
            StartCoroutine(TemporaryIgnoreCollision(playerCol, col, postRicochetIgnoreTime));
    }

    private IEnumerator TemporaryIgnoreCollision(Collider2D a, Collider2D b, float time)
    {
        if (a == null || b == null) yield break;
        Physics2D.IgnoreCollision(a, b, true);
        yield return new WaitForSeconds(time);
        if (a != null && b != null)
            Physics2D.IgnoreCollision(a, b, false);
    }
}