using UnityEngine;
using System.Collections;

/// <summary>
/// Orbe que gira alrededor del jugador y destruye
/// enemigos/proyectiles cuyo color lógico coincide.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DefenseOrb : MonoBehaviour
{
    /*──────────── Ajustes públicos ────────────*/
    [Header("Lógica de color y durabilidad")]
    public Color orbColor   = Color.white;   // color “lógico”
    public int   durability = 3;             // golpes que soporta

    [Header("Movimiento orbital (set desde DefenseOrbShooting)")]
    [HideInInspector] public float currentAngle = 0f;   // grados
    [HideInInspector] public float orbitRadius  = 2f;
    [HideInInspector] public float orbitSpeed   = 90f;  // °/seg

    [Header("Visual")]
    [Tooltip("Actívalo solo si tu sprite base es blanco y quieres teñirlo por código.")]
    public bool tintSprite = false;

    /*──────────── Propiedad pública ────────────*/
    /// <summary>Referencia al jugador (para otros scripts).</summary>
    public Transform Player => player;

    /*──────────── privados ────────────*/
    Transform      player;
    SpriteRenderer sr;

    /*──────────── Métodos Unity ────────────*/
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Tintar solo si el arte es blanco y lo deseas
        if (tintSprite && sr != null)
            sr.color = orbColor;

        // cachear jugador
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null)
            player = obj.transform;
    }

    void Update()
    {
        // Avanzar ángulo y orbitar
        currentAngle += orbitSpeed * Time.deltaTime;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;

        if (player != null)
            transform.position = player.position + offset;
    }

    /*──────────── Colisiones ────────────*/
    void OnTriggerEnter2D(Collider2D other)
    {
        bool didDamage = false;

        // 1) Proyectiles enemigos
        if (other.TryGetComponent(out EnemyProjectile eProj) && eProj.bulletColor == orbColor)
        {
            Destroy(other.gameObject);
            didDamage = true;
        }

        // 2) TankEnemy
        if (!didDamage && other.GetComponentInParent<TankEnemy>() is TankEnemy tank && tank.enemyColor == orbColor)
        {
            tank.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            didDamage = true;
        }

        // 3) ShooterEnemy
        if (!didDamage && other.GetComponentInParent<ShooterEnemy>() is ShooterEnemy shooter && shooter.enemyColor == orbColor)
        {
            shooter.SendMessage("DestroyShooterEnemy", SendMessageOptions.DontRequireReceiver);
            didDamage = true;
        }

        // 4) EnemyZZ
        if (!didDamage && other.GetComponentInParent<EnemyZZ>() is EnemyZZ zz && zz.enemyColor == orbColor)
        {
            zz.SendMessage("DestroyEnemy", SendMessageOptions.DontRequireReceiver);
            didDamage = true;
        }

        // 5) Enemy base
        if (!didDamage && other.GetComponentInParent<Enemy>() is Enemy baseEnemy && baseEnemy.enemyColor == orbColor)
        {
            baseEnemy.SendMessage("DestroyEnemy", SendMessageOptions.DontRequireReceiver);
            didDamage = true;
        }

        if (didDamage)
            DecreaseDurability();
    }

    /*──────────── Helpers ────────────*/
    void DecreaseDurability()
    {
        durability--;
        if (durability <= 0)
            Destroy(gameObject);
    }
}