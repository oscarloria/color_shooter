using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;        // Prefab del enemigo normal (Enemy)
    public GameObject tankEnemyPrefab;      // Prefab del enemigo tipo tanque
    [Tooltip("Prefab del nuevo ShooterEnemy.")]
    public GameObject shooterEnemyPrefab;   // NUEVO: ShooterEnemy
    [Tooltip("Prefab del nuevo EnemyZZ.")]
    public GameObject enemyZZPrefab;        // NUEVO: EnemyZZ

    [Header("Probabilidades de spawn")]
    public float tankEnemySpawnChance = 0.2f; // Probabilidad de generar un TankEnemy
    [Tooltip("Probabilidad de generar un ShooterEnemy (0..1).")]
    public float shooterSpawnChance = 0.1f;   // Probabilidad de generar un ShooterEnemy
    [Tooltip("Probabilidad de generar un EnemyZZ (0..1).")]
    public float enemyZZSpawnChance = 0.1f;   // Probabilidad de generar un EnemyZZ

    public float spawnDistance = 10f;         // Distancia desde el centro para aparecer

    // Lista completa de colores posibles para los enemigos
    public List<Color> enemyColors = new List<Color>();

    [Header("Dificultad Incremental")]
    public int initialEnemiesPerWave = 4;     // Reducido para una introducción más suave
    public int maxEnemiesPerWave = 24;        // Límite máximo de enemigos por oleada
    public float initialSpawnRate = 2.5f;     // Tiempo entre spawns al inicio (más lento)
    public float minSpawnRate = 0.3f;         // Límite mínimo de tiempo entre spawns
    public float initialEnemySpeed = 1.8f;    // Velocidad inicial de los enemigos (más lenta)
    public float maxEnemySpeed = 4.0f;        // Velocidad máxima de los enemigos
    public float wavePause = 5f;              // Pausa entre oleadas (aumentada)

    [Header("Eventos Aleatorios")]
    public float eventChance = 0.3f;          // Probabilidad de que ocurra un evento especial

    private int currentWave = 1;
    private int enemiesPerWave;
    private float currentSpawnRate;
    private float currentEnemySpeed;
    private bool isSpecialWave = false;

    void Start()
    {
        // Define los colores de los enemigos (si no se establecieron en el inspector)
        if (enemyColors.Count == 0)
        {
            enemyColors.Add(Color.yellow);
            enemyColors.Add(Color.blue);
            enemyColors.Add(Color.green);
            enemyColors.Add(Color.red);
        }

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
            // Anunciar la oleada (podría conectarse con UI)
            Debug.Log("¡Comienza la Oleada " + currentWave + "!");

            // Decidir si es una oleada especial
            isSpecialWave = (Random.value < eventChance && currentWave > 3);
            
            if (isSpecialWave)
            {
                yield return StartCoroutine(ExecuteSpecialWave());
            }
            else
            {
                // Ejecutar la oleada normal
                yield return StartCoroutine(SpawnWave());
            }

            // Pausa entre oleadas
            yield return new WaitForSeconds(wavePause);

            // Incrementar la dificultad para la siguiente oleada de manera progresiva
            IncrementDifficulty();
        }
    }

    void IncrementDifficulty()
    {
        currentWave++;
        
        // Incremento de enemigos progresivo basado en la oleada actual
        if (currentWave <= 5)
        {
            // Incremento suave al principio
            enemiesPerWave += 1;
        }
        else if (currentWave <= 15)
        {
            // Incremento moderado en oleadas intermedias
            enemiesPerWave += 2;
        }
        else
        {
            // Incremento mayor en oleadas avanzadas
            enemiesPerWave += 3;
        }
        
        // Asegurar que no exceda el máximo
        enemiesPerWave = Mathf.Min(enemiesPerWave, maxEnemiesPerWave);
        
        // Ajuste de velocidad de spawn más gradual
        float spawnRateDecrement = 0.1f * Mathf.Min(1f, currentWave / 10f);
        currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateDecrement);
        
        // Ajuste de velocidad de enemigos gradual con límite
        float speedIncrement = 0.15f * Mathf.Min(1f, currentWave / 15f);
        currentEnemySpeed = Mathf.Min(maxEnemySpeed, currentEnemySpeed + speedIncrement);
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

    IEnumerator ExecuteSpecialWave()
    {
        Debug.Log("¡Oleada Especial! Prepárate para una sorpresa...");
        
        // Elegir aleatoriamente un tipo de evento especial
        int eventType = Random.Range(0, 4);
        
        switch (eventType)
        {
            case 0: // Oleada rápida - muchos enemigos básicos que aparecen rápidamente
                yield return StartCoroutine(RapidWave());
                break;
                
            case 1: // Oleada de élite - pocos enemigos pero más difíciles
                yield return StartCoroutine(EliteWave());
                break;
                
            case 2: // Oleada de colores - todos los enemigos del mismo color
                yield return StartCoroutine(SingleColorWave());
                break;
                
            case 3: // Oleada de formación - enemigos aparecen en patrones específicos
                yield return StartCoroutine(FormationWave());
                break;
        }
    }

    IEnumerator RapidWave()
    {
        // Oleada rápida: Muchos enemigos básicos pero débiles
        Debug.Log("¡Oleada Rápida! Muchos enemigos aparecerán rápidamente.");
        
        int extraEnemies = enemiesPerWave + 6;
        float tempSpawnRate = currentSpawnRate * 0.5f;
        float tempSpeed = currentEnemySpeed * 1.2f;
        
        // Spawn rápido de enemigos normales principalmente
        for (int i = 0; i < extraEnemies; i++)
        {
            SpawnSpecificEnemy(0, tempSpeed); // Mayormente enemigos normales
            yield return new WaitForSeconds(tempSpawnRate);
        }
    }

    IEnumerator EliteWave()
    {
        // Oleada de élite: Pocos enemigos pero más fuertes
        Debug.Log("¡Oleada de Élite! Pocos enemigos pero más poderosos.");
        
        int reducedEnemies = Mathf.Max(3, enemiesPerWave / 2);
        float tempSpawnRate = currentSpawnRate * 1.5f;
        
        for (int i = 0; i < reducedEnemies; i++)
        {
            // Seleccionar aleatoriamente entre TankEnemy y ShooterEnemy (los tipos más fuertes)
            SpawnSpecificEnemy(Random.Range(1, 3), currentEnemySpeed * 0.8f); // Más lentos pero más resistentes
            yield return new WaitForSeconds(tempSpawnRate);
        }
    }
    
    IEnumerator SingleColorWave()
    {
        // Oleada de un solo color: todos los enemigos comparten el mismo color
        Color waveColor = GetRandomColorFromAvailable();
        Debug.Log("¡Oleada de Color! Todos los enemigos son de color " + ColorToString(waveColor));
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemyWithColor(waveColor);
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }
    
    IEnumerator FormationWave()
    {
        // Oleada de formación: los enemigos aparecen en patrones específicos
        Debug.Log("¡Oleada de Formación! Los enemigos aparecen en patrones.");
        
        // Patrón 1: Círculo
        yield return StartCoroutine(SpawnInCircle(enemiesPerWave / 3));
        
        yield return new WaitForSeconds(1f);
        
        // Patrón 2: Línea
        yield return StartCoroutine(SpawnInLine(enemiesPerWave / 3));
        
        yield return new WaitForSeconds(1f);
        
        // Patrón 3: Dos grupos
        int remaining = enemiesPerWave - (2 * (enemiesPerWave / 3));
        yield return StartCoroutine(SpawnInGroups(remaining));
    }
    
    IEnumerator SpawnInCircle(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * 2 * Mathf.PI;
            Vector2 spawnDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
            
            // Alternar entre enemigos normales y tanques
            GameObject prefab = (i % 2 == 0) ? enemyPrefab : tankEnemyPrefab;
            SpawnEnemyAtPosition(prefab, spawnPosition);
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    IEnumerator SpawnInLine(int count)
    {
        Vector2 startPosition = (Vector2)transform.position + Vector2.right * spawnDistance;
        Vector2 endPosition = (Vector2)transform.position - Vector2.right * spawnDistance;
        
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector2 spawnPosition = Vector2.Lerp(startPosition, endPosition, t);
            
            // Usar ShooterEnemy para esta formación
            SpawnEnemyAtPosition(shooterEnemyPrefab, spawnPosition);
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    IEnumerator SpawnInGroups(int count)
    {
        Vector2 position1 = (Vector2)transform.position + Vector2.up * spawnDistance;
        Vector2 position2 = (Vector2)transform.position + Vector2.down * spawnDistance;
        
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPosition = (i % 2 == 0) ? position1 : position2;
            
            // Usar EnemyZZ para un grupo y enemigos normales para el otro
            GameObject prefab = (i % 2 == 0) ? enemyZZPrefab : enemyPrefab;
            SpawnEnemyAtPosition(prefab, spawnPosition);
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    void SpawnEnemyAtPosition(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            prefab = enemyPrefab; // Usar enemigo básico como fallback
        }
        
        GameObject enemyObject = Instantiate(prefab, position, Quaternion.identity);
        
        List<Color> availableColors = GetAvailableColorsForWave(currentWave);
        if (availableColors.Count > 0)
        {
            Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];
            ApplyColorToEnemy(enemyObject, chosenColor);
        }
    }
    
    void SpawnEnemyWithColor(Color specificColor)
    {
        // Genera una posición aleatoria alrededor del spawner
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
        
        // Elegir un tipo de enemigo aleatorio
        float randomValue = Random.Range(0f, 1f);
        GameObject prefab = SelectEnemyPrefabByProbability(randomValue);
        
        GameObject enemyObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        ApplyColorToEnemy(enemyObject, specificColor);
    }
    
    GameObject SelectEnemyPrefabByProbability(float randomValue)
    {
        // Sumar probabilidades para los diferentes tipos de enemigos
        float combinedChanceTank = tankEnemySpawnChance;
        float combinedChanceShooter = combinedChanceTank + shooterSpawnChance;
        float combinedChanceZZ = combinedChanceShooter + enemyZZSpawnChance;
        
        // 1) TankEnemy
        if (randomValue <= combinedChanceTank)
        {
            return tankEnemyPrefab;
        }
        // 2) ShooterEnemy
        else if (randomValue <= combinedChanceShooter)
        {
            return shooterEnemyPrefab ?? enemyPrefab;
        }
        // 3) EnemyZZ
        else if (randomValue <= combinedChanceZZ)
        {
            return enemyZZPrefab ?? enemyPrefab;
        }
        // 4) enemigo normal
        else
        {
            return enemyPrefab;
        }
    }
    
    void SpawnSpecificEnemy(int enemyType, float speedModifier)
    {
        // Genera una posición aleatoria alrededor del spawner
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
        
        GameObject prefab;
        
        // Seleccionar el prefab según el tipo indicado
        switch (enemyType)
        {
            case 1: // TankEnemy
                prefab = tankEnemyPrefab;
                break;
            case 2: // ShooterEnemy
                prefab = shooterEnemyPrefab ?? enemyPrefab;
                break;
            case 3: // EnemyZZ
                prefab = enemyZZPrefab ?? enemyPrefab;
                break;
            default: // Enemigo normal
                prefab = enemyPrefab;
                break;
        }
        
        GameObject enemyObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        List<Color> availableColors = GetAvailableColorsForWave(currentWave);
        if (availableColors.Count > 0)
        {
            Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];
            ApplyColorToEnemy(enemyObject, chosenColor, speedModifier);
        }
    }

    void SpawnEnemy()
    {
        // Genera una posición aleatoria alrededor del spawner
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;

        // Ajustar probabilidades basadas en la oleada actual para una progresión de dificultad
        float adjustedTankChance = GetAdjustedProbability(tankEnemySpawnChance, currentWave, 3);
        float adjustedShooterChance = GetAdjustedProbability(shooterSpawnChance, currentWave, 5);
        float adjustedZZChance = GetAdjustedProbability(enemyZZSpawnChance, currentWave, 8);
        
        // Sumar probabilidades para los diferentes tipos de enemigos
        float combinedChanceTank = adjustedTankChance;
        float combinedChanceShooter = combinedChanceTank + adjustedShooterChance;
        float combinedChanceZZ = combinedChanceShooter + adjustedZZChance;
        float randomValue = Random.Range(0f, 1f);

        GameObject enemyObject = null;

        // 1) Spawn TankEnemy
        if (randomValue <= combinedChanceTank)
        {
            enemyObject = Instantiate(tankEnemyPrefab, spawnPosition, Quaternion.identity);

            List<Color> availableColors = GetAvailableColorsForWave(currentWave);
            if (availableColors.Count > 0)
            {
                Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];
                TankEnemy tankScript = enemyObject.GetComponent<TankEnemy>();
                if (tankScript != null)
                {
                    tankScript.speed = currentEnemySpeed;
                    tankScript.enemyColor = chosenColor;
                    tankScript.ApplyColor();
                }
            }
        }
        // 2) Spawn ShooterEnemy (solo después de la oleada 5)
        else if (randomValue <= combinedChanceShooter && currentWave >= 5)
        {
            if (shooterEnemyPrefab == null)
            {
                Debug.LogWarning("ShooterEnemyPrefab no asignado en EnemySpawner!");
                return;
            }

            enemyObject = Instantiate(shooterEnemyPrefab, spawnPosition, Quaternion.identity);

            List<Color> availableColors = GetAvailableColorsForWave(currentWave);
            if (availableColors.Count > 0)
            {
                Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];

                // Configuramos el ShooterEnemy
                ShooterEnemy shooterScript = enemyObject.GetComponent<ShooterEnemy>();
                if (shooterScript != null)
                {
                    shooterScript.enemyColor = chosenColor;
                    shooterScript.speed = currentEnemySpeed;
                }
            }
        }
        // 3) Spawn EnemyZZ (solo después de la oleada 8)
        else if (randomValue <= combinedChanceZZ && currentWave >= 8)
        {
            if (enemyZZPrefab == null)
            {
                Debug.LogWarning("EnemyZZPrefab no asignado en EnemySpawner!");
                return;
            }

            enemyObject = Instantiate(enemyZZPrefab, spawnPosition, Quaternion.identity);

            List<Color> availableColors = GetAvailableColorsForWave(currentWave);
            if (availableColors.Count > 0)
            {
                Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];

                // Configuramos el EnemyZZ
                EnemyZZ enemyZZScript = enemyObject.GetComponent<EnemyZZ>();
                if (enemyZZScript != null)
                {
                    enemyZZScript.enemyColor = chosenColor;
                    enemyZZScript.speed = currentEnemySpeed;
                }
            }
        }
        // 4) Spawn enemigo normal
        else
        {
            enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            List<Color> availableColors = GetAvailableColorsForWave(currentWave);
            if (availableColors.Count > 0)
            {
                Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];
                Enemy enemyScript = enemyObject.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.enemyColor = chosenColor;
                    enemyScript.speed = currentEnemySpeed;
                }
            }
        }
    }
    
    // Método que ajusta las probabilidades basándose en la progresión de oleadas
    float GetAdjustedProbability(float baseProbability, int wave, int minWave)
    {
        if (wave < minWave)
        {
            return 0f; // No aparece hasta cierta oleada
        }
        
        // Incrementa gradualmente la probabilidad hasta el doble del valor base en la oleada 20
        float scaleFactor = Mathf.Min(2.0f, 1.0f + ((wave - minWave) / 20.0f));
        return Mathf.Min(0.5f, baseProbability * scaleFactor); // Limite máximo de 50%
    }

    // Método para aplicar color a cualquier tipo de enemigo
    void ApplyColorToEnemy(GameObject enemyObject, Color color, float speedModifier = 1.0f)
    {
        // Intentar obtener los componentes de los diferentes tipos de enemigos
        TankEnemy tankScript = enemyObject.GetComponent<TankEnemy>();
        if (tankScript != null)
        {
            tankScript.enemyColor = color;
            tankScript.speed = currentEnemySpeed * speedModifier;
            tankScript.ApplyColor();
            return;
        }
        
        ShooterEnemy shooterScript = enemyObject.GetComponent<ShooterEnemy>();
        if (shooterScript != null)
        {
            shooterScript.enemyColor = color;
            shooterScript.speed = currentEnemySpeed * speedModifier;
            return;
        }
        
        EnemyZZ enemyZZScript = enemyObject.GetComponent<EnemyZZ>();
        if (enemyZZScript != null)
        {
            enemyZZScript.enemyColor = color;
            enemyZZScript.speed = currentEnemySpeed * speedModifier;
            return;
        }
        
        Enemy enemyScript = enemyObject.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.enemyColor = color;
            enemyScript.speed = currentEnemySpeed * speedModifier;
        }
    }
    
    // Método para obtener un color aleatorio de los disponibles
    Color GetRandomColorFromAvailable()
    {
        List<Color> availableColors = GetAvailableColorsForWave(currentWave);
        if (availableColors.Count > 0)
        {
            return availableColors[Random.Range(0, availableColors.Count)];
        }
        return Color.red; // Color por defecto
    }
    
    // Método para convertir un color a su nombre en texto
    string ColorToString(Color color)
    {
        if (color == Color.red) return "rojo";
        if (color == Color.blue) return "azul";
        if (color == Color.green) return "verde";
        if (color == Color.yellow) return "amarillo";
        return "desconocido";
    }

    // Método para obtener la lista de colores disponibles en función de la oleada actual
    List<Color> GetAvailableColorsForWave(int wave)
    {
        // Progresión de colores más gradual para una curva de aprendizaje suave
        Color rojo = Color.red;
        Color azul = Color.blue;
        Color verde = Color.green;
        Color amarillo = Color.yellow;

        List<Color> colorsForThisWave = new List<Color>();

        // Nuevos jugadores solo aprenden un color al principio
        if (wave <= 3)
        {
            // Solo rojo - el primer color para aprender
            colorsForThisWave.Add(rojo);
        }
        else if (wave <= 6)
        {
            // Rojo y azul - introduciendo el segundo color
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
        }
        else if (wave <= 10)
        {
            // Azul y verde - introduciendo el tercer color
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
        }
        else if (wave <= 15)
        {
            // Rojo, azul y verde - tres colores para dominar
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
        }
        else if (wave <= 20)
        {
            // Verde y amarillo - introduciendo el último color
            colorsForThisWave.Add(verde);
            colorsForThisWave.Add(amarillo);
        }
        else if (wave <= 25)
        {
            // Amarillo, rojo y verde - combinación desafiante
            colorsForThisWave.Add(amarillo);
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(verde);
        }
        else
        {
            // Todos los colores - desafío completo
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
            colorsForThisWave.Add(amarillo);
        }

        return colorsForThisWave;
    }
}