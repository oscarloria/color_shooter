using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestor centralizado de todos los enemigos activos.
/// Usa una única lista de EnemyBase en lugar de listas separadas por tipo.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /*═══════════════════  REGISTRO  ═══════════════════*/

    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        activeEnemies.Remove(enemy);
    }

    /*═══════════════════  CONSULTAS  ═══════════════════*/

    /// <summary>Cantidad total de enemigos activos.</summary>
    public int ActiveCount => activeEnemies.Count;

    /// <summary>Lista de solo lectura de todos los enemigos activos.</summary>
    public IReadOnlyList<EnemyBase> GetAllEnemies() => activeEnemies;

    /// <summary>Retorna el enemigo más cercano a una posición, o null.</summary>
    public EnemyBase GetNearestAnyEnemy(Vector3 fromPosition)
    {
        EnemyBase nearest = null;
        float closestDist = Mathf.Infinity;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyBase e = activeEnemies[i];
            if (e == null) { activeEnemies.RemoveAt(i); continue; }

            float dist = Vector2.Distance(fromPosition, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = e;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Retorna el enemigo más cercano que esté dentro de la vista de la cámara, o null.
    /// </summary>
    public EnemyBase GetNearestAnyEnemyOnScreen(Vector3 fromPosition, Camera cam)
    {
        EnemyBase nearest = null;
        float closestDist = Mathf.Infinity;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyBase e = activeEnemies[i];
            if (e == null) { activeEnemies.RemoveAt(i); continue; }

            if (!IsInCameraView(e.transform.position, cam)) continue;

            float dist = Vector2.Distance(fromPosition, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = e;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Retorna todos los enemigos de un tipo específico.
    /// Uso: GetEnemiesOfType&lt;TankEnemy&gt;()
    /// </summary>
    public List<T> GetEnemiesOfType<T>() where T : EnemyBase
    {
        List<T> result = new List<T>();
        foreach (var e in activeEnemies)
        {
            if (e is T typed)
                result.Add(typed);
        }
        return result;
    }

    /*═══════════════════  HELPERS  ═══════════════════*/

    bool IsInCameraView(Vector3 worldPos, Camera cam)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        return vp.z > 0f &&
               vp.x >= 0f && vp.x <= 1f &&
               vp.y >= 0f && vp.y <= 1f;
    }
}
