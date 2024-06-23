using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;

/// <summary>
/// lớp xử lý màn hình menu để cài đặt các tùy chọn trợ năng.
/// </summary>
public class AccessibilityScreen : MonoBehaviour
{
    public GameObject optionsMenuScreen;        // Màn hình menu tùy chọn
    public TextMeshProUGUI accessibilityText;   // Văn bản tên tùy chọn
    public TextMeshProUGUI manualText;          // văn bản thủ công
    public Menu[] menu;         // Sắp xếp menu
    int _currentMenuIndex;      // Chỉ mục menu hiện được chọn

    bool _rightInput;   // Nhập đúng hay không
    bool _leftInput;    // Đầu vào còn lại hay không

    void OnEnable()
    {
        // Làm mới giao diện người dùng khi menu được kích hoạt
        AccessibilityOptionsRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
    }

    void Update()
    {
        // 입력 받아오기
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        _rightInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Right);
        _leftInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Left);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

        if (upInput)
        {
            // Giảm chỉ số khi nhập lên (di chuyển menu chọn lên)
            _currentMenuIndex--;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }
        else if (downInput)
        {
            // Tăng chỉ số khi vào bên dưới (di chuyển menu lựa chọn xuống dưới)
            _currentMenuIndex++;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        }

        if (_rightInput || _leftInput)
        {
            // Thực hiện sự kiện chọn menu khi nhập sang trái hoặc phải (cài đặt tùy chọn khả năng truy cập)
            menu[_currentMenuIndex].menuSelectEvent.Invoke();
        }

        if (backInput)
        {
            // Nhấn nút quay lại sẽ đóng các tùy chọn trợ năng và quay lại menu tùy chọn.
            _currentMenuIndex = 0;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            ReturnToOptionsMenuScreen();
        }
    }

    /// <summary>
    /// đặt xem có sử dụng chế độ rung trên gamepad hay không.
    /// </summary>
    public void SetGamepadVibration()
    {
        bool increase = _leftInput ? false : true;
        AccessibilitySettingsManager.SetGamepadVibration(increase);
        AccessibilityOptionsRefresh();
    }

    /// <summary>
    /// đặt xem màn hình có rung hay không.
    /// </summary>
    public void SetScreenShake()
    {
        AccessibilitySettingsManager.SetScreenShake();
        AccessibilityOptionsRefresh();
    }

    /// <summary>
    /// đặt xem màn hình có nhấp nháy hay không.
    /// </summary>
    public void SetScreenFlashes()
    {
        AccessibilitySettingsManager.SetScreenFlashes();
        AccessibilityOptionsRefresh();
    }

    /// <summary>
    /// truy xuất nội dung của văn bản trong menu tùy chọn trợ năng từ dữ liệu ngôn ngữ và làm mới nó.
    /// </summary>
    void AccessibilityOptionsRefresh()
    {
        accessibilityText.text = LanguageManager.GetText("Accessibility");
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "ControllerVibrationText":
                    menu[i].text[0].text = LanguageManager.GetText("ControllerVibration");
                    menu[i].text[1].text = AccessibilitySettingsManager.GetGamepadVibrationToUI();
                    break;
                case "ScreenShakeText":
                    menu[i].text[0].text = LanguageManager.GetText("ScreenShake");
                    menu[i].text[1].text = AccessibilitySettingsManager.GetScreenShakeToggle();
                    break;
                case "ScreenFlashesText":
                    menu[i].text[0].text = LanguageManager.GetText("ScreenFlashes");
                    menu[i].text[1].text = AccessibilitySettingsManager.GetScreenFlashesToggle();
                    break;
            }
        }
    }

    /// <summary>
    /// đóng các tùy chọn trợ năng và quay lại menu tùy chọn.
    /// </summary>
    void ReturnToOptionsMenuScreen()
    {
        gameObject.SetActive(false);
        optionsMenuScreen.SetActive(true);
    }

}
