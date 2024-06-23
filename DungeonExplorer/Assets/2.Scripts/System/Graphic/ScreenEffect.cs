using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenEffect : MonoBehaviour
{
    public static ScreenEffect instance = null; 

    float _magnitude;           
    float _shakeDuration;       
    float _bulletTimeScale;    
    float _bulletTimeDuration;  

    bool isTimeStopping;        

    Vector3 _cameraOriginPos;   
    Transform _cameraTransform; 

    Coroutine _shakeEffect = null;      
    Coroutine _bulletTimeEffect = null; 

    
    public delegate void StartShakeEventHandler();
    public StartShakeEventHandler StartShake;

    
    public delegate void EndShakeEventHandler();
    public EndShakeEventHandler EndShake;

    void Awake()
    {
        
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        _cameraTransform = Camera.main.GetComponent<Transform>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _cameraTransform = Camera.main.GetComponent<Transform>();
        StartShake = null;
        EndShake = null;
    }

    
    public void ShakeEffectStart(float magnitude, float duration)
    {
        
        if(!AccessibilitySettingsManager.screenShake) return;

       
        if(_shakeEffect != null)
        {
            if(_shakeDuration > duration) return;
            _cameraTransform.position = _cameraOriginPos;
            StopCoroutine(_shakeEffect);
            _shakeEffect = null;
        }

        
        _magnitude = magnitude;
        _shakeDuration = duration;
        _shakeEffect = StartCoroutine(ShakeEffectCoroutine());
    }

    
    
    public void ShakeEffectStop()
    {
        if(_shakeEffect == null) return;
        _cameraTransform.position = _cameraOriginPos;
        StopCoroutine(_shakeEffect);
        EndShake();
        _shakeEffect = null;
    }

    IEnumerator ShakeEffectCoroutine()
    {
        Vector3 setPos;

        _cameraOriginPos = _cameraTransform.position;
        StartShake();   

        
        while (_shakeDuration > 0)
        {
            float shakePosX = Random.Range(-1f, 1f) * _magnitude;
            float shakePosY = Random.Range(-1f, 1f) * _magnitude;

            setPos = _cameraTransform.position;
            setPos.x += shakePosX;
            setPos.y += shakePosY;
            _cameraTransform.position = setPos;

            yield return YieldInstructionCache.WaitForSecondsRealtime(0.012f);
            _shakeDuration -= 0.012f;

            _cameraTransform.position = _cameraOriginPos;

            yield return YieldInstructionCache.WaitForSecondsRealtime(0.012f);
            _shakeDuration -= 0.012f;
        }

        _cameraTransform.position = _cameraOriginPos;
        EndShake(); 
        _shakeEffect = null;
    }

    
    public void TimeStopStart()
    {
        BulletTimeStop();
        isTimeStopping = true;
        Time.timeScale = 0;
    }

    
    public void TimeStopCancle()
    {
        isTimeStopping = false;
        Time.timeScale = 1;
    }

    
    public void BulletTimeStart(float timeScale, float duration)
    {
        
        if(isTimeStopping) return;

        
        if (_bulletTimeEffect != null)
        {

            StopCoroutine(_bulletTimeEffect);
            _bulletTimeEffect = null;
        }

        
        _bulletTimeDuration = duration;
        _bulletTimeScale = timeScale;
        _bulletTimeEffect = StartCoroutine(BulletTimeEffect());
    }

    
    public void BulletTimeStop()
    {
        if(_bulletTimeEffect == null) return;

        StopCoroutine(_bulletTimeEffect);
        _bulletTimeEffect = null;

        Time.timeScale = 1f;
    }

    
    IEnumerator BulletTimeEffect()
    {
        Time.timeScale = _bulletTimeScale;
        yield return YieldInstructionCache.WaitForSecondsRealtime(_bulletTimeDuration);
        Time.timeScale = 1f;
        _bulletTimeEffect = null;
    }
}
