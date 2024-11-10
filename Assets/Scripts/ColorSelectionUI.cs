using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Agrega System.Linq para usar Where

public class ColorSelectorUI : MonoBehaviour
{
    public Image wImage;
    public Image aImage;
    public Image sImage;
    public Image dImage;

    private const float activeAlpha = 1f; // Opacidad completa (sin transparencia)
    private const float inactiveAlpha = 0.2f; // Opacidad reducida (transparente)

    // Pila de teclas presionadas para realizar el seguimiento
    private Stack<KeyCode> pressedKeys = new Stack<KeyCode>();

    void Update()
    {
        // Detecta las teclas presionadas y actualiza la pila
        if (Input.GetKeyDown(KeyCode.W)) UpdatePressedKeys(KeyCode.W);
        if (Input.GetKeyDown(KeyCode.A)) UpdatePressedKeys(KeyCode.A);
        if (Input.GetKeyDown(KeyCode.S)) UpdatePressedKeys(KeyCode.S);
        if (Input.GetKeyDown(KeyCode.D)) UpdatePressedKeys(KeyCode.D);

        // Detecta las teclas soltadas y actualiza la pila
        if (Input.GetKeyUp(KeyCode.W)) RemoveKeyFromStack(KeyCode.W);
        if (Input.GetKeyUp(KeyCode.A)) RemoveKeyFromStack(KeyCode.A);
        if (Input.GetKeyUp(KeyCode.S)) RemoveKeyFromStack(KeyCode.S);
        if (Input.GetKeyUp(KeyCode.D)) RemoveKeyFromStack(KeyCode.D);

        // Activa la última tecla presionada en la pila o resetea la transparencia si la pila está vacía
        if (pressedKeys.Count > 0)
        {
            SetActiveKey(pressedKeys.Peek());
        }
        else
        {
            ResetTransparency();
        }
    }

    void UpdatePressedKeys(KeyCode key)
    {
        // Agregar tecla a la pila si aún no está en ella
        if (!pressedKeys.Contains(key))
        {
            pressedKeys.Push(key);
            SetActiveKey(key);
        }
    }

    void RemoveKeyFromStack(KeyCode key)
    {
        // Eliminar la tecla soltada de la pila
        if (pressedKeys.Contains(key))
        {
            pressedKeys = new Stack<KeyCode>(pressedKeys.Where(k => k != key).Reverse());
        }
    }

    void SetActiveKey(KeyCode key)
    {
        SetImageAlpha(wImage, key == KeyCode.W ? activeAlpha : inactiveAlpha);
        SetImageAlpha(aImage, key == KeyCode.A ? activeAlpha : inactiveAlpha);
        SetImageAlpha(sImage, key == KeyCode.S ? activeAlpha : inactiveAlpha);
        SetImageAlpha(dImage, key == KeyCode.D ? activeAlpha : inactiveAlpha);
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color color = img.color;
        color.a = alpha;
        img.color = color;
    }

    void ResetTransparency()
    {
        SetImageAlpha(wImage, inactiveAlpha);
        SetImageAlpha(aImage, inactiveAlpha);
        SetImageAlpha(sImage, inactiveAlpha);
        SetImageAlpha(dImage, inactiveAlpha);
    }
}