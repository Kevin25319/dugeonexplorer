using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnSkillsObject : MonoBehaviour
{
    
    public enum Skills
    {
        ClimbingWall,
        DoubleJump
    }
    public SkillLearnEffect skillLearnEffect;   
    public Skills skill;    
    public float radius;    
    Animator _anim;
    LayerMask _playerLayer;

    void Awake()
    {
        
        switch(skill)
        {
            case Skills.ClimbingWall:
                if(PlayerLearnedSkills.hasLearnedClimbingWall)
                {
                    Destroy(gameObject);
                    return;
                }
                break;
            case Skills.DoubleJump:
                if(PlayerLearnedSkills.hasLearnedDoubleJump)
                {
                    Destroy(gameObject);
                    return;
                }
                break;
        }
        _playerLayer = LayerMask.GetMask("Player");
        _anim = GetComponent<Animator>();
        _anim.SetTrigger(skill.ToString());
    }

    void Update()
    {
        Vector2 pos = transform.position;
        pos.y += 1.25f;
        
        if(Physics2D.OverlapCircle(pos, radius, _playerLayer))
        {
            _anim.SetTrigger("Empty");
            switch (skill)
            {
                case Skills.ClimbingWall:
                    PlayerLearnedSkills.hasLearnedClimbingWall = true;
                    skillLearnEffect.SkillLearnEffectStart("Climb");
                    break;
                case Skills.DoubleJump:
                    PlayerLearnedSkills.hasLearnedDoubleJump = true;
                    skillLearnEffect.SkillLearnEffectStart("DoubleJump");
                    break;
            }
            
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector2 pos = transform.position;
        pos.y += 1.25f;
        Gizmos.DrawWireSphere(pos, radius);
    }
}