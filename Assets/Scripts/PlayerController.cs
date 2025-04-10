using UnityEngine;
using System.Collections;

/// <summary>
/// El PlayerController actúa como coordinador entre los diferentes componentes del jugador,
/// delegando las tareas específicas a los scripts especializados.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Referencias a los componentes especializados adjuntos al jugador
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;
    private ShotgunShooting shotgunShooting;
    private RifleShooting rifleShooting;
    private DefenseOrbShooting defenseOrbShooting;
    private SlowMotion slowMotion;

    // --- OBJETOS DE LA UI PARA INDICAR ARMA SELECCIONADA ---
    [Header("UI de selección de arma")]
    public GameObject selectPistolImage;
    public GameObject selectShotgunImage;
    public GameObject selectRifleImage;
    public GameObject selectDefenseOrbImage;

    // Lógica de armas:
    // 1 => Pistola
    // 2 => Escopeta
    // 3 => Rifle Automático
    // 4 => Orbe de Defensa
    private int currentWeapon = 1;

    // NUEVO (Idle Scripts):
    [Header("Scripts de Idle en 8 direcciones (uno por arma)")]
    public ShipBodyPistolIdle8Directions pistolIdleScript;
    public ShipBodyShotgunIdle8Directions shotgunIdleScript;
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyOrbsIdle8Directions orbsIdleScript;

    void Awake()
    {
        // Obtener referencias a los componentes
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        shotgunShooting = GetComponent<ShotgunShooting>();
        rifleShooting = GetComponent<RifleShooting>();
        defenseOrbShooting = GetComponent<DefenseOrbShooting>();
        slowMotion = GetComponent<SlowMotion>();
    }

    void Start()
    {
        // FORZAR PISTOL al iniciar, por si tu variable se modificó en otro lado
        currentWeapon = 1;   // <------------------ Importante

        // Actualizar la UI al inicio (arma 1: pistola)
        UpdateWeaponUI();

        // Ocultar y bloquear el cursor en pantalla
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Asegurar que IdleScript de pistola esté activo,
        // y los demás idle scripts apagados, al empezar.
        EnableIdleForCurrentWeapon();
    }

    void Update()
    {
        // Rotación (teclado/mouse)
        playerMovement.RotatePlayer();

        // Manejar el cambio de arma con la rueda del mouse
        HandleMouseScrollWeaponCycle();

        // Cambio de arma con teclas numéricas (1 a 4)
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
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentWeapon = 3;
            UpdateWeaponUI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentWeapon = 4;
            UpdateWeaponUI();
        }

        // Actualizar el color del arma activa
        switch (currentWeapon)
        {
            case 1:
                playerShooting.UpdateCurrentColor();
                break;
            case 2:
                shotgunShooting.UpdateCurrentColor();
                break;
            case 3:
                rifleShooting.UpdateCurrentColor();
                break;
            case 4:
                defenseOrbShooting.UpdateCurrentColor();
                break;
        }

        // Comprobaciones de recarga/munición
        bool isReloading = false;
        int currentAmmo = 0;
        int magazineSize = 0;

        switch (currentWeapon)
        {
            case 1:
                isReloading = playerShooting.isReloading;
                currentAmmo = playerShooting.currentAmmo;
                magazineSize = playerShooting.magazineSize;
                break;
            case 2:
                isReloading = shotgunShooting.isReloading;
                currentAmmo = shotgunShooting.currentAmmo;
                magazineSize = shotgunShooting.magazineSize;
                break;
            case 3:
                isReloading = rifleShooting.isReloading;
                currentAmmo = rifleShooting.currentAmmo;
                magazineSize = rifleShooting.magazineSize;
                break;
            case 4:
                isReloading = defenseOrbShooting.isReloading;
                currentAmmo = defenseOrbShooting.currentAmmo;
                magazineSize = defenseOrbShooting.magazineSize;
                break;
        }

        if (isReloading) return;

        if (currentAmmo <= 0 && !isReloading)
        {
            switch (currentWeapon)
            {
                case 1:
                    StartCoroutine(playerShooting.Reload());
                    break;
                case 2:
                    StartCoroutine(shotgunShooting.Reload());
                    break;
                case 3:
                    StartCoroutine(rifleShooting.Reload());
                    break;
                case 4:
                    StartCoroutine(defenseOrbShooting.Reload());
                    break;
            }
            return;
        }

        // Disparar con Mouse Izquierdo (para pistola, escopeta, orbe; rifle usa disparo continuo)
        if (Input.GetMouseButtonDown(0))
        {
            if (currentWeapon == 1)
                playerShooting.Shoot();
            else if (currentWeapon == 2)
                shotgunShooting.Shoot();
            else if (currentWeapon == 3)
                rifleShooting.StartFiring(); // Iniciar ráfaga
            else if (currentWeapon == 4)
                defenseOrbShooting.ShootOrb();
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentWeapon == 3)
                rifleShooting.StopFiring();
        }

        // Recargar con la tecla 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo < magazineSize)
            {
                switch (currentWeapon)
                {
                    case 1:
                        StartCoroutine(playerShooting.Reload());
                        break;
                    case 2:
                        StartCoroutine(shotgunShooting.Reload());
                        break;
                    case 3:
                        StartCoroutine(rifleShooting.Reload());
                        break;
                    case 4:
                        StartCoroutine(defenseOrbShooting.Reload());
                        break;
                }
            }
        }

        // Cámara lenta con Barra Espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && slowMotion.remainingSlowMotionTime > 0f)
        {
            if (slowMotion.isSlowMotionActive)
                slowMotion.PauseSlowMotion();
            else
                slowMotion.ActivateSlowMotion();
        }
    }

    // -------------------------------------------
    // LÓGICA PARA MOUSE SCROLL - CAMBIO DE ARMA
    // -------------------------------------------
   private void HandleMouseScrollWeaponCycle()
{
    float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

    // Si la rueda se mueve hacia arriba => retroceder arma
    if (scrollDelta > 0f)
    {
        currentWeapon--;
        if (currentWeapon < 1) currentWeapon = 4;
        Debug.Log("Rueda mouse HACIA ARRIBA => Arma:" + currentWeapon);
        UpdateWeaponUI();
    }
    // Si la rueda se mueve hacia abajo => avanzar arma
    else if (scrollDelta < 0f)
    {
        currentWeapon++;
        if (currentWeapon > 4) currentWeapon = 1;
        Debug.Log("Rueda mouse HACIA ABAJO => Arma:" + currentWeapon);
        UpdateWeaponUI();
    }
}


    // -------------------------------------------
    // MÉTODO PARA ACTUALIZAR LA UI DE ARMA ACTIVA
    // -------------------------------------------
    private void UpdateWeaponUI()
    {
        if (selectPistolImage == null || selectShotgunImage == null) return;

        // Apagar todas las imágenes de selección
        selectPistolImage.SetActive(false);
        selectShotgunImage.SetActive(false);
        if (selectRifleImage != null) selectRifleImage.SetActive(false);
        if (selectDefenseOrbImage != null) selectDefenseOrbImage.SetActive(false);

        switch (currentWeapon)
        {
            case 1:
                selectPistolImage.SetActive(true);
                break;
            case 2:
                selectShotgunImage.SetActive(true);
                break;
            case 3:
                if (selectRifleImage != null) selectRifleImage.SetActive(true);
                break;
            case 4:
                if (selectDefenseOrbImage != null) selectDefenseOrbImage.SetActive(true);
                break;
        }

        // Cada vez que cambiamos de arma, habilitamos/deshabilitamos Idle
        EnableIdleForCurrentWeapon();
    }

    private void EnableIdleForCurrentWeapon()
    {
        // 1) Desactivar todos
        if (pistolIdleScript   != null) pistolIdleScript.enabled   = false;
        if (shotgunIdleScript  != null) shotgunIdleScript.enabled  = false;
        if (rifleIdleScript    != null) rifleIdleScript.enabled    = false;
        if (orbsIdleScript     != null) orbsIdleScript.enabled     = false;

        // 2) Encender solo el del arma actual
        switch (currentWeapon)
        {
            case 1:
                if (pistolIdleScript != null) pistolIdleScript.enabled = true;
                break;
            case 2:
                if (shotgunIdleScript != null) shotgunIdleScript.enabled = true;
                break;
            case 3:
                if (rifleIdleScript != null) rifleIdleScript.enabled = true;
                break;
            case 4:
                if (orbsIdleScript != null) orbsIdleScript.enabled = true;
                break;
        }
    }
}