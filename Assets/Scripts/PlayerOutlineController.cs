using UnityEngine;

/// <summary>
/// Controla el shader de contorno del jugador, actualizando el color
/// basado en el arma y el color activo leídos desde PlayerController
/// y los scripts de disparo correspondientes.
/// </summary>
public class PlayerOutlineController : MonoBehaviour
{
    [Header("Outline Shader Control")]
    [Tooltip("Arrastra aquí el GameObject 'Character' (hijo de ShipBody) que tiene el SpriteRenderer con el material de contorno.")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer; // Asignar en Inspector

    [Header("Player References")]
    [Tooltip("Referencia al PlayerController para saber el arma activa. Se busca en el padre si no se asigna.")]
    [SerializeField] private PlayerController playerController;
    // Necesitaremos referencias a los scripts de disparo para leer su 'currentColor'
    private PlayerShooting playerShooting;
    private ShotgunShooting shotgunShooting;
    private RifleShooting rifleShooting;
    private DefenseOrbShooting defenseOrbShooting;

    // --- Variables del Material ---
    private Material outlineMaterialInstance;
    private const string OUTLINE_COLOR_PROPERTY = "_OutlineColor"; // Debe coincidir con Shader Graph

    void Awake()
    {
        // --- Obtener Referencias ---
        // Buscar PlayerController en el mismo objeto o en padres si no está asignado
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }
        // Obtener referencias a los scripts de disparo (asumiendo que están en el mismo objeto que PlayerController)
        if (playerController != null)
        {
            playerShooting = playerController.GetComponent<PlayerShooting>();
            shotgunShooting = playerController.GetComponent<ShotgunShooting>();
            rifleShooting = playerController.GetComponent<RifleShooting>();
            defenseOrbShooting = playerController.GetComponent<DefenseOrbShooting>();
        }
        else
        {
             Debug.LogError("PlayerOutlineController: No se pudo encontrar PlayerController!");
        }

        // Validar que tenemos el SpriteRenderer
        if (characterSpriteRenderer == null)
        {
             Debug.LogError("PlayerOutlineController ERROR: ¡'Character Sprite Renderer' no asignado en el Inspector!");
             return; // Salir si no hay renderer asignado
        }

        // Obtener instancia del material
        outlineMaterialInstance = characterSpriteRenderer.material;
        if (outlineMaterialInstance == null)
        {
             Debug.LogError("PlayerOutlineController ERROR: No se pudo obtener la instancia del material del Character Sprite Renderer!");
        } else {
             Debug.Log("PlayerOutlineController: Instancia de material para contorno obtenida.");
        }
    }

    void Start()
    {
        // --- Inicializar Contorno Oculto ---
        if (outlineMaterialInstance != null)
        {
            Color initialOutlineColor = Color.white;
            initialOutlineColor.a = 0f;
            outlineMaterialInstance.SetColor(OUTLINE_COLOR_PROPERTY, initialOutlineColor);
            Debug.Log("PlayerOutlineController: Contorno inicializado oculto (Alpha 0).");
        }
    }

    void Update()
    {
        // Salir si no tenemos las referencias necesarias
        if (playerController == null || outlineMaterialInstance == null) return;

        // --- Lógica de Actualización del Contorno ---
        Color activeColor = Color.white; // Color por defecto
        int currentWeapon = playerController.CurrentWeapon; // Leer arma activa desde PlayerController

        // 1. Leer el color del script del ARMA ACTIVA correspondiente
        switch (currentWeapon)
        {
            case 1: if (playerShooting) { activeColor = playerShooting.currentColor; } break;
            case 2: if (shotgunShooting) { activeColor = shotgunShooting.currentColor; } break;
            case 3: if (rifleShooting) { activeColor = rifleShooting.currentColor; } break;
            case 4: if (defenseOrbShooting) { activeColor = defenseOrbShooting.currentColor; } break;
        }
        // Nota: Los scripts de disparo siguen actualizando su propio 'currentColor' internamente
        //       basado en la lógica WASD que ya tenían. PlayerController solo les dice cuándo hacerlo.

        // --- DEBUG: Ver qué color se leyó (Puedes mantenerlo o quitarlo) ---
        // Debug.Log($"OutlineController - Color Activo leído: {activeColor}");

        // 2. Calcular color y alpha para el shader
        Color targetOutlineColor = activeColor;
        targetOutlineColor.a = (activeColor == Color.white) ? 0f : 1f; // Alpha 0 si es blanco, 1 si no

        // --- DEBUG: Ver qué color se intentará poner (Puedes mantenerlo o quitarlo) ---
        // Debug.Log($"OutlineController - Intentando poner: R={targetOutlineColor.r} G={targetOutlineColor.g} B={targetOutlineColor.b} A={targetOutlineColor.a}");

        // 3. Aplicar al material
        outlineMaterialInstance.SetColor(OUTLINE_COLOR_PROPERTY, targetOutlineColor);
    }
}