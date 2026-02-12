using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Genera enemigos en oleadas con sistema de cuadrantes para color,
/// dificultad incremental y eventos especiales.
/// Refactorizado: usa EnemyBase en lugar de checks individuales por tipo.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs de Enemigos")]
    public GameObject enemyPrefab;
    public GameObject tankEnemyPrefab;
    [Tooltip("Prefab del ShooterEnemy.")]
    public GameObject shooterEnemyPrefab;
    [Tooltip("Prefab del EnemyZZ.")]
    public GameObject enemyZZPrefab;

    [Header("Probabilidades de spawn")]
    public float tankEnemySpawnChance = 0.2f;
    [Tooltip("Probabilidad de generar un ShooterEnemy (0..1).")]
    public float shooterSpawnChance = 0.1f;
    [Tooltip("Probabilidad de generar un EnemyZZ (0..1).")]
    public float enemyZZSpawnChance = 0.1f;

    [Header("Spawn")]
    public float spawnDistance = 10f;
    public List<Color> enemyColors = new List<Color>();

    [Header("Dificultad Incremental")]
    public int initialEnemiesPerWave = 4;
    public int maxEnemiesPerWave = 24;
    public float initialSpawnRate = 2.5f;
    public float minSpawnRate = 0.3f;
    public float initialEnemySpeed = 1.8f;
    public float maxEnemySpeed = 4.0f;

    [Header("Eventos Aleatorios")]
    public float eventChance = 0.3f;

    [HideInInspector] public int enemiesPerWave;
    [HideInInspector] public float currentSpawnRate;
    [HideInInspector] public float currentEnemySpeed;

    /*═══════════════════  INIT  ═══════════════════*/

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

    /*═══════════════════  CUADRANTES  ═══════════════════*/

    /// <summary>
    /// Retorna posición de spawn y color según cuadrante:
    /// Superior=Amarillo, Derecho=Rojo, Inferior=Verde, Izquierdo=Azul.
    /// </summary>
    (Vector2 spawnPosition, Color quadrantColor) GetSpawnData()
    {
        int quadrant = Random.Range(0, 4);
        float angle = 0f;
        Color chosenColor = Color.white;

        switch (quadrant)
        {
            case 0: angle = Random.Range(45f, 135f);  chosenColor = Color.yellow; break;
            case 1: angle = Random.Range(-45f, 45f);   chosenColor = Color.red;    break;
            case 2: angle = Random.Range(225f, 315f);  chosenColor = Color.green;  break;
            case 3: angle = Random.Range(135f, 225f);  chosenColor = Color.blue;   break;
        }

        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector2 pos = (Vector2)transform.position + dir * spawnDistance;
        return (pos, chosenColor);
    }

    Color GetQuadrantColorFromDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        if (angle >= 45f && angle < 135f)  return Color.yellow;
        if (angle >= 135f && angle < 225f) return Color.blue;
        if (angle >= 225f && angle < 315f) return Color.green;
        return Color.red;
    }

    /*═══════════════════  OLEADAS PRINCIPALES  ═══════════════════*/

    public IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }

    public IEnumerator ExecuteSpecialWave()
    {
        int eventType = Random.Range(0, 4);
        switch (eventType)
        {
            case 0: yield return StartCoroutine(RapidWave());       break;
            case 1: yield return StartCoroutine(EliteWave());       break;
            case 2: yield return StartCoroutine(SingleColorWave()); break;
            case 3: yield return StartCoroutine(FormationWave());   break;
        }
    }

    /*═══════════════════  EVENTOS ESPECIALES  ═══════════════════*/

    IEnumerator RapidWave()
    {
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
        Color waveColor = GetRandomColorFromAvailable();
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemyWithColor(waveColor);
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }

    IEnumerator FormationWave()
    {
        yield return StartCoroutine(SpawnInCircle(enemiesPerWave / 3));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(SpawnInLine(enemiesPerWave / 3));
        yield return new WaitForSeconds(1f);
        int remaining = enemiesPerWave - (2 * (enemiesPerWave / 3));
        yield return StartCoroutine(SpawnInGroups(remaining));
    }

    /*═══════════════════  PATRONES DE FORMACIÓN  ═══════════════════*/

    IEnumerator SpawnInCircle(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * 2 * Mathf.PI;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 pos = (Vector2)transform.position + dir * spawnDistance;
            Color color = GetQuadrantColorFromDirection(dir);
            SpawnEnemyAtPosition(enemyPrefab, pos, color);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator SpawnInLine(int count)
    {
        Vector2 start = (Vector2)transform.position + Vector2.right * spawnDistance;
        Vector2 end = (Vector2)transform.position - Vector2.right * spawnDistance;
        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : i / (float)(count - 1);
            Vector2 pos = Vector2.Lerp(start, end, t);
            Vector2 dir = (pos - (Vector2)transform.position).normalized;
            Color color = GetQuadrantColorFromDirection(dir);
            SpawnEnemyAtPosition(shooterEnemyPrefab, pos, color);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator SpawnInGroups(int count)
    {
        Vector2 pos1 = (Vector2)transform.position + Vector2.up * spawnDistance;
        Vector2 pos2 = (Vector2)transform.position + Vector2.down * spawnDistance;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = (i % 2 == 0) ? pos1 : pos2;
            Vector2 dir = (pos - (Vector2)transform.position).normalized;
            Color color = GetQuadrantColorFromDirection(dir);
            GameObject prefab = (i % 2 == 0) ? enemyZZPrefab : enemyPrefab;
            SpawnEnemyAtPosition(prefab, pos, color);
            yield return new WaitForSeconds(0.3f);
        }
    }

    /*═══════════════════  SPAWN CORE  ═══════════════════*/

    /// <summary>
    /// Spawn principal: posición por cuadrante, prefab aleatorio por probabilidad.
    /// </summary>
    public void SpawnEnemy()
    {
        (Vector2 spawnPosition, Color quadrantColor) = GetSpawnData();
        GameObject prefab = SelectEnemyPrefabByProbability(Random.Range(0f, 1f));
        GameObject go = Instantiate(prefab, spawnPosition, Quaternion.identity);
        ApplyColorToEnemy(go, quadrantColor);
    }

    void SpawnEnemyWithColor(Color specificColor)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 pos = (Vector2)transform.position + dir * spawnDistance;
        GameObject prefab = SelectEnemyPrefabByProbability(Random.Range(0f, 1f));
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        ApplyColorToEnemy(go, specificColor);
    }

    /// <summary>
    /// Spawn de tipo específico: 0=normal, 1=tank, 2=shooter, 3=zigzag.
    /// </summary>
    public void SpawnSpecificEnemy(int enemyType, float speedModifier)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 pos = (Vector2)transform.position + dir * spawnDistance;

        GameObject prefab = enemyType switch
        {
            1 => tankEnemyPrefab,
            2 => shooterEnemyPrefab ?? enemyPrefab,
            3 => enemyZZPrefab ?? enemyPrefab,
            _ => enemyPrefab,
        };

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        Color color = GetQuadrantColorFromDirection(dir);
        ApplyColorToEnemy(go, color, speedModifier);
    }

    void SpawnEnemyAtPosition(GameObject prefab, Vector2 position, Color overrideColor = default)
    {
        if (prefab == null) prefab = enemyPrefab;
        GameObject go = Instantiate(prefab, position, Quaternion.identity);

        Color color = overrideColor;
        if (color == default(Color))
        {
            Vector2 dir = (position - (Vector2)transform.position).normalized;
            color = GetQuadrantColorFromDirection(dir);
        }
        ApplyColorToEnemy(go, color);
    }

    /*═══════════════════  APLICAR PROPIEDADES (REFACTORIZADO)  ═══════════════════*/

    /// <summary>
    /// Asigna color y velocidad a cualquier enemigo usando EnemyBase.
    /// Reemplaza la cadena de 4x GetComponent por un solo GetComponent.
    /// </summary>
    void ApplyColorToEnemy(GameObject enemyObject, Color color, float speedModifier = 1.0f)
    {
        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null) return;

        enemy.enemyColor = color;
        enemy.speed = currentEnemySpeed * speedModifier;
        enemy.ApplyVisualColor();
    }

    /*═══════════════════  SELECCIÓN DE PREFAB  ═══════════════════*/

    GameObject SelectEnemyPrefabByProbability(float randomValue)
    {
        float cumulative = tankEnemySpawnChance;
        if (randomValue <= cumulative) return tankEnemyPrefab;

        cumulative += shooterSpawnChance;
        if (randomValue <= cumulative) return shooterEnemyPrefab ?? enemyPrefab;

        cumulative += enemyZZSpawnChance;
        if (randomValue <= cumulative) return enemyZZPrefab ?? enemyPrefab;

        return enemyPrefab;
    }

    /*═══════════════════  DIFICULTAD  ═══════════════════*/

    public void IncrementDifficulty()
    {
        if (enemiesPerWave <= 5) enemiesPerWave += 1;
        else if (enemiesPerWave <= 15) enemiesPerWave += 2;
        else enemiesPerWave += 3;

        enemiesPerWave = Mathf.Min(enemiesPerWave, maxEnemiesPerWave);
        currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - 0.1f);
        currentEnemySpeed = Mathf.Min(maxEnemySpeed, currentEnemySpeed + 0.15f);
    }

    /*═══════════════════  UTILIDADES  ═══════════════════*/

    Color GetRandomColorFromAvailable()
    {
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
}
