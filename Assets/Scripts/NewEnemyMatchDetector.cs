using UnityEngine;

/// <summary>
/// MOLDE C (HIJO - Prueba): 
/// Se adjunta a un hijo del enemigo (un trigger).
/// Detecta proyectiles del jugador. Si el color coincide,
/// le avisa al script 'NewIsometricEnemy' del padre.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NewEnemyMatchDetector : MonoBehaviour
{
    // Referencia al script principal del padre
    private NewIsometricEnemy parentEnemy; 

    void Awake()
    {
        // 1. Encontrar la referencia al padre
        parentEnemy = GetComponentInParent<NewIsometricEnemy>();
        
        // 2. Asegurarse de que este collider sea un Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (parentEnemy == null)
        {
            Debug.LogError("MatchDetector no pudo encontrar el script 'NewIsometricEnemy' en su padre!", gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentEnemy == null) return; // Seguridad

        // 1. Revisar si es un proyectil del jugador
        if (other.CompareTag("Projectile"))
        {
            Projectile proj = other.GetComponent<Projectile>();
            if (proj == null) return;

            // 2. Revisar si hay MATCH de color (compara con el color público del padre)
            if (proj.projectileColor == parentEnemy.enemyColor)
            {
                // 3. ¡SÍ HAY MATCH!
                // Le decimos al padre que se destruya
                parentEnemy.HandleMatch(); 
                
                // Destruimos el proyectil del jugador
                Destroy(other.gameObject);
            }
            // 4. Si NO hay match (else)
            // No hacemos NADA. El proyectil golpeará el collider
            // SÓLIDO del padre y rebotará.
        }
    }
}