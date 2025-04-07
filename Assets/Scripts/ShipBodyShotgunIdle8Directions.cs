using UnityEngine;

/*
===========================================================================================
ShipBodyShotgunIdle8Directions.cs

Muestra la animación idle de la ESCOPETA en 8 direcciones, sin Animator.
Cada dirección puede tener 1..N sprites para un mini-loop.

FUNCIONAMIENTO:
1) Toma angleZ de 'shipTransform' para ver hacia dónde mira.
2) Divide [0..360) en 8 sectores de 45°, sumando 22.5° => rawIndex [0..7].
3) Mapea rawIndex => finalIndex (0=Up,1=UpLeft,2=Left,3=DownLeft,4=Down,5=DownRight,6=Right,7=UpRight).
4) Usa arrays shotgunIdleUpSprites, shotgunIdleLeftSprites, etc. 
   (1 array por dirección).
5) Avanza la animación manualmente a 'framesPerSecond'.
6) Debe habilitarse (enabled=true) sólo cuando la escopeta sea el arma actual
   y deshabilitarse (enabled=false) al cambiar a otra arma.

CONFIGURACIÓN:
- Agregar este script al "ShipBody" que tenga un SpriteRenderer.
- Rellenar en el Inspector los arrays con sprites idle de la escopeta para cada dirección.
- Por defecto, framesPerSecond=4. Ajusta a tu gusto.
- Asegúrate de que, en el Inspector, si NO quieres que empiece activo, desmarcar “Enabled”
  o dejar que PlayerController desactive/active según currentWeapon=2.

===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyShotgunIdle8Directions : MonoBehaviour
{
    [Header("Sprites Idle (Escopeta) en 8 direcciones")]
    public Sprite[] shotgunIdleUpSprites;         
    public Sprite[] shotgunIdleUpLeftSprites;     
    public Sprite[] shotgunIdleLeftSprites;       
    public Sprite[] shotgunIdleDownLeftSprites;   
    public Sprite[] shotgunIdleDownSprites;       
    public Sprite[] shotgunIdleDownRightSprites;  
    public Sprite[] shotgunIdleRightSprites;      
    public Sprite[] shotgunIdleUpRightSprites;    

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Frames por segundo (Idle de la escopeta)")]
    public float framesPerSecond = 4f; // Ajusta a tu gusto

    // Variables internas
    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Debug.Log("[ShipBodyShotgunIdle8Directions] Awake() => Referencia al SpriteRenderer obtenida.");
    }

    // Cada vez que se habilita este script, reiniciamos la animación
    void OnEnable()
    {
        animTimer = 0f;
        currentFrame = 0;

        Debug.Log("[ShipBodyShotgunIdle8Directions] OnEnable() => Idle ESCOPETA ACTIVADA. Reset animTimer/currentFrame.");
    }

    void OnDisable()
    {
        Debug.Log("[ShipBodyShotgunIdle8Directions] OnDisable() => Idle ESCOPETA DESACTIVADA.");
    }

    void Update()
    {
        if (shipTransform == null)
        {
            Debug.LogWarning("[ShipBodyShotgunIdle8Directions] shipTransform es null; no se puede actualizar Idle Escopeta.");
            return;
        }

        // 1) Alinear la posición con el Ship
        transform.position = shipTransform.position;
        transform.rotation = Quaternion.identity;

        // 2) Calcular angleZ en [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 3) Dividir en 8 sectores: (angleZ + 22.5) => rawIndex [0..7]
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f);

        // 4) Convertir rawIndex => finalIndex
        int finalIndex = 0;
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
        }

       // Debug.Log($"[ShipBodyShotgunIdle8Directions] Update => angleZ={angleZ:F2}, rawIndex={rawIndex}, finalIndex={finalIndex}");

        // 5) Seleccionar el array de sprites segun finalIndex
        switch (finalIndex)
        {
            case 0: currentAnim = shotgunIdleUpSprites; break;
            case 1: currentAnim = shotgunIdleUpLeftSprites; break;
            case 2: currentAnim = shotgunIdleLeftSprites; break;
            case 3: currentAnim = shotgunIdleDownLeftSprites; break;
            case 4: currentAnim = shotgunIdleDownSprites; break;
            case 5: currentAnim = shotgunIdleDownRightSprites; break;
            case 6: currentAnim = shotgunIdleRightSprites; break;
            case 7: currentAnim = shotgunIdleUpRightSprites; break;
        }

        if (currentAnim == null || currentAnim.Length == 0)
        {
            Debug.LogWarning("[ShipBodyShotgunIdle8Directions] No hay sprites en currentAnim => no se dibuja nada.");
            return;
        }

        // 6) Avanzar animación manual
        if (framesPerSecond > 0f)
        {
            animTimer += Time.deltaTime * framesPerSecond;

            // Evitar saltos bruscos si la tasa de frames es alta y el juego se pausa un momento
            while (animTimer >= 1f)
            {
                animTimer -= 1f;
                currentFrame++;
                if (currentFrame >= currentAnim.Length)
                {
                    currentFrame = 0; // loop
                }
            }
        }
        else
        {
            // framesPerSecond <= 0 => frame 0
            currentFrame = 0;
        }

        // 7) Asignar el sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}
