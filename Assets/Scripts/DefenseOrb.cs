using UnityEngine;

/// <summary>
/// Orbe que gira alrededor del jugador y destruye
/// enemigos/proyectiles cuyo color lógico coincide.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DefenseOrb : MonoBehaviour
{
    /*──────────── Ajustes públicos ────────────*/
    [Header("Lógica de color y durabilidad")]
    public Color orbColor = Color.white;
    public int durability = 3;

    [Header("Movimiento orbital (set desde DefenseOrbShooting)")]
    [HideInInspector] public float currentAngle = 0f;
    [HideInInspector] public float orbitRadius = 2f;
    [HideInInspector] public float orbitSpeed = 90f;

    [Header("Visual")]
    [Tooltip("Actívalo si tu sprite base es blanco y quieres teñirlo por código.")]
    public bool tintSprite = false;

    /*──────────── Propiedad pública ────────────*/
    public Transform Player => player;

    /*──────────── Privados ────────────*/
    Transform player;
    SpriteRenderer sr;

    /*──────────── Unity ────────────*/
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (tintSprite && sr != null) sr.color = orbColor;

        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }

    void Update()
    {
        currentAngle += orbitSpeed * Time.deltaTime;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        if (player != null) transform.position = player.position + offset;
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

        // 2) Cualquier tipo de enemigo → usa EnemyBase
        if (!didDamage)
        {
            EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null && enemy.enemyColor == orbColor)
            {
                enemy.TakeDamage(1);
                didDamage = true;
            }
        }

        if (didDamage)
            DecreaseDurability();
    }

    /*──────────── Helpers ────────────*/
    void DecreaseDurability()
    {
        durability--;
        if (durability <= 0) Destroy(gameObject);
    }
}
