using UnityEngine;
using System.Collections;

// Quitamos las referencias y lógica del contorno de aquí
public class PlayerController : MonoBehaviour
{
    // --- Referencias a componentes existentes ---
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;
    private ShotgunShooting shotgunShooting;
    private RifleShooting rifleShooting;
    private DefenseOrbShooting defenseOrbShooting;
    private SlowMotion slowMotion;

    // --- Referencias UI existentes ---
    [Header("UI de selección de arma")]
    public GameObject selectPistolImage;
    public GameObject selectShotgunImage;
    public GameObject selectRifleImage;
    public GameObject selectDefenseOrbImage;

    // --- Estado del arma ---
    private int currentWeapon = 1;
    // NUEVO: Propiedad pública para que otros scripts lean el arma actual
    public int CurrentWeapon => currentWeapon;

    // --- Referencias a Scripts de Animación Idle existentes ---
    [Header("Scripts de Idle en 8 direcciones (uno por arma)")]
    public ShipBodyPistolIdle8Directions pistolIdleScript;
    public ShipBodyShotgunIdle8Directions shotgunIdleScript;
    public ShipBodyRifleIdle8Directions rifleIdleScript;
    public ShipBodyOrbsIdle8Directions orbsIdleScript;

    // --- YA NO NECESITAMOS LAS VARIABLES DEL CONTORNO AQUÍ ---
    // [Header("Outline Shader Control")]
    // [SerializeField] private SpriteRenderer characterSpriteRenderer;
    // private Material outlineMaterialInstance;
    // private const string OUTLINE_COLOR_PROPERTY = "_OutlineColor";
    // --- FIN VARIABLES ELIMINADAS ---

    void Awake()
    {
        // --- Obtener referencias existentes ---
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        shotgunShooting = GetComponent<ShotgunShooting>();
        rifleShooting = GetComponent<RifleShooting>();
        defenseOrbShooting = GetComponent<DefenseOrbShooting>();
        slowMotion = GetComponent<SlowMotion>();

        // --- YA NO OBTENEMOS LA INSTANCIA DEL MATERIAL AQUÍ ---
    }

    void Start()
    {
        // --- Configuración inicial ---
        currentWeapon = 1;
        UpdateWeaponUI();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // --- YA NO INICIALIZAMOS EL CONTORNO AQUÍ ---
    }

