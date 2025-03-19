using UnityEngine;

/// <summary>
/// Clase que representa el comportamiento de un enemigo en el juego.
/// Los enemigos persiguen al jugador, rotan y reaccionan a colisiones con proyectiles y el jugador.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public Color enemyColor; // Color del enemigo
    public float speed = 2f; // Velocidad de movimiento hacia el jugador
    public float minRotationSpeed = -180f; // Velocidad mínima de rotación en grados por segundo
    public float maxRotationSpeed = 180f;  // Velocidad máxima de rotación en grados por segundo
    public GameObject explosionPrefab; // Prefab de la explosión que se instancia al destruir al enemigo

    [Header("Score Settings")]
    public int scoreValue = 100; // Puntos otorgados al destruir este enemigo

    // Variables privadas
    private float rotationSpeed;       // Velocidad de rotación asignada al enemigo
    private Transform player;          // Referencia al transform del jugador
    private SpriteRenderer spriteRenderer; // Referencia al SpriteRenderer del enemigo

    /// <summary>
    /// Método llamado al iniciar el script. Inicializa referencias y configura el enemigo.
    /// </summary>
    void Start()
    {
        // Obtener referencia al jugador mediante su etiqueta
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
    }

    /// <summary>
    /// Método llamado una vez por frame. Actualiza el movimiento y rotación del enemigo.
    /// </summary>
    void Update()
    {
        MoveTowardsPlayer(); // Moverse hacia el jugador
        Rotate();            // Rotar sobre su eje Z
    }

    /// <summary>
    /// Se llama cuando el objeto se habilita. Registra este enemigo en el EnemyManager.
    /// </summary>
    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterEnemy(this);
        }
    }

    /// <summary>
    /// Se llama cuando el objeto se deshabilita. Desregistra este enemigo del EnemyManager.
    /// </summary>
    void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
    }

    /// <summary>
    /// Rota al enemigo sobre su eje Z a una velocidad constante.
    /// </summary>
    void Rotate()
    {
        // Rotar alrededor del eje Z (perpendicular al plano XY)
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Mueve al enemigo hacia la posición actual del jugador.
    /// </summary>
    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            // Calcular la dirección hacia el jugador y mover al enemigo
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Maneja las colisiones del enemigo con otros objetos.
    /// </summary>
    /// <param name="collision">Información sobre la colisión detectada.</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si colisiona con un proyectil
        if (collision.collider.CompareTag("Projectile"))
        {
            Projectile projectile = collision.collider.GetComponent<Projectile>();

            if (projectile != null)
            {
                // Compara los colores del enemigo y el proyectil
                if (projectile.projectileColor == enemyColor)
                {
                    // Destruye al enemigo y al proyectil si los colores coinciden
                    DestroyEnemy();
                    Destroy(collision.gameObject);
                }
                else
                {
                    // El proyectil rebotará automáticamente debido a la física
                    // Aquí puedes añadir lógica adicional si lo deseas
                }
            }
        }
        // Verificar si colisiona con el jugador
        else if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("El enemigo ha tocado al jugador.");

            // Obtener el componente PlayerHealth del jugador
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }

            // Llamar al efecto de cámara shake
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera();
            }

            // Destruir al enemigo
            DestroyEnemy();
        }
    }

    /// <summary>
    /// Maneja la destrucción del enemigo, incluyendo efectos visuales y la asignación de puntos.
    /// </summary>
    public void DestroyEnemy()
    {
        // Añadir puntos al destruir al enemigo (valor configurable desde el Inspector)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }



// Llamar a TryDropCoins() si este enemigo tiene el script EnemyCoinDrop
        EnemyCoinDrop coinDrop = GetComponent<EnemyCoinDrop>();
        if (coinDrop != null)
        {
            coinDrop.TryDropCoins();
        }





        // Instanciar el efecto de explosión en la posición del enemigo
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // Ajustar el color de las partículas al color del enemigo
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = enemyColor;
            }
        }

        // Obtener referencia al jugador para recargar la cámara lenta
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            SlowMotion slowMotion = playerObject.GetComponent<SlowMotion>();
            if (slowMotion != null)
            {
                // Recargar la cámara lenta al destruir al enemigo
                slowMotion.AddSlowMotionCharge();
            }
        }

        // Destruir al enemigo
        Destroy(gameObject);
    }
}
