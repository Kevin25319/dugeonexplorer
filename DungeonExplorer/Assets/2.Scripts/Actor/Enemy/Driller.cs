using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driller : Enemy
{
    [SerializeField] float _timeItTakesToFlip = 0.3f;

    [SerializeField] Transform _detector;           
    [SerializeField] Transform _frontCliffChecker;  
    [SerializeField] Transform _backCliffChecker;  
    [SerializeField] AudioClip _drillSound;    
    
    bool _isAttacking;  
    bool _isChasing;    

    float _timeRemainingToAttack;  
    float _timeRemainingToFlip;     
    
    LayerMask _groundLayer;
    Transform _playerTransform;

    protected override void Awake()
    {
        base.Awake();

        _playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        _groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        
        if (isDead) return;

        deltaTime = Time.deltaTime;

        
        if (controller.IsSliding)
        {
            if (FrontCliffChecked() || (BackCliffChecked()))
            {
                controller.SlideCancle();
                controller.VelocityX = 0;
            }
        }

        if (!_isChasing)
        {
          

            
            controller.VelocityX = enemyData.patrolSpeed * actorTransform.localScale.x;

           
            if (WallChecked() || FrontCliffChecked())
            {
                Flip();
            }

            
            if (IsPlayerDetected())
            {
                _isChasing = true;
                animator.SetFloat(GetAnimationHash("Speed"), 2f);
            }
        }
        else
        {
            
            if (!_isAttacking)
            {
                

               
                controller.VelocityX = enemyData.followSpeed * actorTransform.localScale.x;

                
                if (WallChecked() || FrontCliffChecked())
                {
                    Flip();
                }

                
                if (_timeRemainingToAttack > 0)
                {
                    _timeRemainingToAttack -= deltaTime;
                }

                
                bool enemyFacingRight = actorTransform.localScale.x == 1;
                bool playerIsRight = actorTransform.position.x < _playerTransform.position.x;
                if (enemyFacingRight != playerIsRight)
                {
                    
                    if (_timeRemainingToFlip >= _timeItTakesToFlip)
                    {
                        Flip();
                        _timeRemainingToFlip = 0;
                    }
                    else
                    {
                        _timeRemainingToFlip += deltaTime;
                    }
                }
                else if (_timeRemainingToAttack <= 0)
                {
                    
                    _isAttacking = true;
                    _timeRemainingToAttack = enemyData.attackDelay;
                    StartCoroutine(Attack());
                }

                
                if (!IsPlayerDetected())
                {
                    _isChasing = false;
                    animator.SetFloat(GetAnimationHash("Speed"), 1f);
                }
            }
        }
    }

    
    IEnumerator Attack()
    {
      
        animator.ResetTrigger(GetAnimationHash("AttackEnd"));
        animator.SetTrigger(GetAnimationHash("Attack"));

        bool isSlided = false;

        
        yield return null;

        
        while (!IsAnimationEnded())
        {
            
            {
                if (IsAnimatorNormalizedTimeInBetween(0.53f, 0.6f))
                {
                    SoundManager.instance.SoundEffectPlay(_drillSound);
                    controller.SlideMove(60f, actorTransform.localScale.x, 220f);
                    isSlided = true;
                }
            }
            yield return null;
        }

        
        animator.SetTrigger(GetAnimationHash("AttackEnd"));
        controller.SlideCancle();
        _isAttacking = false;
    }

    
    bool IsPlayerDetected()
    {
        
        float playerDistance = Vector2.Distance(actorTransform.position, _playerTransform.position);
        if (playerDistance <= enemyData.detectRange)
        {
            
            Vector2 playerDirection = (_playerTransform.position - actorTransform.position).normalized;
            
            var playerDetected = !Physics2D.Raycast(actorTransform.position, playerDirection, playerDistance, LayerMask.GetMask("Ground"));
#if UNITY_EDITOR
            Debug.DrawRay(actorTransform.position, playerDirection * playerDistance, Color.yellow);
#endif
            return playerDetected;
        }
        return false;
    }

    bool FrontCliffChecked()
    {
        var cliffed = Physics2D.Raycast(_frontCliffChecker.position, Vector2.down, 1.0f, _groundLayer);

        return !cliffed;
    }

    bool BackCliffChecked()
    {
        var cliffed = Physics2D.Raycast(_backCliffChecker.position, Vector2.down, 1.0f, _groundLayer);

        return !cliffed;
    }

 
    bool WallChecked()
    {
         bool isWalled = actorTransform.localScale.x == 1 ? controller.IsRightWalled : controller.IsLeftWalled;
         return isWalled;
    }
}