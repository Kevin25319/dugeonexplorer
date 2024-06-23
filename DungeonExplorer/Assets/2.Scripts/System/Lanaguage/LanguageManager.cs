using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Một lớp tĩnh để quản lý chức năng và dữ liệu liên quan đến ngôn ngữ.
/// </summary>
public static class LanguageManager
{
    // 지원하는 언어 목록
    public enum Language
    {
        English,
        VietNamese,
        Last     // Được sử dụng để chỉ ra rằng kích thước ngôn ngữ và dữ liệu ngôn ngữ chưa được đặt
    }
    public static Language currentLanguage; // Ngôn ngữ hiện đang sử dụng

    static Dictionary<string, LanguageData> languageData = new Dictionary<string, LanguageData>();

    /// <summary>
    /// khởi tạo lớp quản lý ngôn ngữ.
    /// </summary>
    public static void Init()
    {
        if (OptionsData.optionsSaveData.language != (int)Language.Last)
        {
            // Nếu có ngôn ngữ được đặt trong tùy chọn lưu dữ liệu, hãy tải ngôn ngữ đó.
            currentLanguage = (Language)OptionsData.optionsSaveData.language;
        }
        else
        {
            // Nếu ngôn ngữ chưa được đặt, hãy đặt ngôn ngữ theo ngôn ngữ của bạn.
            switch (CultureInfo.CurrentCulture.Name)
            {
                case "ko-KR":
                    currentLanguage = Language.VietNamese;
                    break;
                default:
                    currentLanguage = Language.English;
                    break;
            }
        }

        // Sau khi đọc và lưu trữ tệp CSV chứa dữ liệu ngôn ngữ giao diện người dùng, hãy thêm lớp dữ liệu ngôn ngữ
        List<Dictionary<string, object>> getUILanguageData = CSVReader.Read("LanguageData/UILanguageData");
        for (int i = 0; i < getUILanguageData.Count; i++)
        {
            var newLanguageData = (LanguageData)ScriptableObject.CreateInstance(typeof(LanguageData));
            string keyName = getUILanguageData[i]["Key"].ToString();                // Key
            newLanguageData.english = getUILanguageData[i]["English"].ToString();
            newLanguageData.vietnamese = getUILanguageData[i]["Vietnamese"].ToString();
            languageData.Add(keyName, newLanguageData);
        }
        OptionsData.optionsSaveData.language = (int)currentLanguage;    // Lưu ngôn ngữ hiện tại vào dữ liệu lưu tùy chọn
    }

    /// <summary>
    /// lấy ngôn ngữ từ key.
    /// </summary>
    /// <param name="key">key của ngôn ngữ muốn lấy</param>
    /// <returns>Ngôn ngữ được thay đổi dựa trên cài đặt ngôn ngữ chính và hiện tại</returns>
    public static string GetText(string key)
    {
        string text;
        switch (currentLanguage)
        {
            case Language.English:
                text = languageData[key].english;
                break;
            case Language.VietNamese:
                text = languageData[key].vietnamese;
                break;
            default:
                text = "ERROR!!";
                break;
        }
        return text;
    }

    /// <summary>
    /// Đặt ngôn ngữ hiện đang được sử dụng..
    /// </summary>
    /// <param name="right">Nếu đúng thì chỉ số sẽ tăng, nếu sai thì chỉ số sẽ giảm</param>
    public static void SetLanguage(bool right)
    {
        if (right)
        {
            currentLanguage++;
            if (currentLanguage >= Language.Last)
            {
                currentLanguage = 0;
            }
        }
        else
        {
            currentLanguage--;
            if (currentLanguage < 0)
            {
                currentLanguage = Language.Last - 1;
            }
        }
        OptionsData.optionsSaveData.language = (int)currentLanguage; // Lưu ngôn ngữ hiện tại dựa vào dữ liệu tùy chọn
    }

    /// <summary>
    /// Kiểm tra cài đặt ngôn ngữ hiện tại trong các tùy chọn
    /// </summary>
    /// <returns>Tên của ngôn ngữ hiện được đặt</returns>
    public static string GetCurrentLanguageToText()
    {
        string languageToString;
        switch (currentLanguage)
        {
            case Language.English:
                languageToString = "ENGLISH";
                break;
            case Language.VietNamese:
                languageToString = "Vietnamese";
                break;
            default:
                languageToString = "ERROR!!";
                break;
        }

        return languageToString;
    }
}