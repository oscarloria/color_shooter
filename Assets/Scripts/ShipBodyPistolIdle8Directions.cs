using UnityEngine;

/*
===========================================================================================
ShipBodyPistolIdle8Directions.cs

Muestra la animación idle (4..N sprites) de la pistola en 8 direcciones, 
sin usar Animator.

FUNCIONAMIENTO:
1) Toma angleZ de 'shipTransform'.
2) Divide [0..360) en 8 sectores de 45°, usando (angleZ + 22.5f).
3) Mapea rawIndex => 0=Up,1=UpLeft,2=Left,3=DownLeft,4=Down,5=DownRight,6=Right,7=UpRight
4) Usa arrays pistolIdleUpSprites, pistolIdleLeftSprites, etc.
5) Recorre frames con framesPerSecond.
6) Debes activar este script (enabled=true) sólo cuando la pistola sea el arma actual
   y desactivarlo (enabled=false) al cambiar a otra arma.

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
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Al habilitar el script, reiniciamos la animación
        animTimer = 0f;
        currentFrame = 0;
    }

    void Update()
    {
        if (shipTransform == null) return;

        // Alinear posición con el Ship
        transform.position = shipTransform.position;
        // Sin rotación local
        transform.rotation = Quaternion.identity;

        // 1) Tomar ángulo z
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividir en 8 sectores de 45°, sumando 22.5
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f);

        // 3) Convertir rawIndex => finalIndex
        int finalIndex;
        switch (rawIndex)
        {
            case 0: finalIndex = 0; break; // Up
            case 1: finalIndex = 1; break; // UpLeft
            case 2: finalIndex = 2; break; // Left
            case 3: finalIndex = 3; break; // DownLeft
            case 4: finalIndex = 4; break; // Down
            case 5: finalIndex = 5; break; // DownRight
            case 6: finalIndex = 6; break; // Right
            case 7: finalIndex = 7; break; // UpRight
            default: finalIndex = 0; break;
        }

        // 4) Elegir array
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

        if (currentAnim == null || currentAnim.Length == 0) return;

        // 5) Avanzar anim manual
        if (framesPerSecond > 0f)
        {
            animTimer += Time.deltaTime * framesPerSecond;
            if (animTimer >= 1f)
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
            currentFrame = 0;
        }

        // 6) Asignar sprite
        sr.sprite = currentAnim[currentFrame];
    }
}
