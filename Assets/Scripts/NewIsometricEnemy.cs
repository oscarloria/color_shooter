using UnityEngine;

/// <summary>
/// MOLDE C (PADRE - Prueba): 
/// Reemplazo de 'Enemy.cs' para probar la nueva física.
/// Tiene Rigidbody Kinematic y un Collider SÓLIDO (Trigger=OFF). 
/// Se encarga de moverse, rebotar proyectiles (mismatch) y morir.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] // Forzamos los componentes
public class NewIsometricEnemy : MonoBehaviour
{
    // --- Tus variables del Inspector (sin cambios) ---
    [Header("Apariencia")]
    public Color enemyColor = Color.white;
    public float minRotationSpeed = -180f;
    public float maxRotationSpeed =  180f;
    public GameObject explosionPrefab;

    [Header("Movimiento")]
    public float speed = 2f;
    public float pauseDuration = 0.6f;

    [Header("Visibilidad (Viewport)")]
    public bool randomizeViewportMargin = false;
    [Range(0f,0.49f)]
    public float viewportMargin = 0.03f;
    [Range(0f,0.49f)]
    public float minRandomMargin = 0.10f;
    [Range(0f,0.49f)]
    public float maxRandomMargin = 0.20f;

    [Header("Puntuación")]
    public int scoreValue = 100;

    // --- Variables Privadas ---
    Transform player;
    SpriteRenderer sr;
    Camera mainCam;
    Rigidbody2D rb; // <--- Añadido

    float rotationSpeed;
    float pauseTimer;
    float margin;

    enum State { Approaching, Paused, Attacking }
    State state = State.Approaching;

    /*───────────────────  CICLO DE VIDA (Modificado) ───────────────────*/

    void Start()
    {
        player  = GameObject.FindGameObjectWithTag("Player")?.transform;
        sr      = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
        
        // --- INICIO DE MODIFICACIONES ---
        // 1. Configurar el Rigidbody
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; 
        
        // 2. Configurar Collider Sólido
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = false; // Asegurarse de que sea SÓLIDO
        // --- FIN DE MODIFICACIONES ---

        if (sr) sr.color = enemyColor; // El padre muestra el color
        
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        margin = randomizeViewportMargin ? Random.Range(minRandomMargin, maxRandomMargin) : viewportMargin;
    }

    /*───────────────────  MOVIMIENTO (Sin Cambios) ───────────────────*/
    // Estos métodos son idénticos a tu 'Enemy.cs'
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

    /*───────────────────  COLISIONES (Modificado) ───────────────────*/

    // --- NUEVO MÉTODO ---
    /// <summary>
    /// El hijo (NewEnemyMatchDetector) llama a esto cuando hay un MATCH de color.
    /// </summary>
    public void HandleMatch()
    {
        // Este enemigo básico muere de 1 golpe.
        DestroyEnemy();
    }

    // --- MÉTODO DE COLISIÓN MODIFICADO ---
    // Esto es para colisiones SÓLIDAS (Trigger = OFF)
    void OnCollisionEnter2D(Collision2D col)
    {
        // 1. Si un PROYECTIL nos golpea, es un MISMATCH.
        // El hijo trigger no lo interceptó.
        if (col.collider.CompareTag("Projectile"))
        {
            // No hacemos nada. La física se encarga del rebote.
            return;
        }

        // 2. Si chocamos con el JUGADOR
        if (col.collider.CompareTag("Player"))
        {
            col.collider.GetComponent<PlayerHealth>()?.TakeDamage();
            CameraShake.Instance?.ShakeCamera();
            DestroyEnemy();
        }
    }

    /*───────────────────  MUERTE Y REGISTRO ───────────────────*/

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

    // --- REGISTRO EN ENEMYMANAGER (Comentado para la prueba) ---
    // NOTA: He comentado esto para que puedas probar sin modificar tu EnemyManager.
    // Si la prueba funciona, necesitaremos enseñar al EnemyManager a
    // manejar 'NewIsometricEnemy' o unificar los scripts.
    
    // void OnEnable()  => EnemyManager.Instance?.RegisterEnemy(this); // <-- 'this' ya no es 'Enemy'
    // void OnDisable() => EnemyManager.Instance?.UnregisterEnemy(this); // <-- 'this' ya no es 'Enemy'
}