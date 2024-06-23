using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] string _nextScene;              
    [SerializeField] Vector2 _size = Vector2.one;      
    [SerializeField] bool _isPlayerFacingRight = true; 

    LayerMask _playerLayer;
    Transform _portalTransform;

    bool _isHit; 

    void Awake() 
    {
        _playerLayer = LayerMask.GetMask("Player");
        _portalTransform = GetComponent<Transform>();
    }

    void Update()
    {
        if(_isHit) return;

        var coll = Physics2D.OverlapBox(_portalTransform.position, _size, 0, _playerLayer);

        
        if(coll)
        {
            _isHit = true;
            SceneTransition.instance.LoadScene(_nextScene);
            
            GameManager.instance.playerStartPos = coll.transform.position;
            GameManager.instance.playerStartlocalScaleX = _isPlayerFacingRight ? 1f : -1f;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, _size);
    }
}