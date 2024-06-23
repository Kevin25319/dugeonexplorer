using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyBodyTackle : MonoBehaviour
{
    [HideInInspector] public bool isBodyTackled = true; 
    [HideInInspector] public int damage = 1; 
    public Vector2 size = Vector2.one;  
    public Transform tacklePos;         

    KnockBack _knockBack;
    PlayerDamage _playerDamage;

    LayerMask _playerLayer;

    void Awake()
    {
        _playerDamage = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDamage>();
        if(tacklePos == null)
        {
            tacklePos = GetComponent<Transform>();
        }
        
        _playerLayer = LayerMask.GetMask("Player");

        _knockBack = new KnockBack();
        _knockBack.force = 18.0f;
    }

    void Update()
    {
        
        if(!isBodyTackled || size.x == 0 || size.y == 0)
        {
            return;
        }

        
        Collider2D hit = Physics2D.OverlapBox(tacklePos.position, size, 0, _playerLayer);
        if(hit)
        {
            
            _knockBack.direction = hit.transform.position.x < tacklePos.position.x ? -1 : 1; 
            _playerDamage.TakeDamage(damage, _knockBack);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(tacklePos.position, size);
    }
}
