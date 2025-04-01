using UnityEngine;

/*
===========================================================================================
DOCUMENTACIÓN INTERNA - ShipBody8Directions.cs

PROYECTO Y CONFIGURACIÓN

1. Este proyecto es un shooter 2D top-down donde el objeto "Ship" rota en base a:
   - El cursor del mouse (cuando autoAim está desactivado),
   - O un autoAim que busca el enemigo más cercano (cuando está activado).
   De cualquier forma, "Ship" obtiene un ángulo Euler (z) en [0..360), 
   donde Unity define:
       0°   = derecha
       90°  = arriba
       180° = izquierda
       270° = abajo
   Esto no siempre coincide con cómo está dibujado el sprite en la herramienta de arte.

2. Nuestro Gato Mago (reemplazando a la nave) se ve "arriba" cuando 
   Unity dice 0°. Sin embargo, queremos que:
       - 0°  signifique "Up" 
       - 90° signifique "Left"
       - 180° => "Down"
       - 270° => "Right"
   y las diagonales entre medias.

3. Para lograrlo sin usar un Animator:
   - Dividimos manualmente los 360° en 8 sectores de 45° cada uno,
   - Centramos cada sector sumando 22.5°, 
   - Luego usamos un switch-case para mapear 
     rawIndex (0..7) a una dirección finalIndex (0..7):
       0 => Up
       1 => Up-Left
       2 => Left
       3 => Down-Left
       4 => Down
       5 => Down-Right
       6 => Right
       7 => Up-Right
   - De ese modo, "cursor a la izquierda" produce angleZ ~90°, 
     rawIndex=2 => "leftSprites".

4. Cada dirección tiene un array de sprites (1..N frames). El script reproduce 
   esos frames en bucle a "framesPerSecond". 
   Si framesPerSecond=0, permanece en el frame 0 (efecto estático).

5. Con este planteamiento, el Gato Mago muestra un sprite (o mini-animación)
   correspondiente a la dirección real del cursor en 2D, sin necesidad de
   Animator Controller ni transiciones.

CONVENCIÓN DE ANGULOS Y SECTORES

- Unity en 2D (por defecto):
    angleZ=0°   => Derecha
    angleZ=90°  => Arriba
    angleZ=180° => Izquierda
    angleZ=270° => Abajo

- Aquí, forzamos:
    angleZ=0°   => Up (dirIndex=0)
    angleZ=90°  => Left (dirIndex=2)
    angleZ=180° => Down (dirIndex=4)
    angleZ=270° => Right (dirIndex=6)
  y usamos una conversión manual:
    sector = (angleZ + 22.5) % 360
    rawIndex = floor(sector/45)  => [0..7]
    luego un switch-case => finalIndex (0..7),
    0=Up,1=Up-Left,2=Left,3=Down-Left,4=Down,5=Down-Right,6=Right,7=Up-Right

6. Si en algún futuro se cambia la forma de rotar el sprite en la escena 
   (por ejemplo, se decide que 0° en Unity deba corresponder a Derecha 
   y 90°=Arriba), podrías ajustar la tabla de conversión. 
   Pero por ahora, este método garantiza que "cursor a la izquierda" => 
   "leftSprites", "cursor arriba" => "upSprites", y así sucesivamente.

===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBody8Directions : MonoBehaviour
{
    [Header("Sprites para cada dirección (arrays de 1..N frames)")]
    public Sprite[] upSprites;         // finalIndex=0 => Up
    public Sprite[] upLeftSprites;     // finalIndex=1 => Up-Left
    public Sprite[] leftSprites;       // finalIndex=2 => Left
    public Sprite[] downLeftSprites;   // finalIndex=3 => Down-Left
    public Sprite[] downSprites;       // finalIndex=4 => Down
    public Sprite[] downRightSprites;  // finalIndex=5 => Down-Right
    public Sprite[] rightSprites;      // finalIndex=6 => Right
    public Sprite[] upRightSprites;    // finalIndex=7 => Up-Right

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform; // Donde se ejecuta PlayerMovement, etc.

    [Header("Frames por Segundo (para la animación manual)")]
    [Tooltip("Si es 0, mostrará siempre frame 0 (sin animar). Si >0, hará loop con esa velocidad.")]
    public float framesPerSecond = 4f;

    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Si no hay referencia, no podemos hacer nada
        if (shipTransform == null) return;

        // Mantener la posición del Ship
        transform.position = shipTransform.position;

        // Evitar rotación local
        transform.rotation = Quaternion.identity;

        // 1) angleZ en [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividimos en sectores de 45°, sumando 22.5 para centrar
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f); // [0..7]

        // 3) Convertir rawIndex => finalIndex, 
        //    para que 0 => Up, 2 => Left, 4 => Down, 6 => Right
        int finalIndex;
        switch (rawIndex)
        {
            case 0: finalIndex = 0; break; // Up
            case 1: finalIndex = 1; break; // Up-Left
            case 2: finalIndex = 2; break; // Left
            case 3: finalIndex = 3; break; // Down-Left
            case 4: finalIndex = 4; break; // Down
            case 5: finalIndex = 5; break; // Down-Right
            case 6: finalIndex = 6; break; // Right
            case 7: finalIndex = 7; break; // Up-Right
            default: finalIndex = 0; break; 
        }

        // 4) Elegir el array de sprites según finalIndex
        switch (finalIndex)
        {
            case 0: currentAnim = upSprites;        break;
            case 1: currentAnim = upLeftSprites;    break;
            case 2: currentAnim = leftSprites;      break;
            case 3: currentAnim = downLeftSprites;  break;
            case 4: currentAnim = downSprites;      break;
            case 5: currentAnim = downRightSprites; break;
            case 6: currentAnim = rightSprites;     break;
            case 7: currentAnim = upRightSprites;   break;
        }

        // 5) Si no hay sprites en esa dirección, no asignamos nada
        if (currentAnim == null || currentAnim.Length == 0)
        {
            return;
        }

        // 6) Avanzar la animación manual
        if (framesPerSecond > 0f)
        {
            animTimer += Time.deltaTime * framesPerSecond;
            if (animTimer >= 1f)
            {
                animTimer -= 1f;
                currentFrame++;
                if (currentFrame >= currentAnim.Length)
                {
                    currentFrame = 0; // volver al inicio
                }
            }
        }
        else
        {
            // framesPerSecond <= 0 => siempre frame 0
            currentFrame = 0;
        }

        // 7) Asignar el sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}
