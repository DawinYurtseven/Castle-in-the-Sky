using System.Collections;
using System.Collections.Generic;
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

    private new void Awake()
    {
        base.Awake();
    }

    #endregion

    protected override IEnumerator BasicAttack(Unit enemy)
    {
        CalculateTimeValue(1f);
        yield return base.BasicAttack(enemy);
        yield return EndTurn();
    }

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        BattleSystem.system.UpdatePlayerValues(this);
    }


    public override IEnumerator BeginningOfTurn()
    { 
        stateStack.Push(CombatState.Root);
        SetActionUI(true);
        yield return BattleSystem.system.MoveCamera(cameraTargets[0]);
        yield return base.BeginningOfTurn();
    }


    #region UI, Camera and Input
    
    private enum CombatState
    {
        Root,
        Skill,
        Inspect,
        TargetEnemy,
        TargetAlly,
    }

    /// <summary>
    /// TODO: make a visualization of how deep the stack can go
    /// so you don't have to fight the urge to punch the current me
    /// 
    /// </summary>
    private readonly Stack<CombatState> stateStack = new ();

    public IEnumerator Submit(Unit targetUnit)
    {
        //this will simulate the ui for now until I have actually implemented ui
        if (stateStack.Count == 0) yield break; 
        
        switch (stateStack.Peek())
        {
            case CombatState.Root:
                stateStack.Push(CombatState.TargetEnemy);
                BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(1f));
                SetActionUI(false);
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

    public IEnumerator Cancel()
    {
        if (stateStack.Count < 2) yield break;
        stateStack.Pop();
        switch (stateStack.Peek())
        {
            case CombatState.Root:
                BattleSystem.system.ClearSelection();
                BattleSystem.system.FreeNewQueuePosition();
                yield return BattleSystem.system.MoveCamera(cameraTargets[0]);
                SetActionUI(true);
                break;
            case CombatState.Skill:
                BattleSystem.system.ClearSelection();
                BattleSystem.system.FreeNewQueuePosition();
                stateStack.Pop();
                yield return SkillTab();
                break;
            case CombatState.Inspect:
                BattleSystem.system.ClearSelection();
                yield return BattleSystem.system.MoveCamera(cameraTargets[0]);
                SetActionUI(true);
                stateStack.Pop();
                break;
            case CombatState.TargetEnemy:
                break;
            case CombatState.TargetAlly:
                break;
        }
    }

    public IEnumerator SkillTab()
    {
        if (stateStack.Peek() != CombatState.Root) yield break;
        stateStack.Push(CombatState.Skill);
        SetActionUI(false);
        yield return BattleSystem.system.MoveCamera(cameraTargets[1]);
        Debug.Log("SkillTab");
    }

    public IEnumerator Inspect()
    {
        if (stateStack.Count > 1) yield break;
        stateStack.Push(CombatState.Inspect);
        stateStack.Push(CombatState.TargetEnemy);
        SetActionUI(false);
        BattleSystem.system.MoveCameraToIndex(1);
        yield return null;
    }


    private void SetActionUI(bool active)
    {
        playerActionCanvas?.gameObject.SetActive(active);
    }

    #endregion
}