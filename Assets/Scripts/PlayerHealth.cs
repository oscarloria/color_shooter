using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Maneja la vida del jugador, daño recibido e invulnerabilidad temporal.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    public float invulnerabilityDuration = 2f;
    public GameObject explosionPrefab; // Prefab de la explosión al recibir daño

    private int currentHealth;
    private bool isInvulnerable = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Actualizar la UI de vida
        LifeUI.Instance.UpdateLives(currentHealth);
    }

    public void TakeDamage()
    {
        if (isInvulnerable)
            return;

        currentHealth--;

        // Actualizar la UI de vida
        LifeUI.Instance.UpdateLives(currentHealth);

        // Feedback de daño
        StartCoroutine(InvulnerabilityCoroutine());

        // Eliminar enemigos cercanos
        DestroyNearbyEnemies();

        // Verificar si el jugador ha muerto
        if (currentHealth <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        // Guardar la puntuación actual
        if (ScoreManager.Instance != null)
        {
            int finalScore = ScoreManager.Instance.CurrentScore;
            PlayerPrefs.SetInt("FinalScore", finalScore);
        }
        else
        {
            PlayerPrefs.SetInt("FinalScore", 0);
        }

        // Cargar la escena de Game Over
        SceneManager.LoadScene("GameOverScene");
    }


    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        // Feedback visual de invulnerabilidad (parpadeo)
        float elapsedTime = 0f;
        bool isVisible = true;
        float flashInterval = 0.2f;

        while (elapsedTime < invulnerabilityDuration)
        {
            spriteRenderer.enabled = isVisible;
            isVisible = !isVisible;

            elapsedTime += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    void DestroyNearbyEnemies()
    {
        float explosionRadius = 5f; // Radio de la explosión
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                // Instanciar efecto de explosión
                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, collider.transform.position, Quaternion.identity);
                }

                Destroy(collider.gameObject);
            }
        }
    }

    public void GainHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            // Actualizar la UI de vida
            LifeUI.Instance.UpdateLives(currentHealth);
        }
    }
}