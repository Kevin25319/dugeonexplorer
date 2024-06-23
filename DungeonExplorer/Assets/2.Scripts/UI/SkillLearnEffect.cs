using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillLearnEffect : MonoBehaviour
{
    public RectTransform window;   
    public TextMeshProUGUI skillName;   

   
    /// <param name="skillNameKey"></param>
    public void SkillLearnEffectStart(string skillNameKey)
    {
        StartCoroutine(SkillLearnEffectCoroutine(skillNameKey));
    }

    IEnumerator SkillLearnEffectCoroutine(string skillNameKey)
    {
        
        ScreenEffect.instance.TimeStopStart();
        GameManager.instance.currentGameState = GameManager.GameState.MenuOpen;

        
        while(window.sizeDelta.y < 50)
        {
            float x = window.sizeDelta.x;
            float y = window.sizeDelta.y + 2;
            window.sizeDelta = new Vector2(x, y);
            yield return YieldInstructionCache.WaitForSecondsRealtime(0.015f);
        }

       
        skillName.text = LanguageManager.GetText(skillNameKey);
        yield return YieldInstructionCache.WaitForSecondsRealtime(2.0f);

        
        skillName.text = "";
        while (window.sizeDelta.y > 0)
        {
            float x = window.sizeDelta.x;
            float y = window.sizeDelta.y - 2;
            window.sizeDelta = new Vector2(x, y);
            yield return YieldInstructionCache.WaitForSecondsRealtime(0.015f);
        }
        
        GameManager.instance.currentGameState = GameManager.GameState.Play;
        ScreenEffect.instance.TimeStopCancle();
    }
}
