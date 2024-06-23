using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class SelectProfile
{
    public Image profileImage;              
    public TextMeshProUGUI playTimeText;    
    public TextMeshProUGUI newText;        
    public Image saveDeleteGuage;          
}


public class SelectProfileScreen : MonoBehaviour
{
    
    public delegate void PrevScreenReturnEventHandler();
    public PrevScreenReturnEventHandler PrevScreenReturn;

    public Sprite selectImage;                  
    public Sprite notSelectImage;               
    public TextMeshProUGUI selectProfileText;   
    public TextMeshProUGUI manualText;          
    public SelectProfile[] profile;             

    int _currentSaveFileIndex;  


    void Awake()
    {
        SelectProfileInit();
        SelectProfileRefresh();
    }

    void OnEnable()
    {
        
        selectProfileText.text = LanguageManager.GetText("SelectProfile");

        string backKey = LanguageManager.GetText("Back");
        string selectKey = LanguageManager.GetText("Confirm");
        string deleteKey = LanguageManager.GetText("Delete");
        string backInput, selectInput, deleteInput;

        
        if (GameInputManager.usingController)
        {
            backInput = GameInputManager.MenuControlToButtonText(GameInputManager.MenuControl.Cancle);
            selectInput = GameInputManager.MenuControlToButtonText(GameInputManager.MenuControl.Select);
            deleteInput = GameInputManager.MenuControlToButtonText(GameInputManager.MenuControl.Delete);
        }
        else
        {
            backInput = GameInputManager.MenuControlToKeyText(GameInputManager.MenuControl.Cancle);
            selectInput = GameInputManager.MenuControlToKeyText(GameInputManager.MenuControl.Select);
            deleteInput = GameInputManager.MenuControlToKeyText(GameInputManager.MenuControl.Delete);
        }
        manualText.text = string.Format("{0} [ <color=#ffaa5e>{1}</color> ] {2} [ <color=#ffaa5e>{3}</color> ] {4} [ <color=#ffaa5e>{5}</color> ]", backKey, backInput, selectKey, selectInput, deleteKey, deleteInput);
    }

    void Update()
    {
        
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);
        bool selectInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select);
        bool deleteInput = GameInputManager.MenuInput(GameInputManager.MenuControl.Delete);

        if (upInput)
        {
            
            _currentSaveFileIndex--;
            SelectProfileRefresh();
        }
        else if (downInput)
        {
            
            _currentSaveFileIndex++;
            SelectProfileRefresh();
        }

        if (selectInput)
        {
            
            GameManager.instance.GameLoad(_currentSaveFileIndex + 1);
            GameManager.instance.SetGameState(GameManager.GameState.Play);
        }
        else if (backInput)
        {
            
            gameObject.SetActive(false);
            PrevScreenReturn();
        }
        if (deleteInput)
        {
            
            SelectProfileDelete();
        }
        DeleteGaugeValueToDefault(deleteInput);
    }

    void SelectProfileInit()
    {
        for (int i = 0; i < profile.Length; i++)
        {
            GameSaveData saveData = SaveSystem.GameLoad(i + 1);
            if (saveData != null)
            {
               
                profile[i].playTimeText.text = GameManager.instance.IntToTimeText(saveData.playTime);
                profile[i].newText.text = "";
            }
            else
            {
                
                profile[i].playTimeText.text = "";
                profile[i].newText.text = "NEW";
            }
            profile[i].saveDeleteGuage.fillAmount = 0f;
        }
    }

    void SelectProfileRefresh()
    {
        if (_currentSaveFileIndex >= profile.Length)
        {
            _currentSaveFileIndex = 0;
        }
        else if (_currentSaveFileIndex < 0)
        {
            _currentSaveFileIndex = profile.Length - 1;
        }

        for(int i = 0; i < profile.Length; i++)
        {
            profile[i].profileImage.sprite = notSelectImage;
        }
        profile[_currentSaveFileIndex].profileImage.sprite = selectImage;
    }

    void SelectProfileDelete()
    {
       
        if(SaveSystem.GameLoad(_currentSaveFileIndex + 1) == null)
        {
            return;
        }

        float fillAmount = profile[_currentSaveFileIndex].saveDeleteGuage.fillAmount + (0.3f * Time.deltaTime);
        fillAmount = Mathf.Clamp(fillAmount, 0f, 1.0f);
        profile[_currentSaveFileIndex].saveDeleteGuage.fillAmount = fillAmount;
        
        if(GameInputManager.usingController)
        {
            
            GamepadVibrationManager.instance.GamepadRumbleStart(fillAmount * 0.5f, 0.02f);
        }

        
        if(fillAmount >= 0.5f && fillAmount < 0.75f)
        {
            profile[_currentSaveFileIndex].saveDeleteGuage.color = new Color32(255,170,94,255);
        }
        else if(fillAmount >= 0.75f && fillAmount < 1.0f)
        {
            profile[_currentSaveFileIndex].saveDeleteGuage.color = new Color32(142, 67, 91, 255);            
        }
        else if(fillAmount >= 1.0f)
        {
            
            profile[_currentSaveFileIndex].saveDeleteGuage.fillAmount = 0;
            SaveSystem.GameDelete(_currentSaveFileIndex + 1);
            SelectProfileInit();
        }
    }

    /// <param name="deleteInput"></param>
    void DeleteGaugeValueToDefault(bool deleteInput)
    {
        for(int i = 0; i < profile.Length; i++)
        {
            if(profile[i].saveDeleteGuage.fillAmount == 0 || (deleteInput && i == _currentSaveFileIndex))
            {
               
                continue;
            }
            float fillAmount = profile[i].saveDeleteGuage.fillAmount - Time.deltaTime;
            fillAmount = Mathf.Clamp(fillAmount, 0f, 1.0f);
            profile[i].saveDeleteGuage.fillAmount = fillAmount;

            if (fillAmount >= 0.5f && fillAmount < 0.75f)
            {
                profile[_currentSaveFileIndex].saveDeleteGuage.color = new Color32(255, 170, 94, 255);
            }
            else if(fillAmount < 0.5f)
            {
                profile[_currentSaveFileIndex].saveDeleteGuage.color = new Color32(255, 236, 214, 255);
            }
        }
    }
}
