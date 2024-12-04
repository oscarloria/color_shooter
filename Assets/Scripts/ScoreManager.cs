using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int CurrentScore { get; private set; } = 0;

    public TextMeshProUGUI scoreText;

    private int nextLifeScoreThreshold = 5000; // Umbral para ganar una vida extra

    void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            // No destruir este objeto al cargar nuevas escenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Reiniciar la puntuación si estamos en la escena principal
        if (SceneManager.GetActiveScene().name == "SampleScene") // Reemplaza "SampleScene" con el nombre de tu escena principal si es diferente
        {
            CurrentScore = 0;
            nextLifeScoreThreshold = 5000; // Reiniciar el umbral de vida extra
            UpdateScoreText();
        }
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        UpdateScoreText();

        // Verificar si se ha alcanzado el umbral para ganar una vida extra
        if (CurrentScore >= nextLifeScoreThreshold)
        {
            // Obtener referencia al jugador
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.GainHealth();
                }
            }

            // Incrementar el siguiente umbral
            nextLifeScoreThreshold += 5000;
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntuación: " + CurrentScore;
        }
    }
}