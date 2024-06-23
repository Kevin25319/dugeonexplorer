using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour, IActorDamage
{
   
    public IActorDamage.KnockbackEventHandler KnockBack = null;
    
    public IActorDamage.DiedEventHandler Died;

    SpriteRenderer _spriteRenderer; 
    Material _defalutMaterial;      

    public int CurrentHealth { get; set; }      
    public int MaxHealth { get; set; }         
    public bool SuperArmor { get; set; }       
    public bool IsDead { get; protected set; }   
    public bool IsInvincibled { get; set; }     
    public Material BlinkMaterial { get; set; } 

    
    public float GetHealthPercent() =>  CurrentHealth / (float)MaxHealth;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defalutMaterial = _spriteRenderer.material;

        
        if (KnockBack == null)
        {
            SuperArmor = true;
        }

        CurrentHealth = MaxHealth;
    }

    
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    public virtual void TakeDamage(int damage, KnockBack knockBack)
    {
        // Không thực thi nếu đã chết
        if (IsDead) return;

        // Gây knockback nếu không có siêu giáp
        if (!SuperArmor)
        {
            KnockBack(knockBack);
        }

        // Sau khi giảm sức chịu đựng, nếu sức chịu đựng dưới 0 thì sẽ chết
        CurrentHealth -= damage;
        if(CurrentHealth <= 0)
        {
            StartCoroutine(Died());
            IsDead = true;
            return;
        }

        // Xử lý hiệu ứng nhấp nháy Sprite
        StartCoroutine(BlinkEffect());
    }

    /// <summary>
    /// xử lý hiệu ứng nhấp nháy của sprite khi nhận sát thương.
    /// </summary>
    IEnumerator BlinkEffect()
    {
        _spriteRenderer.material = BlinkMaterial;

        yield return YieldInstructionCache.WaitForSeconds(0.1f);

        _spriteRenderer.material = _defalutMaterial;
    }
}
