using UnityEngine;

public class ExplosionRadiusIndicator : MonoBehaviour
{
    [Tooltip("Duración del efecto de indicación en segundos.")]
    public float duration = 1f;

    private float timer = 0f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
