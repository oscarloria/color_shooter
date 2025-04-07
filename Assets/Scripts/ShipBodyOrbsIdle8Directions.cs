using UnityEngine;

/*
===========================================================================================
ShipBodyOrbsIdle8Directions.cs

Muestra la animación idle de los ORBS (orbes de defensa) en 8 direcciones, 
sin usar Animator. Similar a los demás scripts de Idle.

FUNCIONAMIENTO:
1) Toma el ángulo .z de "shipTransform".
2) Divide [0..360) en 8 sectores de 45°, sumando 22.5 => rawIndex [0..7].
3) Mapea:
   0 => Up, 1 => Up-Left, 2 => Left, 3 => Down-Left, 4 => Down, 
   5 => Down-Right, 6 => Right, 7 => Up-Right.
4) Usa arrays orbsIdleUpSprites, orbsIdleLeftSprites, etc.
5) Avanza los frames manualmente con "framesPerSecond".
6) Debe activarse sólo cuando los orbs sean el arma actual 
   (desactiva este script si se elige otra arma).

CONFIGURACIÓN:
- Agregar a "ShipBody" con un SpriteRenderer.
- Asignar en el Inspector los 8 arrays: orbsIdleUpSprites, etc.
- framesPerSecond define la velocidad de la mini-animación.
- Por defecto, en tu PlayerController, cuando currentWeapon=4 (Orbs), 
  enciende este script y apaga los idle de las otras armas.

===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyOrbsIdle8Directions : MonoBehaviour
{
    [Header("Sprites Idle (Orbs) en 8 direcciones")]
    public Sprite[] orbsIdleUpSprites;        
    public Sprite[] orbsIdleUpLeftSprites;    
    public Sprite[] orbsIdleLeftSprites;      
    public Sprite[] orbsIdleDownLeftSprites;  
    public Sprite[] orbsIdleDownSprites;      
    public Sprite[] orbsIdleDownRightSprites; 
    public Sprite[] orbsIdleRightSprites;     
    public Sprite[] orbsIdleUpRightSprites;   

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Velocidad de animación (frames por segundo)")]
    public float framesPerSecond = 4f; // Ajusta a tu preferencia

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
        // Resetear la animación al habilitar
        animTimer = 0f;
        currentFrame = 0;
    }

    void Update()
    {
        if (shipTransform == null) return;

        // 1) Mantener posición con el Ship
        transform.position = shipTransform.position;
        // Sin rotación local
        transform.rotation = Quaternion.identity;

        // 2) Tomar angleZ [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 3) Sector de 45°, sumando 22.5 para centrar
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f); // [0..7]

        // 4) rawIndex => finalIndex
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

        // 5) Seleccionar el array segun finalIndex
        switch (finalIndex)
        {
            case 0: currentAnim = orbsIdleUpSprites; break;
            case 1: currentAnim = orbsIdleUpLeftSprites; break;
            case 2: currentAnim = orbsIdleLeftSprites; break;
            case 3: currentAnim = orbsIdleDownLeftSprites; break;
            case 4: currentAnim = orbsIdleDownSprites; break;
            case 5: currentAnim = orbsIdleDownRightSprites; break;
            case 6: currentAnim = orbsIdleRightSprites; break;
            case 7: currentAnim = orbsIdleUpRightSprites; break;
        }

        if (currentAnim == null || currentAnim.Length == 0) return;

        // 6) Avanzar la animación
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
            // si framesPerSecond <= 0 => frame 0
            currentFrame = 0;
        }

        // 7) Asignar el sprite
        sr.sprite = currentAnim[currentFrame];
    }
}
