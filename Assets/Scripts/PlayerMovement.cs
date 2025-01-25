using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 10f;       // Velocidad de rotación normal
    public float zoomedRotationSpeed = 3f;  // Velocidad de rotación al hacer Zoom

    private CameraZoom cameraZoom;          // Referencia al script CameraZoom

    // Variable para activar/desactivar apuntado automático
    public bool autoAim = true;

    void Start()
    {
        // Obtener referencia al script de Zoom (si existe en la escena)
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    // Rotación de la nave hacia la posición del mouse
    // o, si autoAim está activo, hacia el enemigo más cercano
    public void RotatePlayer()
    {
        if (autoAim)
        {
            // 1. Obtener el enemigo (Enemy o TankEnemy) más cercano usando EnemyManager
            MonoBehaviour nearestAny = EnemyManager.Instance.GetNearestAnyEnemy(transform.position);

            // 2. Si existe un enemigo cercano, apuntar a él
            if (nearestAny != null)
            {
                Vector3 directionToEnemy = nearestAny.transform.position - transform.position;
                float targetAngle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg - 90f;

                float currentAngle = transform.eulerAngles.z;

                float currentRotationSpeed = (cameraZoom != null && cameraZoom.IsZoomedIn)
                    ? zoomedRotationSpeed
                    : rotationSpeed;

                float angle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * currentRotationSpeed);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            // Si no hay ningún enemigo, no rotamos o conservamos el ángulo actual
        }
        else
        {
            // Rotación manual con el mouse (legacy input)
            // 1) Tomar posición del mouse
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            float currentAngle = transform.eulerAngles.z;

            float currentRotationSpeed = (cameraZoom != null && cameraZoom.IsZoomedIn)
                ? zoomedRotationSpeed
                : rotationSpeed;

            float angle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * currentRotationSpeed);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
