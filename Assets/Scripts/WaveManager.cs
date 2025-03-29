using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Se encarga del ciclo de oleadas: 
/// - Llama a EnemySpawner para spawnear (oleada normal o especial).
/// - Muestra anuncios de oleadas y mensajes aleatorios.
/// - Ajusta la dificultad (velocidad, número de enemigos).
/// - Actualiza la UI de waveNumberText y waveAnnouncementText.
/// 
/// Mantiene la funcionalidad de waveCycle, ShowWaveAnnouncement, incrementDifficulty, etc.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Spawners y Ajustes")]
    [Tooltip("Referencia al EnemySpawner que realmente spawnea los enemigos.")]
    public EnemySpawner enemySpawner;
    
    [Header("UI de Oleadas")]
    [Tooltip("Texto UI central (TextMeshPro) para anunciar la oleada. Se activa unos segundos y se oculta.")]
    public TextMeshProUGUI waveAnnouncementText;

    [Tooltip("Texto UI permanente (TextMeshPro) que muestra la ola actual en pantalla (p.ej. en la esquina).")]
    public TextMeshProUGUI waveNumberText;

    [Tooltip("Tiempo de pausa entre oleadas.")]
    public float wavePause = 5f; 

    [Header("Mensajes aleatorios tras Wave #")]
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
        "A pelear!", "Ikuzo!", "Spin to Win!", "Don't give up!"
    };

    // Variables internas de ola/dificultad
    private int currentWave = 1;
    private bool isSpecialWave = false;

    void Start()
    {
        // Asegurar que EnemySpawner esté asignado
        if (enemySpawner == null)
        {
            Debug.LogError("WaveManager requiere una referencia a EnemySpawner. Arrástralo en el Inspector.");
            return;
        }

        // UI: al inicio ocultar waveAnnouncementText
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.gameObject.SetActive(false);
        }

        // Inicia con wave #1 en waveNumberText
        if (waveNumberText != null)
        {
            waveNumberText.text = "Wave: 1";
        }

        // Iniciar el ciclo de oleadas
        StartCoroutine(WaveCycle());
    }

    IEnumerator WaveCycle()
    {
        while (true)
        {
            // 1) Mostrar anuncio de la oleada
            yield return StartCoroutine(ShowWaveAnnouncement(currentWave));

            // 2) Decidir si es una oleada especial
            isSpecialWave = (Random.value < enemySpawner.eventChance && currentWave > 3);

            // 3) Ejecutar la oleada (sea normal o especial) usando EnemySpawner
            if (isSpecialWave)
            {
                yield return StartCoroutine(enemySpawner.ExecuteSpecialWave());
            }
            else
            {
                yield return StartCoroutine(enemySpawner.SpawnWave());
            }

            // 4) Pausa entre oleadas
            yield return new WaitForSeconds(wavePause);

            // 5) Incrementar la dificultad (y wave) en EnemySpawner
            IncrementDifficultyInSpawner();

            // 6) Subir el contador local de oleadas y actualizar waveNumberText
            currentWave++;
            if (waveNumberText != null)
            {
                waveNumberText.text = "Wave: " + currentWave;
            }
        }
    }

    /// <summary>
    /// Muestra un texto "Wave #N" durante 1.0s y luego un mensaje aleatorio 0.5s.
    /// Después lo oculta.
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
            Debug.Log("Wave #" + waveNumber);
            yield return new WaitForSeconds(1.0f);
            Debug.Log("Random message or 'Let's go!'");
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Llama a un método en EnemySpawner que incremente la dificultad 
    /// (nº enemigos, spawn rate, speed).
    /// </summary>
    private void IncrementDifficultyInSpawner()
    {
        enemySpawner.IncrementDifficulty();
    }
}