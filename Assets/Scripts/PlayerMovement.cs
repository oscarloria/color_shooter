using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 10f;       // Velocidad de rotación normal
    public float zoomedRotationSpeed = 3f;    // Velocidad de rotación al hacer Zoom

    private CameraZoom cameraZoom;          // Referencia al script CameraZoom

    // Variable para activar/desactivar apuntado automático (configurada globalmente)
    public bool autoAim = true;

    void Start()
    {
        // Obtener referencia al script de Zoom (si existe en la escena)
        cameraZoom = FindObjectOfType<CameraZoom>();

        // Asignar autoAim a partir de la configuración global (GameSettings)
        autoAim = GameSettings.autoAim;
    }

    void Update()
    {
        // Si el juego está pausado, no se modifica el cursor ni se procesa la rotación
        if (Time.timeScale == 0)
            return;

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

        // Llamar al método de rotación cada frame
        RotatePlayer();
    }

    // Rotación de la nave hacia el enemigo más cercano (si autoAim está activo)
    // o usando la posición del mouse (si autoAim está desactivado)
    public void RotatePlayer()
    {
        if (autoAim)
        {
            // Usar el método GetNearestAnyEnemyOnScreen del EnemyManager
            MonoBehaviour nearestAny = EnemyManager.Instance.GetNearestAnyEnemyOnScreen(
                transform.position,
                Camera.main
            );

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
