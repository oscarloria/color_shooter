using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Para TextMeshProUGUI

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;        
    public GameObject tankEnemyPrefab;      
    [Tooltip("Prefab del nuevo ShooterEnemy.")]
    public GameObject shooterEnemyPrefab;   
    [Tooltip("Prefab del nuevo EnemyZZ.")]
    public GameObject enemyZZPrefab;        

    [Header("Probabilidades de spawn")]
    public float tankEnemySpawnChance = 0.2f;
    [Tooltip("Probabilidad de generar un ShooterEnemy (0..1).")]
    public float shooterSpawnChance = 0.1f;  
    [Tooltip("Probabilidad de generar un EnemyZZ (0..1).")]
    public float enemyZZSpawnChance = 0.1f;  

    public float spawnDistance = 10f;

    // Lista completa de colores posibles para los enemigos (usada en formaciones si se quiere)
    public List<Color> enemyColors = new List<Color>();

    [Header("Dificultad Incremental")]
    public int initialEnemiesPerWave = 4;     
    public int maxEnemiesPerWave = 24;        
    public float initialSpawnRate = 2.5f;     
    public float minSpawnRate = 0.3f;         
    public float initialEnemySpeed = 1.8f;    
    public float maxEnemySpeed = 4.0f;        

    [Header("Eventos Aleatorios")]
    public float eventChance = 0.3f; // Probabilidad de que ocurra un evento especial

    #region Variables para oleadas (para uso por WaveManager)
    [HideInInspector] public int enemiesPerWave; 
    [HideInInspector] public float currentSpawnRate;
    [HideInInspector] public float currentEnemySpeed;
    #endregion

    #region Métodos de Quadrant Spawn

    /// <summary>
    /// Retorna una posición de spawn y el color asignado según el cuadrante.
    /// Los cuadrantes se definen usando dos líneas diagonales:
    /// - Superior: ángulos entre 45° y 135° → Amarillo.
    /// - Derecho: ángulos entre -45° y 45° → Rojo.
    /// - Inferior: ángulos entre 225° y 315° → Verde.
    /// - Izquierdo: ángulos entre 135° y 225° → Azul.
    /// </summary>
    private (Vector2 spawnPosition, Color quadrantColor) GetSpawnData()
    {
        int quadrant = Random.Range(0, 4); // 0: Superior, 1: Derecho, 2: Inferior, 3: Izquierdo
        float angle = 0f;
        Color chosenColor = Color.white;
        switch (quadrant)
        {
            case 0: // Superior
                angle = Random.Range(45f, 135f);
                chosenColor = Color.yellow;
                break;
            case 1: // Derecho
                angle = Random.Range(-45f, 45f);
                chosenColor = Color.red;
                break;
            case 2: // Inferior
                angle = Random.Range(225f, 315f);
                chosenColor = Color.green;
                break;
            case 3: // Izquierdo
                angle = Random.Range(135f, 225f);
                chosenColor = Color.blue;
                break;
        }
        Vector2 spawnDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
        return (spawnPosition, chosenColor);
    }

    /// <summary>
    /// Calcula el color de cuadrante a partir de una dirección (relativa a la posición del spawner).
    /// </summary>
    private Color GetQuadrantColorFromDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        if (angle >= 45f && angle < 135f) return Color.yellow;
        else if (angle >= 135f && angle < 225f) return Color.blue;
        else if (angle >= 225f && angle < 315f) return Color.green;
        else return Color.red;
    }

    #endregion

    void Awake()
    {
        if (enemyColors.Count == 0)
        {
            enemyColors.Add(Color.yellow);
            enemyColors.Add(Color.blue);
            enemyColors.Add(Color.green);
            enemyColors.Add(Color.red);
        }

        enemiesPerWave = initialEnemiesPerWave;
        currentSpawnRate = initialSpawnRate;
        currentEnemySpeed = initialEnemySpeed;
    }

    #region Métodos Principales de Spawn (llamados desde WaveManager)

    public IEnumerator SpawnWave()
    {
        int enemiesSpawned = 0;
        while (enemiesSpawned < enemiesPerWave)
        {
            SpawnEnemy();
            enemiesSpawned++;
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }

    public IEnumerator ExecuteSpecialWave()
    {
        Debug.Log("¡Oleada Especial! Prepárate para una sorpresa...");
        int eventType = Random.Range(0, 4);
        switch (eventType)
        {
            case 0:
                yield return StartCoroutine(RapidWave());
                break;
            case 1:
                yield return StartCoroutine(EliteWave());
                break;
            case 2:
                yield return StartCoroutine(SingleColorWave());
                break;
            case 3:
                yield return StartCoroutine(FormationWave());
                break;
        }
    }

    #endregion

    #region Métodos de Eventos Especiales

    IEnumerator RapidWave()
    {
        Debug.Log("¡Oleada Rápida! Muchos enemigos aparecerán rápidamente.");
        int extraEnemies = enemiesPerWave + 6;
        float tempSpawnRate = currentSpawnRate * 0.5f;
        float tempSpeed = currentEnemySpeed * 1.2f;
        for (int i = 0; i < extraEnemies; i++)
        {
            SpawnSpecificEnemy(0, tempSpeed);
            yield return new WaitForSeconds(tempSpawnRate);
        }
    }

    IEnumerator EliteWave()
    {
        Debug.Log("¡Oleada de Élite! Pocos enemigos pero más poderosos.");
        int reducedEnemies = Mathf.Max(3, enemiesPerWave / 2);
        float tempSpawnRate = currentSpawnRate * 1.5f;
        for (int i = 0; i < reducedEnemies; i++)
        {
            SpawnSpecificEnemy(Random.Range(1, 3), currentEnemySpeed * 0.8f);
            yield return new WaitForSeconds(tempSpawnRate);
        }
    }

    IEnumerator SingleColorWave()
    {
        // En oleadas de color, se usa el color específico pasado por parámetro,
        // por lo que no se aplica la lógica de cuadrante.
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
        Debug.Log("¡Oleada de Formación! Los enemigos aparecen en patrones.");
        yield return StartCoroutine(SpawnInCircle(enemiesPerWave / 3));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(SpawnInLine(enemiesPerWave / 3));
        yield return new WaitForSeconds(1f);
        int remaining = enemiesPerWave - (2 * (enemiesPerWave / 3));
        yield return StartCoroutine(SpawnInGroups(remaining));
    }

    #endregion

    #region Métodos de Patrones de Spawn

    IEnumerator SpawnInCircle(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * 2 * Mathf.PI;
            Vector2 spawnDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
            // En formaciones, usamos GetQuadrantColorFromDirection
            Color quadrantColor = GetQuadrantColorFromDirection(spawnDirection);
            SpawnEnemyAtPosition(enemyPrefab, spawnPosition, quadrantColor);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator SpawnInLine(int count)
    {
        Vector2 startPosition = (Vector2)transform.position + Vector2.right * spawnDistance;
        Vector2 endPosition = (Vector2)transform.position - Vector2.right * spawnDistance;
        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : i / (float)(count - 1);
            Vector2 spawnPosition = Vector2.Lerp(startPosition, endPosition, t);
            // Calculamos la dirección desde el spawner a la posición generada
            Vector2 direction = (spawnPosition - (Vector2)transform.position).normalized;
            Color quadrantColor = GetQuadrantColorFromDirection(direction);
            SpawnEnemyAtPosition(shooterEnemyPrefab, spawnPosition, quadrantColor);
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
            Vector2 direction = (spawnPosition - (Vector2)transform.position).normalized;
            Color quadrantColor = GetQuadrantColorFromDirection(direction);
            GameObject prefab = (i % 2 == 0) ? enemyZZPrefab : enemyPrefab;
            SpawnEnemyAtPosition(prefab, spawnPosition, quadrantColor);
            yield return new WaitForSeconds(0.3f);
        }
    }

    #endregion

    #region Métodos Principales de Spawn

    /// <summary>
    /// Genera la posición de spawn y utiliza la lógica de cuadrante para asignar el color.
    /// </summary>
    public void SpawnEnemy()
    {
        // Usamos el helper para obtener la posición y color según cuadrante.
        (Vector2 spawnPosition, Color quadrantColor) = GetSpawnData();
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        // Asignamos el color calculado
        TankEnemy tankScript = enemyObject.GetComponent<TankEnemy>();
        if (tankScript != null)
        {
            tankScript.speed = currentEnemySpeed;
            tankScript.enemyColor = quadrantColor;
            tankScript.ApplyColor();
            return;
        }
        ShooterEnemy shooterScript = enemyObject.GetComponent<ShooterEnemy>();
        if (shooterScript != null)
        {
            shooterScript.enemyColor = quadrantColor;
            shooterScript.speed = currentEnemySpeed;
            return;
        }
        EnemyZZ enemyZZScript = enemyObject.GetComponent<EnemyZZ>();
        if (enemyZZScript != null)
        {
            enemyZZScript.enemyColor = quadrantColor;
            enemyZZScript.speed = currentEnemySpeed;
            return;
        }
        Enemy enemyScript = enemyObject.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.enemyColor = quadrantColor;
            enemyScript.speed = currentEnemySpeed;
        }
    }

    /// <summary>
    /// Similar a SpawnEnemy, pero se utiliza cuando se desea forzar un color específico (por ejemplo, en oleadas de color).
    /// </summary>
    void SpawnEnemyWithColor(Color specificColor)
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
        GameObject enemyObject = Instantiate(SelectEnemyPrefabByProbability(Random.Range(0f, 1f)), spawnPosition, Quaternion.identity);
        ApplyColorToEnemy(enemyObject, specificColor);
    }

    /// <summary>
    /// Genera un enemigo de tipo específico (0: normal, 1: TankEnemy, 2: ShooterEnemy, 3: EnemyZZ) con speedModifier.
    /// </summary>
    public void SpawnSpecificEnemy(int enemyType, float speedModifier)
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;
        GameObject prefab;
        switch (enemyType)
        {
            case 1:
                prefab = tankEnemyPrefab;
                break;
            case 2:
                prefab = shooterEnemyPrefab ?? enemyPrefab;
                break;
            case 3:
                prefab = enemyZZPrefab ?? enemyPrefab;
                break;
            default:
                prefab = enemyPrefab;
                break;
        }
        GameObject enemyObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        // Usamos la dirección para determinar el color del cuadrante
        Color quadrantColor = GetQuadrantColorFromDirection(spawnDirection);
        ApplyColorToEnemy(enemyObject, quadrantColor, speedModifier);
    }

    /// <summary>
    /// Genera un enemigo en una posición específica y le asigna el color pasado.
    /// </summary>
    void SpawnEnemyAtPosition(GameObject prefab, Vector2 position, Color overrideColor = default)
    {
        if (prefab == null) prefab = enemyPrefab;
        GameObject enemyObject = Instantiate(prefab, position, Quaternion.identity);

        Color colorToApply = overrideColor;
        // Si no se pasó un color específico (default es (0,0,0,0) o Color.clear), lo calculamos a partir de la dirección
        if (colorToApply == default(Color))
        {
            Vector2 direction = (position - (Vector2)transform.position).normalized;
            colorToApply = GetQuadrantColorFromDirection(direction);
        }
        ApplyColorToEnemy(enemyObject, colorToApply);
    }

    GameObject SelectEnemyPrefabByProbability(float randomValue)
    {
        float adjustedTankChance = tankEnemySpawnChance;
        float adjustedShooterChance = adjustedTankChance + shooterSpawnChance;
        float adjustedZZChance = adjustedShooterChance + enemyZZSpawnChance;

        if (randomValue <= adjustedTankChance)
            return tankEnemyPrefab;
        else if (randomValue <= adjustedShooterChance)
            return shooterEnemyPrefab ?? enemyPrefab;
        else if (randomValue <= adjustedZZChance)
            return enemyZZPrefab ?? enemyPrefab;
        else
            return enemyPrefab;
    }
    #endregion

    #region Ajuste de Dificultad
    /// <summary>
    /// Incrementa la dificultad: más enemigos, menor spawnRate, mayor velocidad.
    /// Llamado por WaveManager tras cada oleada.
    /// </summary>
    public void IncrementDifficulty()
    {
        if (enemiesPerWave <= 5) { enemiesPerWave += 1; }
        else if (enemiesPerWave <= 15) { enemiesPerWave += 2; }
        else { enemiesPerWave += 3; }

        enemiesPerWave = Mathf.Min(enemiesPerWave, maxEnemiesPerWave);

        float spawnRateDecrement = 0.1f;
        currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateDecrement);

        float speedIncrement = 0.15f;
        currentEnemySpeed = Mathf.Min(maxEnemySpeed, currentEnemySpeed + speedIncrement);
    }
    #endregion

    #region Color y Utilidades
    void ApplyColorToEnemy(GameObject enemyObject, Color color, float speedModifier = 1.0f)
    {
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

    Color GetRandomColorFromAvailable()
    {
        // Si se quiere usar la lista enemyColors (por ejemplo en oleadas de color)
        if (enemyColors != null && enemyColors.Count > 0)
            return enemyColors[Random.Range(0, enemyColors.Count)];
        return Color.red;
    }


string ColorToString(Color color)
{
    if (color == Color.red) return "rojo";
    if (color == Color.blue) return "azul";
    if (color == Color.green) return "verde";
    if (color == Color.yellow) return "amarillo";
    return "desconocido";
}
    #endregion
}