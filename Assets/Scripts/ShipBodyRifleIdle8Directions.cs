using UnityEngine;

/*
===========================================================================================
ShipBodyRifleIdle8Directions.cs

Muestra la animación idle del RIFLE en 8 direcciones (sin usar Animator).
Cada dirección puede tener varios sprites para generar un mini-loop de idle.

FUNCIONAMIENTO:
1) Lee angleZ del 'shipTransform' (objeto que rota).
2) Divide [0..360) en 8 sectores de 45°, usando (angleZ + 22.5f).
3) Mapea rawIndex => 0=Up, 1=UpLeft, 2=Left, 3=DownLeft, 4=Down, 5=DownRight, 6=Right, 7=UpRight.
4) Usa arrays rifleIdleUpSprites, rifleIdleLeftSprites, etc. para cada dirección.
5) Avanza la animación manualmente a 'framesPerSecond'.
6) Debe activarse (enabled=true) sólo cuando el RIFLE sea el arma actual
   y desactivarse (enabled=false) al cambiar de arma, para evitar conflictos
   con otros idle scripts (pistola, escopeta, etc.).

CONFIGURACIÓN:
- Este script se agrega en el mismo objeto "ShipBody" con SpriteRenderer.
- Rellena en el Inspector las 8 direcciones de idle (rifleIdleUpSprites, etc.).
- "framesPerSecond" determina qué tan rápido avanza la animación de idle.
- Por defecto, se sugiere mantenerlo desactivado en el Inspector (o en tu PlayerController.Start),
  y encenderlo cuando el rifle sea seleccionado.

===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyRifleIdle8Directions : MonoBehaviour
{
    [Header("Sprites Idle (Rifle) en 8 direcciones")]
    public Sprite[] rifleIdleUpSprites;        
    public Sprite[] rifleIdleUpLeftSprites;    
    public Sprite[] rifleIdleLeftSprites;      
    public Sprite[] rifleIdleDownLeftSprites;  
    public Sprite[] rifleIdleDownSprites;      
    public Sprite[] rifleIdleDownRightSprites; 
    public Sprite[] rifleIdleRightSprites;     
    public Sprite[] rifleIdleUpRightSprites;   

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Frames por segundo (Idle del rifle)")]
    public float framesPerSecond = 4f; // Ajusta para animación del rifle

    // Internos
    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Al habilitar este script, reiniciamos la animación
    void OnEnable()
    {
        animTimer = 0f;
        currentFrame = 0;
    }

    void Update()
    {
        if (shipTransform == null) return;

        // Alinear la posición con el Ship
        transform.position = shipTransform.position;
        // Evitar rotación local
        transform.rotation = Quaternion.identity;

        // 1) Ángulo z en [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividimos en 8 sectores de 45°, centrando con +22.5f
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f); // [0..7]

        // 3) Convertir rawIndex => finalIndex
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

        // 4) Seleccionar el array de sprites según finalIndex
        switch (finalIndex)
        {
            case 0: currentAnim = rifleIdleUpSprites;        break;
            case 1: currentAnim = rifleIdleUpLeftSprites;    break;
            case 2: currentAnim = rifleIdleLeftSprites;      break;
            case 3: currentAnim = rifleIdleDownLeftSprites;  break;
            case 4: currentAnim = rifleIdleDownSprites;      break;
            case 5: currentAnim = rifleIdleDownRightSprites; break;
            case 6: currentAnim = rifleIdleRightSprites;     break;
            case 7: currentAnim = rifleIdleUpRightSprites;   break;
        }

        if (currentAnim == null || currentAnim.Length == 0) return;

        // 5) Avanzar animación manual
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

        // 6) Asignar el sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}
