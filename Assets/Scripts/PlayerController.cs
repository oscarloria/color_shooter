using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Variables de movimiento y disparo
    public float speed = 5f; // Velocidad de movimiento de la nave
    public GameObject projectilePrefab; // Prefab del proyectil que dispara la nave
    public float projectileSpeed = 20f; // Velocidad del proyectil
    public float fireRate = 0.01f; // Tiempo entre disparos consecutivos
    private float nextFireTime = 0f; // Tiempo en que el jugador puede disparar de nuevo

    // Variables para el efecto de escala (feedback visual al disparar)
    public float scaleMultiplier = 1.1f; // Factor por el cual la nave se agranda al disparar
    public float scaleDuration = 0.1f;   // Duración del efecto de escala en segundos

    // Variables de color para proyectiles y nave
    public Color currentColor = Color.white; // Color inicial de la nave

    // Variables para el cargador de munición
    public int magazineSize = 6; // Capacidad del cargador de proyectiles
    private int currentAmmo; // Proyectiles actuales en el cargador
    public float reloadTime = 1f; // Tiempo que tarda en recargar el cargador
    private bool isReloading = false; // Controla si el jugador está en proceso de recarga

    // UI
    public TextMeshProUGUI ammoText; // Texto que muestra el estado de munición en la interfaz

    // Variables de cámara lenta
    public float slowMotionDuration = 5f; // Duración máxima de la cámara lenta en segundos
    public float chargePerEnemy = 0.05f; // Carga que se suma a la barra de cámara lenta por enemigo destruido
    private bool isSlowMotionActive = false; // Indica si la cámara lenta está actualmente activa
    public Image slowMotionBar; // Barra de UI que representa la cantidad de cámara lenta restante
    private float remainingSlowMotionTime; // Tiempo restante de cámara lenta disponible

    void Start()
    {
        // Inicializar el cargador lleno y actualizar el texto de munición en la UI
        currentAmmo = magazineSize;
        UpdateAmmoText();

        // Inicializar el tiempo de cámara lenta disponible al máximo
        remainingSlowMotionTime = slowMotionDuration;

        // Inicializar la barra de cámara lenta llena (al 100%)
        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = 1f;
        }
    }

    void Update()
    {
        RotatePlayer(); // Controla la rotación de la nave hacia el mouse
        ChangeColor(); // Cambia el color de la nave basado en la tecla presionada

        
        if (isReloading) return; // Salir del método si el jugador está recargando

        // Verificación automática de recarga cuando la munición es 0
        if (currentAmmo <= 0)
        {
            Debug.Log("Proyectiles agotados. Recargando automáticamente...");
            StartCoroutine(Reload());
            return;
        }



        // Activar o pausar la cámara lenta al presionar la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && remainingSlowMotionTime > 0f)
        {
            if (isSlowMotionActive)
            {
                PauseSlowMotion(); // Pausar la cámara lenta si ya está activa
            }
            else
            {
                ActivateSlowMotion(); // Activar la cámara lenta si hay tiempo restante
            }
        }

        // Recargar manualmente al presionar la tecla 'R' si el cargador no está lleno
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        // Control de disparo al hacer clic con el botón izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(); // Llamar a Shoot solo si hay munición en el cargador
        }

    }

    // Rotación de la nave hacia la posición del mouse
    void RotatePlayer()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Posición del mouse en el mundo
        Vector2 direction = (mousePosition - transform.position).normalized; // Dirección hacia el mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Ángulo de rotación en grados
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // Aplicar la rotación
    }

    // Cambiar el color de la nave y los proyectiles según la tecla presionada
    void ChangeColor()
    {
        // Comprobaciones para cada tecla de cambio de color
        if (Input.GetKey(KeyCode.W))
        {
            currentColor = Color.yellow;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            currentColor = Color.blue;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentColor = Color.green;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            currentColor = Color.red;
        }
        else
        {
            currentColor = Color.white; // Si ninguna tecla está presionada, el color es blanco
        }

        // Aplicar el color seleccionado al sprite de la nave
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = currentColor;
        }
    }

    // Método para disparar proyectiles
    void Shoot()
    {
        if (currentColor == Color.white) return; // No disparar si el color es blanco (sin efecto)

        // Reducir la munición y actualizar la UI
        currentAmmo--;
        Debug.Log("Munición restante: " + currentAmmo);
        UpdateAmmoText();

        // Crear el proyectil y asignarle propiedades
        GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * projectileSpeed;

        // Asignar el color del proyectil
        SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projSpriteRenderer != null)
        {
            projSpriteRenderer.color = currentColor;
        }

        StartCoroutine(ScaleEffect()); // Iniciar el efecto de escala al disparar
    }

    // Corrutina para recargar el cargador
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recargando proyectiles...");

        UpdateAmmoText(); // Actualizar el texto de munición en UI a "RELOADING"
        yield return new WaitForSeconds(reloadTime); // Esperar el tiempo de recarga

        // Rellenar la munición al máximo y actualizar UI
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Recarga completa.");
        UpdateAmmoText();
    }

    // Actualizar el texto de munición en la UI
    void UpdateAmmoText()
    {
         if (isReloading)
         {
             ammoText.text = "RELOADING"; // Mostrar "RELOADING" mientras se recarga
         }
         else
         {
             ammoText.text = currentAmmo.ToString(); // Mostrar munición restante
         }
    }

    // Corrutina para el efecto de escala al disparar
    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;

        float elapsedTime = 0f;

        // Escalar hacia arriba
        while (elapsedTime < scaleDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / (scaleDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        elapsedTime = 0f;

        // Escalar hacia abajo
        while (elapsedTime < scaleDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / (scaleDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale; // Restaurar al tamaño original
    }

    // Activar cámara lenta y reducir la velocidad de tiempo global
    void ActivateSlowMotion()
    {
        isSlowMotionActive = true;
        Debug.Log("Cámara lenta activada");

        Time.timeScale = 0.4f; // Reducir el tiempo de juego a la mitad
        Time.fixedDeltaTime = Time.timeScale * 0.02f; // Ajustar el fixedDeltaTime para física

        StartCoroutine(UpdateSlowMotionBarWhileActive());
    }

    // Desactivar cámara lenta y restaurar la velocidad de tiempo
    void DeactivateSlowMotion()
    {
        isSlowMotionActive = false;
        Debug.Log("Cámara lenta desactivada.");

        Time.timeScale = 1f; // Restaurar el tiempo a su velocidad normal
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    // Pausar la cámara lenta sin reducir el tiempo restante
    void PauseSlowMotion()
    {
        isSlowMotionActive = false;
        Debug.Log("Cámara lenta pausada");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    // Corrutina para disminuir la barra de cámara lenta durante el uso
    IEnumerator UpdateSlowMotionBarWhileActive()
    {
        while (remainingSlowMotionTime > 0f && isSlowMotionActive)
        {
            remainingSlowMotionTime -= Time.unscaledDeltaTime; // Reducir el tiempo restante en cámara lenta

            if (slowMotionBar != null)
            {
                slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration; // Actualizar visualmente la barra
            }

            yield return null;
        }

        // Desactivar la cámara lenta automáticamente cuando el tiempo se agote
        if (remainingSlowMotionTime <= 0f)
        {
            DeactivateSlowMotion();
        }
    }

    // Añadir carga de cámara lenta al destruir un enemigo
    public void AddSlowMotionCharge()
    {
        // Aumentar el tiempo de cámara lenta restante hasta un máximo de `slowMotionDuration`
        remainingSlowMotionTime = Mathf.Min(remainingSlowMotionTime + (slowMotionDuration * chargePerEnemy), slowMotionDuration);

        // Actualizar la barra visual de cámara lenta
        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration;
        }

        Debug.Log("Cámara lenta recargada parcialmente al destruir un enemigo.");
    }
}