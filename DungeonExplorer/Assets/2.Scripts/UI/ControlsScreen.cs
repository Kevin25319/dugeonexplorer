using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuUI;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


public class ControlsScreen : MonoBehaviour
{
    public GameObject optionsMenuScreen;    // Màn hình tùy chọn
    public TextMeshProUGUI controlsText;    // Văn bản tên tùy chọn
    public TextMeshProUGUI actionsText;     // hành động của người chơi
    public TextMeshProUGUI keyboardText;    // Hiển thị văn bản để cho biết tùy chọn có liên quan đến việc nhập bằng bàn phím.
    public TextMeshProUGUI controllerText;  // Hiển thị văn bản để cho biết tùy chọn có liên quan đến đầu vào của bộ điều khiển.
    public TextMeshProUGUI manualText;      // văn bản thủ công
    public Menu[] menu; // Sắp xếp menu

    int _currentMenuIndex;  // Chỉ mục của menu hiện tại
    bool _isMapping;        // Kiểm tra xem việc lập bản đồ có đang được tiến hành không

    void Awake()
    {
        if (manualText == null)
        {
            manualText = GameObject.Find("ManualText").GetComponent<TextMeshProUGUI>();
        }

        KeyRefresh();
        ButtonRefresh();
    }

    void OnEnable()
    {
        ControlsOptionsRefresh();
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        SetNonSelectedDeviceTextColor();
    }

