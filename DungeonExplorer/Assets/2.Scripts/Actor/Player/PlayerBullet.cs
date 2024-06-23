using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerAttack))]
public class PlayerBullet : MonoBehaviour
{
    [SerializeField] int damage = 3;             
    [SerializeField] float _bulletSpeed = 10f;      
    [SerializeField] float bulletDuration = 2.0f;  
    [SerializeField] float knockBackForce = 8.0f;  
    [SerializeField] AudioClip hitSound;            

    float _angle;
    float _duration;
    bool _isHit;

    Vector3 _bulletDirection;
    KnockBack _knockBack = new KnockBack();

    Animator _anim;
    Transform _bulletTransform;
    PlayerAttack _attack;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _attack = GetComponent<PlayerAttack>();
        _bulletTransform = GetComponent<Transform>();

        _knockBack.force = knockBackForce;
    }

    void OnEnable()
    {
        var rotation = _bulletTransform.rotation;
        _bulletDirection = rotation * Vector3.left;
        _angle = rotation.eulerAngles.z;
        _bulletTransform.rotation = Quaternion.identity;

       
        _knockBack.direction = _angle > 90 && _angle < 270 ? 1 : -1;
    }

    void Update()
    {
        
        if(_isHit) return;

        _bulletTransform.Translate(_bulletDirection * _bulletSpeed * Time.deltaTime);

        
        if(_attack.IsAttacking(damage, _knockBack, false) || _attack.GroundHit())
        {
            _isHit = true;
            _anim.SetTrigger("Hit");
            SoundManager.instance.SoundEffectPlay(hitSound);
        }

        
        _duration += Time.deltaTime;
        if(_duration >= bulletDuration)
        {
            ObjectPoolManager.instance.ReturnPoolObject(gameObject);
        }
    }

    void OnDisable()
    {
        
        _isHit = false;
        _duration = 0;
    }
}
