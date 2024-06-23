using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BossEnemyDamage : EnemyDamage
{
    public Image bossHelathUI; 
    public List<float> nextPhaseHealthPercent = new List<float>(); 

    
    public delegate void PhaseChangedEventHandler();
    public PhaseChangedEventHandler phaseChangedEvent;

    Coroutine damageUIEffect = null;

    protected override void Start()
    {
        base.Start();
        nextPhaseHealthPercent.Sort(); 
    }

   
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    public override void TakeDamage(int damage, KnockBack knockBack)
    {
        base.TakeDamage(damage,knockBack);


        if (damageUIEffect != null)
        {
            StopCoroutine(damageUIEffect);
            damageUIEffect = null;
        }
        damageUIEffect = StartCoroutine(DamageUIEffect());


        if (nextPhaseHealthPercent.Count == 0) return;
        else if(GetHealthPercent() <= nextPhaseHealthPercent[0])
        {
            nextPhaseHealthPercent.RemoveAt(0);
            phaseChangedEvent();
        }
    }

    IEnumerator DamageUIEffect()
    {

        float percent = GetHealthPercent();
        while(bossHelathUI.fillAmount < percent)
        {
            bossHelathUI.fillAmount -= 0.005f;
            yield return YieldInstructionCache.WaitForSeconds(0.02f);
        }
        bossHelathUI.fillAmount = percent;

        damageUIEffect = null;
    }
}
