using UnityEngine;
using System.Collections;

public class DefenseOrb : MonoBehaviour
{
    [Header("Configuración del Orbe")]
    public Color orbColor = Color.white; // Color asignado desde DefenseOrbShooting
    public int durability = 3;           // Durabilidad: golpes que soporta

    [HideInInspector]
    public float currentAngle = 0f;      // Ángulo actual en grados (inicializado en DefenseOrbShooting)
    [HideInInspector]
    public float orbitRadius = 2f;       // Radio de la órbita (configurable)
    [HideInInspector]
    public float orbitSpeed = 90f;       // Velocidad angular (grados/seg)

    private Transform player;           // Referencia al jugador
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = orbColor;

        // Buscar al jugador por su tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Actualizar el ángulo de órbita
        currentAngle += orbitSpeed * Time.deltaTime;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 orbitOffset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        if (player != null)
        {
            // Posicionar el orbe en función de la posición del jugador (sin depender de su rotación)
            transform.position = player.position + orbitOffset;
        }
    }

    // Manejo de colisiones usando triggers.
    void OnTriggerEnter2D(Collider2D other)
    {
        bool inflictedDamage = false;

        // Primero, verificar si colisiona con un EnemyProjectile.
        EnemyProjectile enemyProj = other.GetComponent<EnemyProjectile>();
        if (enemyProj != null)
        {
            if (enemyProj.bulletColor == orbColor)
            {
                Destroy(other.gameObject);
                inflictedDamage = true;
            }
        }

        // Si no se infligió daño por proyectil, buscar componentes de enemigo.
        if (!inflictedDamage)
        {
            // Buscar TankEnemy
            TankEnemy tank = other.GetComponentInParent<TankEnemy>();
            if (tank != null && tank.enemyColor == orbColor)
            {
                tank.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
                inflictedDamage = true;
            }
        }
        if (!inflictedDamage)
        {
            // Buscar ShooterEnemy
            ShooterEnemy shooter = other.GetComponentInParent<ShooterEnemy>();
            if (shooter != null && shooter.enemyColor == orbColor)
            {
                shooter.SendMessage("DestroyShooterEnemy", SendMessageOptions.DontRequireReceiver);
                inflictedDamage = true;
            }
        }
        if (!inflictedDamage)
        {
            // Buscar EnemyZZ
            EnemyZZ zz = other.GetComponentInParent<EnemyZZ>();
            if (zz != null && zz.enemyColor == orbColor)
            {
                zz.SendMessage("DestroyEnemy", SendMessageOptions.DontRequireReceiver);
                inflictedDamage = true;
            }
        }
        if (!inflictedDamage)
        {
            // Buscar Enemy (base)
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null && enemy.enemyColor == orbColor)
            {
                enemy.SendMessage("DestroyEnemy", SendMessageOptions.DontRequireReceiver);
                inflictedDamage = true;
            }
        }
        
        if (inflictedDamage)
        {
            DecreaseDurability();
        }
    }

    void DecreaseDurability()
    {
        durability--;
        if (durability <= 0)
            Destroy(gameObject);
    }
}
