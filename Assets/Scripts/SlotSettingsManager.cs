using UnityEngine;
using UnityEngine.UI;

public class SlotSettingsManager : MonoBehaviour
{
    // Toggle para activar o desactivar el AutoAim.
    public Toggle autoAimToggle;

    void Start()
    {
        if (autoAimToggle == null)
        {
            Debug.LogError("autoAimToggle is not assigned in the inspector!");
            return;
        }
        // Inicializamos el toggle con el valor actual de GameSettings.autoAim.
        autoAimToggle.isOn = GameSettings.autoAim;
        autoAimToggle.onValueChanged.AddListener(OnAutoAimToggleChanged);
        Debug.Log("SlotSettingsManager started. AutoAim value: " + autoAimToggle.isOn);
    }

    // Este método se llama cuando cambia el valor del toggle.
    void OnAutoAimToggleChanged(bool isOn)
    {
        GameSettings.autoAim = isOn;
        Debug.Log("AutoAim toggled. New value: " + GameSettings.autoAim);
    }
}
