using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyAttack : MonoBehaviour
{
    public Transform attackPoint;   
    public Vector2 attackRange;     

    LayerMask _playerLayer;

    PlayerDamage _playerDamage;

    void Awake()
    {
        _playerLayer = LayerMask.GetMask("Player");
        _playerDamage = GameObject.Find("Player").GetComponent<PlayerDamage>();
    }

    
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    /// <returns></returns>
    public bool IsAttacking(int damage, KnockBack knockBack)
    {
        
        if(attackRange == Vector2.zero) return false;

        
        if(Physics2D.OverlapBox(attackPoint.position, attackRange, 0, _playerLayer))
        {
            _playerDamage.TakeDamage(damage, knockBack);
            return true;
        }

        
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackRange);
    }
}