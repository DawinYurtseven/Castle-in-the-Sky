using System.Collections;
using UnityEngine;

public class PlayerUnit : Unit
{
    #region Components

    /// <summary>
    /// 0- standard camera angle for when it is players turn
    /// 1- view towards the enemies
    /// maybe I'll need later more so this is an array now
    /// </summary>
    [SerializeField] private Transform[] cameraTargets; // this is for the camera to move to depending on the situation.

    [SerializeField] private BattleUIPanel rootPanel;
    
    #endregion

    public override void BasicAttack()
    {
        CalculateTimeValue(1f);
        base.BasicAttack();
    }


    public override IEnumerator BeginningOfTurn()
    {
        yield return null;
        currentState = CombatState.Root;
        StartCoroutine(BattleSystem.system.MoveCamera(cameraTargets[0]));
        yield return base.BeginningOfTurn();
    }


    #region UI and Camera

    public enum CombatState
    {
        Root,
        Skill,
        Inspect,
        TargetEnemy,
        TargetAlly,
    }

    public CombatState currentState,preState;

    public void Submit()
    {
        
        //this will simulate the ui for now until I have actually implemented ui
        
        switch (currentState)
        {
            case CombatState.Root:
                currentState = CombatState.TargetEnemy;
                preState = CombatState.Root;
                break;
            case CombatState.Skill:
                SkillUsage(SkillTypes.Damage);
                break;
            case CombatState.Inspect:

                break;
            case CombatState.TargetEnemy:
                switch (preState)
                {
                    case CombatState.Root:
                        BasicAttack();
                        break;
                }
                currentState = CombatState.Root;
                break;
            case CombatState.TargetAlly:
                break;
        }
    }

    #endregion
}