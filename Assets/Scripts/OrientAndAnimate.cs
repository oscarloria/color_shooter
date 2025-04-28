using UnityEngine;

/// <summary>
///  • Mantiene el sprite mirando en la dirección de rb.linearVelocity
///  • Reproduce un bucle de sprites sin usar Animator
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class OrientAndAnimate : MonoBehaviour
{
    [Header("Animación")]
    [Tooltip("Sprites en orden (0-6).")]
    public Sprite[] frames;
    [Tooltip("Frames por segundo.")]
    public float fps = 12f;

    [Header("Rotación")]
    [Tooltip("Compensa la orientación del sprite en el prefab (-90, 0, 90…).")]
    public float spriteOffset = -90f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    float timer;
    int currentFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // ---------- ROTACIÓN ----------
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + spriteOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // ---------- ANIMACIÓN ----------
        if (frames == null || frames.Length == 0 || fps <= 0f) return;

        timer += Time.deltaTime * fps;
        if (timer >= 1f)
        {
            timer -= 1f;
            currentFrame = (currentFrame + 1) % frames.Length;
            sr.sprite = frames[currentFrame];
        }
    }
}
