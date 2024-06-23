using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DelegateEye : MonoBehaviour
{
    [SerializeField] Transform _eye;
    Transform _playerTransform;

    void Awake()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector2 direction = (_playerTransform.position - transform.position).normalized;

      
        float posX = direction.x < -0.5f ? -0.125f :
                     direction.x > 0.5f ? 0.125f :
                     0;
        float posY = direction.y < -0.5f ? -0.125f :
                     direction.y > 0.5f ? 0.125f :
                     0;
        _eye.position = new Vector3(transform.position.x + posX, transform.position.y + posY, _eye.position.z);
    }
}
