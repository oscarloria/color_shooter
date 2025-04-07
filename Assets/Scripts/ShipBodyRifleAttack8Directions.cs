using UnityEngine;

/*
===========================================================================================
ShipBodyRifleAttack8Directions.cs

Muestra la animación de ATAQUE del RIFLE en 8 direcciones (sin usar Animator).
Se habilita (enabled=true) mientras isFiring sea true en RifleShooting 
y se desactiva (enabled=false) al dejar de disparar.

FUNCIONAMIENTO:
1) Lee angleZ de "shipTransform" (0..360).
2) Divide la circunferencia en 8 sectores de 45°, sumando +22.5° => rawIndex [0..7].
3) Mapea rawIndex => finalIndex (0=Up,1=UpLeft,2=Left,3=DownLeft,4=Down,5=DownRight,6=Right,7=UpRight).
4) Usa arrays rifleAttackUpSprites, rifleAttackLeftSprites, etc., 
   avanzando frames a 'framesPerSecond'.
5) Se desactiva cuando dejas de disparar, volviendo al script de Idle (Rifle).

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
    public float framesPerSecond = 8f; // Valor mayor => animación más rápida

    // Internos
    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Debug.Log("[ShipBodyRifleAttack8Directions] Awake => SpriteRenderer asignado.");
    }

    void OnEnable()
    {
        // Al habilitar, reiniciamos la animación
        animTimer = 0f;
        currentFrame = 0;
        Debug.Log("[ShipBodyRifleAttack8Directions] OnEnable => ATAQUE RIFLE ACTIVADO. Reseteando frames.");
    }

    void OnDisable()
    {
        Debug.Log("[ShipBodyRifleAttack8Directions] OnDisable => ATAQUE RIFLE DESACTIVADO.");
    }

    void Update()
    {
        if (shipTransform == null)
        {
            Debug.LogWarning("[ShipBodyRifleAttack8Directions] shipTransform es null => no se puede animar.");
            return;
        }

        // Igualar posición con el Ship
        transform.position = shipTransform.position;
        transform.rotation = Quaternion.identity;

        // 1) Obtener angleZ en [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividir en 8 sectores de 45°, sumando 22.5
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f);

        // 3) mapear rawIndex => finalIndex
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

//        Debug.Log($"[ShipBodyRifleAttack8Directions] Update => angleZ={angleZ:F2}, rawIndex={rawIndex}, finalIndex={finalIndex}");

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

        if (currentAnim == null || currentAnim.Length == 0)
        {
            Debug.LogWarning("[ShipBodyRifleAttack8Directions] currentAnim está vacío => no se dibuja nada.");
            return;
        }

        // 5) Avanzar animación manual
        if (framesPerSecond > 0f)
        {
            animTimer += Time.deltaTime * framesPerSecond;

            // Evitamos saltos grandes usando while
            while (animTimer >= 1f)
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
            // framesPerSecond <= 0 => frame 0
            currentFrame = 0;
        }

        // 6) Asignar sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}