    void Update()
    {
        // --- Movimiento y Cambio de Arma ---
        if (playerMovement != null) playerMovement.RotatePlayer();
        HandleMouseScrollWeaponCycle();

        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentWeapon = 1; UpdateWeaponUI(); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { currentWeapon = 2; UpdateWeaponUI(); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { currentWeapon = 3; UpdateWeaponUI(); }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { currentWeapon = 4; UpdateWeaponUI(); }

        // --- ACTUALIZADO: Solo llamamos a UpdateCurrentColor del arma activa ---
        // Ya no necesitamos leer el color aquí ni actualizar el material.
        switch (currentWeapon)
        {
            case 1: if (playerShooting) { playerShooting.UpdateCurrentColor(); } break;
            case 2: if (shotgunShooting) { shotgunShooting.UpdateCurrentColor(); } break;
            case 3: if (rifleShooting) { rifleShooting.UpdateCurrentColor(); } break;
            case 4: if (defenseOrbShooting) { defenseOrbShooting.UpdateCurrentColor(); } break;
        }
        // --- FIN LÓGICA DE CONTORNO ELIMINADA DE AQUÍ ---


        // --- Lógica de Disparo y Recarga (sin cambios) ---
        bool isReloading = false;
        int currentAmmo = 0;
        int magazineSize = 0;

        switch (currentWeapon) { /* ... leer estado ... */
            case 1: if(playerShooting) { isReloading = playerShooting.isReloading; currentAmmo = playerShooting.currentAmmo; magazineSize = playerShooting.magazineSize; } break;
            case 2: if(shotgunShooting) { isReloading = shotgunShooting.isReloading; currentAmmo = shotgunShooting.currentAmmo; magazineSize = shotgunShooting.magazineSize; } break;
            case 3: if(rifleShooting) { isReloading = rifleShooting.isReloading; currentAmmo = rifleShooting.currentAmmo; magazineSize = rifleShooting.magazineSize; } break;
            case 4: if(defenseOrbShooting) { isReloading = defenseOrbShooting.isReloading; currentAmmo = defenseOrbShooting.currentAmmo; magazineSize = defenseOrbShooting.magazineSize; } break;
        }

        if (isReloading) return;

        if (currentAmmo <= 0 && !isReloading) { StartCoroutine(ReloadCurrentWeapon()); return; }
        if (Input.GetMouseButtonDown(0)) { ShootCurrentWeapon(); }
        if (Input.GetMouseButtonUp(0)) { StopRifleFire(); }
        if (Input.GetKeyDown(KeyCode.R)) { if (currentAmmo < magazineSize) StartCoroutine(ReloadCurrentWeapon()); }
        if (Input.GetKeyDown(KeyCode.Space)) { ToggleSlowMotion(); }

    } // Fin de Update()


    // --- Métodos Helper (sin cambios) ---

    private void HandleMouseScrollWeaponCycle() { /* ... código existente ... */
         float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
         if (scrollDelta > 0f) { currentWeapon--; if (currentWeapon < 1) currentWeapon = 4; UpdateWeaponUI(); }
         else if (scrollDelta < 0f) { currentWeapon++; if (currentWeapon > 4) currentWeapon = 1; UpdateWeaponUI(); }
     }

    private void UpdateWeaponUI() { /* ... código existente ... */
         if (selectPistolImage == null || selectShotgunImage == null || selectRifleImage == null || selectDefenseOrbImage == null) { Debug.LogWarning("PlayerController: Alguna imagen UI de arma no asignada."); }
         else {
              selectPistolImage.SetActive(currentWeapon == 1);
              selectShotgunImage.SetActive(currentWeapon == 2);
              selectRifleImage.SetActive(currentWeapon == 3);
              selectDefenseOrbImage.SetActive(currentWeapon == 4);
         }
         EnableIdleForCurrentWeapon();
     }

    private void EnableIdleForCurrentWeapon() { /* ... código existente ... */
         if (pistolIdleScript != null) pistolIdleScript.enabled = (currentWeapon == 1);
         if (shotgunIdleScript != null) shotgunIdleScript.enabled = (currentWeapon == 2);
         if (rifleIdleScript != null) rifleIdleScript.enabled = (currentWeapon == 3);
         if (orbsIdleScript != null) orbsIdleScript.enabled = (currentWeapon == 4);
     }

    private void ShootCurrentWeapon() { /* ... código existente ... */
         switch (currentWeapon) {
             case 1: if(playerShooting) playerShooting.Shoot(); break;
             case 2: if(shotgunShooting) shotgunShooting.Shoot(); break;
             case 3: if(rifleShooting) rifleShooting.StartFiring(); break;
             case 4: if(defenseOrbShooting) defenseOrbShooting.ShootOrb(); break;
         }
     }

    private void StopRifleFire() { /* ... código existente ... */
          if (currentWeapon == 3 && rifleShooting) rifleShooting.StopFiring();
      }

    private IEnumerator ReloadCurrentWeapon() { /* ... código existente ... */
         switch (currentWeapon) {
             case 1: if(playerShooting) yield return StartCoroutine(playerShooting.Reload()); break;
             case 2: if(shotgunShooting) yield return StartCoroutine(shotgunShooting.Reload()); break;
             case 3: if(rifleShooting) yield return StartCoroutine(rifleShooting.Reload()); break;
             case 4: if(defenseOrbShooting) yield return StartCoroutine(defenseOrbShooting.Reload()); break;
         }
      }

     private void ToggleSlowMotion() { /* ... código existente ... */
         if(slowMotion == null) { Debug.LogWarning("PlayerController: SlowMotion component not found!"); return; }
          if (slowMotion.remainingSlowMotionTime > 0f || slowMotion.isSlowMotionActive) {
             if (slowMotion.isSlowMotionActive) slowMotion.PauseSlowMotion();
             else slowMotion.ActivateSlowMotion();
          } else { Debug.Log("SlowMotion: No hay carga restante."); }
      }

} // Fin de la clase PlayerController