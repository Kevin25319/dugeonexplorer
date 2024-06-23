using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Lớp quản lý các tùy chọn cài đặt video.
/// </summary>
public static class VideoSettingsManager
{
    static List<Resolution> resolutions = new List<Resolution>();   // Danh sách độ phân giải màn hình
    static bool fullScreen = Screen.fullScreen; // Toàn màn hình hay không
    static int prevResolutionIndex;             // Chỉ số độ phân giải màn hình trước đó
    static int currentResolutionIndex;          // Chỉ số độ phân giải màn hình hiện tại

    public static bool vsync;           // VSync có được bật hay không

    /// <summary>
    /// phương pháp để khởi tạo các tùy chọn video.
    /// </summary>
    public static void VideoOptionsInit()
    {
        // danh sách độ phân giải màn hình không trống thì nó được coi là đã được khởi tạo và quá trình thực thi sẽ dừng lại.
        if (resolutions.Count > 0) return;

        // Đặt lại danh sách độ phân giải
        resolutions = Screen.resolutions.ToList();
        for (int i = resolutions.Count - 1; i > 0; i--)
        {
            // Xóa tất cả các độ phân giải có cùng chiều rộng và chiều cao
            int prevResolutionWidth = resolutions[i - 1].width;
            int prevResolutionHeight = resolutions[i - 1].height;
            int currentResolutionWidth = resolutions[i].width;
            int currentResolutionHeight = resolutions[i].height;

            if (prevResolutionWidth == currentResolutionWidth && prevResolutionHeight == currentResolutionHeight)
            {
                resolutions.RemoveAt(i);
            }
        }

        // Toàn màn hình và VSync được lấy từ tùy chọn lưu dữ liệu.
        Screen.fullScreen = fullScreen = OptionsData.optionsSaveData.fullScreenMode;
        vsync = OptionsData.optionsSaveData.vSync;
        QualitySettings.vSyncCount = vsync ? 1 : 0; // Đặt thành 1 nếu vsync đúng

        if (OptionsData.optionsSaveData.resolution == null)
        {
            // Nếu chỉ số độ phân giải của dữ liệu lưu tùy chọn là null, hãy đặt độ phân giải theo kích thước màn hình của người dùng.
            SetResolutionToScreenSize();
        }
        else
        {
            // Khi chỉ mục độ phân giải của tùy chọn lưu dữ liệu không phải là rỗng
            // Nhận độ phân giải từ dữ liệu lưu tùy chọn và đặt thành độ phân giải màn hình hiện tại
            bool isFound = false;
            Resolution optionResolution = (Resolution)OptionsData.optionsSaveData.resolution;
            for (int i = 0; i < resolutions.Count; i++)
            {
                // Nhận chỉ mục của màn hình với độ phân giải phù hợp
                if (optionResolution.width == resolutions[i].width && optionResolution.height == resolutions[i].height)
                {
                    // Đặt cả độ phân giải trước đó và độ phân giải hiện tại thành chỉ mục của màn hình hiện tại
                    prevResolutionIndex = currentResolutionIndex = i;
                    isFound = true;
                    break;
                }
            }
            // Nếu không có độ phân giải phù hợp, hãy đặt độ phân giải theo kích thước màn hình của bạn.
            if (!isFound)
            {
                SetResolutionToScreenSize();
            }
        }

        // Đặt độ phân giải màn hình hiện tại thông qua chỉ mục
        int newWidth = resolutions[currentResolutionIndex].width;
        int newHeight = resolutions[currentResolutionIndex].height;
        Screen.SetResolution(newWidth, newHeight, fullScreen);
    }

    /// <summary>
    /// Phương pháp tĩnh khớp độ phân giải của trò chơi hiện tại với kích thước màn hình mà người chơi sử dụng.
    /// </summary>
    static void SetResolutionToScreenSize()
    {
        int width = Screen.width;
        int height = Screen.height;

        // Đặt độ phân giải hiện tại phù hợp với màn hình của người dùng
        for (int i = 0; i < resolutions.Count; i++)
        {
            // Nhận chỉ mục của màn hình với độ phân giải phù hợp
            if (width == resolutions[i].width && height == resolutions[i].height)
            {
                // Đặt cả độ phân giải trước đó và độ phân giải hiện tại thành chỉ mục của màn hình hiện tại
                prevResolutionIndex = currentResolutionIndex = i;
                break;
            }
        }

        // Lưu độ phân giải hiện tại vào dữ liệu lưu tùy chọn
        OptionsData.optionsSaveData.resolution = resolutions[currentResolutionIndex];
    }

