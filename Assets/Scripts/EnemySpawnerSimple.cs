using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawner simplificado para testing.
/// Refactorizado: usa EnemyBase para asignar propiedades (igual que EnemySpawner).
/// </summary>
public class EnemySpawnerSimple : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject shooterEnemyPrefab;
    public GameObject enemyZZPrefab;
    public GameObject cometEnemyPrefab;

    [Header("Cantidades a spawnear")]
    public int normalCount = 5;
    public int tankCount = 2;
    public int shooterCount = 3;
    public int zzCount = 1;
    public int cometCount = 2;

    [Header("Configuración")]
    public float spawnDistance = 10f;
    public float spawnInterval = 0.5f;
    public float enemySpeed = 2f;

    public List<Color> enemyColors = new List<Color>();

    void Awake()
    {
        if (enemyColors.Count == 0)
        {
            enemyColors.Add(Color.yellow);
            enemyColors.Add(Color.blue);
            enemyColors.Add(Color.green);
            enemyColors.Add(Color.red);
        }
    }

    void Start()
    {
        StartCoroutine(SpawnAllEnemies());
    }

    private IEnumerator SpawnAllEnemies()
    {
        // 1) Normal
        for (int i = 0; i < normalCount; i++)
        {
            SpawnEnemyOfType(enemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 2) Tank
        for (int i = 0; i < tankCount; i++)
        {
            SpawnEnemyOfType(tankEnemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 3) Shooter
        for (int i = 0; i < shooterCount; i++)
        {
            SpawnEnemyOfType(shooterEnemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 4) EnemyZZ
        for (int i = 0; i < zzCount; i++)
        {
            SpawnEnemyOfType(enemyZZPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 5) Comet
        for (int i = 0; i < cometCount; i++)
        {
            SpawnEnemyOfType(cometEnemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("[EnemySpawnerSimple] ¡Todos los enemigos solicitados han sido generados!");
    }

    private void SpawnEnemyOfType(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[EnemySpawnerSimple] Prefab es null => no se spawnea nada.");
            return;
        }

        (Vector2 spawnPos, Color quadrantColor) = GetSpawnData();
        GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        ApplyColorToEnemy(enemyObj, quadrantColor);
    }

    private (Vector2, Color) GetSpawnData()
    {
        int quadrant = Random.Range(0, 4);
        float angleMin = 0f, angleMax = 0f;
        Color chosenColor = Color.white;

        switch (quadrant)
        {
            case 0: angleMin = 45f;  angleMax = 135f; chosenColor = Color.yellow; break;
            case 1: angleMin = -45f; angleMax = 45f;  chosenColor = Color.red;    break;
            case 2: angleMin = 225f; angleMax = 315f; chosenColor = Color.green;  break;
            case 3: angleMin = 135f; angleMax = 225f; chosenColor = Color.blue;   break;
        }

        float angle = Random.Range(angleMin, angleMax);
        Vector2 spawnDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector2 spawnPos = (Vector2)transform.position + spawnDir * spawnDistance;

        return (spawnPos, chosenColor);
    }

    /// <summary>
    /// Asigna color y velocidad usando EnemyBase.
    /// Un solo GetComponent en vez de cadena por tipo.
    /// </summary>
    private void ApplyColorToEnemy(GameObject enemyObject, Color color)
    {
        if (enemyObject == null) return;

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.enemyColor = color;
            enemy.speed = enemySpeed;
            enemy.ApplyVisualColor();
        }
    }
}
