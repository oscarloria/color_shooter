using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 10f;       // Velocidad de rotación normal
    public float zoomedRotationSpeed = 3f;  // Velocidad de rotación al hacer Zoom

    private CameraZoom cameraZoom;          // Referencia al script CameraZoom

    // Variable para activar/desactivar apuntado automático (desde el Inspector)
    public bool autoAim = true;

    void Start()
    {
        // Obtener referencia al script de Zoom (si existe en la escena)
        cameraZoom = FindObjectOfType<CameraZoom>();
    }

    void Update()
    {
        // Bloquear/ocultar el cursor si autoAim está activo; de lo contrario, visible/desbloqueado.
        if (autoAim)
        {
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
        }

        // Llamamos a nuestro método de rotación cada frame
        RotatePlayer();
    }

    // Rotación de la nave hacia el enemigo más cercano (si autoAim está activo)
    // o usando la posición del mouse (si autoAim está desactivado)
    public void RotatePlayer()
    {
        if (autoAim)
        {
            // Usamos el nuevo método: GetNearestAnyEnemyOnScreen
            MonoBehaviour nearestAny = EnemyManager.Instance.GetNearestAnyEnemyOnScreen(
                transform.position,
                Camera.main
            );

            // 2. Si existe un enemigo cercano (y en pantalla), apuntar a él
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
            // Si no hay ningún enemigo visible en pantalla, no rotamos o mantenemos el ángulo actual
        }
        else
        {
            // Rotación manual con el mouse
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
