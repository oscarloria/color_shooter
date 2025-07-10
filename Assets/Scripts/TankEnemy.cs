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
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        bodySpriteRenderer = GetComponent<SpriteRenderer>();
        ApplyColor();
    }

    void Update()
    {
        if (player == null) return;
        RotateTowardsPlayer();
        MoveTowardsPlayer();
    }

    void OnEnable()
    {
        EnemyManager.Instance?.RegisterTankEnemy(this);
    }

    void OnDisable()
    {
        EnemyManager.Instance?.UnregisterTankEnemy(this);
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        float rotationSpeed = 200f;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void TakeDamage()
    {
        if (isDead) return;

        currentHealth--;
        StartCoroutine(DamageFeedback());

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    void Die()
    {
        ScoreManager.Instance?.AddScore(100);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (transform.Find("WeakPoint")?.GetComponent<SpriteRenderer>() is SpriteRenderer weakPointSprite)
            {
                if (explosion.TryGetComponent(out ParticleSystem ps))
                {
                    var main = ps.main;
                    main.startColor = weakPointSprite.color;
                }
            }
        }

        player?.GetComponent<SlowMotion>()?.AddSlowMotionCharge();
        
        Destroy(gameObject, 0.1f);
    }

    IEnumerator DamageFeedback()
    {
        SpriteRenderer bodySR = GetComponent<SpriteRenderer>();
        if (bodySR == null) yield break;
        SpriteRenderer weakPointSR = transform.Find("WeakPoint")?.GetComponent<SpriteRenderer>();
        Color originalBodyColor = bodySR.color;
        
        if(weakPointSR != null)
        {
            Color originalWeakPointColor = weakPointSR.color;
            Color flashColor = Color.white;
            bodySR.color = flashColor;
            weakPointSR.color = flashColor;
            yield return new WaitForSeconds(0.05f);
            bodySR.color = originalBodyColor;
            weakPointSR.color = originalWeakPointColor;
        } 
        else
        {
            bodySR.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            bodySR.color = originalBodyColor;
        }
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
            // Llama al daño en el jugador
            collision.collider.GetComponent<PlayerHealth>()?.TakeDamage();

            // --- LÍNEA FALTANTE AÑADIDA ---
            // Llama explícitamente al efecto de vibración de la cámara
            CameraShake.Instance?.ShakeCamera();

            if (!isDead)
            {
                isDead = true;
                Die();
            }
        }
    }
}