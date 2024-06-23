using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;


public class SoundScreen : MonoBehaviour
{
    public GameObject optionsMenuScreen;    
    public TextMeshProUGUI soundText;    
    public TextMeshProUGUI manualText;   
    public Menu[] menu;     
    int _currentMenuIndex;  

    bool _rightInput;  
    bool _leftInput;   
    void OnEnable()
    {
        SoundOptionsRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
    }

    void Update()
    {
      
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        _rightInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Right);
        _leftInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Left);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

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

        if (_rightInput || _leftInput)
        {
            
            menu[_currentMenuIndex].menuSelectEvent.Invoke();
        }

        if (backInput)
        {
            
            _currentMenuIndex = 0;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            ReturnToOptionsMenuScreen();
        }
    }

    public void SetMasterVolume()
    {
        bool increase = _leftInput ? false : true;
        SoundSettingsManager.SetMasterVolume(increase);
        SoundOptionsRefresh();
    }

    public void SetMusicVolume()
    {
        bool increase = _leftInput ? false : true;
        SoundSettingsManager.SetMusicVolume(increase);
        SoundManager.instance.MusicVolumeRefresh();
        SoundOptionsRefresh();
    }

    public void SetEffectsVolume()
    {
        bool increase = _leftInput ? false : true;
        SoundSettingsManager.SetEffectsVolume(increase);
        SoundOptionsRefresh();
    }

    void SoundOptionsRefresh()
    {
        soundText.text = LanguageManager.GetText("Sound");
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "MasterVolumeText":
                    menu[i].text[0].text = LanguageManager.GetText("MasterVolume");
                    menu[i].text[1].text = SoundSettingsManager.GetMasterVolumeToTextUI();
                    break;
                case "MusicVolumeText":
                    menu[i].text[0].text = LanguageManager.GetText("MusicVolume");
                    menu[i].text[1].text = SoundSettingsManager.GetMusicVolumeToTextUI();
                    break;
                case "EffectsVolumeText":
                    menu[i].text[0].text = LanguageManager.GetText("EffectsVolume");
                    menu[i].text[1].text = SoundSettingsManager.GetEffectsVolumeToTextUI();
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
