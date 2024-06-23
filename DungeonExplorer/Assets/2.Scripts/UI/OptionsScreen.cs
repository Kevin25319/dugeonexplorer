using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;

/// <summary>
/// Lớp xử lý hành vi của menu tùy chọn.
/// </summary>
public class OptionsScreen : MonoBehaviour
{
    // quay lại màn hình trước đó (chỉ được gọi ở tittle)
    public delegate void PrevScreenReturnEventHandler();
    public PrevScreenReturnEventHandler PrevScreenReturn;

    public GameObject optionsMenuScreen;    // Màn hình menu tùy chọn
    public TextMeshProUGUI optionsText;     // Tên văn bản (TÙY CHỌN)
    public TextMeshProUGUI manualText;      // văn bản mô tả
    public List<Menu> optionMenu;           // Danh sách menu tùy chọn
    int _currentMenuIndex;                  // Chỉ mục của menu hiện được chọn

    void Awake()
    {
        if (manualText == null)
        {
            manualText = GameObject.Find("ManualText").GetComponent<TextMeshProUGUI>();
        }

        // Khi màn hình tùy chọn được khởi chạy từ tittle
        // Đã xóa menu để quay lại màn hình tiêu đề và thoát khỏi menu trò chơi
        if (GameManager.instance.currentGameState == GameManager.GameState.Title)
        {
            List<int> menuToRemove = new List<int>();
            for (int i = optionMenu.Count - 1; i >= 0; i--)
            {
                if (optionMenu[i].text[0].name.Equals("ReturnToTitleScreenText") || optionMenu[i].text[0].name.Equals("QuitToDesktopText"))
                {
                    menuToRemove.Add(i);
                    if (menuToRemove.Count >= 2) break;
                }
            }
            for (int i = 0; i < menuToRemove.Count; i++)
            {
                MenuUIController.SelectMenuTextHide(optionMenu[menuToRemove[i]]);
                optionMenu.RemoveAt(menuToRemove[i]);
            }
        }
    }

    private void OnEnable()
    {
        // Refresh
        OptionTextRefresh();
        MenuUIController.MenuRefresh(optionMenu, ref _currentMenuIndex, manualText);
    }

    void Update()
    {
        // Dừng khi màn hình menu tùy chọn không hoạt động
        if (!optionsMenuScreen.activeSelf) return;

        OptionTextRefresh();
        MenuUIController.MenuRefresh(optionMenu, ref _currentMenuIndex, manualText);

        // Nhận đầu vào
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);
        bool selectInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select);

        if (upInput)
        {
            // Giảm chỉ số khi nhập lên (di chuyển menu chọn lên)
            _currentMenuIndex--;
            MenuUIController.MenuRefresh(optionMenu, ref _currentMenuIndex, manualText);
        }
        else if (downInput)
        {
            // Tăng chỉ số khi vào bên dưới (di chuyển menu lựa chọn xuống dưới)
            _currentMenuIndex++;
            MenuUIController.MenuRefresh(optionMenu, ref _currentMenuIndex, manualText);
        }
        if (selectInput)
        {
            // Thực hiện sự kiện chọn menu khi nhập lựa chọn
            optionMenu[_currentMenuIndex].menuSelectEvent.Invoke();
        }

        if (GameManager.instance.currentGameState == GameManager.GameState.Title)
        {
            if (backInput)
            {
                _currentMenuIndex = 0;
                gameObject.SetActive(false);
                PrevScreenReturn?.Invoke();
                OptionsData.OptionsSave();
                MenuUIController.MenuRefresh(optionMenu, ref _currentMenuIndex, manualText);
            }
        }
    }

    /// <summary>
    /// quay trở lại màn hình tiêu đề.
    /// Lưu trò chơi, thiết lập lại dữ liệu, v.v. cùng một lúc.
    /// </summary>
    public void ReturnToTitleScreen()
    {
        GameManager.instance.GameSave();
        SceneTransition.instance.LoadScene("Title");
        DeadEnemyManager.ClearDeadBosses();
        DeadEnemyManager.ClearDeadEnemies();
        PlayerLearnedSkills.hasLearnedClimbingWall = false;
        PlayerLearnedSkills.hasLearnedDoubleJump = false;
        TutorialManager.SeenTutorialClear();
        MapManager.ClearDiscoveredMaps();
        GameManager.instance.SetGameState(GameManager.GameState.Title);
    }

    /// <summary>
    /// phương pháp để quay lại chơi trò chơi.
    /// </summary>
    void ReturnToGamePlay()
    {
        _currentMenuIndex = 0;
        GameManager.instance.SetGameState(GameManager.GameState.Play);
        MenuUIController.MenuRefresh(optionMenu, ref _currentMenuIndex, manualText);
        gameObject.SetActive(false);
        OptionsData.OptionsSave();
        PrevScreenReturn?.Invoke();
    }

    /// <summary>
    /// phương pháp di chuyển đến một màn hình cụ thể (không phải một cảnh!).
    /// Được sử dụng để chạy một màn hình tùy chọn cụ thể.
    /// </summary>
    /// <param name="nextScreen">Màn hình bạn muốn bỏ qua (màn hình tùy chọn)</param>
    public void GoToNextScreen(GameObject nextScreen)
    {
        nextScreen.SetActive(true);
        optionsMenuScreen.SetActive(false);
    }

    /// <summary>
    /// phương pháp để kết thúc trò chơi.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// làm mới tất cả văn bản trong các tùy chọn theo cài đặt ngôn ngữ.
    /// </summary>
    void OptionTextRefresh()
    {
        optionsText.text = LanguageManager.GetText("Options");
        for (int i = 0; i < optionMenu.Count; i++)
        {
            switch (optionMenu[i].text[0].name)
            {
                case "VideoText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("Video");
                    break;
                case "SoundText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("Sound");
                    break;
                case "ControlsText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("Controls");
                    break;
                case "AccessibilityText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("Accessibility");
                    break;
                case "LanguageText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("Language");
                    break;
                case "ReturnToTitleScreenText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("ReturnToTitleScreen");
                    break;
                case "QuitToDesktopText":
                    optionMenu[i].text[0].text = LanguageManager.GetText("QuitToDesktop");
                    break;
            }
        }
    }
}
