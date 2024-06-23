using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;


public class VideoScreen : MonoBehaviour
{
    public GameObject optionsMenuScreen;   
    public TextMeshProUGUI videoText;       
    public TextMeshProUGUI manualText;     
    public Menu[] menu;     
    int _currentMenuIndex;  

    bool _rightInput, _leftInput, _selectInput; 
    bool _increase;

    void Awake()
    {
        if (manualText == null)
        {
            manualText = GameObject.Find("ManualText").GetComponent<TextMeshProUGUI>();
        }
    }

    void OnEnable()
    {
        VideoOptionsRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
    }

    void Update()
    {
        
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        _rightInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Left);
        _leftInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Right);
        _selectInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

        if (upInput)
        {
            
            _currentMenuIndex--;
            VideoSettingsManager.ResolutionIndexReturn();
            VideoOptionsRefresh();
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }
        else if (downInput)
        {
            
            _currentMenuIndex++;
            VideoSettingsManager.ResolutionIndexReturn();
            VideoOptionsRefresh();
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }
        else if (_rightInput || _leftInput)
        {
            
            _increase = _leftInput;
            menu[_currentMenuIndex].menuSelectEvent.Invoke();
        }
        else if (_selectInput)
        {
            if (menu[_currentMenuIndex].text[0].name == "ResolutionText")
            {
               
                VideoSettingsManager.NewResolutionAccept();
                MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            }
        }
        else if (backInput)
        {
            
            _currentMenuIndex = 0;
            VideoSettingsManager.ResolutionIndexReturn();
            VideoOptionsRefresh();
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            ReturnToOptionsMenuScreen();
        }
    }

    
    public void SetFullScreen()
    {
        VideoSettingsManager.SetFullScreen();
        VideoOptionsRefresh();
    }

    
    public void SetResolution()
    {
        VideoSettingsManager.SetResolution(_increase);
        menu[_currentMenuIndex].text[1].color = new Color32(141, 105, 122, 255); 

        VideoOptionsRefresh();
    }

    
    public void SetVSync()
    {
        VideoSettingsManager.SetVSync();
        VideoOptionsRefresh();
    }

    
    void VideoOptionsRefresh()
    {
        videoText.text = LanguageManager.GetText("Video");
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "FullScreenText":
                    menu[i].text[0].text = LanguageManager.GetText("FullScreen");
                    menu[i].text[1].text = VideoSettingsManager.GetFullScreenStatusText();
                    break;
                case "ResolutionText":
                    menu[i].text[0].text = LanguageManager.GetText("Resolution");
                    menu[i].text[1].text = VideoSettingsManager.GetCurrentResolutionText();
                    break;
                case "VSyncText":
                    menu[i].text[0].text = LanguageManager.GetText("VSync");
                    menu[i].text[1].text = VideoSettingsManager.GetVSyncStatusText();
                    break;
            }
        }
    }

    void ReturnToOptionsMenuScreen()
    {
        this.gameObject.SetActive(false);
        optionsMenuScreen.SetActive(true);
    }
}
