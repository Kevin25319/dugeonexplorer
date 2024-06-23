using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// lớp đơn quản lý độ rung của gamepad.
/// </summary>
public class GamepadVibrationManager : MonoBehaviour
{
    // ví dụ đơn lẻ
    public static GamepadVibrationManager instance = null;
    Coroutine _gamepadRumble = null;

    void Awake()
    {
        // Đảm bảo rằng chỉ tồn tại một phiên bản người quản lý (singleton)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Phương pháp khởi động gamepad rung bằng cách chỉ định cường độ cho động cơ trái và phải.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="duration">Thời gian rung (giây)</param>
    public void GamepadRumbleStart(float left, float right, float duration)
    {
        // Hủy bỏ nếu bộ điều khiển không được sử dụng
        if (!GameInputManager.usingController) return;

        // Nếu nó rung, hãy dừng rung động hiện có.
        if (_gamepadRumble != null)
        {
            StopCoroutine(_gamepadRumble);
            _gamepadRumble = null;
        }

        // Điều chỉnh cường độ rung, sau đó khởi động rountine
        left = left * AccessibilitySettingsManager.gamepadVibration;
        right = right * AccessibilitySettingsManager.gamepadVibration;
        _gamepadRumble = StartCoroutine(GamepadRumble(left, right, duration));
    }

    /// <summary>
    /// Phương pháp này bắt đầu rung gamepad bằng cách chỉ định cường độ.
    /// </summary>
    /// <param name="intensity">Cường độ rung của động cơ (độ rung giống nhau áp dụng cho cả bên trái và bên phải)</param>
    /// <param name="duration">Thời gian rung (giây)</param>
    public void GamepadRumbleStart(float intensity, float duration)
    {
        // Hủy bỏ nếu bộ điều khiển không được sử dụng
        if (!GameInputManager.usingController) return;

        // Nếu nó rung, hãy dừng rung động hiện có.
        if (_gamepadRumble != null)
        {
            StopCoroutine(_gamepadRumble);
            _gamepadRumble = null;
        }

        // Điều chỉnh cường độ rung theo cài đặt hỗ trợ tiếp cận, sau đó khởi động coroutine rung
        intensity = intensity * AccessibilitySettingsManager.gamepadVibration;
        _gamepadRumble = StartCoroutine(GamepadRumble(intensity * 0.3f, intensity, duration));
    }

    /// <summary>
    /// Phương pháp này ngăn chặn độ rung của gamepad.
    /// </summary>
    public void GamepadRumbleStop()
    {
        // Hủy bỏ nếu bộ điều khiển không được sử dụng
        if (!GameInputManager.usingController) return;

        // Dừng rung và thiết lập lại xúc giác
        if (_gamepadRumble != null)
        {
            StopCoroutine(_gamepadRumble);
            InputSystem.ResetHaptics();
            _gamepadRumble = null;
        }
    }

    /// <summary>
    /// xử lý độ rung của gamepad trong một khoảng thời gian nhất định.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="duration"></param>
    IEnumerator GamepadRumble(float left, float right, float duration)
    {
        // Cài đặt rung của gamepad
        Gamepad.current.SetMotorSpeeds(left, right);

        // Chờ một thời gian nhất định
        yield return YieldInstructionCache.WaitForSecondsRealtime(duration);

        // Sau một thời gian, hãy đặt lại xúc giác và giải phóng tham chiếu coroutine.
        InputSystem.ResetHaptics();
        _gamepadRumble = null;
    }
}
