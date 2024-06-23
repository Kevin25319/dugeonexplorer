using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;


public static class DeadEnemyManager
{
    
    struct DeadEnemy
    {
        int _sceneIndex;     
        string _enemyName;   

        public DeadEnemy(int sceneIndex, string enemyName)
        {
            _sceneIndex = sceneIndex;
            _enemyName = enemyName;
        }
    }
    static HashSet<DeadEnemy> _deadEnemies = new HashSet<DeadEnemy>();  
    static HashSet<string> _deadBosses = new HashSet<string>();        

   
    public static void AddDeadEnemy(string enemyName)
    {
        
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        _deadEnemies.Add(new DeadEnemy(sceneIndex, enemyName));
    }

    
    public static bool IsDeadEnemy(string enemyName)
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        DeadEnemy deadEnemy = new DeadEnemy(sceneIndex, enemyName);
        return _deadEnemies.Contains(deadEnemy);
    }

   
    public static void ClearDeadEnemies() => _deadEnemies.Clear();

    
    public static void AddDeadBoss(string bossName) => _deadBosses.Add(bossName);


    
    public static bool IsDeadBoss(string bossName) => _deadBosses.Contains(bossName);

    
    public static void ClearDeadBosses() => _deadBosses.Clear();

    
    public static List<string> GetDeadBosses() => _deadBosses.ToList();
}