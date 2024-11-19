using UnityEngine;

public class AimLineController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float lineLength = 10f; // Longitud de la línea de mira
    public LayerMask collisionMask; // Para detectar colisiones con el entorno

    private CameraZoom cameraZoom;
    private PlayerShooting playerShooting;
    private Transform playerTransform;

    void Start()
    {
        cameraZoom = FindObjectOfType<CameraZoom>();

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // Obtener referencia al script PlayerShooting y al transform del jugador
        playerShooting = GetComponentInParent<PlayerShooting>();
        playerTransform = playerShooting.transform;

        // Al iniciar, desactivar la línea
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (cameraZoom != null && cameraZoom.IsZoomedIn)
        {
            lineRenderer.enabled = true;
            UpdateAimLine();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void UpdateAimLine()
    {
        // Dirección hacia adelante de la nave
        Vector3 direction = playerTransform.up; // Asumiendo que el eje 'up' es el frente de la nave

        // Punto inicial y final de la línea
        Vector3 startPoint = playerTransform.position;
        Vector3 endPoint = startPoint + direction * lineLength;

        // Detectar colisiones en la dirección de la línea
        RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, lineLength, collisionMask);
        if (hit.collider != null)
        {
            endPoint = hit.point;
        }

        // Actualizar los puntos de la línea
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        // Actualizar el color de la línea para que coincida con el color seleccionado
        if (playerShooting != null)
        {
            lineRenderer.startColor = playerShooting.currentColor;
            lineRenderer.endColor = playerShooting.currentColor;
        }
    }
}