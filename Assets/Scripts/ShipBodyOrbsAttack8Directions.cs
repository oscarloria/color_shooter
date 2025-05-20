using UnityEngine;

/*
===========================================================================================
ShipBodyOrbsAttack8Directions.cs

Muestra la animación de ATAQUE de los ORBS (orbes) en 8 direcciones (sin usar Animator).
Se habilita (enabled=true) mientras disparas con los orbes y
se desactiva (enabled=false) al dejar de atacar, volviendo al Idle.

FUNCIONAMIENTO:
1) Lee angleZ de "shipTransform" (0..360).
2) Divide [0..360) en 8 sectores de 45°, sumando +22.5°, => rawIndex [0..7].
3) Mapea rawIndex => finalIndex (0=Up,1=UpLeft,2=Left,3=DownLeft,4=Down,5=DownRight,6=Right,7=UpRight).
4) Usa arrays orbsAttackUpSprites, orbsAttackLeftSprites, etc., 
   avanzando frames a 'framesPerSecond'.
5) Se desactiva cuando terminas de disparar, volviendo al script de Idle (Orbs).

CONFIGURACIÓN:
- Agrega este script al "ShipBody" con un SpriteRenderer.
- En el Inspector, rellena las 8 direcciones: orbsAttackUpSprites, orbsAttackLeftSprites, etc.
- framesPerSecond > 0 para que haya animación. 
- DefenseOrbShooting (o tu manager de orbes) habilitará (enabled=true) este script
  cuando inicies el ataque, y lo deshabilitará (enabled=false) cuando termines.
===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyOrbsAttack8Directions : MonoBehaviour
{
    [Header("Sprites de ATAQUE (Orbs) en 8 direcciones")]
    public Sprite[] orbsAttackUpSprites;
    public Sprite[] orbsAttackUpLeftSprites;
    public Sprite[] orbsAttackLeftSprites;
    public Sprite[] orbsAttackDownLeftSprites;
    public Sprite[] orbsAttackDownSprites;
    public Sprite[] orbsAttackDownRightSprites;
    public Sprite[] orbsAttackRightSprites;
    public Sprite[] orbsAttackUpRightSprites;

    [Header("Referencia al Ship que rota")]
    public Transform shipTransform;

    [Header("Velocidad de animación (frames por segundo)")]
    public float framesPerSecond = 6f; // Ajusta a tu gusto

    // Variables internas
    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Debug.Log("[ShipBodyOrbsAttack8Directions] Awake => SpriteRenderer asignado.");
    }

    void OnEnable()
    {
        // Al habilitar, reiniciar la anim
        animTimer = 0f;
        currentFrame = 0;
        // Debug.Log("[ShipBodyOrbsAttack8Directions] OnEnable => ATAQUE ORBS ACTIVADO. Reseteando frames.");
    }

    void OnDisable()
    {
        // Debug.Log("[ShipBodyOrbsAttack8Directions] OnDisable => ATAQUE ORBS DESACTIVADO.");
    }

    void Update()
    {
        if (shipTransform == null)
        {
            // Debug.LogWarning("[ShipBodyOrbsAttack8Directions] shipTransform es null => no se puede animar Orbs Attack.");
            return;
        }

        // Igualar posición
        transform.position = shipTransform.position;
        transform.rotation = Quaternion.identity;

        // 1) Ángulo [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) sector => [0..7], sumando 22.5
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f);

        // 3) mapear rawIndex => finalIndex
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

        // Debug.Log($"[ShipBodyOrbsAttack8Directions] Update => angleZ={angleZ:F2}, rawIndex={rawIndex}, finalIndex={finalIndex}");

        // 4) Elegir array
        switch (finalIndex)
        {
            case 0: currentAnim = orbsAttackUpSprites; break;
            case 1: currentAnim = orbsAttackUpLeftSprites; break;
            case 2: currentAnim = orbsAttackLeftSprites; break;
            case 3: currentAnim = orbsAttackDownLeftSprites; break;
            case 4: currentAnim = orbsAttackDownSprites; break;
            case 5: currentAnim = orbsAttackDownRightSprites; break;
            case 6: currentAnim = orbsAttackRightSprites; break;
            case 7: currentAnim = orbsAttackUpRightSprites; break;
        }

        if (currentAnim == null || currentAnim.Length == 0)
        {
            // Debug.LogWarning("[ShipBodyOrbsAttack8Directions] currentAnim vacío => no se muestra nada.");
            return;
        }

        // 5) Avanzar anim manual
        if (framesPerSecond > 0f)
        {
            animTimer += Time.deltaTime * framesPerSecond;
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
            currentFrame = 0;
        }

        // 6) Asignar sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}