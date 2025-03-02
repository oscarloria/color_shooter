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

    private Transform player;          // Referencia al jugador
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = orbColor;

        // Buscar al jugador por su tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Actualizar el ángulo
        currentAngle += orbitSpeed * Time.deltaTime;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 orbitOffset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        if (player != null)
        {
            // Posicionar el orbe en función de la posición del jugador, sin depender de la rotación del jugador
            transform.position = player.position + orbitOffset;
        }
    }

    // Manejo de colisiones con enemigos usando triggers.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Obtener componente Enemy para verificar el color
            Enemy enemyComponent = other.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                if (enemyComponent.enemyColor == orbColor)
                {
                    // Infligir daño: se usa SendMessage para llamar a TakeDamage(1) si existe
                    other.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
                    DecreaseDurability();
                }
            }
        }
    }

    void DecreaseDurability()
    {
        durability--;
        if (durability <= 0)
            Destroy(gameObject);
    }
}