using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
 
    public delegate void MapOpendEventHandler();
    public MapOpendEventHandler MapOpend;

    [SerializeField] GameObject _pauseScreen;       
    [SerializeField] GameObject _optionsMenuScreen; 
    [SerializeField] GameObject _mapScreen;         

    bool _playerDead;   

    void Awake()
    {
        _pauseScreen.SetActive(false);
    }

    void Start()
    {
        GameManager.instance.PlayerDieEvent += OnPlayerDied;
    }

    void Update()
    {

        bool optionsMenuInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Pause);
        bool mapInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Map);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

        if (GameManager.instance.currentGameState == GameManager.GameState.Play)
        {

            if (!_playerDead)
            {
                if (optionsMenuInput)
                {
                    OptionsMenuOpen();
                }
                else if (mapInput)
                {
                    MapOpen();
                }
            }
        }
        else if (GameManager.instance.currentGameState == GameManager.GameState.MenuOpen)
        {
            
            if (_optionsMenuScreen.activeSelf == true)
            {
                if (optionsMenuInput || backInput)
                {
                    ReturnToGamePlay();
                }
            }
            else if (_mapScreen.activeSelf == true)
            {
                if (mapInput || backInput)
                {
                    ReturnToGamePlay();
                }
            }
        }
    }

    void ReturnToGamePlay()
    {
        GameManager.instance.SetGameState(GameManager.GameState.Play);
        _pauseScreen.SetActive(false);
    }

    void OptionsMenuOpen()
    {
        GameManager.instance.SetGameState(GameManager.GameState.MenuOpen);
        _pauseScreen.SetActive(true);
        _optionsMenuScreen.SetActive(true);
        _mapScreen.SetActive(false);
    }

    void MapOpen()
    {
        GameManager.instance.SetGameState(GameManager.GameState.MenuOpen);
        _pauseScreen.SetActive(true);
        _mapScreen.SetActive(true);
        _optionsMenuScreen.SetActive(false);
        MapOpend?.Invoke();
    }

    void OnPlayerDied()
    {
        _playerDead = true;
    }
}