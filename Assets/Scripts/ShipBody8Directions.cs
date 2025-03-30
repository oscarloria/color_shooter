using UnityEngine;

/// <summary>
/// Muestra uno de 8 sprites en ShipBody según la rotación del Ship.
/// Ship rota físicamente; ShipBody se mantiene sin rotar.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ShipBody8Directions : MonoBehaviour
{
    [Header("Sprites en 8 direcciones")]
    [Tooltip("Orden: 0=Arriba, 1=Arriba-Derecha, 2=Derecha, 3=Abajo-Derecha, 4=Abajo, 5=Abajo-Izquierda, 6=Izquierda, 7=Arriba-Izquierda")]
    public Sprite[] directionalSprites;

    [Header("Referencia al objeto Ship (que sí rota)")]
    public Transform shipTransform; // Arrástralo desde la jerarquía

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Asegurarnos de que tenemos 8 sprites y una referencia válida
        if (directionalSprites == null || directionalSprites.Length < 8) return;
        if (shipTransform == null) return;

        // 1) Coincidir la posición de ShipBody con la de Ship (si quieres que estén “sobrepuestos”)
        transform.position = shipTransform.position;

        // 2) Evitar que ShipBody rote físicamente: 
        //    lo forzamos a identity o a una rotación que no cambie.
        transform.rotation = Quaternion.identity;

        // 3) Obtener la rotación del Ship en Z [0..360)
        float angleZ = shipTransform.eulerAngles.z;

        // OPCIONAL: Si quieres que “angleZ=0” signifique “arriba” o “derecha”, 
        // ajusta con un offset:
        // angleZ -= 90f;  // si tu arte asume 0°=derecha, por ejemplo
        // Normalizar:
        // if (angleZ < 0) angleZ += 360f;

        // 4) Convertir el ángulo en un índice [0..7], 
        //    cada sector de 45° (360°/8=45°).
        int index = Mathf.RoundToInt(angleZ / 45f) % 8;

        // 5) Asignar el sprite correspondiente
        spriteRenderer.sprite = directionalSprites[index];
    }
}
