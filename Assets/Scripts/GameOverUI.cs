using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // Asegurarse de que el cursor esté visible y desbloqueado en la escena de Game Over.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        // Cargar la escena del menú principal
        SceneManager.LoadScene("MainMenuScene");
    }
}
