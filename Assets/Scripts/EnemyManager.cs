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

    // -------------------------
    // NUEVO: Lista con ShooterEnemy
    // -------------------------
    private List<ShooterEnemy> activeShooterEnemies = new List<ShooterEnemy>();

    // -------------------------
    // NUEVO: Lista con EnemyZZ
    // -------------------------
    private List<EnemyZZ> activeZZEnemies = new List<EnemyZZ>();

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
    // Sección: ShooterEnemy
    // -------------------------
    public void RegisterShooterEnemy(ShooterEnemy shooterEnemy)
    {
        if (!activeShooterEnemies.Contains(shooterEnemy))
        {
            activeShooterEnemies.Add(shooterEnemy);
        }
    }

    public void UnregisterShooterEnemy(ShooterEnemy shooterEnemy)
    {
        if (activeShooterEnemies.Contains(shooterEnemy))
        {
            activeShooterEnemies.Remove(shooterEnemy);
        }
    }

    // -------------------------
    // Sección: EnemyZZ
    // -------------------------
    public void RegisterEnemyZZ(EnemyZZ enemyZZ)
    {
        if (!activeZZEnemies.Contains(enemyZZ))
        {
            activeZZEnemies.Add(enemyZZ);
        }
    }

    public void UnregisterEnemyZZ(EnemyZZ enemyZZ)
    {
        if (activeZZEnemies.Contains(enemyZZ))
        {
            activeZZEnemies.Remove(enemyZZ);
        }
    }

    // -------------------------
    // Método unificado existente
    // -------------------------
    /// <summary>
    /// Retorna el enemigo más cercano (sea Enemy, TankEnemy, ShooterEnemy o EnemyZZ)
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
                nearest = enemy;
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

        // 3) Recorrer ShooterEnemy
        foreach (var shooter in activeShooterEnemies)
        {
            if (shooter == null) continue;
            float dist = Vector2.Distance(fromPosition, shooter.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearest = shooter;
            }
        }

        // 4) Recorrer EnemyZZ
        foreach (var enemyZZ in activeZZEnemies)
        {
            if (enemyZZ == null) continue;
            float dist = Vector2.Distance(fromPosition, enemyZZ.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearest = enemyZZ;
            }
        }

        return nearest;
    }

    // -------------------------
    // NUEVO MÉTODO:
    // Obtener el enemigo más cercano SOLO si está en pantalla
    // -------------------------
    public MonoBehaviour GetNearestAnyEnemyOnScreen(Vector3 fromPosition, Camera cam)
    {
        MonoBehaviour nearestOnScreen = null;
        float closestDistance = Mathf.Infinity;

        // 1) Recorrer Enemy normales
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null) continue;
            if (IsInCameraView(enemy.transform.position, cam))
            {
                float dist = Vector2.Distance(fromPosition, enemy.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearestOnScreen = enemy;
                }
            }
        }

        // 2) Recorrer TankEnemy
        foreach (var tank in activeTankEnemies)
        {
            if (tank == null) continue;
            if (IsInCameraView(tank.transform.position, cam))
            {
                float dist = Vector2.Distance(fromPosition, tank.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearestOnScreen = tank;
                }
            }
        }

        // 3) Recorrer ShooterEnemy
        foreach (var shooter in activeShooterEnemies)
        {
            if (shooter == null) continue;
            if (IsInCameraView(shooter.transform.position, cam))
            {
                float dist = Vector2.Distance(fromPosition, shooter.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearestOnScreen = shooter;
                }
            }
        }

        // 4) Recorrer EnemyZZ
        foreach (var enemyZZ in activeZZEnemies)
        {
            if (enemyZZ == null) continue;
            if (IsInCameraView(enemyZZ.transform.position, cam))
            {
                float dist = Vector2.Distance(fromPosition, enemyZZ.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    nearestOnScreen = enemyZZ;
                }
            }
        }

        return nearestOnScreen;
    }

    /// <summary>
    /// Determina si un objeto (posición en mundo) está dentro de la vista de la cámara.
    /// </summary>
    private bool IsInCameraView(Vector3 worldPosition, Camera cam)
    {
        // Convertir posición en espacio Viewport (0..1 en x e y significa que está en pantalla)
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPosition);

        // z > 0 => está delante de la cámara
        // x e y entre 0 y 1 => dentro de los límites horizontales y verticales
        if (viewportPos.z > 0f &&
            viewportPos.x >= 0f && viewportPos.x <= 1f &&
            viewportPos.y >= 0f && viewportPos.y <= 1f)
        {
            return true;
        }
        return false;
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

    // (Opcional) Retornar la lista de ShooterEnemy, si necesitas algo más
    public List<ShooterEnemy> GetAllShooterEnemies()
    {
        return activeShooterEnemies;
    }
    
    // (Opcional) Retornar la lista de EnemyZZ, si necesitas algo más
    public List<EnemyZZ> GetAllEnemyZZEnemies()
    {
        return activeZZEnemies;
    }
}
