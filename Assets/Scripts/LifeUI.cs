using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la interfaz de usuario de las vidas del jugador.
/// </summary>
public class LifeUI : MonoBehaviour
{
    public static LifeUI Instance;

    public Image[] lifeImages; // Array de imágenes que representan las vidas

    void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateLives(int currentHealth)
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (i < currentHealth)
            {
                Color color = lifeImages[i].color;
                color.a = 1f; // Opacidad total
                lifeImages[i].color = color;
            }
            else
            {
                Color color = lifeImages[i].color;
                color.a = 0.3f; // Transparencia para indicar vida perdida
                lifeImages[i].color = color;
            }
        }
    }
}