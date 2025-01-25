using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Clase que representa al enemigo tipo tanque, con un punto débil central.
/// </summary>
public class TankEnemy : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public float speed = 1f;        // Velocidad de movimiento hacia el jugador
    public int maxHealth = 3;       // Vida máxima del enemigo
    public GameObject explosionPrefab; // Prefab de la explosión al morir

    [HideInInspector]
    public int currentHealth;       // Vida actual del enemigo

    // Nuevo campo para el color del enemigo (vulnerable)
    [HideInInspector]
    public Color enemyColor = Color.white;

    private Transform player;
    private SpriteRenderer bodySpriteRenderer; // Sprite del cuerpo principal del tanque

    void Start()
    {
        currentHealth = maxHealth;

        // Obtener referencia al jugador
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Obtener el sprite renderer del cuerpo principal del tanque
        bodySpriteRenderer = GetComponent<SpriteRenderer>();
        if (bodySpriteRenderer == null)
        {
            Debug.LogWarning("TankEnemy: No se encontró SpriteRenderer en el cuerpo principal del tanque.");
        }

        // Aplicar el color del cuerpo (blanco) y dejar que el WeakPoint tenga su color
        ApplyColor();
    }

    void Update()
    {
        RotateTowardsPlayer();
        MoveTowardsPlayer();
    }

    void OnEnable()
    {
        // Registrar este TankEnemy en el EnemyManager
        if (EnemyManager.Instance != null)
        {
            // Suponiendo que tienes un método específico para tank-enemies:
            EnemyManager.Instance.RegisterTankEnemy(this);
        }
    }

    void OnDisable()
    {
        // Desregistrar este TankEnemy del EnemyManager
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterTankEnemy(this);
        }
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            // Moverse hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void RotateTowardsPlayer()
    {
        if (player != null)
        {
            // Calcular la dirección hacia el jugador
            Vector3 direction = player.position - transform.position;

            // Calcular el ángulo hacia el jugador
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Crear la rotación objetivo
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            // Rotar suavemente hacia el jugador
            float rotationSpeed = 200f; // Velocidad de rotación en grados por segundo
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        currentHealth--;

        // Feedback de daño (flash y shake)
        StartCoroutine(DamageFeedback());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
        }

        // Instanciar partículas de explosión
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // Ajustar el color de las partículas al color del punto débil
            SpriteRenderer weakPointSprite = transform.Find("WeakPoint").GetComponent<SpriteRenderer>();
            if (weakPointSprite != null)
            {
                ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = weakPointSprite.color;
                }
            }
        }

        // Recargar cámara lenta
        if (player != null)
        {
            SlowMotion slowMotion = player.GetComponent<SlowMotion>();
            if (slowMotion != null)
            {
                slowMotion.AddSlowMotionCharge();
            }
        }

        // Destruir el enemigo
        Destroy(gameObject);
    }

    IEnumerator DamageFeedback()
    {
        // Obtener los SpriteRenderers del cuerpo y del WeakPoint
        SpriteRenderer bodySpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer weakPointSpriteRenderer = transform.Find("WeakPoint").GetComponent<SpriteRenderer>();

        // Almacenar los colores originales
        Color originalBodyColor = bodySpriteRenderer.color;
        Color originalWeakPointColor = weakPointSpriteRenderer.color;

        // Obtener el color del WeakPoint
        Color flashColor = weakPointSpriteRenderer.color;

        // Cambiar los colores al color del WeakPoint para el destello solo durante el shake
        bodySpriteRenderer.color = flashColor;
        weakPointSpriteRenderer.color = flashColor;

        // Pequeño shake
        Vector3 originalPosition = transform.position;
        float shakeDuration = 0.2f;
        float elapsedTime = 0f;
        float magnitude = 0.5f;

        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPosition + new Vector3(x, y, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Restaurar la posición y los colores originales
        transform.position = originalPosition;
        bodySpriteRenderer.color = originalBodyColor;
        weakPointSpriteRenderer.color = originalWeakPointColor;
    }

    // Este método se llama después de asignar el enemyColor desde EnemySpawner
    // Se asegura de que el cuerpo principal del tanque permanezca blanco, mientras
    // que el WeakPoint mantiene su propio color (asignado en WeakPoint.cs)
    public void ApplyColor()
    {
        if (bodySpriteRenderer != null)
        {
            // El cuerpo del tanque siempre blanco
            bodySpriteRenderer.color = Color.white;
        }
        // El WeakPoint se encarga de su propio color en WeakPoint.cs al inicio.
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si colisiona con el jugador
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("El TankEnemy ha tocado al jugador.");

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
            Die();

            // Aquí puedes añadir lógica adicional si lo deseas
        }
    }
}
