using UnityEngine;

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

    /// <summary>
    /// Método llamado al iniciar el script. Obtiene las referencias a los componentes necesarios.
    /// </summary>
    void Awake()
    {
        // Obtener referencias a los componentes adjuntos al jugador
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();
        slowMotion = GetComponent<SlowMotion>();
    }

    /// <summary>
    /// Método llamado una vez por frame. Maneja las entradas del usuario y coordina las acciones.
    /// </summary>
    void Update()
    {
        // Actualizar la rotación del jugador según la posición del mouse
        playerMovement.RotatePlayer();

        // Actualizar el color del jugador basado en la tecla presionada
        playerShooting.ChangeColor();

        // Si el jugador está recargando, no se permiten otras acciones
        if (playerShooting.isReloading) return;

        // Verificar si la munición se ha agotado y necesita recargar automáticamente
        if (playerShooting.currentAmmo <= 0 && !playerShooting.isReloading)
        {
            // Iniciar la corrutina de recarga automática
            StartCoroutine(playerShooting.Reload());
            return;
        }

        // Manejar el disparo al hacer clic con el botón izquierdo del mouse
        if (Input.GetMouseButtonDown(0))
        {
            playerShooting.Shoot();
        }

        // Manejar la recarga manual al presionar la tecla 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Solo recargar si no se tiene el cargador lleno
            if (playerShooting.currentAmmo < playerShooting.magazineSize)
            {
                StartCoroutine(playerShooting.Reload());
            }
        }

        // Manejar la activación y desactivación de la cámara lenta al presionar la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && slowMotion.remainingSlowMotionTime > 0f)
        {
            if (slowMotion.isSlowMotionActive)
            {
                // Pausar la cámara lenta si ya está activa
                slowMotion.PauseSlowMotion();
            }
            else
            {
                // Activar la cámara lenta si no está activa
                slowMotion.ActivateSlowMotion();
            }
        }
    }
}