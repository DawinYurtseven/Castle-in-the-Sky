using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerUnit : Unit
{
    public override void BasicAttack()
    {
        CalculateTimeValue(1f);
        base.BasicAttack();
    }

    public override void BeginningOfCombat(UnityEvent<Unit, float> e)
    {
        base.BeginningOfCombat(e);
    }

    public override IEnumerator BeginningOfTurn()
    {
        yield return null;
        currentState = combatState.root;
        yield return base.BeginningOfTurn();
    }

    public override void SkillUsage(SkillTypes type)
    {
        base.SkillUsage(type);
    }

    #region UI and Camera

    public enum combatState
    {
        root,
        skill,
        inspect,
        targetEnemy,
        targetAlly,
    }

    public combatState currentState,preState;

    public void Submit()
    {
        
        //this will simulate the ui for now until I have actually implemented ui
        
        switch (currentState)
        {
            case combatState.root:
                currentState = combatState.targetEnemy;
                preState = combatState.root;
                break;
            case combatState.skill:
                SkillUsage(SkillTypes.damage);
                break;
            case combatState.inspect:

                break;
            case combatState.targetEnemy:
                switch (preState)
                {
                    case combatState.root:
                        BasicAttack();
                        break;
                }
                currentState = combatState.root;
                break;
            case combatState.targetAlly:
                break;
        }
    }

    #endregion
}