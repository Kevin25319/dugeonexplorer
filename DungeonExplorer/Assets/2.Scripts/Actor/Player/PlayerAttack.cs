using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;   
    public Transform effectPoint;   
    public Vector2 attackRange = Vector2.one;  

    public int HitCount { get; private set; }  

    LayerMask _enemyLayer;  
    LayerMask _groundLayer; 
    void Awake()
    {
        _enemyLayer = LayerMask.GetMask("Enemy");
        _groundLayer = LayerMask.GetMask("Ground");
    }

    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    /// <param name="effect"></param>
    /// <returns></returns>
    public bool IsAttacking(int damage, KnockBack knockBack, bool effect = true)
    {
        HitCount = 0;   
        if(attackRange == Vector2.zero) return false;   

        bool isHit = false; 

        var hits = Physics2D.OverlapBoxAll(attackPoint.position, attackRange, 0, _enemyLayer);
        if(hits.Length > 0)
        {
            
            for(int i = 0; i < hits.Length; i++)
            {
                EnemyDamage enemyDamage = hits[i].GetComponent<EnemyDamage>();
                if(enemyDamage.IsDead || enemyDamage.IsInvincibled) continue;   

                hits[i].GetComponent<EnemyDamage>().TakeDamage(damage, knockBack);  

               
                ScreenEffect.instance.BulletTimeStart(0.0f, 0.05f);
                ScreenEffect.instance.ShakeEffectStart(0.2f, 0.05f);
                
                if(effect)
                {
                    
                    ObjectPoolManager.instance.GetPoolObject("SlashEffect", effectPoint.position, angle: Random.Range(0f,359f));
                }

                isHit = true; 
                HitCount++; 
            }
        }

        return isHit;   
    }

    
    public bool GroundHit()
    {
        if(attackRange == Vector2.zero) return false;   
        return Physics2D.OverlapBox(attackPoint.position, attackRange, 0, _groundLayer);    
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackRange);
    }
}