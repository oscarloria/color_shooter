using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // Instancia global del ScoreManager (no persiste entre escenas, salvo High Score en PlayerPrefs)
    public static ScoreManager Instance;  

    [Header("Current Score Settings")]
    [Tooltip("Current score for this session. Resets each time the scene is loaded.")]
    public int CurrentScore = 0;

    [Header("UI References")]
    // Texto para mostrar el puntaje actual
    public TextMeshProUGUI scoreText;
    // Texto para mostrar el puntaje máximo (High Score almacenado en PlayerPrefs)
    public TextMeshProUGUI highScoreText;

    // Clave en PlayerPrefs para el High Score
    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        // Evita duplicados en la misma escena
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

    void Start()
    {
        // Al iniciar la escena, mostrar el puntaje actual (que suele iniciar en 0)
        UpdateScoreUI();

        // Leer el High Score almacenado en PlayerPrefs
        int storedHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateHighScoreUI(storedHighScore);
    }

    /// <summary>
    /// Suma puntos al puntaje actual. Si supera el High Score, se actualiza en PlayerPrefs.
    /// </summary>
    /// <param name="points">Cantidad de puntos que se añaden al puntaje.</param>
    public void AddScore(int points)
    {
        CurrentScore += points;
        Debug.Log($"Score added: {points}, Total Score: {CurrentScore}");
        UpdateScoreUI();

        // Revisar si superamos el High Score
        int storedHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (CurrentScore > storedHighScore)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, CurrentScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI(CurrentScore);
            Debug.Log($"New High Score: {CurrentScore}");
        }
    }

    /// <summary>
    /// Reinicia el puntaje actual a cero (no afecta el High Score).
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0;
        UpdateScoreUI();
    }

    /// <summary>
    /// Actualiza la UI del puntaje actual, agregando la etiqueta "Score:".
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {CurrentScore}";
        }
        else
        {
            Debug.LogWarning("ScoreManager: scoreText is not assigned.");
        }
    }

    /// <summary>
    /// Actualiza la UI del High Score, agregando la etiqueta "Hi Score:".
    /// </summary>
    /// <param name="newHighScore">El nuevo high score (o el ya almacenado en PlayerPrefs).</param>
    private void UpdateHighScoreUI(int newHighScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"Hi Score: {newHighScore}";
        }
        else
        {
            Debug.LogWarning("ScoreManager: highScoreText is not assigned.");
        }
    }


    public void ClearHighScore()
{
    // Eliminar la clave que almacena el High Score
    PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
    PlayerPrefs.Save();
    // Actualizamos la UI para mostrar 0 (o lo que prefieras)
    UpdateHighScoreUI(0);
    Debug.Log("High Score borrado");
}

}
