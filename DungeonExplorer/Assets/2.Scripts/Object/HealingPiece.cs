using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealingPiece : MonoBehaviour
{
    readonly int hashBroken = Animator.StringToHash("Broken"); 
    readonly int hashInit = Animator.StringToHash("Init"); 

    [SerializeField] float _collisionRange = 0.5f;   
    [SerializeField] bool _shouldDefinitelyDrop;   
    [SerializeField] AudioClip _pickUpSound;       

    LayerMask _playerLayer;

    Animator _anim;
    Transform _transform;
    PlayerDamage _playerDamage;
    ActorController _controller;

    void Awake()
    {
        _anim = GetComponent<Animator>(); // L?y tham chi?u t?i Animator c?a v?t ph?m
        _transform = GetComponent<Transform>(); // L?y tham chi?u t?i Transform c?a v?t ph?m
        _controller = GetComponent<ActorController>(); // L?y tham chi?u t?i ActorController c?a v?t ph?m

        // L?y tham chi?u t?i PlayerDamage c?a ng??i ch?i
        _playerDamage = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDamage>();

        // Xác ??nh l?p LayerMask cho ng??i ch?i
        _playerLayer = LayerMask.GetMask("Player");
    }

    void OnEnable()
    {
        if (ShouldDropBasedOnProbability())
        {
            // Kích ho?t tr?ng l?c cho v?t ph?m
            _controller.UseGravity = true;

            // B?t ??u quá trình làm h?i ph?c cho ng??i ch?i
            StartCoroutine(PlayerHealing()); 
        }
        else
        {
            // Tr? v?t ph?m v? PoolManager n?u không c?n thi?t
            ObjectPoolManager.instance.ReturnPoolObject(gameObject);
        }
    }

    
    bool ShouldDropBasedOnProbability()
    {
        // N?u ???c ??t là r?i ra ch?c ch?n, tr? v? true ngay l?p t?c
        if (_shouldDefinitelyDrop) return true;

        // Tính xác su?t r?i ra d?a trên ph?n tr?m máu c?a ng??i ch?i
        float healthPercent = _playerDamage.GetHealthPercent(); 
        float dropRate;

        // Xác ??nh xác su?t d?a trên ph?n tr?m máu
        if (healthPercent > 0.75f)
        {
            dropRate = 0.15f;
        }
        else if (healthPercent > 0.5f)
        {
            dropRate = 0.375f;
        }
        else if (healthPercent > 0.25f)
        {
            dropRate = 0.6f;
        }
        else
        {
            dropRate = 0.85f;
        }

        // So sánh v?i m?t s? ng?u nhiên ?? xác ??nh li?u v?t ph?m có r?i ra hay không
        return Random.value <= dropRate;
    }

    IEnumerator PlayerHealing()
    {
        // Kích ho?t trigger "Init" trong Animator c?a v?t ph?m
        _anim.SetTrigger(hashInit);
        float time = 0;

        // T?o m?t giá tr? ng?u nhiên cho v?n t?c di chuy?n theo tr?c X
        float moveX = Random.Range(6.0f, 18.0f);

        // Ch?n h??ng di chuy?n ng?u nhiên
        float direction = Mathf.Sign(Random.Range(-1f, 1f));
        float velocityX = moveX * direction;

        // ??t v?n t?c ban ??u theo tr?c Y
        _controller.VelocityY = Random.Range(6f, 14f);

        // Bi?n ki?m tra v?t ph?m có ?ang ? trên m?t ??t hay không
        bool isGrounded = false;

        // Vòng l?p này s? ch?y cho ??n khi v?t ph?m ???c nh?t ho?c th?i gian trôi qua
        while (time < 15.0f)
        {
            time += Time.deltaTime; // C?p nh?t th?i gian
            if (!isGrounded)
            {
                
                _controller.VelocityX = velocityX; // ??t v?n t?c theo tr?c X
                if (_controller.IsGrounded)
                {
                    isGrounded = true; // ?ã ??t m?t ??t
                    _controller.VelocityX = 0f; // D?ng di chuy?n theo tr?c X
                    _controller.SlideMove(moveX, direction); // Di chuy?n v?t ph?m theo h??ng ?ã ch?n
                }
            }

            
            bool isLeftWalled = _controller.VelocityX < 0 && _controller.IsLeftWalled;
            bool isRightWalled = _controller.VelocityX > 0 && _controller.IsRightWalled;
            if (isLeftWalled || isRightWalled)
            {
                direction = -direction;
                _controller.VelocityX = moveX * direction;
            }

            // Ki?m tra va ch?m v?i ng??i ch?i và th?c hi?n h?i ph?c n?u c?n
            if (Physics2D.OverlapCircle(_transform.position, _collisionRange, _playerLayer))
            {
                _playerDamage.HealthRecovery(1); // Th?c hi?n h?i ph?c máu cho ng??i ch?i
                _anim.SetTrigger(hashBroken);    // Kích ho?t trigger "Broken" trong Animator c?a v?t ph?m
                _controller.UseGravity = false;  // T?t tr?ng l?c cho v?t ph?m
                SoundManager.instance.SoundEffectPlay(_pickUpSound); // Phát âm thanh khi nh?t v?t ph?m

                yield return YieldInstructionCache.WaitForSeconds(1.0f); // ??i m?t kho?ng th?i gian tr??c khi ti?p t?c
                break;
            }

            yield return null; // Ch? m?t frame ti?p theo
        }
        // Tr? v?t ph?m v? PoolManager sau khi hoàn thành
        ObjectPoolManager.instance.ReturnPoolObject(gameObject); 
    }

    void OnDrawGizmos()
    {
        // ??t màu c?a hình v? là màu xanh lá cây
        Gizmos.color = Color.green;
        // V? hình c?u t?i v? trí c?a v?t ph?m v?i bán kính là _collisionRange
        Gizmos.DrawWireSphere(transform.position, _collisionRange);
    }
}
