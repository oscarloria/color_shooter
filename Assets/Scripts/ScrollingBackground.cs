using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollingBackground : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float initialSpeed = 0.1f;
    [SerializeField] private float maxSpeed = 2.0f;
    [SerializeField] private float incrementAmount = 0.1f;
    [SerializeField] private float incrementInterval = 1.0f;

    [Header("Current Status")]
    [SerializeField] private float currentSpeed;
    
    private Material myMaterial;
    private float speedIncreaseTimer;
    private Vector2 offset;
    private float accumulatedOffset;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        myMaterial = rend.material;
        currentSpeed = initialSpeed;
        speedIncreaseTimer = 0f;
        accumulatedOffset = 0f;
    }

    void Update()
    {
        UpdateScrolling();
        HandleSpeedIncrease();
    }

    private void UpdateScrolling()
    {
        // Acumulamos el offset basado en la velocidad actual
        accumulatedOffset += currentSpeed * Time.deltaTime;
        offset = new Vector2(0f, accumulatedOffset);
        myMaterial.mainTextureOffset = offset;
    }

    private void HandleSpeedIncrease()
    {
        if (currentSpeed >= maxSpeed) return;

        speedIncreaseTimer += Time.deltaTime;
        
        if (speedIncreaseTimer >= incrementInterval)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + incrementAmount, initialSpeed, maxSpeed);
            speedIncreaseTimer = 0f;
        }
    }

    // Métodos para ajustar valores en tiempo de ejecución (opcional)
    public void SetSpeedParameters(float newInitial, float newMax, float newIncrement, float newInterval)
    {
        initialSpeed = newInitial;
        maxSpeed = newMax;
        incrementAmount = newIncrement;
        incrementInterval = newInterval;
        ResetSpeed();
    }

    public void ResetSpeed()
    {
        currentSpeed = initialSpeed;
        speedIncreaseTimer = 0f;
        accumulatedOffset = 0f;
    }
}