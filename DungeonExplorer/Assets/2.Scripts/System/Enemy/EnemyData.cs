using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyData : ScriptableObject
{
    public string enemyName = "NAME";   
    public float patrolSpeed = 1.5f;   
    public float followSpeed = 0f;      

    public bool isBodyTackled = true; 
    public int bodyTackleDamage = 1;    

    public float detectRange = 3.0f;    
    public float attackDelay = 1.5f;   
    public int attackDamage = 1;      

    public int health;                  
    public bool superArmor;             

    public int money;                 

    public Material blinkMaterial;     
}