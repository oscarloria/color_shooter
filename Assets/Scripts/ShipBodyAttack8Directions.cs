using UnityEngine;

/*
===========================================================================================
DOCUMENTACIÓN INTERNA - ShipBodyAttack8Directions.cs

Este script maneja la animación de ATAQUE (arma tipo pistola) en 8 direcciones, 
sin usar Animator. Similar a "ShipBody8Directions.cs" (que maneja Idle).

FUNCIONAMIENTO:
1) Se basa en la rotación z de 'shipTransform' (igual que el idle).
2) Divide los 360° en 8 sectores, 
   mapeando 0°=Up, 90°=Left, 180°=Down, 270°=Right, 
   con diagonales intermedias (arriba-izquierda, etc.). 
3) Usa 8 arrays: attackUpSprites, attackUpLeftSprites, etc. 
   para reproducir la animación de ataque (4 frames, p.ej). 
4) framesPerSecond define cuán rápido se muestran los frames. 
5) En general, se activa este script (enabled=true) 
   durante ~0.5s cuando el jugador dispara. Luego se desactiva 
   y se re-activa el script de Idle, 
   evitando que ambos scripts compitan por asignar sprites.

REQUISITOS:
- "ShipBodyAttack8Directions.cs" agregado al mismo objeto (ShipBody) 
  que tiene un SpriteRenderer.
- El objeto "Ship" (transform) que rota físicamente y define eulerAngles.z.
- Lógica externa que active/desactive este script al disparar.
===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyAttack8Directions : MonoBehaviour
{
    [Header("Sprites de Ataque en 8 direcciones")]
    public Sprite[] attackUpSprites;         // finalIndex=0 => Up
    public Sprite[] attackUpLeftSprites;     // finalIndex=1 => Up-Left
    public Sprite[] attackLeftSprites;       // finalIndex=2 => Left
    public Sprite[] attackDownLeftSprites;   // finalIndex=3 => Down-Left
    public Sprite[] attackDownSprites;       // finalIndex=4 => Down
    public Sprite[] attackDownRightSprites;  // finalIndex=5 => Down-Right
    public Sprite[] attackRightSprites;      // finalIndex=6 => Right
    public Sprite[] attackUpRightSprites;    // finalIndex=7 => Up-Right

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Frames por segundo (ataque)")]
    public float framesPerSecond = 6f; // algo más rápido que idle

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
        // Al habilitar el script, reiniciamos los contadores 
        // para empezar la animación de ataque desde frame 0
        animTimer = 0f;
        currentFrame = 0;
    }

    void Update()
    {
        if (shipTransform == null) return;

        // 1) Posición igual al Ship
        transform.position = shipTransform.position;
        // 2) Sin rotación local
        transform.rotation = Quaternion.identity;

        // 3) Calcular ángulo [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 4) Dividir en 8 sectores => rawIndex
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f); // [0..7]

        // 5) Mapeo manual => finalIndex
        //    0 => Up, 1 => UpLeft, 2 => Left, 3 => DownLeft,
        //    4 => Down, 5 => DownRight, 6 => Right, 7 => UpRight.
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

        // 6) Elegir el array de ataque
        switch (finalIndex)
        {
            case 0: currentAnim = attackUpSprites;        break;
            case 1: currentAnim = attackUpLeftSprites;    break;
            case 2: currentAnim = attackLeftSprites;      break;
            case 3: currentAnim = attackDownLeftSprites;  break;
            case 4: currentAnim = attackDownSprites;      break;
            case 5: currentAnim = attackDownRightSprites; break;
            case 6: currentAnim = attackRightSprites;     break;
            case 7: currentAnim = attackUpRightSprites;   break;
        }

        if (currentAnim == null || currentAnim.Length == 0) return;

        // 7) Avanzar animación
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
            // si framesPerSecond <= 0 => frame 0 fijo
            currentFrame = 0;
        }

        sr.sprite = currentAnim[currentFrame];
    }
}