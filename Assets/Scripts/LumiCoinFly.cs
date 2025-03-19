using UnityEngine;

public class LumiCoinFly : MonoBehaviour
{
    [Tooltip("Velocidad de vuelo de la moneda hacia el jugador.")]
    public float flySpeed = 5f;

    [Tooltip("Tiempo que tarda en destruirse si no encuentra al jugador (prevención).")]
    public float destroyTime = 3f;

    private Transform player;
    private float timer = 0f;

    void Start()
    {
        // Buscar al jugador por etiqueta si la tienes configurada, por ejemplo "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Por seguridad, destruir si pasa el tiempo y no hay jugador
        timer += Time.deltaTime;
        if (timer >= destroyTime)
        {
            Destroy(gameObject);
            return;
        }

        if (player != null)
        {
            // Mover la moneda hacia la posición del jugador
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * flySpeed * Time.deltaTime;
        }
    }
}