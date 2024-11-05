using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Variables de movimiento y disparo
    public float speed = 5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float fireRate = 0.01f;
    private float nextFireTime = 0f;

    // Variables para el efecto de escala
    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.1f;

    // Variables de color
    public Color currentColor = Color.white;

    // Variables para el cargador de munición
    public int magazineSize = 6;
    private int currentAmmo;
    public float reloadTime = 1f;
    private bool isReloading = false;

    // UI
    public TextMeshProUGUI ammoText;

    // Variables de cámara lenta
    public float slowMotionDuration = 5f;
    public float chargePerEnemy = 0.05f; // Carga por enemigo destruido
    private bool isSlowMotionActive = false;
    public Image slowMotionBar;
    private float remainingSlowMotionTime;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoText();
        remainingSlowMotionTime = slowMotionDuration;

        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = 1f;
        }
    }   

    void Update()
    {
        RotatePlayer();
        ChangeColor();

        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.Space) && remainingSlowMotionTime > 0f)
        {
            if (isSlowMotionActive)
            {
                PauseSlowMotion();
            }
            else
            {
                ActivateSlowMotion();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                Debug.Log("Proyectiles agotados. Recargando automáticamente...");
                StartCoroutine(Reload());
            }
        }
    }

    void RotatePlayer()
    {
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
            currentColor = Color.white;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = currentColor;
        }
    }

    void Shoot()
    {
        if (currentColor == Color.white) return;

        currentAmmo--;
        Debug.Log("Munición restante: " + currentAmmo);
        UpdateAmmoText();

        GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * projectileSpeed;

        SpriteRenderer projSpriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projSpriteRenderer != null)
        {
            projSpriteRenderer.color = currentColor;
        }

        StartCoroutine(ScaleEffect());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recargando proyectiles...");

        UpdateAmmoText();
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Recarga completa.");
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

        while (elapsedTime < scaleDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / (scaleDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        elapsedTime = 0f;

        while (elapsedTime < scaleDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / (scaleDuration / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    void ActivateSlowMotion()
    {
        isSlowMotionActive = true;
        Debug.Log("Cámara lenta activada");

        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        StartCoroutine(UpdateSlowMotionBarWhileActive());
    }

    void DeactivateSlowMotion()
    {
        isSlowMotionActive = false;
        Debug.Log("Cámara lenta desactivada.");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    void PauseSlowMotion()
    {
        isSlowMotionActive = false;
        Debug.Log("Cámara lenta pausada");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    IEnumerator UpdateSlowMotionBarWhileActive()
    {
        while (remainingSlowMotionTime > 0f && isSlowMotionActive)
        {
            remainingSlowMotionTime -= Time.unscaledDeltaTime;

            if (slowMotionBar != null)
            {
                slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration;
            }

            yield return null;
        }

        if (remainingSlowMotionTime <= 0f)
        {
            DeactivateSlowMotion();
        }
    }

    public void AddSlowMotionCharge()
    {
        remainingSlowMotionTime = Mathf.Min(remainingSlowMotionTime + (slowMotionDuration * chargePerEnemy), slowMotionDuration);

        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration;
        }

        Debug.Log("Cámara lenta recargada parcialmente al destruir un enemigo.");
    }
}