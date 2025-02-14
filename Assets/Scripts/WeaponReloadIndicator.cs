using UnityEngine;
using UnityEngine.UI;

public class WeaponReloadIndicator : MonoBehaviour
{
    private Image indicatorImage;

    void Awake()
    {
        indicatorImage = GetComponent<Image>();
        if(indicatorImage == null)
        {
            Debug.LogError("WeaponReloadIndicator: No se encontró un componente Image.");
        }
    }

    /// <summary>
    /// Actualiza el indicador de recarga con un valor de 0 a 1.
    /// </summary>
    /// <param name="progress">Progreso de recarga (0 = vacío, 1 = completo).</param>
    public void UpdateIndicator(float progress)
    {
        indicatorImage.fillAmount = Mathf.Clamp01(progress);
    }

    /// <summary>
    /// Reinicia el indicador (lo deja vacío).
    /// </summary>
    public void ResetIndicator()
    {
        indicatorImage.fillAmount = 0f;
    }
}
