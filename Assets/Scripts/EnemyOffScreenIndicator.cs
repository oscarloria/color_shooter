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

    // Variable to store the enemy's color from various scripts.
    private Color enemyColor = Color.white;

    // Temporizador local para el parpadeo.
    private float blinkTimer = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = FindObjectOfType<Canvas>();

        if (indicatorPrefab != null && canvas != null)
        {
            // Instanciar el indicador como hijo del Canvas
            indicatorInstance = Instantiate(indicatorPrefab, canvas.transform);
            indicatorImage = indicatorInstance.GetComponent<Image>();
        }
    }

    // Método para actualizar el color del enemigo desde distintos componentes.
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

        // Determinar si el enemigo está en pantalla
        bool isOnScreen = (viewportPos.z > 0 &&
                           viewportPos.x > 0 && viewportPos.x < 1 &&
                           viewportPos.y > 0 && viewportPos.y < 1);

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
        viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1 - margin);
        viewportPos.y = Mathf.Clamp(viewportPos.y, margin, 1 - margin);

        // Convertir a posición de pantalla
        Vector3 screenPos = mainCamera.ViewportToScreenPoint(viewportPos);

        RectTransform indicatorRect = indicatorInstance.GetComponent<RectTransform>();
        if (indicatorRect != null)
        {
            // Posicionar el indicador en la pantalla
            indicatorRect.position = screenPos;

            // Calcular dirección desde el centro de la pantalla y ajustar la rotación
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 direction = (screenPos - (Vector3)screenCenter).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicatorRect.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Usar la distancia euclidiana desde el centro para determinar la escala
            float distance = Vector2.Distance(screenPos, screenCenter);
            float maxPossibleDistance = Vector2.Distance(Vector2.zero, screenCenter);
            float normalizedDistance = distance / maxPossibleDistance;
            float scaleFactor = Mathf.Lerp(maxIndicatorScale, minIndicatorScale, normalizedDistance);
            indicatorRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            // Calcular la velocidad de parpadeo basada en la distancia
            float currentBlinkSpeed = Mathf.Lerp(maxBlinkSpeed, minBlinkSpeed, normalizedDistance);

            // Incrementar el temporizador local para el parpadeo
            blinkTimer += Time.deltaTime * currentBlinkSpeed;
            // Calcular alpha usando PingPong con el temporizador local
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(blinkTimer, 1f));

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
