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
    private float rotationSpeed;          // Velocidad de rotación asignada aleatoriamente
    private Transform player;             // Referencia al transform del jugador
    private SpriteRenderer spriteRenderer;// Referencia al SpriteRenderer del enemigo
    private float phaseOffset;            // Desfase aleatorio para que cada enemigo zigzaguee de forma distinta

    /// <summary>
    /// Método de inicialización. Configura las referencias, el color y los parámetros de rotación y zigzag.
    /// </summary>
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

    /// <summary>
    /// Método llamado una vez por frame. Actualiza el movimiento y la rotación del enemigo.
    /// </summary>
    void Update()
    {
        MoveTowardsPlayerZZ();  // Movimiento hacia el jugador con efecto zigzag
        Rotate();               // Rotación constante del enemigo
    }

    /// <summary>
    /// Se llama cuando el objeto se habilita. Registra este enemigo en el EnemyManager.
    /// Nota: Asegúrate de que EnemyManager tenga métodos para registrar enemigos de tipo EnemyZZ.
    /// </summary>
    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            // Se asume que EnemyManager cuenta con métodos RegisterEnemyZZ y UnregisterEnemyZZ.
            EnemyManager.Instance.RegisterEnemyZZ(this);
        }
    }

    /// <summary>
    /// Se llama cuando el objeto se deshabilita. Desregistra este enemigo del EnemyManager.
    /// </summary>
    void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemyZZ(this);
        }
    }

    /// <summary>
    /// Rota al enemigo alrededor del eje Z a una velocidad constante.
    /// </summary>
    void Rotate()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Mueve al enemigo hacia el jugador aplicando un desplazamiento lateral para lograr el efecto zigzag.
    /// </summary>
    void MoveTowardsPlayerZZ()
    {
        if (player != null)
        {
            // Calcular la dirección directa hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;

            // Calcular un vector perpendicular a la dirección hacia el jugador
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            // Calcular el offset lateral usando una función seno para generar el zigzag
            float offset = Mathf.Sin(Time.time * zigzagFrequency + phaseOffset) * zigzagAmplitude;

            // Combinar el movimiento directo con el desplazamiento lateral
            Vector3 moveVector = (direction * speed + perpendicular * offset) * Time.deltaTime;
            transform.position += moveVector;
        }
    }

    /// <summary>
    /// Maneja las colisiones del enemigo con proyectiles y con el jugador.
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
                // Si el color del proyectil coincide con el color del enemigo, destruir ambos
                if (projectile.projectileColor == enemyColor)
                {
                    DestroyEnemy();
                    Destroy(collision.gameObject);
                }
                else
                {
                    // El proyectil rebotará automáticamente por la física
                    // Aquí puedes agregar lógica adicional si lo deseas
                }
            }
        }
        // Verificar si colisiona con el jugador
        else if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("EnemyZZ ha colisionado con el jugador.");

            // Obtener el componente PlayerHealth del jugador y aplicarle daño
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }

            // Ejecutar el efecto de cámara shake
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeCamera();
            }

            // Destruir al enemigo
            DestroyEnemy();
        }
    }

    /// <summary>
    /// Maneja la destrucción del enemigo, incluyendo la instanciación de efectos visuales y la lógica de juego.
    /// </summary>
    void DestroyEnemy()
    {
        // Sumar puntos al destruir al enemigo
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
        }

        // Instanciar el efecto de explosión en la posición del enemigo y ajustar el color de las partículas
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

        // Recargar la cámara lenta al destruir al enemigo (si aplica)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            SlowMotion slowMotion = playerObject.GetComponent<SlowMotion>();
            if (slowMotion != null)
            {
                slowMotion.AddSlowMotionCharge();
            }
        }

        // Destruir el objeto enemigo
        Destroy(gameObject);
    }
}
