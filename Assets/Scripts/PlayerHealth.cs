using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    public float invulnerabilityDuration = 2f;
    public GameObject explosionPrefab; // Prefab de la explosión que se instancia al destruir enemigos

    [Header("Explosión al recibir daño")]
    [Tooltip("Radio de la explosión que elimina a los enemigos cercanos.")]
    public float explosionRadius = 5f;
    [Tooltip("Prefab de la onda expansiva (shockwave) que se muestra al recibir daño. Se redimensiona según el radio.")]
    public GameObject shockwavePrefab;
    [Tooltip("Prefab del indicador visual del radio de explosión. Su diámetro se ajusta a 2 * explosionRadius.")]
    public GameObject explosionRadiusIndicatorPrefab;

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

        // Iniciar invulnerabilidad con efecto de parpadeo
        StartCoroutine(InvulnerabilityCoroutine());

        // Ejecutar la explosión que elimina a enemigos cercanos y muestra el indicador
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
        // Instanciar el efecto de onda expansiva (shockwave)
        if (shockwavePrefab != null)
        {
            GameObject shockwave = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
            // Escalar el shockwave para que su diámetro sea 2x explosionRadius
            shockwave.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, 1f);
        }

        // Instanciar el indicador visual del radio de explosión
        if (explosionRadiusIndicatorPrefab != null)
        {
            GameObject radiusIndicator = Instantiate(explosionRadiusIndicatorPrefab, transform.position, Quaternion.identity);
            // Escalarlo para que su diámetro sea igual a 2 * explosionRadius
            radiusIndicator.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, 1f);
        }

        // Buscar todos los colliders en el radio de la explosión
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                // Instanciar efecto de explosión en la posición del enemigo
                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, collider.transform.position, Quaternion.identity);
                }
                // Destruir el enemigo
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
