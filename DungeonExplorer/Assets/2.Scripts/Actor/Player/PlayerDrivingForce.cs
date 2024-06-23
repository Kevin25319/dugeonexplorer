using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerDrivingForce : MonoBehaviour
{
    readonly int hashFill = Animator.StringToHash("Fill");
    readonly int hashFilled = Animator.StringToHash("Filled");
    readonly int hashUse = Animator.StringToHash("Use");

    public int maxDrivingForce = 8; 
    int _currentDrivingForce;      

    
    [System.Serializable]
    struct DrivingForceUI
    {
        [HideInInspector] public Image image;
        [HideInInspector] public Animator animator;
    }
    DrivingForceUI[] _drivingForceUI;

    public int CurrentDrivingForce
    {
        get => _currentDrivingForce;
        set
        {
            
            _currentDrivingForce = value;
            GameManager.instance.playerCurrentDrivingForce = value;
            for (int i = 0; i < _currentDrivingForce; i++)
            {
                _drivingForceUI[i].animator.SetTrigger(hashFill);
            }
        }
    }

    void Awake()
    {
        _currentDrivingForce = 0;

        
        GameObject drivingForceUI = GameObject.Find("DrivingForce");
        int drivingForceCount = drivingForceUI.transform.childCount;
        _drivingForceUI = new DrivingForceUI[drivingForceCount];
        for (int i = 0; i < _drivingForceUI.Length; i++)
        {
            Transform drivingForce = drivingForceUI.transform.GetChild(i);

            _drivingForceUI[i].image = drivingForce.GetComponent<Image>();
            _drivingForceUI[i].animator = drivingForce.GetComponent<Animator>();
        }
    }

   
    public void IncreaseDrivingForce()
    {
       
        if(_currentDrivingForce == maxDrivingForce) return;

       
        _currentDrivingForce++;
        _drivingForceUI[_currentDrivingForce - 1].animator.SetTrigger(hashFilled);

        
        GameManager.instance.playerCurrentDrivingForce = _currentDrivingForce;
    }

    
    /// <param name="drivingForce"></param>
    public bool TryConsumeDrivingForce(int drivingForce)
    {
        
        if(_currentDrivingForce < drivingForce) return false;

       
        int prevDrivingForce = _currentDrivingForce;
        _currentDrivingForce -= drivingForce;

        
        for(int i = prevDrivingForce - 1; i >= prevDrivingForce - drivingForce; i--)
        {
            _drivingForceUI[i].animator.SetTrigger(hashUse);
        }

       
        GameManager.instance.playerCurrentDrivingForce = _currentDrivingForce;

        return true;
    }
}