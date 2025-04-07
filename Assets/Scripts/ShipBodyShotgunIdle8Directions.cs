using UnityEngine;

/*
===========================================================================================
ShipBodyShotgunIdle8Directions.cs

Muestra la animación idle de la escopeta en 8 direcciones, sin Animator.
Cada dirección puede tener 1..N sprites para un mini-loop.

FUNCIONAMIENTO:
1) Lee angleZ de "shipTransform" (objeto que rota).
2) Divide [0..360) en 8 sectores de 45°, centrados con +22.5° => rawIndex [0..7].
3) Mapea rawIndex => finalIndex (0 => Up, 1 => Up-Left, 2 => Left, ...).
4) Usa arrays shotgunIdleUpSprites, shotgunIdleLeftSprites, etc.
5) Avanza la animación manualmente según framesPerSecond.
6) Activa este script (enabled = true) sólo cuando la escopeta sea
   el arma actual, y desactívalo (enabled = false) al cambiar a otra arma.

CONFIGURACIÓN:
- Agregar este script al "ShipBody" que tenga un SpriteRenderer.
- Rellenar en el Inspector los arrays con sprites idle para cada dirección.
- Por defecto, framesPerSecond = 4 (puedes ajustar).
- PlayerController u otro sistema se encarga de habilitar/deshabilitar
  este script según el arma seleccionada.

===========================================================================================
*/

[RequireComponent(typeof(SpriteRenderer))]
public class ShipBodyShotgunIdle8Directions : MonoBehaviour
{
    [Header("Sprites Idle (Escopeta) en 8 direcciones")]
    public Sprite[] shotgunIdleUpSprites;         // finalIndex=0 => Up
    public Sprite[] shotgunIdleUpLeftSprites;     // finalIndex=1 => Up-Left
    public Sprite[] shotgunIdleLeftSprites;       // finalIndex=2 => Left
    public Sprite[] shotgunIdleDownLeftSprites;   // finalIndex=3 => Down-Left
    public Sprite[] shotgunIdleDownSprites;       // finalIndex=4 => Down
    public Sprite[] shotgunIdleDownRightSprites;  // finalIndex=5 => Down-Right
    public Sprite[] shotgunIdleRightSprites;      // finalIndex=6 => Right
    public Sprite[] shotgunIdleUpRightSprites;    // finalIndex=7 => Up-Right

    [Header("El objeto que rota (Ship)")]
    public Transform shipTransform;

    [Header("Frames por segundo (Idle de la escopeta)")]
    public float framesPerSecond = 4f;   // Ajusta a tu gusto

    // Internos
    private SpriteRenderer sr;
    private float animTimer = 0f;
    private int currentFrame = 0;
    private Sprite[] currentAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Cada vez que se habilita este script, reiniciamos la animación
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
        // Evitar rotación local (el sprite no gira)
        transform.rotation = Quaternion.identity;

        // 1) Tomar ángulo z, normalizar a [0..360)
        float angleZ = shipTransform.eulerAngles.z;
        angleZ = (angleZ + 360f) % 360f;

        // 2) Dividir [0..360) en 8 sectores de 45°, sumando 22.5
        float sector = (angleZ + 22.5f) % 360f;
        int rawIndex = Mathf.FloorToInt(sector / 45f); // [0..7]

        // 3) Convertir rawIndex => finalIndex
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

        // 4) Elegir el array de sprites idle segun finalIndex
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

        // Si no hay sprites, no hacemos nada
        if (currentAnim == null || currentAnim.Length == 0) return;

        // 5) Avanzar animación manualmente
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
            // Si framesPerSecond <= 0 => frame 0 fijo
            currentFrame = 0;
        }

        // 6) Asignar el sprite actual
        sr.sprite = currentAnim[currentFrame];
    }
}
