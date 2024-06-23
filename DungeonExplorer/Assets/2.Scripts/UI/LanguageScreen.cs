using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;

public class LanguageScreen : MonoBehaviour
{
    public GameObject optionsMenuScreen;    
    public TextMeshProUGUI languageText;    
    public TextMeshProUGUI manualText;      
    public Menu[] menu; 

    bool _rightInput, _leftInput;   

    void Awake()
    {
        if (manualText == null)
        {
            manualText = GameObject.Find("ManualText").GetComponent<TextMeshProUGUI>();
        }
    }

    void OnEnable()
    {
        LanguageOptionsRefresh();
        MenuUIController.SetMenualText(menu[0], manualText);
    }

    void Update()
    {
        _rightInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Right);
        _leftInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Left);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

        if (_rightInput || _leftInput)
        {
            menu[0].menuSelectEvent.Invoke();
            MenuUIController.SetMenualText(menu[0], manualText);
        }

        if (backInput)
        {
            ReturnToOptionsMenuScreen();
        }
    }

    
    public void SetLanguage()
    {
        bool right = _rightInput ? false : true;
        LanguageManager.SetLanguage(right);
        LanguageOptionsRefresh();
    }

    
    void LanguageOptionsRefresh()
    {
        languageText.text = LanguageManager.GetText("Language");
        menu[0].text[0].text = LanguageManager.GetCurrentLanguageToText();
    }

    
    void ReturnToOptionsMenuScreen()
    {
        this.gameObject.SetActive(false);
        optionsMenuScreen.SetActive(true);
    }
}