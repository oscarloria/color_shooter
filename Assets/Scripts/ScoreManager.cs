using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;  // Instancia global del ScoreManager

    public int CurrentScore = 0;          // Puntaje actual del jugador

    [Header("UI")]
    public TextMeshProUGUI scoreText;       // Referencia al TextMeshPro que muestra el puntaje

    void Awake()
    {
        // Al no querer persistir el ScoreManager entre partidas, no usamos DontDestroyOnLoad.
        // Si ya existe una instancia en la escena, la destruimos para que se use la nueva.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Método para agregar puntos al puntaje actual
    public void AddScore(int points)
    {
        CurrentScore += points;
        Debug.Log("Score added: " + points + ", Total Score: " + CurrentScore);
        UpdateScoreUI();
    }

    // Método para reiniciar el puntaje (al iniciar una nueva partida)
    public void ResetScore()
    {
        CurrentScore = 0;
        UpdateScoreUI();
    }

    // Actualiza el texto del puntaje en la UI
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = CurrentScore.ToString();
        }
        else
        {
            Debug.LogWarning("ScoreText is not assigned in ScoreManager.");
        }
    }
}
