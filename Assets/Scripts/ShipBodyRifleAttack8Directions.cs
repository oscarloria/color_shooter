using UnityEngine;

/*
===========================================================================================
ShipBodyRifleAttack8Directions.cs

Análogo a los demás scripts de animación sin Animator, pero para el ataque del rifle
(en 8 direcciones). Se activa mientras el rifle esté disparando (isFiring) 
y se desactiva cuando se deja de disparar.

FUNCIONAMIENTO:
- Revisa la rotación .z del "shipTransform".
- Divide la circunferencia en 8 sectores, centrados con +22.5°, para determinar [0..7].
- Usa arrays "rifleAttackUpSprites", "rifleAttackLeftSprites", etc.
- Avanza frames a "framesPerSecond".
- Se asume que otro script (RifleShooting) habilita/deshabilita este cuando se dispara.

===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyRifleAttack8Directions : MonoBehaviour
{
    [Header("Sprites de Ataque (Rifle) en 8 direcciones")]
    public Sprite[] rifleAttackUpSprites;        
    public Sprite[] rifleAttackUpLeftSprites;    
    public Sprite[] rifleAttackLeftSprites;      
    public Sprite[] rifleAttackDownLeftSprites;  
    public Sprite[] rifleAttackDownSprites;      
    public Sprite[] rifleAttackDownRightSprites; 
    public Sprite[] rifleAttackRightSprites;     
    public Sprite[] rifleAttackUpRightSprites;   

    [Header("Referencia al Ship que rota")]
    public Transform shipTransform;

    [Header("Velocidad de animación (frames por segundo)")]
    public float framesPerSecond = 8f; // un poco rápido, ya que es rifle

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
        // Al habilitar el script, reiniciamos la anim
        animTimer = 0f;
        currentFrame = 0;
    }

    void Update()
    {
        if (shipTransform == null) return;

        // Igualar posición
        transform.position = shipTransform.position;
        // Sin rotación local
        transform.rotation = Quaternion.identity;

        // 1) Ángulo [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) sector => [0..7]
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f);

        // 3) mapear a 0..7 => Up, UpLeft, Left, etc.
        int finalIndex = 0;
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
        }

        // 4) Elegir array
        switch (finalIndex)
        {
            case 0: currentAnim = rifleAttackUpSprites; break;
            case 1: currentAnim = rifleAttackUpLeftSprites; break;
            case 2: currentAnim = rifleAttackLeftSprites; break;
            case 3: currentAnim = rifleAttackDownLeftSprites; break;
            case 4: currentAnim = rifleAttackDownSprites; break;
            case 5: currentAnim = rifleAttackDownRightSprites; break;
            case 6: currentAnim = rifleAttackRightSprites; break;
            case 7: currentAnim = rifleAttackUpRightSprites; break;
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
                    currentFrame = 0;
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