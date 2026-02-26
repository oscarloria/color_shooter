using UnityEngine;
using System.Collections;

/// <summary>
/// Escudo del Sentinel Boss. Pieza separada con su propio collider.
/// Todo proyectil que lo toque rebota (ricochet), sin importar color.
///
/// Setup:
/// - Hijo del SentinelBoss
/// - SpriteRenderer (blanco)
/// - Collider2D (IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SentinelShield : MonoBehaviour
{
    [Header("Ricochet")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    [Header("Activation Flash")]
    public float flashDuration = 0.3f;

    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// Llamado por el Controller durante la intro.
    /// El escudo ya es blanco, así que hace un pulse cyan/brillante
    /// para indicar que se "activó".
    /// </summary>
    public void FlashActivation()
    {
        StartCoroutine(DoActivationFlash());
    }

    IEnumerator DoActivationFlash()
    {
        if (sr == null) yield break;

        Color originalColor = sr.color;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            float pulse = Mathf.PingPong(elapsed * 10f, 1f);
            sr.color = Color.Lerp(originalColor, Color.cyan, pulse * 0.8f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Projectile")) return;

        Projectile playerBullet = other.GetComponent<Projectile>();
        if (playerBullet == null) return;

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