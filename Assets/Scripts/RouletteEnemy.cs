using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo “Ruleta”.
/// • Órbita elíptica alrededor del jugador (radio X / radio Y) y giro propio.
/// • Dispara con sus 4 TriangleGun cada ‘shootInterval’.
/// • Recibe daño solo con proyectiles del color correspondiente.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class RouletteEnemy : MonoBehaviour
{
    /*──────────────────────  MOVIMIENTO  ───────────────────────*/
    [Header("Movimiento orbital elíptico")]
    public float orbitRadiusX = 7f;             // semieje horizontal
    public float orbitRadiusY = 4f;             // semieje vertical
    public float orbitPeriod  = 8f;             // seg para una vuelta

    [Tooltip("¿La órbita se recorre en sentido horario?")]
    public bool orbitClockwise = false;

    [Tooltip("Velocidad de giro sobre su propio eje (°/seg, signo + = CCW).")]
    public float selfRotationSpeed = 90f;

    /*──────────────────────  COMBATE  ─────────────────────────*/
    [Header("Ataque")]
    public float shootInterval = 0.5f;

    [Header("Vida")]
    public int   maxHP          = 100;
    public GameObject explosionPrefab;

    /*──────────────────────  FEEDBACK DAÑO  ───────────────────*/
    [Header("Feedback de daño")]
    public float scalePopMultiplier = 1.05f;
    public float shakeDuration      = 0.20f;
    public float shakeMagnitude     = 0.15f;
    public float flashTime          = 0.05f;

    /*──────────────────────  PUNTUACIÓN  ──────────────────────*/
    public int scoreValue = 500;

    /*──────────────────────  INTERNAS  ───────────────────────*/
    int           currentHP;
    float         orbitAngle;          // rad
    float         shootTimer;
    Transform     player;
    TriangleGun[] guns;
    SpriteRenderer sr;
    Rigidbody2D   rb;

    Vector3       baseScale;           // ← escala original (para restaurar)
    bool          feedbackRunning;     // ← evita acumulación

    /*──────────────────────  SET-UP  ─────────────────────────*/
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        guns   = GetComponentsInChildren<TriangleGun>(true);
        sr     = GetComponent<SpriteRenderer>();
        rb     = GetComponent<Rigidbody2D>();

        foreach (var g in guns) g.SetOwner(this);
    }

    void Start()
    {
        currentHP  = maxHP;
        shootTimer = shootInterval;
        baseScale  = transform.localScale;                  // guardar escala de fábrica

        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /*──────────────────────  UPDATE  ─────────────────────────*/
    void Update()
    {
        if (!player) return;

        /* 1) ÓRBITA ELÍPTICA */
        float dir = orbitClockwise ? -1f : 1f;
        orbitAngle += dir * (2f * Mathf.PI / orbitPeriod) * Time.deltaTime;

        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle) * orbitRadiusX,
            Mathf.Sin(orbitAngle) * orbitRadiusY);

        transform.position = player.position + (Vector3)offset;

        /* 2) GIRO PROPIO */
        transform.Rotate(0f, 0f, selfRotationSpeed * Time.deltaTime);

        /* 3) DISPARO */
        if (shootInterval > 0f)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                foreach (var g in guns) g.Shoot();
                shootTimer += shootInterval;
            }
        }
    }

    /*───────────────────  DAÑO & MUERTE  ─────────────────────*/
    public void ApplyDamage(int dmg)
    {
        if (dmg <= 0) return;

        currentHP = Mathf.Max(currentHP - dmg, 0);

        // Lanza feedback sólo si no está ejecutándose
        if (!feedbackRunning)
            StartCoroutine(DamageFeedback());

        if (currentHP == 0) Die();
    }

    IEnumerator DamageFeedback()
    {
        feedbackRunning = true;

        Color origColor = sr ? sr.color : Color.white;

        /*—— Flash + Pop ——*/
        if (sr) sr.color = Color.white;
        transform.localScale = baseScale * scalePopMultiplier;

        /*—— Shake sin alterar escala original ——*/
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * shakeMagnitude,
                Random.Range(-1f, 1f) * shakeMagnitude,
                0f);

            transform.position += offset;
            yield return null;
            transform.position -= offset;

            elapsed += Time.deltaTime;
        }

        yield return new WaitForSeconds(flashTime);

        /*—— Restaurar ——*/
        if (sr) sr.color = origColor;
        transform.localScale = baseScale;

        feedbackRunning = false;
    }

    void Die()
    {
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();
        GameObject.FindGameObjectWithTag("Player")
                 ?.GetComponent<SlowMotion>()
                 ?.AddSlowMotionCharge();

        Destroy(gameObject);
    }
}
