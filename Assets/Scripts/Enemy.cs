using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Color enemyColor; // Color del enemigo
    public float speed = 3f; // Velocidad de movimiento hacia el jugador
    public GameObject explosionPrefab; // Prefab de la explosión
    public float minRotationSpeed = -180f; // Velocidad mínima de rotación
    public float maxRotationSpeed = 180f;  // Velocidad máxima de rotación
    private float rotationSpeed;           // Velocidad de rotación asignada al enemigo
    private Transform player; // Referencia a la nave del jugador
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = enemyColor;

        // Asignar una velocidad de rotación aleatoria dentro del rango especificado
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
    }

    void Update()
    {
        MoveTowardsPlayer();
        Rotate();
    }

    void Rotate()
    {
        // Rotar alrededor del eje Z (perpendicular al plano XY)
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Mensaje de depuración para cualquier colisión
        Debug.Log("Enemy ha colisionado con: " + collision.collider.name);

        if (collision.collider.CompareTag("Projectile"))
        {
            Debug.Log("Colisión con un proyectil detectada.");

            Projectile projectile = collision.collider.GetComponent<Projectile>();

            if (projectile != null)
            {
                // Compara los colores
                if (projectile.projectileColor == enemyColor)
                {
                    Debug.Log("Color coincidente. Enemigo y proyectil serán destruidos.");

                    // Destruye al enemigo y al proyectil
                    DestroyEnemy();
                    Destroy(collision.gameObject);
                }
                else
                {
                    Debug.Log("Color no coincide. El proyectil rebotará.");
                    // El proyectil rebotará automáticamente debido a la física
                }
            }
        }
        else if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("El enemigo ha tocado al jugador.");

            // Destruir al enemigo
            DestroyEnemy();


// Llamar al efecto de cámara shake si lo deseas
      if (CameraShake.Instance != null)
       {
          CameraShake.Instance.ShakeCamera();
        }
        
            // Aquí puedes añadir lógica para reducir la salud del jugador
        }
        else
        {
            Debug.Log("Colisión con otro objeto: " + collision.collider.name);
        }
    }

    void DestroyEnemy()
    {
        // Instanciar el efecto de explosión
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

// Obtener referencia al PlayerController para llamar a AddSlowMotionCharge
    PlayerController playerController = FindObjectOfType<PlayerController>();
    if (playerController != null)
    {
        playerController.AddSlowMotionCharge();
    }


        // Añadir puntuación si corresponde
        //if (GameManager.Instance != null)
       // {
       //     GameManager.Instance.AddScore(10);
      //  }

        // Destruir al enemigo
        Destroy(gameObject);

        
    }
}