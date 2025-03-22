using UnityEngine;

/// <summary>
/// Clase que representa un enemigo que se desplaza en zigzag hacia el jugador.
/// Este script es independiente (no utiliza herencia) y contiene las mismas características generales que el enemigo base.
/// </summary>
public class EnemyZZ : MonoBehaviour
{
    [Header("Configuración del Enemigo Zigzag")]
    public Color enemyColor;                // Color del enemigo
    public float speed = 2f;                // Velocidad de movimiento hacia el jugador
    public float minRotationSpeed = -180f;  // Velocidad mínima de rotación en grados por segundo
    public float maxRotationSpeed = 180f;   // Velocidad máxima de rotación en grados por segundo
    public GameObject explosionPrefab;      // Prefab de la explosión que se instanciará al destruirse

    [Header("Parámetros de Zigzag")]
    [Tooltip("Amplitud del movimiento lateral para el efecto zigzag.")]
    public float zigzagAmplitude = 1f;
    [Tooltip("Frecuencia de oscilación para el efecto zigzag.")]
    public float zigzagFrequency = 2f;

    // Variables privadas
    private float rotationSpeed;        // Velocidad de rotación asignada aleatoriamente
    private Transform player;           // Referencia al transform del jugador
    private SpriteRenderer spriteRenderer;
    private float phaseOffset;          // Desfase aleatorio para que cada enemigo zigzaguee de forma distinta

    void Start()
    {
        // Obtener referencia al jugador por su etiqueta "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Obtener el componente SpriteRenderer y asignar el color del enemigo
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }

        // Asignar una velocidad de rotación aleatoria dentro del rango especificado
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

        // Asignar un desfase aleatorio para el efecto zigzag
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        MoveTowardsPlayerZZ();  // Movimiento hacia el jugador con zigzag
        Rotate();               // Rotación constante
    }

    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterEnemyZZ(this);
        }
    }

    void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemyZZ(this);
        }
    }

    void Rotate()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void MoveTowardsPlayerZZ()
    {
        if (player != null)
        {
            // Dirección hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;

            // Vector perpendicular
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            // Offset lateral con sin() para zigzag
            float offset = Mathf.Sin(Time.time * zigzagFrequency + phaseOffset) * zigzagAmplitude;

            // Combinar avance y zigzag
            Vector3 moveVector = (direction * speed + perpendicular * offset) * Time.deltaTime;
            transform.position += moveVector;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Proyectil
        if (collision.collider.CompareTag("Projectile"))
        {
            Projectile projectile = collision.collider.GetComponent<Projectile>();
            if (projectile != null)
            {
                if (projectile.projectileColor == enemyColor)
                {
                    DestroyEnemy(); 
                    Destroy(collision.gameObject);
                }
                // si no coincide color, el proyectil rebota por la física
            }
        }
        // Jugador
        else if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("EnemyZZ ha colisionado con el jugador.");

            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera();
            }

            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        // +Score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
        }

        // Intentar soltar Lumi-Coins
        EnemyCoinDrop coinDrop = GetComponent<EnemyCoinDrop>();
        if (coinDrop != null)
        {
            coinDrop.TryDropCoins();
        }

        // Explosion
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = enemyColor;
            }
        }

        // Recargar la cámara lenta al destruir al enemigo
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            SlowMotion slowMotion = playerObject.GetComponent<SlowMotion>();
            if (slowMotion != null)
            {
                slowMotion.AddSlowMotionCharge();
            }
        }

        // Destruir
        Destroy(gameObject);
    }
}