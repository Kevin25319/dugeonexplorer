using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(EnemyDamage))]
public abstract class Enemy : Actor
{
    public string keyName;  

    protected EnemyDamage enemyDamage;              
    protected EnemyBodyTackle bodyTackle = null;    
    protected EnemyData enemyData;                  
    protected LayerMask playerLayer;

    protected override void Awake()
    {
       
        if (DeadEnemyManager.IsDeadEnemy(gameObject.name))
        {
            isDead = true;
            Destroy(gameObject);
            return;
        }

        base.Awake();
        enemyDamage = GetComponent<EnemyDamage>();

        
        enemyData = EnemyDataManager.GetEnemyData(keyName); 
        if (enemyData.isBodyTackled && TryGetComponent(out EnemyBodyTackle bodyTackle))
        {
            this.bodyTackle = bodyTackle;
            this.bodyTackle.damage = enemyData.bodyTackleDamage;
        }
        enemyDamage.MaxHealth = enemyData.health;
        enemyDamage.SuperArmor = enemyData.superArmor;
        enemyDamage.BlinkMaterial = enemyData.blinkMaterial;

     
        enemyDamage.KnockBack += OnKnockedBack;
        enemyDamage.Died += OnDied;

        playerLayer = LayerMask.GetMask("Player");
    }

    
    /// <param name="knockBack"></param>
    protected virtual void OnKnockedBack(KnockBack knockBack)
    {
        if(controller == null) return;
       
        controller.SlideMove(knockBack.force, knockBack.direction);
    }

    
    protected virtual IEnumerator OnDied()
    {
        isDead = true;
        DeadEnemyManager.AddDeadEnemy(gameObject.name); 

        
        if(bodyTackle != null)
        {
            bodyTackle.isBodyTackled = false;
        }

        
        Vector2 position = new Vector2(transform.position.x, transform.position.y + 2);
        ObjectPoolManager.instance.GetPoolObject("HealingPiece", position);
        
        yield return null;

        
        Destroy(gameObject);
    }
}