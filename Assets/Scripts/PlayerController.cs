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
    private PlayerMovement playerMovement;     // Controla la rotación del jugador
    private PlayerShooting playerShooting;     // Gestiona el disparo y la recarga (Pistola)
    private ShotgunShooting shotgunShooting;   // Script de disparo para la Escopeta
    private SlowMotion slowMotion;             // Controla la mecánica de cámara lenta

    // Referencia al input actions principal
    private LuminityControls inputActions;

    // Lógica de armas:
    // 1 => Pistola (playerShooting)
    // 2 => Escopeta (shotgunShooting)
    private int currentWeapon = 1;

    void Awake()
    {
        // Obtener referencias a los componentes adjuntos al jugador
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        shotgunShooting = GetComponent<ShotgunShooting>();
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

        // Suscribirse para cambiar de arma con la tecla "Y" del gamepad
        // Asumiendo que definiste una acción "WeaponCycle" en tu input actions
        inputActions.Player.WeaponCycle.performed += OnWeaponCycle;
    }

    private void OnDisable()
    {
        // Desuscribir los eventos
        inputActions.Player.Shoot.performed -= OnShoot;
        inputActions.Player.Zoom.performed -= OnZoom;
        inputActions.Player.SlowMotion.performed -= OnSlowMotion;
        inputActions.Player.Reload.performed -= OnReload;

        inputActions.Player.WeaponCycle.performed -= OnWeaponCycle;

        // Deshabilitar el Action Map
        inputActions.Player.Disable();
    }

    void Update()
    {
        // Rotación (Teclado/Mouse) - Queda para compatibilidad
        playerMovement.RotatePlayer();

        // Lógica para cambio de arma con Teclado (1 y 2)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon = 2;
        }

        // Actualizar color (WASD / Stick Izquierdo) siempre, 
        // la pistola y la escopeta comparten el sistema de color:
        if (currentWeapon == 1)
        {
            // Pistola
            playerShooting.UpdateCurrentColor();
        }
        else if (currentWeapon == 2)
        {
            // Escopeta
            shotgunShooting.UpdateCurrentColor();
        }

        // Comprobaciones de recarga y munición (Teclado/Mouse)
        // Revisamos si el arma actual está recargando
        bool isReloading = (currentWeapon == 1) ? playerShooting.isReloading
                                               : shotgunShooting.isReloading;

        int currentAmmo = (currentWeapon == 1) ? playerShooting.currentAmmo
                                              : shotgunShooting.currentAmmo;

        int magazineSize = (currentWeapon == 1) ? playerShooting.magazineSize
                                               : shotgunShooting.magazineSize;

        if (isReloading) return;

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine((currentWeapon == 1)
                ? playerShooting.Reload()
                : shotgunShooting.Reload());
            return;
        }

        // Disparar con Mouse Izquierdo (teclado/ratón)
        if (Input.GetMouseButtonDown(0))
        {
            if (currentWeapon == 1) playerShooting.Shoot();
            else shotgunShooting.Shoot();
        }

        // Recargar con tecla 'R' (teclado/ratón)
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < magazineSize)
            {
                StartCoroutine((currentWeapon == 1)
                    ? playerShooting.Reload()
                    : shotgunShooting.Reload());
            }
        }

        // Cámara Lenta con Barra Espaciadora (teclado)
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
        // Depende del arma actual
        bool isReloading = (currentWeapon == 1) ? playerShooting.isReloading
                                               : shotgunShooting.isReloading;
        int currentAmmo = (currentWeapon == 1) ? playerShooting.currentAmmo
                                              : shotgunShooting.currentAmmo;

        if (!isReloading && currentAmmo > 0)
        {
            if (currentWeapon == 1) playerShooting.Shoot();
            else shotgunShooting.Shoot();
        }
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        Debug.Log("Botón LT presionado (Zoom) - Nuevo Input System");

        // Toggle Zoom en CameraZoom (ejemplo)
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
        bool isReloading = (currentWeapon == 1) ? playerShooting.isReloading
                                               : shotgunShooting.isReloading;
        int currentAmmo = (currentWeapon == 1) ? playerShooting.currentAmmo
                                              : shotgunShooting.currentAmmo;
        int magSize = (currentWeapon == 1) ? playerShooting.magazineSize
                                          : shotgunShooting.magazineSize;

        if (!isReloading && currentAmmo < magSize)
        {
            StartCoroutine((currentWeapon == 1)
                ? playerShooting.Reload()
                : shotgunShooting.Reload());
        }
    }

    // Maneja la pulsación del botón "WeaponCycle" (Y en Gamepad)
    private void OnWeaponCycle(InputAction.CallbackContext ctx)
    {
        // Ejemplo sencillo de cycle con 2 armas
        if (currentWeapon == 1) currentWeapon = 2;
        else currentWeapon = 1;

        Debug.Log("Cambio de arma vía gamepad Y (WeaponCycle). Arma activa: " + currentWeapon);
    }
}
