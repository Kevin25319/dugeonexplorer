using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamage : MonoBehaviour, IActorDamage
{
    readonly int hashDamage = Animator.StringToHash("Damage");
    readonly int hashEmpty = Animator.StringToHash("Empty");
    readonly int hashRecovery = Animator.StringToHash("Recovery");

    public int maxHealth = 5;  
    public AudioClip _damageSound;  
    int _currentHealth;       

   
    [System.Serializable]
    struct HealthUI
    {
        [HideInInspector] public Image image;       
        [HideInInspector] public Animator animator; 
    }
    HealthUI[] _healthUI;

    // Đối tượng có hiệu ứng sát thương
    [Header("Damage Effect")]
    [SerializeField] float _screenStopDuration = 0.15f;     
    [SerializeField] float _screenEffectMagnitude = 0.15f;  
    [SerializeField] float _screenEffectDuration = 0.25f;   
    [SerializeField] Material _blinkMaterial;   
    [SerializeField] float _blinkDelay = 0.2f;  

    [Space(10)]

    [SerializeField] float _invincibleTime = 1.5f;  

    bool _isDead;           
    bool _isInvincibleing;

    SpriteRenderer _spriteRenderer; 
    Material _defaultMaterial;     

    
    public IActorDamage.KnockbackEventHandler KnockBack;
    
    public IActorDamage.DiedEventHandler Died;

    
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            
            _currentHealth = value;
            for (int i = _currentHealth; i < maxHealth; i++)
            {
                _healthUI[i].animator.SetTrigger(hashEmpty);
            }
            GameManager.instance.playerCurrentHealth = value;
        }
    }

    
    public bool IsDodged { get; set; }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentHealth = maxHealth;
        _defaultMaterial = _spriteRenderer.material;

        
        GameObject healthUI = GameObject.Find("Health");
        int healthCount = healthUI.transform.childCount;
        _healthUI = new HealthUI[healthCount];
        for (int i = 0; i < _healthUI.Length; i++)
        {
            Transform health = healthUI.transform.GetChild(i);

            _healthUI[i].image = health.GetComponent<Image>();
            _healthUI[i].animator = health.GetComponent<Animator>();
        }
    }

    public float GetHealthPercent() => _currentHealth / (float)maxHealth;

    
    /// <param name="heal"></param>
    public void HealthRecovery(int heal)
    {
        int prevHealth = _currentHealth;
        _currentHealth += heal;
        
        if(_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }

       
        for (int i = prevHealth; i < _currentHealth; i++)
        {
            _healthUI[i].animator.SetTrigger(hashRecovery);
        }
        
        
        GameManager.instance.playerCurrentHealth = _currentHealth;
    }

    
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    public void TakeDamage(int damage, KnockBack knockBack)
    {
        
        if(_isInvincibleing || _isDead || IsDodged) return;

        
        int prevHealth = _currentHealth;
        _currentHealth -= damage;
        GameManager.instance.playerCurrentHealth = _currentHealth;

        
        if(_currentHealth <= 0)
        {
            StartCoroutine(Died());
            _isDead = true;
            _currentHealth = 0;
        }

      
        for(int i = _currentHealth; i < prevHealth; i++)
        {
            _healthUI[i].animator.SetTrigger(hashDamage);
        }

        
        KnockBack(knockBack);

       
        SoundManager.instance.SoundEffectPlay(_damageSound);
        ScreenEffect.instance.BulletTimeStart(0f, _screenStopDuration);
        ScreenEffect.instance.ShakeEffectStart(_screenEffectMagnitude, _screenEffectDuration);

        
        _isInvincibleing = true;
        StartCoroutine(DamageEffect());
    }

    
    IEnumerator DamageEffect()
    {
        
        StartCoroutine("BlinkEffect");

        
        yield return YieldInstructionCache.WaitForSeconds(_invincibleTime);

        
        StopCoroutine("BlinkEffect");
        _spriteRenderer.material = _defaultMaterial;

        
        _isInvincibleing = false;
    }

    
    IEnumerator BlinkEffect()
    {
        while(true)
        {
            _spriteRenderer.material = _blinkMaterial;
            yield return YieldInstructionCache.WaitForSeconds(_blinkDelay);

            _spriteRenderer.material = _defaultMaterial;
            yield return YieldInstructionCache.WaitForSeconds(_blinkDelay);
        }
    }
}