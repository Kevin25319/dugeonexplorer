using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI;
using TMPro;


public class TitleScreen : MonoBehaviour
{
    public TextMeshProUGUI title;         
    public TextMeshProUGUI manualText;      
    public TextMeshProUGUI copyrightText;   
    public TextMeshProUGUI versionText;   
    public GameObject optionsScreen;      
    public GameObject selectProfileScreen; 
    public Menu[] menu; 

    bool _isOtherScreenOpening; 
    int _currentMenuIndex = 0;  
    void Start()
    {
        if (manualText == null)
        {
            manualText = GameObject.Find("ManualText").GetComponent<TextMeshProUGUI>();
        }
        if (copyrightText == null)
        {
            copyrightText = GameObject.Find("CopyrightText").GetComponent<TextMeshProUGUI>();
        }
        if (versionText == null)
        {
            versionText = GameObject.Find("VersionText").GetComponent<TextMeshProUGUI>();
        }

        TitleTextRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        optionsScreen.GetComponent<OptionsScreen>().PrevScreenReturn += OnPrevScreenReturn;
        selectProfileScreen.GetComponent<SelectProfileScreen>().PrevScreenReturn += OnPrevScreenReturn;
    }

    void Update()
    {
        
        if (_isOtherScreenOpening) return;

        TitleTextRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);

        
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        bool selectInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select);
        if (upInput)
        {
            
            _currentMenuIndex--;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }
        else if (downInput)
        {
            
            _currentMenuIndex++;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }
        if (selectInput)
        {
            
            menu[_currentMenuIndex].menuSelectEvent.Invoke();
        }
    }

    
    /// <param name="openScreen"></param>
    public void OpenOtherScreen(GameObject openScreen)
    {
        _isOtherScreenOpening = true;
        title.alpha = 0;
        versionText.alpha = copyrightText.alpha = 0;
        MenuUIController.AllMenuTextHide(menu);
        openScreen.SetActive(true);
    }

    
    void OnPrevScreenReturn()
    {
        title.alpha = 1;
        versionText.alpha = copyrightText.alpha = 1;
        _isOtherScreenOpening = false;
        TitleTextRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
    }

   
    public void GameExit()
    {
        OptionsData.OptionsSave();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

   
    void TitleTextRefresh()
    {
        for(int i = 0; i < menu.Length; i++)
        {
            switch(menu[i].text[0].name)
            {
                case "StartText":
                    menu[i].text[0].text = LanguageManager.GetText("Start");
                    break;
                case "OptionsText":
                    menu[i].text[0].text = LanguageManager.GetText("Options");
                    break;
                case "ExitText":
                    menu[i].text[0].text = LanguageManager.GetText("Exit");
                    break;
            }
        }
    }
}
