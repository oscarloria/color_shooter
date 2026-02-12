using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // --- Referencias a componentes ---
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;
    private ShotgunShooting shotgunShooting;
    private RifleShooting rifleShooting;
    private DefenseOrbShooting defenseOrbShooting;
    private SlowMotion slowMotion;

    // --- Referencias UI ---
    [Header("UI de selección de arma")]
    public GameObject selectPistolImage;
    public GameObject selectShotgunImage;
    public GameObject selectRifleImage;
    public GameObject selectDefenseOrbImage;

    // --- Estado del arma ---
    private int currentWeapon = 1;
    public int CurrentWeapon => currentWeapon; // Propiedad pública para que otros scripts lean el arma

    // --- Referencias a Scripts de Animación Idle ---
    [Header("Scripts de Idle en 8 direcciones (uno por arma)")]
    public ShipBodyPistolIdle8Directions pistolIdleScript;
    public ShipBodyShotgunIdle8Directions shotgunIdleScript;
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyOrbsIdle8Directions orbsIdleScript;

    void Awake()
    {
        // --- Obtener referencias ---
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        shotgunShooting = GetComponent<ShotgunShooting>();
        rifleShooting = GetComponent<RifleShooting>();
        defenseOrbShooting = GetComponent<DefenseOrbShooting>();
        slowMotion = GetComponent<SlowMotion>();
    }

    void Start()
    {
        // --- Configuración inicial ---
        currentWeapon = 1;
        UpdateWeaponUI();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- Cambio de Arma (Siempre se ejecuta) ---
        // NOTA: RotatePlayer() se removió aquí porque ya se llama en PlayerMovement.Update().
        // Tenerlo en ambos lugares causaba doble rotación por frame.
        HandleMouseScrollWeaponCycle();

        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentWeapon = 1; UpdateWeaponUI(); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { currentWeapon = 2; UpdateWeaponUI(); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { currentWeapon = 3; UpdateWeaponUI(); }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { currentWeapon = 4; UpdateWeaponUI(); }

        // --- Selección de Color (Siempre se ejecuta) ---
        switch (currentWeapon)
        {
            case 1: if (playerShooting) { playerShooting.UpdateCurrentColor(); } break;
            case 2: if (shotgunShooting) { shotgunShooting.UpdateCurrentColor(); } break;
            case 3: if (rifleShooting) { rifleShooting.UpdateCurrentColor(); } break;
            case 4: if (defenseOrbShooting) { defenseOrbShooting.UpdateCurrentColor(); } break;
        }

        // --- Slow Motion (Siempre se ejecuta) ---
        // CORRECCIÓN: La comprobación del Slow Motion se movió aquí para que no sea bloqueada por la recarga.
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            ToggleSlowMotion(); 
        }

        // --- Lógica de Disparo y Recarga ---
        bool isReloading = false;
        int currentAmmo = 0;
        int magazineSize = 0;

        // Determinar el estado del arma actual
        switch (currentWeapon) {
            case 1: if(playerShooting) { isReloading = playerShooting.isReloading; currentAmmo = playerShooting.currentAmmo; magazineSize = playerShooting.magazineSize; } break;
            case 2: if(shotgunShooting) { isReloading = shotgunShooting.isReloading; currentAmmo = shotgunShooting.currentAmmo; magazineSize = shotgunShooting.magazineSize; } break;
            case 3: if(rifleShooting) { isReloading = rifleShooting.isReloading; currentAmmo = rifleShooting.currentAmmo; magazineSize = rifleShooting.magazineSize; } break;
            case 4: if(defenseOrbShooting) { isReloading = defenseOrbShooting.isReloading; currentAmmo = defenseOrbShooting.currentAmmo; magazineSize = defenseOrbShooting.magazineSize; } break;
        }

        // Si se está recargando, no permitir acciones de disparo.
        if (isReloading) return;

        // Las siguientes acciones solo se ejecutan si NO se está recargando.
        if (currentAmmo <= 0) { StartCoroutine(ReloadCurrentWeapon()); return; }
        if (Input.GetMouseButtonDown(0)) { ShootCurrentWeapon(); }
        if (Input.GetMouseButtonUp(0)) { StopRifleFire(); }
        if (Input.GetKeyDown(KeyCode.R)) { if (currentAmmo < magazineSize) StartCoroutine(ReloadCurrentWeapon()); }

    } // Fin de Update()


    // --- Métodos Helper ---

    private void HandleMouseScrollWeaponCycle() 
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta > 0f) { currentWeapon--; if (currentWeapon < 1) currentWeapon = 4; UpdateWeaponUI(); }
        else if (scrollDelta < 0f) { currentWeapon++; if (currentWeapon > 4) currentWeapon = 1; UpdateWeaponUI(); }
    }

    private void UpdateWeaponUI() 
    {
        if (selectPistolImage == null || selectShotgunImage == null || selectRifleImage == null || selectDefenseOrbImage == null) 
        { 
            Debug.LogWarning("PlayerController: Alguna imagen UI de arma no asignada."); 
        }
        else 
        {
            selectPistolImage.SetActive(currentWeapon == 1);
            selectShotgunImage.SetActive(currentWeapon == 2);
            selectRifleImage.SetActive(currentWeapon == 3);
            selectDefenseOrbImage.SetActive(currentWeapon == 4);
        }
        EnableIdleForCurrentWeapon();
    }

    private void EnableIdleForCurrentWeapon() 
    {
        if (pistolIdleScript != null) pistolIdleScript.enabled = (currentWeapon == 1);
        if (shotgunIdleScript != null) shotgunIdleScript.enabled = (currentWeapon == 2);
        if (rifleIdleScript != null) rifleIdleScript.enabled = (currentWeapon == 3);
        if (orbsIdleScript != null) orbsIdleScript.enabled = (currentWeapon == 4);
    }

    private void ShootCurrentWeapon() 
    {
        switch (currentWeapon) 
        {
            case 1: if(playerShooting) playerShooting.Shoot(); break;
            case 2: if(shotgunShooting) shotgunShooting.Shoot(); break;
            case 3: if(rifleShooting) rifleShooting.StartFiring(); break;
            case 4: if(defenseOrbShooting) defenseOrbShooting.ShootOrb(); break;
        }
    }

    private void StopRifleFire() 
    {
        if (currentWeapon == 3 && rifleShooting) rifleShooting.StopFiring();
    }

    private IEnumerator ReloadCurrentWeapon() 
    {
        switch (currentWeapon) 
        {
            case 1: if(playerShooting) yield return StartCoroutine(playerShooting.Reload()); break;
            case 2: if(shotgunShooting) yield return StartCoroutine(shotgunShooting.Reload()); break;
            case 3: if(rifleShooting) yield return StartCoroutine(rifleShooting.Reload()); break;
            case 4: if(defenseOrbShooting) yield return StartCoroutine(defenseOrbShooting.Reload()); break;
        }
    }

    private void ToggleSlowMotion() 
    {
        if(slowMotion != null)
        {
            slowMotion.Toggle();
        }
        else
        {
            Debug.LogWarning("PlayerController: SlowMotion component not found!");
        }
    }

} // Fin de la clase PlayerController