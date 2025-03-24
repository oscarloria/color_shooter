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

    // Lista completa de colores posibles para los enemigos
    public List<Color> enemyColors = new List<Color>();

    [Header("Dificultad Incremental")]
    public int initialEnemiesPerWave = 4;     
    public int maxEnemiesPerWave = 24;        
    public float initialSpawnRate = 2.5f;     
    public float minSpawnRate = 0.3f;         
    public float initialEnemySpeed = 1.8f;    
    public float maxEnemySpeed = 4.0f;        
    public float wavePause = 5f;             

    [Header("Eventos Aleatorios")]
    public float eventChance = 0.3f; // Probabilidad de que ocurra un evento especial

    [Header("UI de Oleadas")]
    [Tooltip("Texto UI central (TextMeshPro) para anunciar la oleada. Se activa unos segundos y se oculta.")]
    public TextMeshProUGUI waveAnnouncementText;

    [Tooltip("Texto UI permanente (TextMeshPro) que muestra la ola actual en pantalla (p.ej. en la esquina).")]
    public TextMeshProUGUI waveNumberText;

    [Header("Mensajes aleatorios tras Wave #")]
    [Tooltip("Lista de frases que se mostrarán al azar en lugar de 'Let's go!'")]
    public string[] randomWaveMessages =
    {
        "Fight!", "GO!", "Get ready!", "Let the battle begin!",
        "Let's Rock!", "The battle begins!", "Start!", "FIGHT!",
        "Tussle!", "Duel!", "The duel begins!", "Go for it!",
        "Let the Beast roar... Fight!", "Battle Start!", "It's time to make history!",
        "Cross the field... Clash!", "Ready Set Melty!", "Let the battle commence!",
        "Let's dance!", "Destroy your enemy!", "Get Ready... Action!", "Hajime!",
        "Show them your true power!", "En garde!", "Slappin' Time!", "Let's... BALL!",
        "Mucha Lucha!", "Let the skies decide!", "Strike! Fight!", "Eliminate the target!",
        "Let's Rumble!", "FIGHTINGU!", "Believe it!", "Set ablaze! Fight!",
        "A pelear!", "Ikuzo!", "Spin to Win!"
    };

    private int currentWave = 1;
    private int enemiesPerWave;
    private float currentSpawnRate;
    private float currentEnemySpeed;
    private bool isSpecialWave = false;

    void Start()
    {
        // Inicializar colores
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

        // Asegurarnos de ocultar el texto de la oleada al inicio
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.gameObject.SetActive(false);
        }

        // Asegurarnos de mostrar en waveNumberText la wave #1 al inicio
        if (waveNumberText != null)
        {
            waveNumberText.text = "Wave: 1";
        }

        // Iniciar ciclo de oleadas
        StartCoroutine(WaveCycle());
    }

    IEnumerator WaveCycle()
    {
        while (true)
        {
            // Mostrar anuncio de oleada (Wave #N y un mensaje random)
            yield return StartCoroutine(ShowWaveAnnouncement(currentWave));

            // Decidir si es una oleada especial
            isSpecialWave = (Random.value < eventChance && currentWave > 3);

            // Ejecutar la oleada (sea normal o especial)
            if (isSpecialWave)
            {
                yield return StartCoroutine(ExecuteSpecialWave());
            }
            else
            {
                yield return StartCoroutine(SpawnWave());
            }

            // Pausa entre oleadas
            yield return new WaitForSeconds(wavePause);

            // Incrementar dificultad
            IncrementDifficulty();
        }
    }

    /// <summary>
    /// Muestra un texto "Wave #N" durante 1.0s y luego un mensaje aleatorio 0.5s.
    /// Después esconde el texto.
    /// </summary>
    IEnumerator ShowWaveAnnouncement(int waveNumber)
    {
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.gameObject.SetActive(true);

            // 1) "Wave #N"
            waveAnnouncementText.text = "Wave #" + waveNumber;
            yield return new WaitForSeconds(1.0f);

            // 2) Escoger un mensaje al azar
            if (randomWaveMessages != null && randomWaveMessages.Length > 0)
            {
                int randomIndex = Random.Range(0, randomWaveMessages.Length);
                waveAnnouncementText.text = randomWaveMessages[randomIndex];
            }
            else
            {
                waveAnnouncementText.text = "Let's go!"; // fallback
            }
            yield return new WaitForSeconds(1.0f);

            // 3) Ocultar
            waveAnnouncementText.text = "";
            waveAnnouncementText.gameObject.SetActive(false);
        }
        else
        {
            // Si no asignaste waveAnnouncementText en inspector, al menos logs
            Debug.Log("Wave #" + waveNumber);
            yield return new WaitForSeconds(1.0f);
            Debug.Log("Random message or 'Let's go!'");
            yield return new WaitForSeconds(0.5f);
        }
    }

    void IncrementDifficulty()
    {
        currentWave++;

        // Actualizar waveNumberText
        if (waveNumberText != null)
        {
            waveNumberText.text = "Wave: " + currentWave;
        }

        if (currentWave <= 5)
        {
            enemiesPerWave += 1;
        }
        else if (currentWave <= 15)
        {
            enemiesPerWave += 2;
        }
        else
        {
            enemiesPerWave += 3;
        }

        enemiesPerWave = Mathf.Min(enemiesPerWave, maxEnemiesPerWave);

        float spawnRateDecrement = 0.1f * Mathf.Min(1f, currentWave / 10f);
        currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateDecrement);

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
            GameObject prefab = (i % 2 == 0) ? enemyZZPrefab : enemyPrefab;
            SpawnEnemyAtPosition(prefab, spawnPosition);

            yield return new WaitForSeconds(0.3f);
        }
    }

    void SpawnEnemyAtPosition(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            prefab = enemyPrefab; 
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
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;

        float randomValue = Random.Range(0f, 1f);
        GameObject prefab = SelectEnemyPrefabByProbability(randomValue);

        GameObject enemyObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        ApplyColorToEnemy(enemyObject, specificColor);
    }

    GameObject SelectEnemyPrefabByProbability(float randomValue)
    {
        float adjustedTankChance = tankEnemySpawnChance;
        float adjustedShooterChance = adjustedTankChance + shooterSpawnChance;
        float adjustedZZChance = adjustedShooterChance + enemyZZSpawnChance;

        if (randomValue <= adjustedTankChance)
        {
            return tankEnemyPrefab;
        }
        else if (randomValue <= adjustedShooterChance)
        {
            return shooterEnemyPrefab ?? enemyPrefab;
        }
        else if (randomValue <= adjustedZZChance)
        {
            return enemyZZPrefab ?? enemyPrefab;
        }
        else
        {
            return enemyPrefab;
        }
    }

    void SpawnSpecificEnemy(int enemyType, float speedModifier)
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

        List<Color> availableColors = GetAvailableColorsForWave(currentWave);
        if (availableColors.Count > 0)
        {
            Color chosenColor = availableColors[Random.Range(0, availableColors.Count)];
            ApplyColorToEnemy(enemyObject, chosenColor, speedModifier);
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)transform.position + spawnDirection * spawnDistance;

        GameObject enemyObject = null;

        float combinedChanceTank = tankEnemySpawnChance;
        float combinedChanceShooter = combinedChanceTank + shooterSpawnChance;
        float combinedChanceZZ = combinedChanceShooter + enemyZZSpawnChance;
        float randomValue = Random.Range(0f, 1f);

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
        else if (randomValue <= combinedChanceShooter)
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
                ShooterEnemy shooterScript = enemyObject.GetComponent<ShooterEnemy>();
                if (shooterScript != null)
                {
                    shooterScript.enemyColor = chosenColor;
                    shooterScript.speed = currentEnemySpeed;
                }
            }
        }
        else if (randomValue <= combinedChanceZZ)
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
                EnemyZZ enemyZZScript = enemyObject.GetComponent<EnemyZZ>();
                if (enemyZZScript != null)
                {
                    enemyZZScript.enemyColor = chosenColor;
                    enemyZZScript.speed = currentEnemySpeed;
                }
            }
        }
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

    float GetAdjustedProbability(float baseProbability, int wave, int minWave)
    {
        if (wave < minWave)
        {
            return 0f;
        }

        float scaleFactor = Mathf.Min(2.0f, 1.0f + ((wave - minWave) / 20.0f));
        return Mathf.Min(0.5f, baseProbability * scaleFactor);
    }

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
        List<Color> availableColors = GetAvailableColorsForWave(currentWave);
        if (availableColors.Count > 0)
        {
            return availableColors[Random.Range(0, availableColors.Count)];
        }
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

    List<Color> GetAvailableColorsForWave(int wave)
    {
        Color rojo = Color.red;
        Color azul = Color.blue;
        Color verde = Color.green;
        Color amarillo = Color.yellow;

        List<Color> colorsForThisWave = new List<Color>();

        if (wave <= 3)
        {
            colorsForThisWave.Add(rojo);
        }
        else if (wave <= 6)
        {
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
        }
        else if (wave <= 10)
        {
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
        }
        else if (wave <= 15)
        {
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
        }
        else if (wave <= 20)
        {
            colorsForThisWave.Add(verde);
            colorsForThisWave.Add(amarillo);
        }
        else if (wave <= 25)
        {
            colorsForThisWave.Add(amarillo);
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(verde);
        }
        else
        {
            colorsForThisWave.Add(rojo);
            colorsForThisWave.Add(azul);
            colorsForThisWave.Add(verde);
            colorsForThisWave.Add(amarillo);
        }

        return colorsForThisWave;
    }
}