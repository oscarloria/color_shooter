using UnityEngine;

/// <summary>
/// • Cicla los sprites del orbe (frames[]) a fps fijo.
/// • Hace que la “nariz” del sprite mire siempre HACIA FUERA de la órbita
///   (dirección orbe → jugador), con un offset opcional.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class OrbOrientAndAnimate : MonoBehaviour
{
    [Header("Animación")]
    [Tooltip("Sprites en orden (0-6). 7 frames = anim fluida en bucle.")]
    public Sprite[] frames;
    public float fps = 12f;

    [Header("Rotación")]
    [Tooltip("Compensa la orientación original del sprite (-90, 0, 90…).")]
    public float spriteOffset = -90f;

    // internos
    SpriteRenderer sr;
    DefenseOrb orb;                 // para saber dónde está el jugador
    float timer;
    int frame;

    void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        orb = GetComponent<DefenseOrb>();              // script hermano
    }

    void LateUpdate()
    {
        // ---------- ORIENTACIÓN ----------
        if (orb != null && orb.Player != null)
        {
            Vector2 dir = (transform.position - orb.Player.position).normalized; // apunta hacia fuera
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteOffset;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // ---------- ANIMACIÓN ----------
        if (frames == null || frames.Length == 0 || fps <= 0f) return;

        timer += Time.deltaTime * fps;
        if (timer >= 1f)
        {
            timer -= 1f;
            frame  = (frame + 1) % frames.Length;
            sr.sprite = frames[frame];
        }
    }
}