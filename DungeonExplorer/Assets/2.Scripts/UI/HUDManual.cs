using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Qu?n lý hi?n th? và ?n manual trên giao di?n ng??i dùng trong trò ch?i Unity.
/// </summary>
public class HUDManual : MonoBehaviour
{
    Transform _transform;
    TextMeshProUGUI _manual;
    readonly Color32 textColor = new Color32(3,6,26,255);
    
    Coroutine showManualCoroutine = null;
    Coroutine closeManualCoroutine = null;

    void Awake()
    {
        _manual = GetComponent<TextMeshProUGUI>();
        _transform = GetComponent<Transform>();

        byte r = textColor.r;
        byte g = textColor.g;
        byte b = textColor.b;

        _manual.color = new Color32(r,g,b,0);
    }

    /// <summary>
    /// ¸Þ´º¾óÀ» Ç¥½ÃÇÏ´Â ¸Þ¼ÒµåÀÔ´Ï´Ù.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    /// <param name="targetPos"></param>
    public void DisplayManual(string key, GameInputManager.PlayerActions action, Vector3 targetPos)
    {
        if(showManualCoroutine != null) return;

        if(closeManualCoroutine != null)
        {
            StopCoroutine(closeManualCoroutine);
            closeManualCoroutine = null;
        }

        string keyToText = LanguageManager.GetText(key);
        string input = !GameInputManager.usingController ? GameInputManager.ActionToKeyText(action) : GameInputManager.ActionToButtonText(action);
        _manual.text = keyToText + " [ <color=#ffaa5e>" + input + "</color> ]";


        targetPos.y += 4.0f;
        Vector3 screenPos = targetPos;
        screenPos.z = _transform.position.z;
        _transform.position = screenPos;

        showManualCoroutine = StartCoroutine(ManualFadeIn());
    }

    IEnumerator ManualFadeIn()
    {
        byte r = textColor.r;
        byte g = textColor.g;
        byte b = textColor.b;
        byte a = Convert.ToByte(_manual.color.a * 255f);

        while(a < 255)
        {
            if(a + 20 < 255)
            {
                a += 20;
            }
            else a = 255;

            _manual.color = new Color32(r,g,b,a);

            yield return YieldInstructionCache.WaitForSeconds(0.02f);
        }

        showManualCoroutine = null;
    }

    public void HideManual()
    {
        if (closeManualCoroutine != null) return;

        if (showManualCoroutine != null)
        {
            StopCoroutine(showManualCoroutine);
            showManualCoroutine = null;
        }

        closeManualCoroutine = StartCoroutine(ManualFadeOut());
    }

    IEnumerator ManualFadeOut()
    {
        byte r = textColor.r;
        byte g = textColor.g;
        byte b = textColor.b;
        byte a = Convert.ToByte(_manual.color.a * 255f);

        while (a > 0)
        {
            if (a - 20 > 0)
            {
                a -= 20;
            }
            else a = 0;

            _manual.color = new Color32(r, g, b, a);

            yield return YieldInstructionCache.WaitForSeconds(0.02f);
        }

        closeManualCoroutine = null;
    }
}
