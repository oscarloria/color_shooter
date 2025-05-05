using UnityEngine;

/// <summary>
/// Enemigo básico: aparece fuera de cámara, avanza hasta ser claramente visible,
/// se detiene un instante y luego se lanza hacia el jugador.
/// </summary>
public class Enemy : MonoBehaviour
{
    /*───────────────────  INSPECTOR  ───────────────────*/

    [Header("Apariencia")]
    public Color enemyColor = Color.white;
    public float minRotationSpeed = -180f;
    public float maxRotationSpeed =  180f;
    public GameObject explosionPrefab;

    [Header("Movimiento")]
    public float speed = 2f;

    [Tooltip("Tiempo que permanece detenido al volverse visible (segundos).")]
    public float pauseDuration = 0.6f;

    [Header("Visibilidad (Viewport)")]
    [Tooltip("Usar un margen aleatorio por enemigo entre 0.10‑0.20?")]
    public bool randomizeViewportMargin = false;

    [Tooltip("Valor fijo (se usa si randomizeViewportMargin = false).")]
    [Range(0f,0.49f)]
    public float viewportMargin = 0.03f;

    [Tooltip("Límite MIN del margen aleatorio (inclusive).")]
    [Range(0f,0.49f)]
    public float minRandomMargin = 0.10f;

    [Tooltip("Límite MAX del margen aleatorio (inclusive).")]
    [Range(0f,0.49f)]
    public float maxRandomMargin = 0.20f;

    [Header("Puntuación")]
    public int scoreValue = 100;

    /*───────────────────  PRIVADAS  ───────────────────*/

    Transform player;
    SpriteRenderer sr;
    Camera mainCam;

    float rotationSpeed;
    float pauseTimer;
    float margin;             // margen efectivo para este enemigo

    enum State { Approaching, Paused, Attacking }
    State state = State.Approaching;

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    void Start()
    {
        // Referencias
        player  = GameObject.FindGameObjectWithTag("Player")?.transform;
        sr      = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        if (sr) sr.color = enemyColor;
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

        // Elegir margen
        margin = randomizeViewportMargin
                 ? Random.Range(minRandomMargin, maxRandomMargin)
                 : viewportMargin;
    }

    void Update()
    {
        switch (state)
        {
            case State.Approaching:
                MoveTowardsPlayer();
                if (IsFullyInsideViewport(margin))
                {
                    state      = State.Paused;
                    pauseTimer = pauseDuration;
                }
                break;

            case State.Paused:
                pauseTimer -= Time.deltaTime;
                if (pauseTimer <= 0f) state = State.Attacking;
                break;

            case State.Attacking:
                MoveTowardsPlayer();
                break;
        }

        Rotate();
    }

    /*───────────────────  MOVIMIENTO  ───────────────────*/

    void MoveTowardsPlayer()
    {
        if (!player) return;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    void Rotate()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    bool IsFullyInsideViewport(float m)
    {
        if (!mainCam) return false;
        Vector3 vp = mainCam.WorldToViewportPoint(transform.position);
        return vp.z > 0f &&
               vp.x >= m && vp.x <= 1f - m &&
               vp.y >= m && vp.y <= 1f - m;
    }

    /*───────────────────  COLISIONES  ───────────────────*/

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Projectile"))
        {
            Projectile p = col.collider.GetComponent<Projectile>();
            if (p && p.projectileColor == enemyColor)
            {
                Destroy(col.gameObject);
                DestroyEnemy();
            }
            return;
        }

        if (col.collider.CompareTag("Player"))
        {
            col.collider.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();
            DestroyEnemy();
        }
    }

    /*───────────────────  MUERTE / EXPLOSIÓN  ───────────────────*/

    public void DestroyEnemy()
    {
        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();

        if (explosionPrefab)
        {
            var boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (boom.TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = enemyColor;
            }
        }

        GameObject.FindGameObjectWithTag("Player")
                 ?.GetComponent<SlowMotion>()
                 ?.AddSlowMotionCharge();

        Destroy(gameObject);
    }

    /*───────────────────  REGISTRO EN ENEMYMANAGER  ───────────────────*/

    void OnEnable()  => EnemyManager.Instance?.RegisterEnemy(this);
    void OnDisable() => EnemyManager.Instance?.UnregisterEnemy(this);
}
