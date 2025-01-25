using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    // Singleton para acceso global
    public static EnemyManager Instance { get; private set; }

    // Lista con todos los enemigos normales (script "Enemy")
    private List<Enemy> activeEnemies = new List<Enemy>();

    // Lista con todos los "TankEnemy"
    private List<TankEnemy> activeTankEnemies = new List<TankEnemy>();

    void Awake()
    {
        // Asegurar que sólo exista un EnemyManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------
    // Sección: Enemigos normales
    // -------------------------
    public void RegisterEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    /// <summary>
    /// Retorna el Enemy (script Enemy) más cercano,
    /// o null si no hay ninguno en la lista de Enemy normales.
    /// </summary>
    public Enemy GetNearestEnemy(Vector3 fromPosition)
    {
        Enemy nearest = null;
        float closestDistance = Mathf.Infinity;

        foreach (var enemy in activeEnemies)
        {
            if (enemy == null) continue; // evitar referencias nulas

            float distance = Vector2.Distance(fromPosition, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    // -------------------------
    // Sección: TankEnemy
    // -------------------------
    public void RegisterTankEnemy(TankEnemy tankEnemy)
    {
        if (!activeTankEnemies.Contains(tankEnemy))
        {
            activeTankEnemies.Add(tankEnemy);
        }
    }

    public void UnregisterTankEnemy(TankEnemy tankEnemy)
    {
        if (activeTankEnemies.Contains(tankEnemy))
        {
            activeTankEnemies.Remove(tankEnemy);
        }
    }

    // -------------------------
    // Método unificado (nuevo)
    // -------------------------
    /// <summary>
    /// Retorna el enemigo más cercano (sea Enemy o TankEnemy)
    /// como un MonoBehaviour para que puedas obtener transform.position.
    /// Retorna null si no hay ninguno.
    /// </summary>
    public MonoBehaviour GetNearestAnyEnemy(Vector3 fromPosition)
    {
        MonoBehaviour nearest = null;
        float closestDistance = Mathf.Infinity;

        // 1) Recorrer Enemy normales
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null) continue;
            float dist = Vector2.Distance(fromPosition, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearest = enemy;  // Guardamos la referencia
            }
        }

        // 2) Recorrer TankEnemy
        foreach (var tank in activeTankEnemies)
        {
            if (tank == null) continue;
            float dist = Vector2.Distance(fromPosition, tank.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearest = tank;
            }
        }

        return nearest;
    }

    // (Opcional) Retornar la lista de enemigos normales, si necesitas algo más
    public List<Enemy> GetAllEnemies()
    {
        return activeEnemies;
    }

    // (Opcional) Retornar la lista de tankEnemies, si necesitas algo más
    public List<TankEnemy> GetAllTankEnemies()
    {
        return activeTankEnemies;
    }
}