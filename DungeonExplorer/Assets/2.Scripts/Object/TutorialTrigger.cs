using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public TutorialManager.Tutorial tutorial; 
    public TutorialScreen tutorialScreen;     
    public Vector2 size;    
    LayerMask _playerLayer;

    void Awake()
    {
        
        if(TutorialManager.HasSeenTutorial(tutorial))
        {
            Destroy(gameObject);
            return;
        }
        _playerLayer = LayerMask.GetMask("Player");
    }

    void Update()
    {
        
        Vector2 pos = transform.position;
        if(Physics2D.OverlapBox(pos, size, 0, _playerLayer))
        {
            TutorialManager.AddSeenTutorial(tutorial);
            tutorialScreen.TotorialStart(tutorial);
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 pos = transform.position;
        Gizmos.DrawWireCube(pos, size);
    }
}