    void Update()
    {
        // Dừng nếu lập bản đồ
        if (_isMapping) return;

        // Nhận đầu vào
        bool upInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Up);
        bool downInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Down);
        bool selectInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Select);
        bool backInput = GameInputManager.MenuInputDown(GameInputManager.MenuControl.Cancle);

        if (upInput)
        {
            // Giảm chỉ số khi nhập lên
            _currentMenuIndex--;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            SetNonSelectedDeviceTextColor();
        }
        else if (downInput)
        {
            // Tăng chỉ số khi nhập vào bên dưới
            _currentMenuIndex++;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            SetNonSelectedDeviceTextColor();
        }

        if (selectInput)
        {
            // Thực hiện sự kiện chọn menu khi nhập lựa chọn
            menu[_currentMenuIndex].menuSelectEvent.Invoke();
            SetNonSelectedDeviceTextColor();
        }

        if (backInput)
        {
            // Khi nhấn nút quay lại, các tùy chọn cài đặt vận hành sẽ bị đóng và menu tùy chọn sẽ quay trở lại.
            _currentMenuIndex = 0;
            MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
            ReturnToOptionsMenuScreen();
        }
    }

    /// <summary>
    /// Phương thức bắt đầu một coroutine ánh xạ nút cho một hành động cụ thể của người chơi.
    /// </summary>
    /// <param name="action">Hành động của người chơi để thay đổi cài đặt đầu vào</param>
    public void SetButtonMapping(string action)
    {
        var stringToAction = (GameInputManager.PlayerActions)Enum.Parse(typeof(GameInputManager.PlayerActions), action); // string을 PlayerActions로 변환
        StartCoroutine(ChooseButtonToMap(stringToAction));
    }

    /// <summary>
    /// Đây là một coroutine thực hiện ánh xạ nút.
    /// </summary>
    /// <param name="action">Hành động của người chơi để thay đổi cài đặt đầu vào</param>
    IEnumerator ChooseButtonToMap(GameInputManager.PlayerActions action)
    {
        _isMapping = true;  // Thay đổi trạng thái ánh xạ

        // Thay đổi màu văn bản của nút (hoặc phím) bạn muốn đặt
        TextMeshProUGUI buttonText;
        if(GameInputManager.usingController)
        {
            buttonText = menu[_currentMenuIndex].text[2];
        }
        else
        {
            buttonText = menu[_currentMenuIndex].text[1];
        }
        buttonText.color = new Color32(141, 105, 122, 255);
        manualText.text = LanguageManager.GetText("Empty"); // Để trống văn bản mô tả

        yield return null;

        // Chạy cho đến khi ánh xạ nút hoàn tất
        while (true)
        {
            if (GameInputManager.usingController)
            {
                // Thay đổi nút điều khiển đã nhập
                GamepadButton inputButton = GameInputManager.GetCurrentInputButton();

                if (inputButton != GamepadButton.Start)
                {
                    GameInputManager.GamepadMapping(action, inputButton);
                    ButtonRefresh();
                    break;
                }
            }
            else
            {
                // Thay đổi bằng phím đã nhập
                Key inputKey = GameInputManager.GetCurrentInputKey();

                if(inputKey != Key.None)
                {
                    GameInputManager.KeyboardMapping(action, inputKey);
                    KeyRefresh();
                    break;
                }
            }
            yield return null;
        }

        // Đặt lại màu về màu gốc và thay đổi sang trạng thái không thực hiện ánh xạ.
        MenuUIController.SelectMenuTextColorChange(menu[_currentMenuIndex]);
        _isMapping = false;

        // Làm mới menu
        MenuUIController.MenuRefresh(menu, ref _currentMenuIndex, manualText);
        SetNonSelectedDeviceTextColor();
    }

    /// <summary>
    /// thay đổi cài đặt đầu vào thành giá trị mặc định.
    /// </summary>
    public void ReturnToDefault()
    {
        if (GameInputManager.usingController)
        {
            GameInputManager.ControllerInputSetDefaults();
            ButtonRefresh();
        }
        else
        {
            GameInputManager.KeyboardInputSetDefaults();
            KeyRefresh();
        }
    }

    /// <summary>
    /// thay đổi màu của mặt thiết bị hiện không được sử dụng trong số văn bản trong menu hiện được chọn để cho biết rằng nó chưa được chọn.
    /// </summary>
    void SetNonSelectedDeviceTextColor()
    {
        if (menu[_currentMenuIndex].text.Length <= 1) return;

        if (GameInputManager.usingController)
        {
            menu[_currentMenuIndex].text[1].color = MenuUIController.notSelectColor;
        }
        else
        {
            menu[_currentMenuIndex].text[2].color = MenuUIController.notSelectColor;
        }
    }

    /// <summary>
    /// hiển thị và làm mới các văn bản tùy chọn điều khiển theo cài đặt ngôn ngữ.
    /// </summary>
    void ControlsOptionsRefresh()
    {
        controlsText.text = LanguageManager.GetText("Controls");
        actionsText.text = LanguageManager.GetText("Actions");
        keyboardText.text = LanguageManager.GetText("Keyboard");
        controllerText.text = LanguageManager.GetText("Controller");
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "MoveLeftText":
                    menu[i].text[0].text = LanguageManager.GetText("MoveLeft");
                    break;
                case "MoveRightText":
                    menu[i].text[0].text = LanguageManager.GetText("MoveRight");
                    break;
                case "MoveUpText":
                    menu[i].text[0].text = LanguageManager.GetText("MoveUp");
                    break;
                case "MoveDownText":
                    menu[i].text[0].text = LanguageManager.GetText("MoveDown");
                    break;
                case "JumpText":
                    menu[i].text[0].text = LanguageManager.GetText("Jump");
                    break;
                case "AttackText":
                    menu[i].text[0].text = LanguageManager.GetText("Attack");
                    break;
                case "SpecialAttackText":
                    menu[i].text[0].text = LanguageManager.GetText("SpecialAttack");
                    break;
                case "DodgeText":
                    menu[i].text[0].text = LanguageManager.GetText("Dodge");
                    break;
                case "ResetToDefaultText":
                    menu[i].text[0].text = LanguageManager.GetText("ResetToDefault");
                    break;
            }
        }
    }

    /// <summary>
    /// hiển thị và làm mới cài đặt nhập phím của bàn phím hiện tại dưới dạng văn bản.
    /// </summary>
    void KeyRefresh()
    {
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "MoveLeftText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.MoveLeft);
                    break;
                case "MoveRightText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.MoveRight);
                    break;
                case "MoveUpText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.MoveUp);
                    break;
                case "MoveDownText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.MoveDown);
                    break;
                case "JumpText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.Jump);
                    break;
                case "AttackText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.Attack);
                    break;
                case "SpecialAttackText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.SpecialAttack);
                    break;
                case "DodgeText":
                    menu[i].text[1].text = GameInputManager.ActionToKeyText(GameInputManager.PlayerActions.Dodge);
                    break;
#if UNITY_EDITOR
                case "ResetToDefaultText":
                    break;
                default:
                    Debug.LogError(menu[i].text[0].name + "은 잘못된 이름입니다");
                    break;
#endif
            }
        }
    }

    /// <summary>
    /// hiển thị và làm mới cài đặt nhập nút của bộ điều khiển hiện tại dưới dạng văn bản.
    /// </summary>
    void ButtonRefresh()
    {
        for (int i = 0; i < menu.Length; i++)
        {
            switch (menu[i].text[0].name)
            {
                case "MoveLeftText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.MoveLeft);
                    break;
                case "MoveRightText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.MoveRight);
                    break;
                case "MoveUpText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.MoveUp);
                    break;
                case "MoveDownText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.MoveDown);
                    break;
                case "JumpText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.Jump);
                    break;
                case "AttackText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.Attack);
                    break;
                case "SpecialAttackText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.SpecialAttack);
                    break;
                case "DodgeText":
                    menu[i].text[2].text = GameInputManager.ActionToButtonText(GameInputManager.PlayerActions.Dodge);
                    break;
#if UNITY_EDITOR
                case "ResetToDefaultText":
                    break;
                default:
                    Debug.LogError(menu[i].text[0].name + "은 잘못된 이름입니다");
                    break;
#endif
            }
        }
    }


    /// <summary>
    /// kết thúc các tùy chọn cài đặt nút thao tác và quay lại menu tùy chọn.
    /// </summary>
    void ReturnToOptionsMenuScreen()
    {
        gameObject.SetActive(false);
        optionsMenuScreen.SetActive(true);
    }
}
