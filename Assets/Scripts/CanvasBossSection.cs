using UnityEngine;
using System.Collections;

/// <summary>
/// Una sección individual del Canvas Boss.
/// Puede estar: vacía (gris), pintada correctamente, o ensuciada (marrón).
///
/// El jugador la "pinta" disparándole con el color correcto.
/// Color incorrecto = se ensucia (necesita 2 hits correctos para arreglar).
///
/// Setup:
/// - SpriteRenderer (el panel de la sección)
/// - Collider2D (BoxCollider2D, IsTrigger = true)
/// - Rigidbody2D (Kinematic)
/// - Hijo: "Indicator" con SpriteRenderer (muestra el color requerido)
/// - Tag: "Enemy", Layer: "Enemy"
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class CanvasBossSection : MonoBehaviour
{
    [Header("═══ Colores ═══")]
    public Color emptyColor = new Color(0.16f, 0.16f, 0.23f, 1f);
    public Color dirtyColor = new Color(0.29f, 0.23f, 0.16f, 1f);

    [Header("═══ Indicador ═══")]
    [Tooltip("Transform hijo que muestra el color requerido (pequeño sprite/barra).")]
    public Transform indicatorTransform;

    [Header("═══ Ricochet ═══")]
    public float minRicochetSpeed = 6f;
    public float postRicochetSeparation = 0.10f;
    public float postRicochetIgnoreTime = 0.08f;

    /*═══════════════════  ESTADO INTERNO  ═══════════════════*/

    private Color requiredColor;
    private SpriteRenderer sr;
    private SpriteRenderer indicatorSR;
    private Collider2D col;
    private bool isAcceptingInput = false;
    private bool isPainted = false;
    private bool isDirty = false;
    private int hitsNeeded = 1;

    private Coroutine flashCoroutine;

    /*═══════════════════  INICIALIZACIÓN  ═══════════════════*/

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        if (indicatorTransform != null)
            indicatorSR = indicatorTransform.GetComponent<SpriteRenderer>();
    }

    /*═══════════════════  CONTROL DESDE CANVAS BOSS  ═══════════════════*/

    /// <summary>
    /// Define qué color necesita esta sección.
    /// </summary>
    public void SetRequired(Color color)
    {
        requiredColor = color;
        isPainted = false;
        isDirty = false;
        hitsNeeded = 1;
    }

    /// <summary>
    /// Pone la sección en estado vacío (gris).
    /// </summary>
    public void SetEmpty()
    {
        isPainted = false;
        isDirty = false;
        hitsNeeded = 1;
        if (sr != null) sr.color = emptyColor;
    }

    /// <summary>
    /// Activa/desactiva si la sección acepta disparos del jugador.
    /// </summary>
    public void SetAcceptingInput(bool accepting)
    {
        isAcceptingInput = accepting;
    }

    /// <summary>
    /// Muestra el indicador de color requerido.
    /// </summary>
    public void ShowIndicator()
    {
        if (indicatorSR != null)
        {
            indicatorSR.color = requiredColor;
            indicatorTransform.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta el indicador.
    /// </summary>
    public void HideIndicator()
    {
        if (indicatorTransform != null)
            indicatorTransform.gameObject.SetActive(false);
    }

    /// <summary>
    /// ¿Está correctamente pintada?
    /// </summary>
    public bool IsCorrectlyPainted()
    {
        return isPainted;
    }

    /// <summary>
    /// Flash temporal de un color (usado en intro y éxito).
    /// </summary>
    public void FlashColor(Color color, float duration)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DoFlash(color, duration));
    }

    IEnumerator DoFlash(Color color, float duration)
    {
        if (sr == null) yield break;
        Color prevColor = sr.color;
        sr.color = color;
        yield return new WaitForSeconds(duration);
        if (sr != null) sr.color = prevColor;
        flashCoroutine = null;
    }

    /*═══════════════════  COLISIONES  ═══════════════════*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Projectile")) return;

        Projectile playerBullet = other.GetComponent<Projectile>();
        if (playerBullet == null) return;

        // Si no está aceptando input, ricochet siempre
        if (!isAcceptingInput || isPainted)
        {
            DoRicochet(playerBullet, other);
            return;
        }

        // Color correcto
        if (playerBullet.projectileColor == requiredColor)
        {
            Destroy(other.gameObject);
            OnCorrectHit();
            return;
        }

        // Color incorrecto
        Destroy(other.gameObject);
        OnWrongHit();
    }

    /*═══════════════════  HITS  ═══════════════════*/

    void OnCorrectHit()
    {
        hitsNeeded--;

        if (hitsNeeded <= 0)
        {
            // ¡Pintada correctamente!
            isPainted = true;
            isDirty = false;
            if (sr != null) sr.color = requiredColor;
            Debug.Log($"CanvasBossSection: ¡Pintada correctamente! ({requiredColor})");
        }
        else
        {
            // Limpiando sección sucia (aún necesita más hits)
            isDirty = false;
            if (sr != null) sr.color = emptyColor;
            Debug.Log($"CanvasBossSection: Limpiada. Falta {hitsNeeded} hit(s).");
        }
    }

    void OnWrongHit()
    {
        if (isDirty) return; // Ya está sucia, no empeora

        isDirty = true;
        hitsNeeded = 2; // 1 para limpiar + 1 para pintar
        if (sr != null) sr.color = dirtyColor;

        Debug.Log("CanvasBossSection: ¡Ensuciada! Necesita 2 hits correctos.");
    }

    /*═══════════════════  RICOCHET  ═══════════════════*/

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