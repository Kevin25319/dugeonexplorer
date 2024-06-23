using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlyingEnemy : Enemy
{
    [SerializeField] float _movingRange = 0.6f;                
    [SerializeField] float _moveDirectionResetTimeMin = 0.5f;  
    [SerializeField] float _moveDirectionResetTimeMax = 1.25f; 

    [SerializeField] Transform _attackPos;     
    [SerializeField] AudioClip _attackSound;    

    float _timeToChangeDirection;   
    float _timeRemainingToAttack;   

    bool _isAttacking;   

    Vector3 _moveDirection;         
    Vector3 _originPos = Vector3.zero;  

    Transform _playerTransform; 

    protected override void Awake()
    {
        base.Awake();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        _originPos = actorTransform.position;
    }
    void Update()
    {
        
        deltaTime = Time.deltaTime;

       
        Vector3 newMoveDirection;
        float movingRangeSqr = _movingRange * _movingRange;
        if(movingRangeSqr <= (_originPos - actorTransform.position).sqrMagnitude)
        {
            newMoveDirection = (_originPos - actorTransform.position).normalized;
            _moveDirection = newMoveDirection;
            _timeToChangeDirection = Random.Range(_moveDirectionResetTimeMin, _moveDirectionResetTimeMax);  
        }

        _timeToChangeDirection -= deltaTime;
        if (_timeToChangeDirection <= 0)
        {
            
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            newMoveDirection = new Vector2(x, y).normalized;
            _moveDirection = newMoveDirection;

            _timeToChangeDirection = Random.Range(_moveDirectionResetTimeMin, _moveDirectionResetTimeMax); 
        }

       
        controller.VelocityX = enemyData.patrolSpeed * _moveDirection.x;
        controller.VelocityY = enemyData.patrolSpeed * _moveDirection.y;

        if (!_isAttacking)
        {
            
            _timeRemainingToAttack -= deltaTime;
            if (_timeRemainingToAttack <= 0)
            {
                if (IsPlayerDetected())
                {
                    
                    bool enemyFacingRight = actorTransform.localScale.x == 1;
                    bool playerIsRight = actorTransform.position.x < _playerTransform.position.x;
                    if(enemyFacingRight != playerIsRight)
                    {
                        Flip();
                    }
                    _isAttacking = true;  
                    _timeRemainingToAttack = enemyData.attackDelay; 

                    
                    StartCoroutine(Attack());
                }
            }
        }
    }

    
    bool IsPlayerDetected()
    {
        
        float playerDistance = Vector2.Distance(actorTransform.position, _playerTransform.position);
        if(playerDistance <= enemyData.detectRange)
        {
            Vector2 playerDirection = (_playerTransform.position - _attackPos.position).normalized;
#if UNITY_EDITOR
            Debug.DrawRay(_attackPos.position, playerDirection * playerDistance, Color.yellow);
#endif
            
            return !Physics2D.Raycast(_attackPos.position, playerDirection, playerDistance, LayerMask.GetMask("Ground"));
        }
        return false;
    }

    
    IEnumerator Attack()
    {
        
        bool fire = false;
        animator.SetTrigger("GuidedMissile");
        yield return null;

        
        while(!IsAnimationEnded())
        {
            
            if (IsAnimatorNormalizedTimeInBetween(0.75f, 0.8f))
            {
                if(!fire)
                {
                    
                    Vector2 firePos = _attackPos.position;
                    float x = firePos.x - _playerTransform.position.x;
                    float y = firePos.y - _playerTransform.position.y;
                    float guidedMissileAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

                    
                    ObjectPoolManager.instance.GetPoolObject("GuidedMissile", firePos, actorTransform.localScale.x, guidedMissileAngle);
                    SoundManager.instance.SoundEffectPlay(_attackSound);
                    fire = true;
                }
            }
            yield return null;
        }

        
        _isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        if(_originPos == Vector3.zero)
        {
            Gizmos.DrawWireSphere(transform.position, _movingRange);
        }
        else
        {
            Gizmos.DrawWireSphere(_originPos, _movingRange);
        }

        if(enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectRange);
    }
}
