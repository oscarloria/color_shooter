using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TankEnemy : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public float speed = 1f;        
    public int maxHealth = 3;       
    public GameObject explosionPrefab; 

    [HideInInspector]
    public int currentHealth;       

    [HideInInspector]
    public Color enemyColor = Color.white;

    private Transform player;
    private SpriteRenderer bodySpriteRenderer;

    // NUEVO: Para evitar múltiples llamadas a Die()
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        bodySpriteRenderer = GetComponent<SpriteRenderer>();
        if (bodySpriteRenderer == null)
        {
            Debug.LogWarning("TankEnemy: No se encontró SpriteRenderer en el cuerpo principal del tanque.");
        }

        ApplyColor();
    }

    void Update()
    {
        RotateTowardsPlayer();
        MoveTowardsPlayer();
    }

    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterTankEnemy(this);
        }
    }

    void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterTankEnemy(this);
        }
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void RotateTowardsPlayer()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            float rotationSpeed = 200f;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        if (isDead) return; // Si ya está muerto, no seguir procesando

        currentHealth--;
        StartCoroutine(DamageFeedback());

        if (currentHealth <= 0)
        {
            // Marcar isDead antes de llamar a Die() para prevenir repetición
            isDead = true;
            Die();
        }
    }

    void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
        }

        // Intentar soltar coins
        EnemyCoinDrop coinDrop = GetComponent<EnemyCoinDrop>();
        if (coinDrop != null)
        {
            coinDrop.TryDropCoins();
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
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

        if (player != null)
        {
            SlowMotion slowMotion = player.GetComponent<SlowMotion>();
            if (slowMotion != null)
            {
                slowMotion.AddSlowMotionCharge();
            }
        }

        Destroy(gameObject);
    }

    IEnumerator DamageFeedback()
    {
        SpriteRenderer bodySR = GetComponent<SpriteRenderer>();
        SpriteRenderer weakPointSR = transform.Find("WeakPoint").GetComponent<SpriteRenderer>();

        Color originalBodyColor = bodySR.color;
        Color originalWeakPointColor = weakPointSR.color;

        Color flashColor = weakPointSR.color;

        bodySR.color = flashColor;
        weakPointSR.color = flashColor;

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

        transform.position = originalPosition;
        bodySR.color = originalBodyColor;
        weakPointSR.color = originalWeakPointColor;
    }

    public void ApplyColor()
    {
        if (bodySpriteRenderer != null)
        {
            bodySpriteRenderer.color = Color.white;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("El TankEnemy ha tocado al jugador.");

            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }

            if (!isDead)
            {
                isDead = true;
                Die();
            }
        }
    }
}