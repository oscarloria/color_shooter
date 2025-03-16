using UnityEngine;
using UnityEngine.SceneManagement;

public class SlotSelectionManager : MonoBehaviour
{
    // Este método se llama cuando se presiona el botón "New Game"
    // y carga la escena del juego (SampleScene).
    public void OnNewGameButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Este método se llama cuando se presiona el botón "Back"
    // y regresa al menú principal (MainMenuScene).
    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}