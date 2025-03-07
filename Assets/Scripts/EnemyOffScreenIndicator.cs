using UnityEngine;
using UnityEngine.UI;

public class EnemyOffScreenIndicator : MonoBehaviour
{
    [Header("Configuración del Indicador")]
    [Tooltip("Prefab del indicador (por ejemplo, una imagen de triángulo) que se mostrará en la UI.")]
    public GameObject indicatorPrefab;
    [Tooltip("Margen en porcentaje (del viewport) para no pegar el indicador al borde exacto.")]
    [Range(0f, 0.5f)]
    public float margin = 0.05f;

    [Header("Escala del Indicador")]
    [Tooltip("Escala mínima del indicador cuando el enemigo está lejos.")]
    public float minIndicatorScale = 0.5f;
    [Tooltip("Escala máxima del indicador cuando el enemigo está cerca.")]
    public float maxIndicatorScale = 1.5f;
    [Tooltip("Distancia a partir de la cual el indicador alcanza su escala máxima.")]
    public float minDistance = 5f;
    [Tooltip("Distancia a partir de la cual el indicador alcanza su escala mínima.")]
    public float maxDistance = 20f;

    [Header("Parpadeo")]
    [Tooltip("Velocidad mínima de parpadeo del indicador (cuando el enemigo está lejos).")]
    public float minBlinkSpeed = 0.5f;
    [Tooltip("Velocidad máxima de parpadeo del indicador (cuando el enemigo está cerca).")]
    public float maxBlinkSpeed = 3f;
    [Tooltip("Alpha mínimo del parpadeo.")]
    [Range(0f, 1f)]
    public float minAlpha = 0.3f;
    [Tooltip("Alpha máximo del parpadeo.")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    private GameObject indicatorInstance;
    private Image indicatorImage;
    private Camera mainCamera;
    private Canvas canvas;
    
    // No vamos a usar una variable de tipo Enemy, sino obtener el color desde cualquiera de los scripts
    private Color enemyColor = Color.white;

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

    // Método para intentar obtener el color del enemigo desde diferentes scripts.
    private void UpdateEnemyColor()
    {
        // Intentar con Enemy
        Enemy enemyComp = GetComponent<Enemy>();
        if (enemyComp != null)
        {
            enemyColor = enemyComp.enemyColor;
            return;
        }
        // Intentar con ShooterEnemy
        ShooterEnemy shooterComp = GetComponent<ShooterEnemy>();
        if (shooterComp != null)
        {
            enemyColor = shooterComp.enemyColor;
            return;
        }
        // Intentar con TankEnemy
        TankEnemy tankComp = GetComponent<TankEnemy>();
        if (tankComp != null)
        {
            enemyColor = tankComp.enemyColor;
            return;
        }
        // Intentar con EnemyZZ
        EnemyZZ zzComp = GetComponent<EnemyZZ>();
        if (zzComp != null)
        {
            enemyColor = zzComp.enemyColor;
            return;
        }
    }

    void LateUpdate()
    {
        if (indicatorInstance == null || mainCamera == null) return;

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
            indicatorRect.position = screenPos;

            // Calcular dirección y ajustar rotación
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 direction = (screenPos - (Vector3)screenCenter).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicatorRect.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Calcular escala en función de la distancia
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
            float tScale = Mathf.InverseLerp(maxDistance, minDistance, distance);
            float scaleFactor = Mathf.Lerp(minIndicatorScale, maxIndicatorScale, tScale);
            indicatorRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            // Calcular velocidad de parpadeo en función de la distancia
            float tBlink = Mathf.InverseLerp(minDistance, maxDistance, distance);
            float currentBlinkSpeed = Mathf.Lerp(maxBlinkSpeed, minBlinkSpeed, tBlink);

            // Calcular alpha usando PingPong
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * currentBlinkSpeed, 1f));

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
