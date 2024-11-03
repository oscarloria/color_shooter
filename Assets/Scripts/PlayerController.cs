using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float fireRate = 0.01f; // Tiempo entre disparos
    private float nextFireTime = 0f; // Próximo momento en que se puede disparar

    // Variables para el efecto de escala
    public float scaleMultiplier = 1.1f; // Factor por el cual la nave se agranda
    public float scaleDuration = 0.1f;   // Duración del efecto de escala en segundos

    public Color currentColor = Color.white; // Color inicial de la nave

    // Variables para el cargador
    public int magazineSize = 6;        // Capacidad del cargador
    private int currentAmmo;            // Cantidad actual de proyectiles en el cargador
    public float reloadTime = 1f;       // Tiempo que tarda en recargar (en segundos)
    private bool isReloading = false;   // Indica si el jugador está recargando


    // TextMeshPro para el texto del cargador
    public TextMeshProUGUI ammoText;

    void Start()
    {
        currentAmmo = magazineSize; // Inicializar el cargador lleno
        UpdateAmmoText(); // Actualizar el texto de munición
    }

    void Update()
    {
        RotatePlayer();
        ChangeColor();

        // Si estamos recargando, no podemos disparar ni recargar de nuevo
        if (isReloading)
        {
            return;
        }

        // Recarga manual al presionar 'R'
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        // Disparar al hacer clic con el botón izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                // Iniciar recarga automática si no hay munición
                Debug.Log("Proyectiles agotados. Recargando automáticamente...");
                StartCoroutine(Reload());
            }
        }
    }

    void RotatePlayer()
    {
        // Tu código para rotar la nave
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void ChangeColor()
    {
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

        // Actualizar el color de la nave para reflejar el color seleccionado
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = currentColor;
        }
    }

    void Shoot()
    {
        // Solo disparar si el color actual no es blanco
        if (currentColor == Color.white)
        {
            return; // Salir del método sin disparar
        }

        // Restar munición
        currentAmmo--;
        Debug.Log("Munición restante: " + currentAmmo);
        
        // Actualizar el texto de munición
        UpdateAmmoText();

        // Instancia el proyectil en la posición de la nave con su rotación actual
        GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);

        // Aplica velocidad al proyectil
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * projectileSpeed;

        // Cambia el color del proyectil
        SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projSpriteRenderer != null)
        {
            projSpriteRenderer.color = currentColor;
        }

        // Iniciar el efecto de escala
        StartCoroutine(ScaleEffect());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recargando proyectiles...");

        // Actualizar el texto de munición
        UpdateAmmoText();

        // Esperar el tiempo de recarga
        yield return new WaitForSeconds(reloadTime);

        // Reponer la munición
        currentAmmo = magazineSize;
        isReloading = false;

        Debug.Log("Recarga completa.");

        // Actualizar el texto de munición
        UpdateAmmoText();
    }


    void UpdateAmmoText()
    {
         if (isReloading)
            {
                ammoText.text = "RELOADING";
            }
            else
            {
                ammoText.text = currentAmmo.ToString();
            }
        }


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

        // Asegurar que alcanza el tamaño objetivo
        transform.localScale = targetScale;

        elapsedTime = 0f;

        // Escalar hacia abajo
        while (elapsedTime < scaleDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / (scaleDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurar que regresa al tamaño original
        transform.localScale = originalScale;
    }
}