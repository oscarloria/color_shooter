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

    // --- OBJETOS DE LA UI PARA INDICAR ARMA SELECCIONADA ---
    [Header("UI de selección de arma")]
    public GameObject selectPistolImage;       // Arrastra aquí el objeto "SelectPistol" (tipo Image) en el Inspector
    public GameObject selectShotgunImage;      // Arrastra aquí el objeto "SelectShotgun" (tipo Image) en el Inspector

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

    void Start()
    {
        // Asegurarnos de que la UI se actualice al inicio,
        // por si la escena inicia con el arma 1
        UpdateWeaponUI();
    }

    void Update()
    {
        // Rotación (Teclado/Mouse) - Queda para compatibilidad
        playerMovement.RotatePlayer();

        // (Cambio) Manejar el cambio de arma con la rueda del mouse
        HandleMouseScrollWeaponCycle();

        // Lógica para cambio de arma con Teclado (1 y 2)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon = 1;
            UpdateWeaponUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon = 2;
            UpdateWeaponUI();
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
        UpdateWeaponUI();
    }

    // ---------------- LÓGICA PARA MOUSE SCROLL ----------------
    // (Cambio) método para manejar el scroll del mouse como ciclo
    private void HandleMouseScrollWeaponCycle()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0f)
        {
            Debug.Log("Mouse Scroll detectado. Valor: " + scrollDelta);

            if (currentWeapon == 1) currentWeapon = 2;
            else currentWeapon = 1;

            Debug.Log("Cambio de arma vía scroll del mouse. Arma activa: " + currentWeapon);
            UpdateWeaponUI();
        }
    }

    // ---------------- MÉTODO PARA ACTUALIZAR LA UI DE ARMA ACTIVA ----------------
    private void UpdateWeaponUI()
    {
        // Verificamos que existan las referencias
        if (selectPistolImage == null || selectShotgunImage == null)
        {
            return; // Si no existen, salimos
        }

        // Activa la imagen de la pistola si el arma actual es 1, desactiva la de escopeta
        // y viceversa
        if (currentWeapon == 1)
        {
            selectPistolImage.SetActive(true);
            selectShotgunImage.SetActive(false);
        }
        else
        {
            selectPistolImage.SetActive(false);
            selectShotgunImage.SetActive(true);
        }
    }
}