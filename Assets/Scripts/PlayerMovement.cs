using UnityEngine;

/// <summary>
/// Rotación de la nave del jugador.
/// AutoAim: apunta al enemigo más cercano en pantalla.
/// Manual: apunta hacia la posición del mouse.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float zoomedRotationSpeed = 3f;

    // Configurada globalmente desde GameSettings
    public bool autoAim = true;

    CameraZoom cameraZoom;

    void Start()
    {
        cameraZoom = FindObjectOfType<CameraZoom>();
        autoAim = GameSettings.autoAim;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        // Cursor
        Cursor.lockState = autoAim ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !autoAim;

        RotatePlayer();
    }

    /// <summary>
    /// Rota la nave. NOTA: Este método solo debe ser llamado desde aquí (Update).
    /// Se removió la llamada duplicada que existía en PlayerController.
    /// </summary>
    public void RotatePlayer()
    {
        Vector3? targetDirection = null;

        if (autoAim)
        {
            // Usa EnemyBase directamente (antes era MonoBehaviour)
            EnemyBase nearest = EnemyManager.Instance?.GetNearestAnyEnemyOnScreen(
                transform.position, Camera.main
            );

            if (nearest != null)
                targetDirection = nearest.transform.position - transform.position;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetDirection = (mousePos - transform.position);
        }

        if (targetDirection.HasValue)
        {
            Vector2 dir = targetDirection.Value.normalized;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;

            float speed = (cameraZoom != null && cameraZoom.IsZoomedIn)
                ? zoomedRotationSpeed
                : rotationSpeed;

            float angle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * speed);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
