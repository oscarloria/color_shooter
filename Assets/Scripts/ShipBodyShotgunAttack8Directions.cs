using UnityEngine;

/*
===========================================================================================
DOCUMENTACIÓN INTERNA - ShipBodyShotgunAttack8Directions.cs

Este script maneja la animación de ataque para la escopeta (shotgun) en 8 direcciones,
sin usar Animator. Es muy similar a "ShipBodyAttack8Directions.cs" (pistola),
pero aquí tendrás arrays específicos para la animación de escopeta.

FUNCIONAMIENTO:
1) Se basa en la rotación z de 'shipTransform' (igual que en los demás scripts).
2) Divide los 360° en 8 sectores, centrando cada uno con +22.5°, para obtener rawIndex [0..7].
3) Se mapea rawIndex => finalIndex, donde:
   0 => Up, 1 => Up-Left, 2 => Left, 3 => Down-Left,
   4 => Down, 5 => Down-Right, 6 => Right, 7 => Up-Right
4) Usa arrays dedicados para escopeta: shotgunAttackUpSprites, shotgunAttackLeftSprites, etc.
5) Avanza los frames a 'framesPerSecond'. 
6) Este script debe ser "enabled = true" mientras dure la animación de ataque
   (lo maneja ShotgunShooting.cs) y luego "enabled = false" para volver a Idle o lo que sea.

REQUISITOS:
- Se agrega este script al mismo objeto "ShipBody" con un SpriteRenderer.
- "ShipBodyShotgunAttack8Directions" desactivado por defecto, 
  y se activa sólo cuando el escopetazo se dispara (ShotgunShooting dispara la corrutina).
- "framesPerSecond" ajusta la velocidad de la animación.
===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyShotgunAttack8Directions : MonoBehaviour
{
    [Header("Sprites de Ataque Escopeta (8 direcciones)")]
    public Sprite[] shotgunAttackUpSprites;         // finalIndex=0 => Up
    public Sprite[] shotgunAttackUpLeftSprites;     // finalIndex=1 => Up-Left
    public Sprite[] shotgunAttackLeftSprites;       // finalIndex=2 => Left
    public Sprite[] shotgunAttackDownLeftSprites;   // finalIndex=3 => Down-Left
    public Sprite[] shotgunAttackDownSprites;       // finalIndex=4 => Down
    public Sprite[] shotgunAttackDownRightSprites;  // finalIndex=5 => Down-Right
    public Sprite[] shotgunAttackRightSprites;      // finalIndex=6 => Right
    public Sprite[] shotgunAttackUpRightSprites;    // finalIndex=7 => Up-Right

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Velocidad de animación (frames por segundo)")]
    public float framesPerSecond = 6f;  // Ajusta según cuán rápida quieras la animación

    // Internos
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
        // Al habilitar este script, reiniciamos la anim
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

        // 1) Obtener ángulo [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividir en sectores de 45°, sumando 22.5 para centrar
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f); // [0..7]

        // 3) Convertir rawIndex => finalIndex 
        //    0=>Up,1=>UpLeft,2=>Left,3=>DownLeft,4=>Down,5=>DownRight,6=>Right,7=>UpRight
        int finalIndex;
        switch (rawIndex)
        {
            case 0: finalIndex = 0; break;
            case 1: finalIndex = 1; break;
            case 2: finalIndex = 2; break;
            case 3: finalIndex = 3; break;
            case 4: finalIndex = 4; break;
            case 5: finalIndex = 5; break;
            case 6: finalIndex = 6; break;
            case 7: finalIndex = 7; break;
            default: finalIndex = 0; break;
        }

        // 4) Elegir el array de sprites de ataque escopeta
        switch (finalIndex)
        {
            case 0: currentAnim = shotgunAttackUpSprites; break;
            case 1: currentAnim = shotgunAttackUpLeftSprites; break;
            case 2: currentAnim = shotgunAttackLeftSprites; break;
            case 3: currentAnim = shotgunAttackDownLeftSprites; break;
            case 4: currentAnim = shotgunAttackDownSprites; break;
            case 5: currentAnim = shotgunAttackDownRightSprites; break;
            case 6: currentAnim = shotgunAttackRightSprites; break;
            case 7: currentAnim = shotgunAttackUpRightSprites; break;
        }

        if (currentAnim == null || currentAnim.Length == 0) return;

        // 5) Avanzar la animación manual
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

        // 6) Mostrar el frame actual
        sr.sprite = currentAnim[currentFrame];
    }
}