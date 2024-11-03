using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f; // Tiempo entre apariciones
    public float spawnDistance = 10f; // Distancia desde el centro para aparecer

    private float nextSpawnTime = 0f;

    // Lista de colores posibles para los enemigos
    public List<Color> enemyColors = new List<Color>();

    void Start()
    {
        // Define los colores de los enemigos
        enemyColors.Add(Color.yellow);
        enemyColors.Add(Color.blue);
        enemyColors.Add(Color.green);
        enemyColors.Add(Color.red);
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        // Genera una posición aleatoria alrededor del jugador
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;

        // Crea el enemigo
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Asigna un color aleatorio al enemigo
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            int randomIndex = Random.Range(0, enemyColors.Count);
            enemyScript.enemyColor = enemyColors[randomIndex];
        }
    }
}
