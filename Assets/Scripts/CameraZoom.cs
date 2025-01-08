using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Camera mainCamera;            // Cámara principal a la que se aplicará el Zoom
    public Transform playerTransform;    // Transform del jugador para seguir su posición y rotación
    public float normalZoom = 5f;        // Tamaño ortográfico de la cámara en modo normal
    public float zoomedInSize = 3f;      // Tamaño ortográfico de la cámara en modo Zoom
    public float zoomSpeed = 5f;         // Velocidad de transición entre el Zoom y el modo normal
    public float zoomForwardOffset = 4f; // Desplazamiento adicional hacia adelante en modo Zoom

    private bool isZoomedIn = false;     // Estado del Zoom (toggle)

    // Propiedad pública para verificar el estado del Zoom
    public bool IsZoomedIn => isZoomedIn;

    // Límites del área visible en el mundo (ajusta según el tamaño de la escena)
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    void Update()
    {
        // Toggle de Zoom con clic derecho del mouse (opcional)
        if (Input.GetMouseButtonDown(1))
        {
            ToggleZoom();
        }

        // Cambiar el tamaño de la cámara entre el modo normal y el Zoom usando Lerp
        float targetSize = isZoomedIn ? zoomedInSize : normalZoom;
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);

        // Ajustar la posición de la cámara hacia adelante en el modo de Zoom
        Vector3 zoomOffset = isZoomedIn
            ? playerTransform.up * zoomForwardOffset // Mayor desplazamiento hacia adelante
            : Vector3.zero;

        // Nueva posición objetivo de la cámara considerando el desplazamiento
        Vector3 targetPosition = playerTransform.position + zoomOffset + new Vector3(0, 0, -10); // Mantener la cámara detrás en Z

        // Aplicar límites a la posición de la cámara
        float cameraHalfHeight = mainCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX + cameraHalfWidth, maxX - cameraHalfWidth);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY + cameraHalfHeight, maxY - cameraHalfHeight);

        // Interpolar la posición de la cámara hacia la posición objetivo usando Lerp
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            Time.deltaTime * zoomSpeed
        );
    }

    /// <summary>
    /// Alterna el estado de Zoom (usado tanto por el mouse como por el nuevo input system).
    /// </summary>
    public void ToggleZoom()
    {
        isZoomedIn = !isZoomedIn;
        Debug.Log($"ToggleZoom() => isZoomedIn ahora es {isZoomedIn}");
    }
}