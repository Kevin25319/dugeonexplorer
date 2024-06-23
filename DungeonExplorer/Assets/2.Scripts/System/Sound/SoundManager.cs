using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// lớp đơn có chức năng phát và quản lý âm thanh.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null; // lớp học đơn

    [SerializeField] AudioSource _music;
    [SerializeField] AudioSource _effect;

    float currentMusicVolume;   // âm lượng hiện tại
    AudioClip _musicClip;
    Coroutine _musicChange = null;

    void Awake()
    {
        // Đặt nó như một lớp đơn
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if(_music == null)
        {
            _music = transform.Find("Music").GetComponent<AudioSource>();
        }
        if(_effect == null)
        {
            _effect = transform.Find("Effect").GetComponent<AudioSource>();
        }

        _music.loop = true; // Âm nhạc luôn được đặt thành vòng lặp
    }

    /// <summary>
    /// Phương pháp này được sử dụng khi phát nhạc.
    /// Một coroutine thay đổi nhạc hiện đang phát sẽ được thực thi.
    /// </summary>
    /// <param name="audioClip">nhạc bạn muốn phát</param>
    /// <param name="volume">âm lượng bạn muốn phát</param>
    public void MusicPlay(AudioClip audioClip, float volume = 1.0f)
    {
        if(audioClip == null || _musicClip == audioClip) return;
        _musicClip = audioClip;

        // Nếu âm thanh đang được thay đổi, hãy ngừng thay đổi nhạc hiện có
        if (_musicChange != null)
        {
            StopCoroutine(_musicChange);
            _musicChange =null;
        }

        // Coroutine để thay đổi âm nhạc
        _musicChange = StartCoroutine(MusicChange(volume));
    }

    /// <summary>
    /// dừng phát nhạc hiện đang phát.
    /// </summary>
    public void MusicStop()
    {
        // Nếu nhạc đang được thay đổi, hãy ngừng thay đổi nhạc hiện có
        if (_musicChange != null)
        {
            StopCoroutine(_musicChange);
            _musicChange = null;
        }
        _musicClip = null;
        _musicChange = StartCoroutine(MusicChange(0));  // Đặt âm lượng nhạc thành 0
    }

    /// <summary>
    /// Phương pháp này lấy nhạc hiện đang phát.
    /// </summary>
    public AudioClip GetCurrentMusic() => _music.clip;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="setVolume"></param>
    /// <returns></returns>
    IEnumerator MusicChange(float setVolume)
    {
        // Lấy âm lượng hiện tại
        currentMusicVolume = SoundSettingsManager.musicVolume;

        float volume = _music.volume;               // Âm lượng phát trước đó
        float volumeReduction = volume * 0.015f;    // âm lượng nhạc

        // Âm lượng nhạc phát trước đó giảm dần.
        while (volume > 0f)
        {
            volume -= volumeReduction;
            _music.volume = Mathf.Clamp(volume, 0f, 1.0f);
            yield return YieldInstructionCache.WaitForSecondsRealtime(0.01f);
        }
        // dừng âm nhạc
        _music.Stop();

        // Thay đổi bản nhạc bạn muốn phát và sau đó phát nó
        _music.clip = _musicClip;
        _music.Play();
        
        // âm lượng nhạc tăng dần cho giống âm cài đặt
        float volumeIncrease = setVolume * 0.015f;
        while (volume < setVolume * currentMusicVolume)
        {
            volume += volumeIncrease;
            _music.volume = Mathf.Clamp(volume, 0f, setVolume * currentMusicVolume);
            yield return YieldInstructionCache.WaitForSecondsRealtime(0.01f);
        }

        // Dành cho khi cài đặt âm lượng trong khi thay đổi nhạc
        if (currentMusicVolume != SoundSettingsManager.musicVolume)
        {
            _music.volume = setVolume * SoundSettingsManager.musicVolume;
        }

        _musicChange = null;
    }

    /// <summary>
    /// phương pháp phát hiệu ứng âm thanh.
    /// </summary>
    /// <param name="audioClip">Hiệu ứng âm thanh bạn muốn phát</param>
    public void SoundEffectPlay(AudioClip audioClip)
    {
        // Hiệu ứng âm thanh chỉ phát một lần
        _effect.volume = SoundSettingsManager.effectsVolume;
        _effect.PlayOneShot(audioClip);
    }

    /// <summary>
    /// Phương pháp này điều chỉnh lại âm lượng của nhạc đang phát khi âm lượng được điều chỉnh trong các tùy chọn trong khi phát nhạc.
    /// </summary>
    public void MusicVolumeRefresh()
    {
        // nếu nhạc đang thay đổi thì ngưng nhạc hiện có
        if (_musicChange != null)
        {
            StopCoroutine(_musicChange);
            _musicChange = null;
        }
        _music.volume = currentMusicVolume * SoundSettingsManager.musicVolume;
    }
}
