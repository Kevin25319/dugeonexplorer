using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckPoint : MonoBehaviour
{
    [SerializeField] float _radius = 0.75f; 
    [SerializeField] Transform _circlePos;  
    [SerializeField] HUDManual _hudManual; 

    private bool _isHUDManualOpened;  

    private Player _player;
    private LayerMask _playerLayer;

    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _playerLayer = LayerMask.GetMask("Player");
    }

    void Update()
    {
        
        if(GameManager.instance.currentGameState != GameManager.GameState.Play) return;

        
        bool playerColl = Physics2D.OverlapCircle(_circlePos.position, _radius, _playerLayer);

        if (playerColl)
        {
           
            if(!_isHUDManualOpened)
            {
                _hudManual.DisplayManual("Rest", GameInputManager.PlayerActions.MoveUp, transform.position);
                _isHUDManualOpened = true;
            }

            
            if (GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.MoveUp))
            {
                Vector3 checkPointPos = transform.position;
                checkPointPos.x -= 2.0f;
                _player.RestAtCheckPoint(checkPointPos);
            }
        }
        else
        {
            
            if (_isHUDManualOpened)
            {
                _hudManual.HideManual();
                _isHUDManualOpened = false;
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_circlePos.position, _radius);
    }
}
