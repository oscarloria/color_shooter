using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 10f;       // Velocidad de rotación normal
    public float zoomedRotationSpeed = 3f;  // Velocidad de rotación al hacer Zoom

    private CameraZoom cameraZoom;          // Referencia al script CameraZoom

    // Referencia a nuestro asset de Input (LuminityControls)
    private LuminityControls controls;

    void Awake()
    {
        // Instanciar y habilitar el Nuevo Input System
        controls = new LuminityControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        cameraZoom = FindObjectOfType<CameraZoom>(); // Obtener referencia al script de Zoom
    }

    // Rotación de la nave hacia la posición del mouse o del joystick derecho
    public void RotatePlayer()
    {
        // 1. Leer entrada del joystick derecho (Action: Rotate / Vector2)
        Vector2 gamepadInput = controls.Player.Rotate.ReadValue<Vector2>();

        // Ver si el stick derecho está activo
        if (gamepadInput.magnitude > 0.1f)
        {
            // a) Calcular el ángulo desde el input del joystick (Vector2)
            float targetAngle = Mathf.Atan2(gamepadInput.y, gamepadInput.x) * Mathf.Rad2Deg - 90f;

            // b) Obtener el ángulo actual de la rotación del jugador
            float currentAngle = transform.eulerAngles.z;

            // c) Determinar la velocidad de rotación según el estado del Zoom
            float currentRotationSpeed = rotationSpeed;
            if (cameraZoom != null && cameraZoom.IsZoomedIn)
            {
                currentRotationSpeed = zoomedRotationSpeed;
            }

            // d) Interpolar suavemente entre el ángulo actual y el objetivo
            float angle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * currentRotationSpeed);

            // e) Aplicar la rotación
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // 2. Si no hay entrada del joystick, usar la posición del mouse (lógica previa)
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            float currentAngle = transform.eulerAngles.z;

            float currentRotationSpeed = rotationSpeed;
            if (cameraZoom != null && cameraZoom.IsZoomedIn)
            {
                currentRotationSpeed = zoomedRotationSpeed;
            }

            float angle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * currentRotationSpeed);

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}