using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // Obtener la puntuación final guardada en PlayerPrefs
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        scoreText.text = "Puntuación: " + finalScore;
    }

    public void Retry()
    {
        // Reiniciar el juego cargando la escena principal
        SceneManager.LoadScene("SampleScene"); // Asegúrate de que "SampleScene" es el nombre de tu escena principal
    }

    public void Quit()
    {
        // Salir del juego (funciona en la compilación final)
        Application.Quit();

        // Si estás en el editor de Unity y quieres probar, puedes usar:
        // UnityEditor.EditorApplication.isPlaying = false;
    }
}