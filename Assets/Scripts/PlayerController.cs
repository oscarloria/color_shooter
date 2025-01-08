using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // Necesario para el Nuevo Input System

/// <summary>
/// El PlayerController actúa como coordinador entre los diferentes componentes del jugador,
/// delegando las tareas específicas a los scripts especializados.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Referencias a los componentes especializados adjuntos al jugador
    private PlayerMovement playerMovement;   // Controla la rotación del jugador
    private PlayerShooting playerShooting;   // Gestiona el disparo y la recarga
    private SlowMotion slowMotion;           // Controla la mecánica de cámara lenta

    // Referencia al input actions principal
    private LuminityControls inputActions;

    void Awake()
    {
        // Obtener referencias a los componentes adjuntos al jugador
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        slowMotion = GetComponent<SlowMotion>();

        // Instanciar el nuevo input actions
        inputActions = new LuminityControls();
    }

    private void OnEnable()
    {
        // Habilitar el Action Map "Player"
        inputActions.Player.Enable();

        // Suscribirse a los eventos del nuevo input system
        inputActions.Player.Shoot.performed += OnShoot;
        inputActions.Player.Zoom.performed += OnZoom;
        inputActions.Player.SlowMotion.performed += OnSlowMotion;
        inputActions.Player.Reload.performed += OnReload;
    }

    private void OnDisable()
    {
        // Desuscribir los eventos
        inputActions.Player.Shoot.performed -= OnShoot;
        inputActions.Player.Zoom.performed -= OnZoom;
        inputActions.Player.SlowMotion.performed -= OnSlowMotion;
        inputActions.Player.Reload.performed -= OnReload;

        // Deshabilitar el Action Map
        inputActions.Player.Disable();
    }

    void Update()
    {
        // Rotación (Teclado/Mouse) - Queda para compatibilidad
        playerMovement.RotatePlayer();

        // Selección de color (Teclado WASD) - Sigue en PlayerShooting
        playerShooting.UpdateCurrentColor();

        // Comprobaciones de recarga y munición (Teclado/Mouse)
        if (playerShooting.isReloading) return;

        if (playerShooting.currentAmmo <= 0 && !playerShooting.isReloading)
        {
            StartCoroutine(playerShooting.Reload());
            return;
        }

        // Disparar con Mouse Izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            playerShooting.Shoot();
        }

        // Recargar con tecla 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (playerShooting.currentAmmo < playerShooting.magazineSize)
            {
                StartCoroutine(playerShooting.Reload());
            }
        }

        // Cámara Lenta con Barra Espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && slowMotion.remainingSlowMotionTime > 0f)
        {
            if (slowMotion.isSlowMotionActive)
                slowMotion.PauseSlowMotion();
            else
                slowMotion.ActivateSlowMotion();
        }
    }

    // ---------------- NUEVO INPUT SYSTEM EVENT HANDLERS ----------------

    private void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!playerShooting.isReloading && playerShooting.currentAmmo > 0)
        {
            playerShooting.Shoot();
        }
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        // Agregamos mensaje para verificar la entrada de LT
        Debug.Log("Botón LT presionado (Zoom) - Nuevo Input System");

        // En lugar de deshabilitar la cámara, llamamos al método ToggleZoom()
        CameraZoom cameraZoom = FindObjectOfType<CameraZoom>();
        if (cameraZoom != null)
        {
            cameraZoom.ToggleZoom(); 
        }
    }

    private void OnSlowMotion(InputAction.CallbackContext ctx)
    {
        if (slowMotion.remainingSlowMotionTime > 0f)
        {
            if (slowMotion.isSlowMotionActive)
                slowMotion.PauseSlowMotion();
            else
                slowMotion.ActivateSlowMotion();
        }
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (!playerShooting.isReloading && playerShooting.currentAmmo < playerShooting.magazineSize)
        {
            StartCoroutine(playerShooting.Reload());
        }
    }
}