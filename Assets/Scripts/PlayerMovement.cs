using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 10f;       // Velocidad de rotación normal
    public float zoomedRotationSpeed = 3f;  // Velocidad de rotación al hacer Zoom

    private CameraZoom cameraZoom;          // Referencia al script CameraZoom

    void Start()
    {
        cameraZoom = FindObjectOfType<CameraZoom>(); // Obtener referencia al script de Zoom
    }

    // Rotación de la nave hacia la posición del mouse con ajuste de velocidad
    public void RotatePlayer()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Posición del mouse en el mundo
        Vector2 direction = (mousePosition - transform.position).normalized;         // Dirección hacia el mouse
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Ángulo objetivo de rotación en grados

        // Obtener el ángulo actual de la rotación del jugador
        float currentAngle = transform.eulerAngles.z;

        // Determinar la velocidad de rotación según el estado del Zoom
        float currentRotationSpeed = rotationSpeed;
        if (cameraZoom != null && cameraZoom.IsZoomedIn)
        {
            currentRotationSpeed = zoomedRotationSpeed;
        }

        // Interpolar suavemente entre el ángulo actual y el objetivo
        float angle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * currentRotationSpeed);

        // Aplicar la rotación
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}