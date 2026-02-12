using UnityEngine;

/// <summary>
/// Enemigo básico: aparece fuera de cámara, avanza hasta ser visible,
/// se detiene un instante y luego se lanza hacia el jugador.
/// </summary>
public class Enemy : EnemyBase
{
    /*───────────────────  INSPECTOR  ───────────────────*/

    [Header("Rotación")]
    public float minRotationSpeed = -180f;
    public float maxRotationSpeed =  180f;

    [Header("Pausa al entrar")]
    [Tooltip("Tiempo que permanece detenido al volverse visible (segundos).")]
    public float pauseDuration = 0.6f;

    [Header("Visibilidad (Viewport)")]
    public bool randomizeViewportMargin = false;

    [Range(0f, 0.49f)]
    public float viewportMargin = 0.03f;

    [Range(0f, 0.49f)]
    public float minRandomMargin = 0.10f;

    [Range(0f, 0.49f)]
    public float maxRandomMargin = 0.20f;

    /*───────────────────  PRIVADAS  ───────────────────*/

    Camera mainCam;
    float rotationSpeed;
    float pauseTimer;
    float margin;

    enum State { Approaching, Paused, Attacking }
    State state = State.Approaching;

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void Start()
    {
        base.Start();
        mainCam = Camera.main;
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        margin = randomizeViewportMargin
                 ? Random.Range(minRandomMargin, maxRandomMargin)
                 : viewportMargin;
    }

    void Update()
    {
        switch (state)
        {
            case State.Approaching:
                MoveTowardsPlayer();
                if (IsFullyInsideViewport(margin))
                {
                    state = State.Paused;
                    pauseTimer = pauseDuration;
                }
                break;

            case State.Paused:
                pauseTimer -= Time.deltaTime;
                if (pauseTimer <= 0f) state = State.Attacking;
                break;

            case State.Attacking:
                MoveTowardsPlayer();
                break;
        }

        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    /*───────────────────  HELPERS  ───────────────────*/

    bool IsFullyInsideViewport(float m)
    {
        if (!mainCam) return false;
        Vector3 vp = mainCam.WorldToViewportPoint(transform.position);
        return vp.z > 0f &&
               vp.x >= m && vp.x <= 1f - m &&
               vp.y >= m && vp.y <= 1f - m;
    }
}
