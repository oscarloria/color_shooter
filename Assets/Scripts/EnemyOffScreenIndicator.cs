using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra un triángulo indicador en el borde de la pantalla
/// apuntando hacia enemigos fuera del viewport. Color, escala y
/// velocidad de parpadeo varían según la distancia.
/// </summary>
public class EnemyOffScreenIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    public GameObject indicatorPrefab;
    [Range(0f, 0.5f)]
    public float margin = 0.05f;

    [Header("Indicator Scale")]
    public float maxIndicatorScale = 1.5f;
    public float minIndicatorScale = 0.5f;

    [Header("Blink Settings")]
    public float minBlinkSpeed = 0.5f;
    public float maxBlinkSpeed = 3f;
    [Range(0f, 1f)] public float minAlpha = 0.3f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    /*───────────────────  PRIVADAS  ───────────────────*/

    GameObject indicatorInstance;
    Image indicatorImage;
    Camera mainCamera;
    EnemyBase enemyBase;
    float blinkTimer;

    /*───────────────────  UNITY  ───────────────────*/

    void Start()
    {
        mainCamera = Camera.main;
        enemyBase = GetComponent<EnemyBase>();

        Canvas canvas = FindObjectOfType<Canvas>();
        if (indicatorPrefab != null && canvas != null)
        {
            indicatorInstance = Instantiate(indicatorPrefab, canvas.transform);
            indicatorImage = indicatorInstance.GetComponent<Image>();
        }
    }

    void LateUpdate()
    {
        if (indicatorInstance == null || mainCamera == null || enemyBase == null) return;

        Color enemyColor = enemyBase.enemyColor;
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        bool isOnScreen = viewportPos.z > 0 &&
                          viewportPos.x > 0 && viewportPos.x < 1 &&
                          viewportPos.y > 0 && viewportPos.y < 1;

        if (isOnScreen)
        {
            indicatorInstance.SetActive(false);
            return;
        }

        indicatorInstance.SetActive(true);

        // Clampear con márgenes
        viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1f - margin);
        viewportPos.y = Mathf.Clamp(viewportPos.y, margin, 1f - margin);

        Vector3 screenPos = mainCamera.ViewportToScreenPoint(viewportPos);
        RectTransform rect = indicatorInstance.GetComponent<RectTransform>();
        if (rect == null) return;

        rect.position = screenPos;

        // Rotación: apuntar desde el centro de pantalla hacia el enemigo
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 direction = ((Vector2)screenPos - screenCenter).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // Distancia normalizada (aspect-ratio corrected)
        Vector2 offset = new Vector2(
            (screenPos.x - screenCenter.x) / (Screen.width * 0.5f),
            (screenPos.y - screenCenter.y) / (Screen.height * 0.5f)
        );
        float distance01 = Mathf.Clamp01(offset.magnitude);

        // Escala: cerca → grande, lejos → pequeño
        float scale = Mathf.Lerp(maxIndicatorScale, minIndicatorScale, distance01);
        rect.localScale = new Vector3(scale, scale, 1f);

        // Parpadeo: cerca → rápido, lejos → lento
        float blinkSpeed = Mathf.Lerp(maxBlinkSpeed, minBlinkSpeed, distance01);
        blinkTimer += Time.deltaTime * blinkSpeed;
        if (blinkTimer > 2f) blinkTimer -= 2f;

        float pingPong = Mathf.PingPong(blinkTimer, 1f);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, pingPong);

        if (indicatorImage != null)
            indicatorImage.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, alpha);
    }

    void OnDestroy()
    {
        if (indicatorInstance != null)
            Destroy(indicatorInstance);
    }
}
