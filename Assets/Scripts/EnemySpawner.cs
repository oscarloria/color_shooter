using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject tankEnemyPrefab; // Prefab del enemigo tipo tanque
    public float tankEnemySpawnChance = 0.2f; // Probabilidad de generar un TankEnemy

    public float spawnDistance = 10f; // Distancia desde el centro para aparecer

    // Lista completa de colores posibles para los enemigos
    public List<Color> enemyColors = new List<Color>();

    [Header("Dificultad Incremental")]
    public int initialEnemiesPerWave = 6;    // Enemigos en la primera oleada
    public int enemiesPerWaveIncrement = 2;  // Cuántos enemigos adicionales por oleada
    public float initialSpawnRate = 2f;       // Tiempo entre spawns al inicio
    public float spawnRateDecrement = 0.1f;   // Reducir el tiempo entre spawns
    public float initialEnemySpeed = 2f;      // Velocidad inicial de los enemigos
    public float enemySpeedIncrement = 0.2f;  // Aumento de velocidad de enemigos por oleada
    public float wavePause = 4f;              // Pausa entre oleadas

    private int currentWave = 1;
    private int enemiesPerWave;
    private float currentSpawnRate;
    private float currentEnemySpeed;

    void Start()
    {
        // Define los colores de los enemigos (lista completa)
        enemyColors.Add(Color.yellow);
        enemyColors.Add(Color.blue);
        enemyColors.Add(Color.green);
        enemyColors.Add(Color.red);

        // Inicializar variables para la primera oleada
        enemiesPerWave = initialEnemiesPerWave;
        currentSpawnRate = initialSpawnRate;
        currentEnemySpeed = initialEnemySpeed;

        // Iniciar el ciclo de oleadas
        StartCoroutine(WaveCycle());
    }

    IEnumerator WaveCycle()
    {
        while (true)
        {
            // Ejecutar la oleada actual
            yield return StartCoroutine(SpawnWave());

            // Pausa entre oleadas
            yield return new WaitForSeconds(wavePause);

            // Incrementar la dificultad para la siguiente oleada
            currentWave++;
            enemiesPerWave += enemiesPerWaveIncrement;
            currentSpawnRate = Mathf.Max(0.1f, currentSpawnRate - spawnRateDecrement);
            currentEnemySpeed += enemySpeedIncrement;
        }
    }

    IEnumerator SpawnWave()
    {
        int enemiesSpawned = 0;
        while (enemiesSpawned < enemiesPerWave)
        {
            SpawnEnemy();
            enemiesSpawned++;
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }

    void SpawnEnemy()
    {
        // Genera una posición aleatoria alrededor del jugador
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;

        // Determinar si se genera un TankEnemy o un enemigo normal
        float randomValue = Random.Range(0f, 1f);
        GameObject enemyObject;

        if (randomValue <= tankEnemySpawnChance)
        {
            // Generar TankEnemy
            enemyObject = Instantiate(tankEnemyPrefab, spawnPosition, Quaternion.identity);

            // Obtener la lista de colores disponibles para la oleada actual
            List<Color> availableColors = GetAvailableColorsForWave(currentWave);
            if (availableColors.Count > 0)
            {
                int randomIndex = Random.Range(0, availableColors.Count);
                Color chosenColor = availableColors[randomIndex];

                TankEnemy tankScript = enemyObject.GetComponent<TankEnemy>();
                if (tankScript != null)
                {
                    tankScript.speed = currentEnemySpeed;
                    tankScript.enemyColor = chosenColor;
                    tankScript.ApplyColor();
                }
            }
        }
        else
        {
            // Generar enemigo normal
            enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            List<Color> availableColors = GetAvailableColorsForWave(currentWave);
            if (availableColors.Count > 0)
            {
                int randomIndex = Random.Range(0, availableColors.Count);
                Color chosenColor = availableColors[randomIndex];

                Enemy enemyScript = enemyObject.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.enemyColor = chosenColor;
                    enemyScript.speed = currentEnemySpeed;
                }
            }
        }
    }

    // Método para obtener la lista de colores disponibles en función de la oleada actual
    List<Color> GetAvailableColorsForWave(int wave)
    {
        // Ejemplo de lógica basada en las oleadas:
        // 1-5: rojo
        // 6-10: azul
        // 11-15: rojo y azul
        // 16-20: verde
        // 21-25: amarillo
        // 26-30: verde y amarillo
        // 31+: todos los colores (rojo, azul, verde, amarillo)

        // Primero definimos colores unitarios
        Color rojo = Color.red;
        Color azul = Color.blue;
        Color verde = Color.green;
        Color amarillo = Color.yellow;

        List<Color> colorsForThisWave = new List<Color>();

        if (wave <= 5)
        {
            // Solo rojo
            colorsForThisWave.Add(rojo);
        }
        else if (wave <= 10)
        {
            // Solo azul
            colorsForThisWave.Add(azul);
        }
        else if (wave <= 15)
        {
            // rojo y azul
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
        }
        else if (wave <= 20)
        {
            // verde
            colorsForThisWave.Add(verde);
        }
        else if (wave <= 25)
        {
            // amarillo
            colorsForThisWave.Add(amarillo);
        }
        else if (wave <= 30)
        {
            // verde y amarillo
            colorsForThisWave.Add(verde);
            colorsForThisWave.Add(amarillo);
        }
        else
        {
            // todos los colores
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
            colorsForThisWave.Add(amarillo);
        }

        return colorsForThisWave;
    }
}