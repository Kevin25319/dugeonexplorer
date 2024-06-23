using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// những thứ liên quan đến cài đặt âm thanh.
/// </summary>
public static class SoundSettingsManager
{
    public static float masterVolume = 1.0f;    // âm lượng tổng thể
    public static float musicVolume = 1.0f;     // âm lượng nhạc
    public static float effectsVolume = 1.0f;   // âm lượng hiệu ứng

    /// <summary>
    /// Phương pháp tĩnh để khởi tạo cài đặt âm thanh.
    /// </summary>
    public static void Init()
    {
        // Cài đặt âm lượng được lấy từ dữ liệu lưu tùy chọn
        masterVolume = OptionsData.optionsSaveData.masterVolume;
        musicVolume = OptionsData.optionsSaveData.musicVolume;
        effectsVolume = OptionsData.optionsSaveData.effectsVolume;

        AudioListener.volume = masterVolume;    // Đặt âm lượng của người nghe âm thanh thành âm lượng chính
    }

    /// <summary>
    /// đặt âm lượng chính.
    /// </summary>
    /// <param name="increase">Nếu true thì âm lượng tăng; nếu false thì âm lượng giảm.</param>
    public static void SetMasterVolume(bool increase)
    {
        // Cài đặt âm lượng
        if (increase)
        {
            if (masterVolume < 1f)
            {
                masterVolume += 0.1f;
            }
        }
        else
        {
            if (masterVolume > 0f)
            {
                masterVolume -= 0.1f;
            }
        }

        AudioListener.volume = masterVolume;    // Áp dụng âm lượng chính
        OptionsData.optionsSaveData.masterVolume = masterVolume;
    }

    /// <summary>
    ///  lấy tập đĩa chính hiện tại ở định dạng văn bản.
    /// </summary>
    /// <returns>Một chuỗi biểu thị âm lượng, sử dụng các ô vuông đầy ("■") và trống ("□").</returns>
    public static string GetMasterVolumeToTextUI()
    {
        int volume = Mathf.RoundToInt(AudioListener.volume * 10);
        StringBuilder volumeToText = new StringBuilder();
        for (int i = 0; i < 10; i++)
        {
            if (i < volume)
            {
                volumeToText.Append("■");
            }
            else
            {
                volumeToText.Append("□");
            }
        }
        return volumeToText.ToString();
    }

    /// <summary>
    /// đặt âm lượng nhạc.
    /// </summary>
    /// <param name="increase">Nếu true thì âm lượng tăng; nếu false thì âm lượng giảm.</param>
    public static void SetMusicVolume(bool increase)
    {
        if(increase)
        {
            if(musicVolume < 1f)
            {
                musicVolume += 0.1f;
            }
        }
        else
        {
            if (musicVolume > 0f)
            {
                musicVolume -= 0.1f;
            }
        }

        OptionsData.optionsSaveData.musicVolume = musicVolume;
    }

    /// <summary>
    /// Lấy âm lượng nhạc hiện tại ở dạng văn bản.
    /// </summary>
    /// <returns>Một chuỗi biểu thị âm lượng, sử dụng các ô vuông đầy ("■") và trống ("□").</returns>
    public static string GetMusicVolumeToTextUI()
    {
        int volume = Mathf.RoundToInt(musicVolume * 10);
        StringBuilder volumeToText = new StringBuilder();
        for (int i = 0; i < 10; i ++)
        {
            if(i < volume)
            {
                volumeToText.Append("■");
            }
            else
            {
                volumeToText.Append("□");
            }
        }
        return volumeToText.ToString();
    }

    /// <summary>
    /// đặt âm lượng hiệu ứng âm thanh.
    /// </summary>
    /// <param name="increase">Nếu true thì âm lượng tăng; nếu false thì âm lượng giảm.</param>
    public static void SetEffectsVolume(bool increase)
    {
        if (increase)
        {
            if (effectsVolume < 1)
            {
                effectsVolume += 0.1f;
            }
        }
        else
        {
            if (effectsVolume > 0)
            {
                effectsVolume -= 0.1f;
            }
        }

        OptionsData.optionsSaveData.effectsVolume = effectsVolume;
    }

    /// <summary>
    /// truy xuất âm lượng hiệu ứng âm thanh hiện tại ở dạng văn bản.
    /// </summary>
    /// <returns>Một chuỗi biểu thị âm lượng, sử dụng các ô vuông đầy ("■") và trống ("□").</returns>
    public static string GetEffectsVolumeToTextUI()
    {
        int volume = Mathf.RoundToInt(effectsVolume * 10);
        StringBuilder volumeToText = new StringBuilder();
        for (int i = 0; i < 10; i++)
        {
            if (i < volume)
            {
                volumeToText.Append("■");
            }
            else
            {
                volumeToText.Append("□");
            }
        }
        return volumeToText.ToString();
    }
}