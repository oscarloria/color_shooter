using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawnerSimple : MonoBehaviour
{
    // Prefabs
    public GameObject enemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject shooterEnemyPrefab;
    public GameObject enemyZZPrefab;

    // Cantidades a spawnear
    public int normalCount = 5;
    public int tankCount = 2;
    public int shooterCount = 3;
    public int zzCount = 1;

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
        // 1) Spawnear Normal
        for (int i = 0; i < normalCount; i++)
        {
            SpawnEnemyOfType(enemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }
        // 2) Spawnear Tank
        for (int i = 0; i < tankCount; i++)
        {
            SpawnEnemyOfType(tankEnemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }
        // 3) Spawnear Shooter
        for (int i = 0; i < shooterCount; i++)
        {
            SpawnEnemyOfType(shooterEnemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }
        // 4) Spawnear EnemyZZ
        for (int i = 0; i < zzCount; i++)
        {
            SpawnEnemyOfType(enemyZZPrefab);
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
        ApplyColorToEnemy(enemyObj, quadrantColor, 1f);
    }

    private (Vector2, Color) GetSpawnData()
    {
        int quadrant = Random.Range(0, 4);
        float angleMin=0f, angleMax=0f;
        Color chosenColor = Color.white;

        switch (quadrant)
        {
            case 0:
                angleMin=45f; angleMax=135f;
                chosenColor = Color.yellow;
                break;
            case 1:
                angleMin=-45f; angleMax=45f;
                chosenColor = Color.red;
                break;
            case 2:
                angleMin=225f; angleMax=315f;
                chosenColor = Color.green;
                break;
            case 3:
                angleMin=135f; angleMax=225f;
                chosenColor = Color.blue;
                break;
        }

        float angle = Random.Range(angleMin, angleMax);
        Vector2 spawnDir = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad), Mathf.Sin(angle*Mathf.Deg2Rad));
        Vector2 spawnPos = (Vector2)transform.position + spawnDir * spawnDistance;

        return (spawnPos, chosenColor);
    }

    private void ApplyColorToEnemy(GameObject enemyObject, Color color, float speedModifier)
    {
        if (enemyObject == null) return;
        float finalSpeed = enemySpeed * speedModifier;

        TankEnemy tank = enemyObject.GetComponent<TankEnemy>();
        if (tank != null)
        {
            tank.enemyColor = color;
            tank.speed = finalSpeed;
            tank.ApplyColor();
            return;
        }

        ShooterEnemy shooter = enemyObject.GetComponent<ShooterEnemy>();
        if (shooter != null)
        {
            shooter.enemyColor = color;
            shooter.speed = finalSpeed;
            return;
        }

        EnemyZZ zz = enemyObject.GetComponent<EnemyZZ>();
        if (zz != null)
        {
            zz.enemyColor = color;
            zz.speed = finalSpeed;
            return;
        }

        Enemy enemyScript = enemyObject.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.enemyColor = color;
            enemyScript.speed = finalSpeed;
        }
    }
}
