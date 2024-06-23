using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StageMusic : MonoBehaviour
{
    public AudioClip stageMusic;     // Âm thanh c?a giai ?o?n
    public float volume = 1.0f;      // Âm l??ng m?c ??nh

    void Start()
    {
        // N?u không có âm thanh giai ?o?n ???c ch? ??nh
        if (stageMusic == null)
        {
            SoundManager.instance.MusicStop(); // D?ng phát âm thanh
        }
        // Phát âm thanh giai ?o?n v?i âm l??ng và th?i l??ng ???c ch? ??nh
        SoundManager.instance.MusicPlay(stageMusic, volume);    
    }
}
