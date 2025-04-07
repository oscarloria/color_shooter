using UnityEngine;

/*
===========================================================================================
ShipBodyPistolIdle8Directions.cs

Muestra la animación idle (4..N sprites) de la PISTOLA en 8 direcciones, sin usar Animator.

1) Toma angleZ de 'shipTransform'.
2) Divide [0..360) en 8 sectores de 45° sumando +22.5° => rawIndex [0..7].
3) Mapea:
   0=Up,1=Up-Left,2=Left,3=Down-Left,4=Down,5=Down-Right,6=Right,7=Up-Right
4) Usa arrays pistolIdleUpSprites, pistolIdleLeftSprites, etc.
5) Avanza frames manualmente a framesPerSecond. 
   Si framesPerSecond=0, se queda en frame 0 (estático).
6) Debe activarse (enabled=true) sólo cuando la PISTOLA sea el arma activa.
7) Desactivarse (enabled=false) al cambiar a otra arma, para no solaparse con Idle de otras armas.
===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyPistolIdle8Directions : MonoBehaviour
{
    [Header("Sprites Idle (Pistola) en 8 direcciones")]
    public Sprite[] pistolIdleUpSprites;         
    public Sprite[] pistolIdleUpLeftSprites;     
    public Sprite[] pistolIdleLeftSprites;       
    public Sprite[] pistolIdleDownLeftSprites;   
    public Sprite[] pistolIdleDownSprites;       
    public Sprite[] pistolIdleDownRightSprites;  
    public Sprite[] pistolIdleRightSprites;      
    public Sprite[] pistolIdleUpRightSprites;    

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Frames por segundo (Idle de la pistola)")]
    public float framesPerSecond = 4f;

    // Variables internas
    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        // Obtener referencia al SpriteRenderer
        sr = GetComponent<SpriteRenderer>();
        Debug.Log("[ShipBodyPistolIdle8Directions] Awake() => SpriteRenderer asignado.");
    }

    void OnEnable()
    {
        // Al habilitar el script, reiniciamos la animación
        animTimer = 0f;
        currentFrame = 0;
        Debug.Log("[ShipBodyPistolIdle8Directions] OnEnable() => Idle de Pistola ACTIVADO.");
    }

    void OnDisable()
    {
        // Mensaje de que se desactiva
        Debug.Log("[ShipBodyPistolIdle8Directions] OnDisable() => Idle de Pistola DESACTIVADO.");
    }

    void Update()
    {
        // Mensaje de depuración en Update
//        Debug.Log("[ShipBodyPistolIdle8Directions] Update() => Calculando dirección para Idle Pistol.");

        if (shipTransform == null)
        {
            Debug.LogWarning("[ShipBodyPistolIdle8Directions] shipTransform es null, no se puede actualizar el Idle.");
            return;
        }

        // Alinear la posición con el Ship
        transform.position = shipTransform.position;
        // Evitar rotación local
        transform.rotation = Quaternion.identity;

        // 1) Tomar ángulo z (0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividir [0..360) en 8 sectores de 45°, sumando 22.5
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f);

        // 3) Convertir rawIndex => finalIndex (0=Up,1=UpLeft,...,7=UpRight)
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

        // Mensaje para ver qué dirección estamos usando
       // Debug.Log($"[ShipBodyPistolIdle8Directions] angleZ={angleZ:F2}, rawIndex={rawIndex}, finalIndex={finalIndex}");

        // 4) Elegir el array de sprites Idle para esa dirección
        switch (finalIndex)
        {
            case 0: currentAnim = pistolIdleUpSprites;        break;
            case 1: currentAnim = pistolIdleUpLeftSprites;    break;
            case 2: currentAnim = pistolIdleLeftSprites;      break;
            case 3: currentAnim = pistolIdleDownLeftSprites;  break;
            case 4: currentAnim = pistolIdleDownSprites;      break;
            case 5: currentAnim = pistolIdleDownRightSprites; break;
            case 6: currentAnim = pistolIdleRightSprites;     break;
            case 7: currentAnim = pistolIdleUpRightSprites;   break;
        }

        // 5) Si no hay sprites en esa dirección, salimos
        if (currentAnim == null || currentAnim.Length == 0)
        {
            Debug.LogWarning("[ShipBodyPistolIdle8Directions] currentAnim está vacío o null => no se muestra nada.");
            return;
        }

        // 6) Avanzar animación de forma manual
        if (framesPerSecond > 0f)
        {
            animTimer += Time.deltaTime * framesPerSecond;

            // Puede avanzar más de un frame si el FPS es alto y el game se lagea,
            // mejor usar while en caso de multiples saltos:
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
            // Si framesPerSecond <= 0 => siempre frame 0
            currentFrame = 0;
        }

        // 7) Asignar el sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}
