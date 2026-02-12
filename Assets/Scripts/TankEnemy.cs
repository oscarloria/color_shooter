using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo tanque: cuerpo blanco resistente (3 HP), solo recibe daño
/// a través de su punto débil (WeakPoint). Rota suavemente hacia el jugador.
/// </summary>
public class TankEnemy : EnemyBase
{
    [Header("Tank — Rotación")]
    public float rotationSpeed = 200f;

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void Start()
    {
        // maxHealth ya se puede configurar en el inspector (default 3 en prefab)
        base.Start();
    }

    void Update()
    {
        if (player == null) return;
        RotateTowardsPlayer();
        MoveTowardsPlayer();
    }

    /*───────────────────  APARIENCIA  ───────────────────*/

    /// <summary>
    /// El cuerpo del tanque siempre es blanco.
    /// El color real se aplica al WeakPoint hijo en WeakPoint.Start().
    /// </summary>
    public override void ApplyVisualColor()
    {
        if (sr != null) sr.color = Color.white;
    }

    /*───────────────────  DAÑO  ───────────────────*/

    /// <summary>
    /// El tanque NO muere por impacto directo de proyectiles en su cuerpo.
    /// Solo recibe daño a través de WeakPoint.OnTriggerEnter2D → TakeDamage().
    /// </summary>
    protected override void HandleProjectileHit(Collision2D collision)
    {
        // No hacer nada: la física del rebote se encarga.
        // El daño solo entra por el WeakPoint (trigger).
    }

    protected override void OnDamageTaken()
    {
        StartCoroutine(DamageFeedback());
    }

    /// <summary>
    /// La explosión del tanque usa el color del WeakPoint, no el del body.
    /// </summary>
    protected override void Die()
    {
        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();

        // Usar el color del WeakPoint para la explosión
        Color explosionColor = enemyColor;
        SpriteRenderer weakPointSR = transform.Find("WeakPoint")?.GetComponent<SpriteRenderer>();
        if (weakPointSR != null) explosionColor = weakPointSR.color;

        SpawnExplosion(explosionColor);
        GiveSlowMotionCharge();
        Destroy(gameObject, 0.1f);
    }

    /*───────────────────  MOVIMIENTO  ───────────────────*/

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime
        );
    }

    /*───────────────────  FEEDBACK VISUAL  ───────────────────*/

    IEnumerator DamageFeedback()
    {
        if (sr == null) yield break;

        SpriteRenderer weakPointSR = transform.Find("WeakPoint")?.GetComponent<SpriteRenderer>();
        Color originalBody = sr.color;

        if (weakPointSR != null)
        {
            Color originalWeak = weakPointSR.color;
            sr.color = Color.white;
            weakPointSR.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = originalBody;
            weakPointSR.color = originalWeak;
        }
        else
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = originalBody;
        }
    }
}