    /// <summary>
    /// phương thức tĩnh để thiết lập xem nó có ở chế độ toàn màn hình hay không.
    /// Nếu màn hình hiện tại là toàn màn hình, nó sẽ chuyển sang màn hình cửa sổ. 
    /// Nếu màn hình hiện tại là màn hình cửa sổ, nó sẽ chuyển sang toàn màn hình.
    /// </summary>
    public static void SetFullScreen()
    {
        fullScreen = !fullScreen;
        Screen.fullScreen = fullScreen;
        OptionsData.optionsSaveData.fullScreenMode = fullScreen; // Tùy chọn lưu dữ liệu ở chế độ toàn màn hình hay không
    }

    /// <summary>
    /// phương pháp tĩnh truy xuất văn bản cho dù toàn màn hình được kích hoạt hay hủy kích hoạt
    /// </summary>
    /// <returns>Bật nếu Enabled, Tắt nếu Disabled</returns>
    public static string GetFullScreenStatusText() => fullScreen ? LanguageManager.GetText("Enabled") : LanguageManager.GetText("Disabled");

    /// <summary>
    /// Phương pháp tĩnh để đặt độ phân giải màn hình.
    /// </summary>
    /// <param name="increase">Nếu true thì chỉ số độ phân giải hiện tại sẽ tăng lên, nếu false thì chỉ số này sẽ giảm đi.</param>
    public static void SetResolution(bool increase)
    {
        if (increase)
        {
            // Nếu giá trị nhận được là đúng thì chỉ số độ phân giải hiện tại sẽ tăng.
            currentResolutionIndex++;
            if (currentResolutionIndex >= resolutions.Count)
            {
                currentResolutionIndex = 0;
            }
        }
        else
        {
            // Nếu giá trị nhận được là sai, chỉ số độ phân giải hiện tại sẽ giảm.
            currentResolutionIndex--;
            if (currentResolutionIndex < 0)
            {
                currentResolutionIndex = resolutions.Count - 1;
            }
        }
    }

    /// <summary>
    /// phương pháp tĩnh lấy độ phân giải màn hình hiện tại dưới dạng Văn bản
    /// </summary>
    /// <returns>Độ phân giải màn hình hiện tại(string)</returns>
    public static string GetCurrentResolutionText()
    {
        int width = resolutions[currentResolutionIndex].width;
        int height = resolutions[currentResolutionIndex].height;
        string resolutionToString = width + " x " + height;
        return resolutionToString;
    }

    /// <summary>
    /// phương thức tĩnh trả về độ phân giải hiện tại về độ phân giải đã đặt trước đó.
    /// </summary>
    public static void ResolutionIndexReturn() => currentResolutionIndex = prevResolutionIndex;

    /// <summary>
    /// phương pháp tĩnh thay đổi độ phân giải hiện tại sang độ phân giải màn hình mới.
    /// </summary>
    public static void NewResolutionAccept()
    {
        // Nếu độ phân giải trước đó và độ phân giải hiện tại giống nhau thì nó sẽ không chạy.
        if (prevResolutionIndex == currentResolutionIndex) return;

        // Cài đặt độ phân giải màn hình
        int width = resolutions[currentResolutionIndex].width;
        int height = resolutions[currentResolutionIndex].height;
        Screen.SetResolution(width, height, fullScreen);

        prevResolutionIndex = currentResolutionIndex;   // Đặt độ phân giải trước đó thành độ phân giải hiện tại

        OptionsData.optionsSaveData.resolution = resolutions[currentResolutionIndex];    // Lưu độ phân giải trong dữ liệu lưu tùy chọn
    }

    /// <summary>
    /// Phương pháp tĩnh để đặt xem có sử dụng VSync hay không.
    /// ON/OFF tùy thuộc vào việc VSync hiện có đang được sử dụng hay không.
    /// </summary>
    public static void SetVSync()
    {
        vsync = !vsync;
        QualitySettings.vSyncCount = vsync ? 1 : 0; // cài đặt vsync
        OptionsData.optionsSaveData.vSync = vsync;  // Lưu độ phân giải trong dữ liệu lưu tùy chọn
    }

    /// <summary>
    /// phương pháp tĩnh truy xuất văn bản cho dù VSync được bật hay tắt
    /// </summary>
    /// <returns>Đã bật nếu Enable VSync, nếu Disable thì bị tắt</returns>
    public static string GetVSyncStatusText() => vsync ? LanguageManager.GetText("Enabled") : LanguageManager.GetText("Disabled");
}