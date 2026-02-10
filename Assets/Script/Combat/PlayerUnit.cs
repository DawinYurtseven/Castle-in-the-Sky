using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    [SerializeField] private Canvas playerActionCanvas;
    private TextMeshProUGUI HUDvalues;

    private new void Awake()
    {
        base.Awake();
        HUDvalues = hudCanvas.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    #endregion

    protected override IEnumerator BasicAttack(Unit enemy)
    {
        CalculateTimeValue(1f);
        yield return base.BasicAttack(enemy);
    }


    public override IEnumerator BeginningOfTurn()
    { 
        stateStack.Push(CombatState.Root);
        hudCanvas?.gameObject.SetActive(true);
        HUDvalues.text = $"{name}:  HP: {CurrentHP}/{MaxHP}   SP: {CurrentSP}/{MaxSP}";
        playerActionCanvas?.gameObject.SetActive(true);
        StartCoroutine(BattleSystem.system.MoveCamera(cameraTargets[0]));
        yield return base.BeginningOfTurn();
    }


    #region UI, Camera and Input
    
    public enum CombatState
    {
        Root,
        Skill,
        Inspect,
        TargetEnemy,
        TargetAlly,
    }

    public Stack<CombatState> stateStack = new ();

    public void Submit(Unit targetUnit)
    {
        //this will simulate the ui for now until I have actually implemented ui
        
        switch (stateStack.Peek())
        {
            case CombatState.Root:
                stateStack.Push(CombatState.TargetEnemy);
                BattleSystem.system.MoveCameraToIndex(1);
                break;
            case CombatState.Skill:
                SkillUsage(SkillTypes.Damage);
                break;
            case CombatState.Inspect:
                break;
            case CombatState.TargetEnemy:
                stateStack.Pop();
                switch (stateStack.Peek())
                {
                    case CombatState.Root:
                        StartCoroutine(BasicAttack(targetUnit));
                        
                        stateStack.Clear();
                        break;
                    case CombatState.Skill:
                        stateStack.Clear();
                        break;
                    case CombatState.Inspect:
                        
                        //inspect logic here for enemies
                        break;
                }
                break;
            case CombatState.TargetAlly:
                break;
        }
    }

    #endregion
}