using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireRate = 0.5f; // Tiempo entre disparos
    private float nextFireTime = 0f; // Próximo momento en que se puede disparar

    // Variables para el efecto de escala
    public float scaleMultiplier = 1.1f; // Factor por el cual la nave se agranda
    public float scaleDuration = 0.1f;   // Duración del efecto de escala en segundos

    public Color currentColor = Color.white; // Color inicial de la nave

    void Update()
    {
        RotatePlayer();
        ChangeColor();

        // Disparar al hacer clic con el botón izquierdo del ratón
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
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
