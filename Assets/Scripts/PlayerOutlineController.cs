using UnityEngine;

public class PlayerOutlineController : MonoBehaviour
{
    [Header("Outline Shader Control")]
    [Tooltip("Arrastra aquí el GameObject 'Character' (hijo de ShipBody) que tiene el SpriteRenderer con el material de contorno.")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer;

    // --- Renombrado para claridad ---
    [Tooltip("Grosor normal del contorno cuando no se dispara.")]
    [Range(0f, 0.1f)]
    public float normalOutlineThickness = 0.02f;

    // --- NUEVAS VARIABLES PARA EL PULSO ---
    [Tooltip("Grosor del contorno justo al disparar.")]
    [Range(0f, 0.1f)]
    public float shootingOutlineThickness = 0.03f;

    [Tooltip("Duración en segundos del pulso de grosor al disparar.")]
    public float thicknessPulseDuration = 0.15f;
    // --- FIN NUEVAS VARIABLES ---

    [Header("Player References")]
    [Tooltip("Referencia al PlayerController para saber el arma activa. Se busca en el padre si no se asigna.")]
    [SerializeField] private PlayerController playerController;
    // Referencias a los scripts de disparo
    private PlayerShooting playerShooting;
    private ShotgunShooting shotgunShooting;
    private RifleShooting rifleShooting;
    private DefenseOrbShooting defenseOrbShooting;

    // --- Variables del Material ---
    private Material outlineMaterialInstance;
    private const string OUTLINE_COLOR_PROPERTY = "_OutlineColor";
    private const string OUTLINE_THICKNESS_PROPERTY = "_Outline_Thickness"; // Corregido

    // --- Variables internas para el pulso ---
    private bool isPulsingThickness = false;
    private float thicknessPulseTimer = 0f;
    // --- Fin variables internas pulso ---

    void Awake()
    {
        // --- Obtener Referencias (sin cambios) ---
        if (playerController == null) { playerController = GetComponentInParent<PlayerController>(); }
        if (playerController != null) {
            playerShooting = playerController.GetComponent<PlayerShooting>();
            shotgunShooting = playerController.GetComponent<ShotgunShooting>();
            rifleShooting = playerController.GetComponent<RifleShooting>();
            defenseOrbShooting = playerController.GetComponent<DefenseOrbShooting>();
        } else { Debug.LogError("PlayerOutlineController: No se pudo encontrar PlayerController!"); }

        if (characterSpriteRenderer == null) { Debug.LogError("PlayerOutlineController ERROR: ¡'Character Sprite Renderer' no asignado!"); return; }

        outlineMaterialInstance = characterSpriteRenderer.material;
        if (outlineMaterialInstance == null) { Debug.LogError("PlayerOutlineController ERROR: No se pudo obtener la instancia del material!"); }
        else { Debug.Log("PlayerOutlineController: Instancia de material para contorno obtenida."); }
    }

    void Start()
    {
        // --- Inicializar Contorno ---
        if (outlineMaterialInstance != null)
        {
            // Ocultar color inicial
            Color initialOutlineColor = Color.white;
            initialOutlineColor.a = 0f;
            outlineMaterialInstance.SetColor(OUTLINE_COLOR_PROPERTY, initialOutlineColor);

            // Aplicar grosor normal inicial
            outlineMaterialInstance.SetFloat(OUTLINE_THICKNESS_PROPERTY, normalOutlineThickness);

            Debug.Log($"PlayerOutlineController: Contorno inicializado oculto y grosor a {normalOutlineThickness}.");
        }
        isPulsingThickness = false; // Asegurar que no empieza pulsando
        thicknessPulseTimer = 0f;
    }

    // --- NUEVO MÉTODO PÚBLICO para ser llamado por los scripts de disparo ---
    /// <summary>
    /// Activa el efecto de pulso en el grosor del contorno.
    /// </summary>
    public void TriggerThicknessPulse()
    {
        if (!this.enabled || outlineMaterialInstance == null) return; // No hacer nada si está desactivado o sin material

        isPulsingThickness = true;
        thicknessPulseTimer = thicknessPulseDuration; // Reiniciar el temporizador del pulso
        // Aplicamos el grosor de disparo inmediatamente
        outlineMaterialInstance.SetFloat(OUTLINE_THICKNESS_PROPERTY, shootingOutlineThickness);
    }
    // --- FIN NUEVO MÉTODO ---

    void Update()
    {
        if (playerController == null || outlineMaterialInstance == null) return;

        // --- Lógica de Actualización del COLOR del Contorno (sin cambios) ---
        Color activeColor = Color.white;
        int currentWeapon = playerController.CurrentWeapon;
        switch (currentWeapon) { /* ... leer activeColor ... */
             case 1: if (playerShooting) { activeColor = playerShooting.currentColor; } break;
             case 2: if (shotgunShooting) { activeColor = shotgunShooting.currentColor; } break;
             case 3: if (rifleShooting) { activeColor = rifleShooting.currentColor; } break;
             case 4: if (defenseOrbShooting) { activeColor = defenseOrbShooting.currentColor; } break;
        }
        Color targetOutlineColor = activeColor;
        targetOutlineColor.a = (activeColor == Color.white) ? 0f : 1f;
        outlineMaterialInstance.SetColor(OUTLINE_COLOR_PROPERTY, targetOutlineColor);

        // --- Lógica de Actualización del GROSOR del Contorno (MODIFICADA) ---
        float targetThickness;

        // Si estamos en medio de un pulso
        if (isPulsingThickness)
        {
            targetThickness = shootingOutlineThickness; // Mantenemos el grosor de disparo
            thicknessPulseTimer -= Time.deltaTime;    // Decrementamos el temporizador

            // Si el tiempo del pulso se acabó
            if (thicknessPulseTimer <= 0f)
            {
                isPulsingThickness = false; // Terminamos el pulso
                targetThickness = normalOutlineThickness; // Volvemos al grosor normal
                // Aplicamos el grosor normal inmediatamente al terminar el pulso
                 outlineMaterialInstance.SetFloat(OUTLINE_THICKNESS_PROPERTY, targetThickness);
            }
             // Nota: El grosor de disparo ya se aplicó en TriggerThicknessPulse()
             // o se mantiene si el timer aún no llega a cero. No necesitamos SetFloat aquí dentro del if(isPulsing).
        }
        else // Si no estamos pulsando
        {
            targetThickness = normalOutlineThickness; // Usamos el grosor normal
             // Aplicamos el grosor normal (podría haber cambiado en el Inspector)
             outlineMaterialInstance.SetFloat(OUTLINE_THICKNESS_PROPERTY, targetThickness);
        }
        // --- Fin Lógica Grosor ---

    } // Fin de Update()
}