using UnityEngine;

public class ShipGlide : MonoBehaviour
{
    // Amplitud del movimiento: qué tanto se desplaza (en unidades)
    public float amplitude = 0.1f;
    
    // Frecuencia del movimiento: qué tan rápido se completa la “onda”
    public float frequency = 1.0f;

    // Opcional: decidir si queremos balanceo en la X, en la Y, en la rotación, etc.
    [Header("Activar/Desactivar ejes")]
    public bool moveInX = false;
    public bool moveInY = true;
    public bool tilt = false; // si quieres que además se incline un poco

    // Para guardar la posición inicial de la nave
    private Vector3 initialPosition;

    void Start()
    {
        // Guardamos la posición local inicial para no alterar el resto del movimiento
        // (así se “superpone” este planeo a lo que haga la nave).
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;

        // Si quieres movimiento en Y (por ejemplo, arriba/abajo)
        float newY = moveInY ? initialPosition.y + offset : transform.localPosition.y;
        // Si quieres movimiento en X
        float newX = moveInX ? initialPosition.x + offset : transform.localPosition.x;

        // Aplicamos la posición local con los valores ajustados
        transform.localPosition = new Vector3(newX, newY, initialPosition.z);

        // Si quieres agregar un pequeño "tilt" o inclinación, 
        // podrías rotar un poco con base en offset:
        if (tilt)
        {
            // Ejemplo: inclinar en Z un máximo de 5° en cada extremo
            float tiltAngle = offset * 5f; 
            transform.localRotation = Quaternion.Euler(0, 0, tiltAngle);
        }
    }
}
