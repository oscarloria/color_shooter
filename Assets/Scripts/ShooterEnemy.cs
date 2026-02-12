using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo tirador con IA compleja:
/// 1) Entra hasta safeDistance
/// 2) Secuencia 3x: Dispara → Espera → Esquiva → Espera
/// 3) Modo Kamikaze: se lanza al jugador a 2x velocidad
/// </summary>
public class ShooterEnemy : EnemyBase
{
    [Header("Shooter — Disparo")]
    public GameObject shooterProjectilePrefab;
    public float projectileSpeed = 5f;

    [Header("Shooter — Distancia")]
    public float safeDistance = 6f;

    [Header("Shooter — Tiempos")]
    [Tooltip("Espera después de disparar antes de esquivar.")]
    public float waitTimeAfterShoot = 1.0f;
    [Tooltip("Espera después de esquivar antes de la siguiente acción.")]
    public float waitTimeAfterDodge = 1.0f;
    [Tooltip("Duración del movimiento de esquive.")]
    public float dodgeDuration = 1.0f;

    /*───────────────────  MÁQUINA DE ESTADOS  ───────────────────*/

    enum ShooterState
    {
        Entering,
        Shooting,
        WaitingAfterShoot,
        Dodging,
        WaitingAfterDodge,
        Kamikaze
    }

    ShooterState currentState;
    float stateTimer;
    Vector3 dodgeTarget;
    int shotsFiredCount;
    const int MAX_SHOTS_BEFORE_KAMIKAZE = 3;
    bool isFirstDodge = true;
    bool dodgeDirectionIsLeft;

    /*───────────────────  CICLO DE VIDA  ───────────────────*/

    protected override void Start()
    {
        base.Start();
        ResetState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetState();
    }

    void ResetState()
    {
        currentState = ShooterState.Entering;
        shotsFiredCount = 0;
        isFirstDodge = true;
        stateTimer = 0f;
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case ShooterState.Entering:         ProcessEntering();         break;
            case ShooterState.Shooting:          ProcessShooting();         break;
            case ShooterState.WaitingAfterShoot: ProcessWaitAfterShoot();   break;
            case ShooterState.Dodging:           ProcessDodging();          break;
            case ShooterState.WaitingAfterDodge: ProcessWaitAfterDodge();   break;
            case ShooterState.Kamikaze:          ProcessKamikaze();         break;
        }
    }

    /*───────────────────  ESTADOS  ───────────────────*/

    void ProcessEntering()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > safeDistance)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            AimAtPlayer();
        }
        else
        {
            currentState = ShooterState.Shooting;
        }
    }

    void ProcessShooting()
    {
        AimAtPlayer();
        ShootProjectile();
        shotsFiredCount++;
        currentState = ShooterState.WaitingAfterShoot;
        stateTimer = waitTimeAfterShoot;
    }

    void ProcessWaitAfterShoot()
    {
        AimAtPlayer();
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = ShooterState.Dodging;
            CalculateDodgeTarget();
            stateTimer = dodgeDuration;
        }
    }

    void ProcessDodging()
    {
        transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, speed * Time.deltaTime);
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = ShooterState.WaitingAfterDodge;
            stateTimer = waitTimeAfterDodge;
        }
    }

    void ProcessWaitAfterDodge()
    {
        AimAtPlayer();
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = shotsFiredCount < MAX_SHOTS_BEFORE_KAMIKAZE
                ? ShooterState.Shooting
                : ShooterState.Kamikaze;
        }
    }

    void ProcessKamikaze()
    {
        AimAtPlayer();
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * (speed * 2f) * Time.deltaTime;
    }

    /*───────────────────  DAÑO  ───────────────────*/

    protected override void OnDamageTaken()
    {
        StartCoroutine(DamageFeedback());
    }

    /// <summary>
    /// Override: desactivar antes de destruir (evita colisiones fantasma).
    /// </summary>
    protected override void Die()
    {
        if (!gameObject.activeInHierarchy) return;

        ScoreManager.Instance?.AddScore(scoreValue);
        GetComponent<EnemyCoinDrop>()?.TryDropCoins();
        SpawnExplosion(enemyColor);
        GiveSlowMotionCharge();

        gameObject.SetActive(false);
        Destroy(gameObject, 0.1f);
    }

    /*───────────────────  HELPERS  ───────────────────*/

    void AimAtPlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Quaternion target = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 10f);
        }
    }

    void ShootProjectile()
    {
        if (shooterProjectilePrefab == null || player == null) return;

        Vector3 spawnPos = transform.position + transform.up * 0.5f;
        GameObject projObj = Instantiate(shooterProjectilePrefab, spawnPos, transform.rotation);

        if (projObj.TryGetComponent(out SpriteRenderer projSR))
            projSR.color = enemyColor;

        if (projObj.TryGetComponent(out EnemyProjectile enemyProj))
            enemyProj.bulletColor = enemyColor;

        if (projObj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = transform.up * projectileSpeed;
        }
        else
        {
            StartCoroutine(MoveProjectileFallback(projObj.transform, transform.up, projectileSpeed));
        }
    }

    void CalculateDodgeTarget()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 perp = new Vector3(-toPlayer.y, toPlayer.x, 0f);

        if (isFirstDodge)
        {
            dodgeDirectionIsLeft = (Random.value < 0.5f);
            isFirstDodge = false;
        }
        else
        {
            dodgeDirectionIsLeft = !dodgeDirectionIsLeft;
        }

        Vector3 dodgeDir = dodgeDirectionIsLeft ? perp : -perp;
        dodgeTarget = transform.position + dodgeDir * 3f;
    }

    /*───────────────────  FEEDBACK VISUAL  ───────────────────*/

    IEnumerator DamageFeedback()
    {
        if (sr == null) yield break;

        Color original = sr.color;

        // Flash x2
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        sr.color = original;
        yield return new WaitForSeconds(0.05f);
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        sr.color = original;

        // Shake
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        float shakeDuration = 0.15f;
        float magnitude = 0.1f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }

    IEnumerator MoveProjectileFallback(Transform projTransform, Vector3 direction, float projSpeed)
    {
        float lifetime = 5f;
        float timer = 0f;
        while (projTransform != null && timer < lifetime)
        {
            projTransform.position += direction * projSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        if (projTransform != null) Destroy(projTransform.gameObject);
    }
}
