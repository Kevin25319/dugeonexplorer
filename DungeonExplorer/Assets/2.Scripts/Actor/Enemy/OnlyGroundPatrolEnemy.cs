using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OnlyGroundPatrolEnemy : Enemy
{
    [SerializeField] Transform _frontCliffChecker;
    [SerializeField] Transform _backCliffChecker;

    protected override void Awake()
    {
        base.Awake();

        if (_frontCliffChecker == null)
        {
            _frontCliffChecker = transform.Find("FrontCliffChecker").GetComponent<Transform>();
        }
        if (_backCliffChecker == null)
        {
            _backCliffChecker = transform.Find("BackCliffChecker").GetComponent<Transform>();
        }
    }

    void Update()
    {
        if (isDead) return;

        
        if (!controller.IsSliding)
        {
            
            controller.VelocityX = enemyData.patrolSpeed * actorTransform.localScale.x;
            
            
            bool isWalled = actorTransform.localScale.x == 1 ? controller.IsRightWalled : controller.IsLeftWalled;
            if (FrontCliffChecked() || isWalled)
            {
                Flip();
            }
        }
        else
        {
            
            if (FrontCliffChecked() || BackCliffChecked())
            {
                controller.VelocityX = 0;
                controller.SlideCancle();
            }
        }
    }
    
    bool FrontCliffChecked() => !Physics2D.Raycast(_frontCliffChecker.position, Vector2.down, 1.0f, LayerMask.GetMask("Ground"));
    
    bool BackCliffChecked() => !Physics2D.Raycast(_backCliffChecker.position, Vector2.down, 1.0f, LayerMask.GetMask("Ground"));
}
