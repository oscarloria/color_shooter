using UnityEngine;
using System.Collections;

/// <summary>
/// El PlayerController actúa como coordinador entre los diferentes componentes del jugador,
/// delegando las tareas específicas a los scripts especializados.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Referencias a los componentes especializados adjuntos al jugador
    private PlayerMovement playerMovement;     // Controla la rotación del jugador
    private PlayerShooting playerShooting;     // Gestiona el disparo (Pistola)
    private ShotgunShooting shotgunShooting;   // Script de disparo para la Escopeta

    // Referencia al Rifle Automático
    private RifleShooting rifleShooting;       // Script de disparo para el Rifle Automático

    private SlowMotion slowMotion;             // Controla la mecánica de cámara lenta

    // --- OBJETOS DE LA UI PARA INDICAR ARMA SELECCIONADA ---
    [Header("UI de selección de arma")]
    public GameObject selectPistolImage;       // Arrastra aquí el objeto "SelectPistol" (tipo Image) en el Inspector
    public GameObject selectShotgunImage;      // Arrastra aquí el objeto "SelectShotgun" (tipo Image) en el Inspector

    // (Opcional) UI para Rifle
    public GameObject selectRifleImage;        // Arrástralo si tienes un ícono de Rifle en la UI

    // Lógica de armas:
    // 1 => Pistola
    // 2 => Escopeta
    // 3 => Rifle Automático
    private int currentWeapon = 1;

    void Awake()
    {
        // Obtener referencias a los componentes adjuntos al jugador
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        shotgunShooting = GetComponent<ShotgunShooting>();
        rifleShooting = GetComponent<RifleShooting>();
        slowMotion = GetComponent<SlowMotion>();
    }

    void Start()
    {
        // Actualizamos la UI al inicio (arma = 1 => pistola)
        UpdateWeaponUI();

        // Ocultar y bloquear el cursor en pantalla
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Rotación (Teclado/Mouse)
        playerMovement.RotatePlayer();

        // Manejar el cambio de arma con la rueda del mouse
        HandleMouseScrollWeaponCycle();

        // Lógica para cambio de arma con Teclado (1, 2 y 3)
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

        // Actualizar color según el arma activa
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
        }

        // Comprobaciones de recarga / munición
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
            }
            return;
        }

        // Disparar con Mouse Izquierdo (teclado/ratón)
        // (Pistola/Escopeta = un disparo; Rifle = disparo continuo si lo maneja el propio script)
        if (Input.GetMouseButtonDown(0))
        {
            if (currentWeapon == 1)
                playerShooting.Shoot();
            else if (currentWeapon == 2)
                shotgunShooting.Shoot();
            else if (currentWeapon == 3)
                rifleShooting.StartFiring(); // Iniciar ráfaga
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentWeapon == 3)
            {
                // Parar la ráfaga
                rifleShooting.StopFiring();
            }
        }

        // Recargar con tecla 'R'
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
                }
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

    // -------------------------------------------
    // LÓGICA PARA MOUSE SCROLL - CAMBIO DE ARMA
    // -------------------------------------------
    private void HandleMouseScrollWeaponCycle()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0f)
        {
            Debug.Log("Mouse Scroll detectado. Valor: " + scrollDelta);

            currentWeapon++;
            if (currentWeapon > 3) currentWeapon = 1;

            Debug.Log("Cambio de arma vía scroll del mouse. Arma activa: " + currentWeapon);
            UpdateWeaponUI();
        }
    }

    // -------------------------------------------
    // MÉTODO PARA ACTUALIZAR LA UI DE ARMA ACTIVA
    // -------------------------------------------
    private void UpdateWeaponUI()
    {
        if (selectPistolImage == null || selectShotgunImage == null) return;

        // Apagamos todo
        selectPistolImage.SetActive(false);
        selectShotgunImage.SetActive(false);
        if (selectRifleImage != null) selectRifleImage.SetActive(false);

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
        }
    }
}
