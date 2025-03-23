using UnityEngine;
using UnityEngine.UI;

public class EnemyOffScreenIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("Indicator prefab (e.g., a triangle image) to show on the UI.")]
    public GameObject indicatorPrefab;
    [Tooltip("Margin (in viewport percentage) to avoid placing the indicator at the very edge.")]
    [Range(0f, 0.5f)]
    public float margin = 0.05f;

    [Header("Indicator Scale")]
    [Tooltip("Maximum scale of the indicator when the enemy is near (center of screen).")]
    public float maxIndicatorScale = 1.5f;
    [Tooltip("Minimum scale of the indicator when the enemy is far (at the edge).")]
    public float minIndicatorScale = 0.5f;

    [Header("Blink Settings")]
    [Tooltip("Minimum blink speed when the enemy is far.")]
    public float minBlinkSpeed = 0.5f;
    [Tooltip("Maximum blink speed when the enemy is near.")]
    public float maxBlinkSpeed = 3f;
    [Tooltip("Minimum alpha value for blinking.")]
    [Range(0f, 1f)]
    public float minAlpha = 0.3f;
    [Tooltip("Maximum alpha value for blinking.")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    private GameObject indicatorInstance;
    private Image indicatorImage;
    private Camera mainCamera;
    private Canvas canvas;

    // Variable para almacenar el color del enemigo (Enemy, TankEnemy, etc.).
    private Color enemyColor = Color.white;

    // Temporizador local para el parpadeo.
    private float blinkTimer = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = FindObjectOfType<Canvas>();

        if (indicatorPrefab != null && canvas != null)
        {
            indicatorInstance = Instantiate(indicatorPrefab, canvas.transform);
            indicatorImage = indicatorInstance.GetComponent<Image>();
        }
    }

    /// <summary>
    /// Método para actualizar el color del enemigo desde distintos scripts (Enemy, TankEnemy, etc.).
    /// </summary>
    private void UpdateEnemyColor()
    {
        Enemy enemyComp = GetComponent<Enemy>();
        if (enemyComp != null)
        {
            enemyColor = enemyComp.enemyColor;
            return;
        }
        ShooterEnemy shooterComp = GetComponent<ShooterEnemy>();
        if (shooterComp != null)
        {
            enemyColor = shooterComp.enemyColor;
            return;
        }
        TankEnemy tankComp = GetComponent<TankEnemy>();
        if (tankComp != null)
        {
            enemyColor = tankComp.enemyColor;
            return;
        }
        EnemyZZ zzComp = GetComponent<EnemyZZ>();
        if (zzComp != null)
        {
            enemyColor = zzComp.enemyColor;
            return;
        }
    }

    void LateUpdate()
    {
        if (indicatorInstance == null || mainCamera == null)
            return;

        // Actualizar el color del enemigo
        UpdateEnemyColor();

        // Convertir la posición del enemigo a coordenadas viewport
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // Verificar si el enemigo está en pantalla
        bool isOnScreen = (viewportPos.z > 0 &&
                           viewportPos.x > 0 && viewportPos.x < 1 &&
                           viewportPos.y > 0 && viewportPos.y < 1);

        // Si está en pantalla, desactivar el indicador
        if (isOnScreen)
        {
            indicatorInstance.SetActive(false);
            return;
        }
        else
        {
            indicatorInstance.SetActive(true);
        }

        // Clampear viewportPos con márgenes
        viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1f - margin);
        viewportPos.y = Mathf.Clamp(viewportPos.y, margin, 1f - margin);

        // Convertir a posición de pantalla
        Vector3 screenPos = mainCamera.ViewportToScreenPoint(viewportPos);

        RectTransform indicatorRect = indicatorInstance.GetComponent<RectTransform>();
        if (indicatorRect != null)
        {
            // Posicionar el indicador en la pantalla
            indicatorRect.position = screenPos;

            // Calcular dirección desde el centro de la pantalla y ajustar la rotación
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 direction = ((Vector2)screenPos - screenCenter).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicatorRect.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

            // ---------------- CORRECCIÓN DE RELACIÓN DE ASPECTO ----------------
            // Normalizar X e Y para que la distancia no dependa del aspect ratio
            Vector2 offset = new Vector2(
                (screenPos.x - screenCenter.x) / (Screen.width * 0.5f),
                (screenPos.y - screenCenter.y) / (Screen.height * 0.5f)
            );
            float distance01 = offset.magnitude;  // Va de 0 (centro) hasta algo >1 si está muy lejos

            // Clampear la distancia a [0..1] para que no se salga de rango
            distance01 = Mathf.Clamp01(distance01);

            // ---------------- ESCALA DEL INDICADOR ----------------
            // Near => distance01 ~ 0 => grande (maxIndicatorScale)
            // Far  => distance01 ~ 1 => pequeño (minIndicatorScale)
            float scaleFactor = Mathf.Lerp(maxIndicatorScale, minIndicatorScale, distance01);
            indicatorRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            // ---------------- PARPADEO (BLINK) ----------------
            // Near => blink rápido => maxBlinkSpeed
            // Far  => blink lento => minBlinkSpeed
            float currentBlinkSpeed = Mathf.Lerp(maxBlinkSpeed, minBlinkSpeed, distance01);

            // Incrementar el temporizador local para el parpadeo
            blinkTimer += Time.deltaTime * currentBlinkSpeed;

            // Reiniciarlo para que no crezca indefinidamente
            if (blinkTimer > 2f) 
            {
                blinkTimer -= 2f; 
            }

            // Calcular alpha usando PingPong con el temporizador local (0..1)
            float pingPong = Mathf.PingPong(blinkTimer, 1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pingPong);

            if (indicatorImage != null)
            {
                indicatorImage.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, alpha);
            }
        }
    }

    void OnDestroy()
    {
        if (indicatorInstance != null)
            Destroy(indicatorInstance);
    }
}