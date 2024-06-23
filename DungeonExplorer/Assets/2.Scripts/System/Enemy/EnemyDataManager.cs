using System;
using System.Collections.Generic;
using UnityEngine;


public static class EnemyDataManager
{
    static bool hasInitialized = false;
    static Dictionary<string, EnemyData> enemiesData = new Dictionary <string, EnemyData>();

    static EnemyDataManager()
    {
        Init();
    }

    
    public static void Init()
    {
        if(hasInitialized) return;
        EnemyDataInit();
        hasInitialized = true;
    }

    static void EnemyDataInit()
    {
        
        List<Dictionary<string, object>> enemyDataList = CSVReader.Read("ActorData/EnemyData");
        Material blinkMaterial = (Material)Resources.Load("BlinkMaterial");

        for (var i = 0; i < enemyDataList.Count; i++)
        {
           
            var newEnemyData = (EnemyData)ScriptableObject.CreateInstance(typeof(EnemyData));

            
            string keyName = enemyDataList[i]["Key"].ToString();
            newEnemyData.enemyName = enemyDataList[i]["Name"].ToString();
            newEnemyData.patrolSpeed = (float)enemyDataList[i]["PatrolSpeed"];
            newEnemyData.followSpeed = (float)enemyDataList[i]["FollowSpeed"];
            newEnemyData.isBodyTackled = (bool)enemyDataList[i]["IsBodyTackled"];
            newEnemyData.bodyTackleDamage = (int)enemyDataList[i]["BodyTackleDamage"];
            newEnemyData.detectRange = (float)enemyDataList[i]["DetectRange"];
            newEnemyData.attackDelay = (float)enemyDataList[i]["AttackDelay"];
            newEnemyData.attackDamage = (int)enemyDataList[i]["AttackDamage"];
            newEnemyData.health = (int)enemyDataList[i]["Health"];
            newEnemyData.superArmor = (bool)enemyDataList[i]["SuperArmor"];
            newEnemyData.money = (int)enemyDataList[i]["Money"];
            newEnemyData.blinkMaterial = blinkMaterial;

            
            enemiesData.Add(keyName, newEnemyData);
        }
    }

    
    public static EnemyData GetEnemyData(string keyName) => enemiesData[keyName];
}