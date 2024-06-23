using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System;


public static class TutorialManager
{
    
    public enum Tutorial
    {
        MoveAndJump,
        Attack,
        SpecialAttack,
        Dodge,
        Map
    }

    static List<Tutorial> seenTutorials = new List<Tutorial>(); 

    
    /// <param name="tutorial"></param>
    /// <returns></returns>
    public static ActionsAndKey[] GetActionsAndKeysForTutorial(Tutorial tutorial)
    {
        List<ActionsAndKey> actionsAndKey = new List<ActionsAndKey>();
        GameInputManager.PlayerActions[] actions = new GameInputManager.PlayerActions[1];
        string key;

        if(tutorial == Tutorial.MoveAndJump)
        {
            

            
            actions = new GameInputManager.PlayerActions[]
            {
                    GameInputManager.PlayerActions.MoveLeft,
                    GameInputManager.PlayerActions.MoveRight
            };
            key = "Move";
            actionsAndKey.Add(new ActionsAndKey(actions, key));

          
            actions = new GameInputManager.PlayerActions[]
            {
                    GameInputManager.PlayerActions.Jump
            };
            key = "Jump";
            actionsAndKey.Add(new ActionsAndKey(actions, key));
        }
        else
        {
            
            switch (tutorial)
            {
                case Tutorial.Attack: 
                    actions[0] = GameInputManager.PlayerActions.Attack;
                    break;
                case Tutorial.SpecialAttack: 
                    actions[0] = GameInputManager.PlayerActions.SpecialAttack;
                    break;
                case Tutorial.Dodge: 
                    actions[0] = GameInputManager.PlayerActions.Dodge;
                    break;
                case Tutorial.Map: 
                    actions[0] = GameInputManager.PlayerActions.Map;
                    break;
            }
            key = tutorial.ToString();
            actionsAndKey.Add(new ActionsAndKey(actions, key));
        }
        return actionsAndKey.ToArray();
    }

    
    /// <param name="tutorial"></param>
    /// <returns></returns>
    public static bool HasSeenTutorial(Tutorial tutorial) => seenTutorials.Contains(tutorial);

    
    /// <param name="tutorial">본 적이 있는 튜토리얼(열거형)</param>
    public static void AddSeenTutorial(Tutorial tutorial) => seenTutorials.Add(tutorial);

    
    /// <param name="tutorial">본 적이 있는 튜토리얼(string)</param>
    public static void AddSeenTutorial(string tutorial)
    {
        Tutorial stringToTutorial = (Tutorial)Enum.Parse(typeof(Tutorial), tutorial);
        seenTutorials.Add(stringToTutorial);
    }

   
    public static List<Tutorial> GetSeenTutorials() => seenTutorials;

    
    public static List<string> GetSeenTutorialsToString()
    {
        List<string> tutorialToString = new List<string>();
        for(int i = 0; i < seenTutorials.Count; i++)
        {
            tutorialToString.Add(seenTutorials[i].ToString());
        }
        return tutorialToString;
    }

   
    public static void SeenTutorialClear() => seenTutorials.Clear();
}

[System.Serializable]
public class ActionsAndKey
{
    public GameInputManager.PlayerActions[] action;
    public string key;
    
    public ActionsAndKey(GameInputManager.PlayerActions[] action, string key)
    {
        this.action = action;
        this.key = key;
    }
}

public class TutorialScreen : MonoBehaviour
{
    public TextMeshProUGUI explain;         
    IEnumerator _tutorialCoroutine = null;  

    
    /// <param name="tutorial"></param>
    public void TotorialStart(TutorialManager.Tutorial tutorial)
    {
        
        ActionsAndKey[] actionsAndKey = TutorialManager.GetActionsAndKeysForTutorial(tutorial);
        if(_tutorialCoroutine != null)
        {
           
            StopCoroutine(_tutorialCoroutine);
            _tutorialCoroutine = null;
        }
        _tutorialCoroutine = TutorialCoroutine(actionsAndKey);
        StartCoroutine(_tutorialCoroutine);
    }

    /// <param name="actionsAndKey"></param>
    IEnumerator TutorialCoroutine(ActionsAndKey[] actionsAndKey)
    {
        yield return null;

       
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < actionsAndKey.Length; i++)
        {
            for(int j = 0; j < actionsAndKey[i].action.Length; j++)
            {
                string action = GameInputManager.usingController ? GameInputManager.ActionToButtonText(actionsAndKey[i].action[j])
                                                                : GameInputManager.ActionToKeyText(actionsAndKey[i].action[j]);
                sb.AppendFormat("[ <color=#ffaa5e>{0}</color> ]", action);
                sb.Append(" ");
            }
            string key = LanguageManager.GetText(actionsAndKey[i].key);
            sb.Append(key);
            if(i < actionsAndKey.Length - 1)
            {
                sb.Append(" ");
            }
        }
        explain.text = sb.ToString();
        yield return YieldInstructionCache.WaitForSeconds(8.0f);
        
      
        explain.text = "";
        _tutorialCoroutine = null;
    }
}
