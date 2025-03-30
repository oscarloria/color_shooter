using UnityEngine;

/// <summary>
/// Muestra uno de 8 sprites según el ángulo de su objeto padre, 
/// pero sin rotar físicamente el hijo (solo cambiando sprites).
/// 
/// Requiere un SpriteRenderer en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Character8DirectionSprite : MonoBehaviour
{
    [Header("Sprites en 8 direcciones")]
    [Tooltip("Orden: 0=Arriba, 1=Arriba-Derecha, 2=Derecha, 3=Abajo-Derecha, 4=Abajo, 5=Abajo-Izquierda, 6=Izquierda, 7=Arriba-Izquierda")]
    public Sprite[] directionalSprites;

    private SpriteRenderer spriteRenderer;
    private Transform parentTransform;  // Referencia al padre, que rota

    void Awake()
    {
        // Conseguir el SpriteRenderer local
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Asumimos que este objeto es hijo del Player
        parentTransform = transform.parent;
    }

    void Update()
    {
        // 1) Asegurarnos de que tengamos 8 sprites
        if (directionalSprites == null || directionalSprites.Length < 8)
            return;

        // 2) Para que este hijo NO gire físicamente, forzamos la rotación local a cero
        transform.localRotation = Quaternion.identity;

        // 3) Obtener el ángulo Z del padre (0..360)
        float angle = parentTransform.eulerAngles.z;
        // Si quieres que "angle=0" equivalga a "arriba" en tu sprite, 
        // no necesitas sumar ni restar nada extra. 
        // (Si tu arte asume "0= derecha", ajusta con: angle -= 90f, etc.)

        // 4) Convertir el ángulo en un índice de 0 a 7
        //    360° / 8 = 45° por sector
        int index = Mathf.RoundToInt(angle / 45f) % 8;

        // 5) Asignar el sprite
        spriteRenderer.sprite = directionalSprites[index];
    }
}
