using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    // Panel de pausa que contiene los botones "Continue" y "Exit"
    public GameObject pauseMenuUI;

    // Variable para saber si el juego está en pausa
    private bool isPaused = false;

    void Update()
    {
        // Si se presiona la tecla Escape, alterna entre pausar y reanudar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // Método para pausar el juego y mostrar el menú de pausa
    public void PauseGame()
    {
        pauseMenuUI.SetActive(true); // Mostrar menú de pausa
        Time.timeScale = 0f;         // Pausar el juego
        isPaused = true;
        // Liberar el cursor para interactuar con el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Método para reanudar el juego y ocultar el menú de pausa
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Ocultar menú de pausa
        Time.timeScale = 1f;          // Reanudar el juego
        isPaused = false;
        // La lógica de bloqueo del cursor se maneja en otros scripts (por ejemplo, PlayerMovement)
    }

    // Método para salir al menú principal
    public void ExitToMainMenu()
    {
        // Reanudar el juego
        Time.timeScale = 1f;
        // Registrar el evento de carga de escena para forzar la visibilidad del cursor
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Cargar la escena MainMenuScene
        SceneManager.LoadScene("MainMenuScene");
    }

    // Este método se llama cuando se carga una escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "MainMenuScene")
        {
            // Forzar que el cursor quede visible y sin bloqueo en la MainMenuScene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Cancelar la suscripción para evitar que se ejecute en futuras cargas
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
