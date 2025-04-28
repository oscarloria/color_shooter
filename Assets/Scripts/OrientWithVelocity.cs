using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class OrientWithVelocity : MonoBehaviour
{
    Rigidbody2D rb;

    // Compensa los 90° que giraste el sprite en el prefab.
    // Si ves que queda al revés, cambia a +90 o 0.
    const float spriteOffset = -90f;     

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void LateUpdate()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.0001f)          // evita problema si está casi quieto
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + spriteOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
