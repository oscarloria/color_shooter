using UnityEngine;

/// <summary>
/// Enemigo zigzag: avanza hacia el jugador con movimiento sinusoidal lateral.
/// </summary>
public class EnemyZZ : EnemyBase
{
    [Header("Rotación")]
    public float minRotationSpeed = -180f;
    public float maxRotationSpeed = 180f;

    [Header("Parámetros de Zigzag")]
    [Tooltip("Amplitud del movimiento lateral para el efecto zigzag.")]
    public float zigzagAmplitude = 1f;
    [Tooltip("Frecuencia de oscilación para el efecto zigzag.")]
    public float zigzagFrequency = 2f;

    /*───────────────────  PRIVADAS  ───────────────────*/

    float rotationSpeed;
    float phaseOffset;

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void Start()
    {
        base.Start();
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        MoveZigzag();
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    /*───────────────────  MOVIMIENTO  ───────────────────*/

    void MoveZigzag()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
        float offset = Mathf.Sin(Time.time * zigzagFrequency + phaseOffset) * zigzagAmplitude;
        Vector3 moveVector = (direction * speed + perpendicular * offset) * Time.deltaTime;

        transform.position += moveVector;
    }
}
