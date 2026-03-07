using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    protected override IEnumerator BasicAttack()
    {
        BattleSystem.system.AcceptNewQueuePosition(this, TimeValue);
        yield return base.BasicAttack();
    }

    protected override IEnumerator SkillUsage()
    {
        yield return BattleSystem.system.MoveCameraToIndexTransform(0);
        if (!selectedSkill.targetOne)
        {
            var list = new List<Unit>();
            list.AddRange(BattleSystem.system.enemyUnits);
            SetCurrentTarget(list);
            yield return transform.DOMove(BattleSystem.system.inFrontOfEnemies.position, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        }
        
        
        //call animation with selectedSkill.animationName here but idk
        yield return new WaitForSeconds(0.3f);
        yield return base.SkillUsage();
    }

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        BattleSystem.system.UpdatePlayerValues(this);
    }


    public override IEnumerator BeginningOfTurn()
    { 
        yield return base.BeginningOfTurn();
        stateStack.Push(CombatState.Root);
        SetActionUI(true);
        yield return BattleSystem.system.MoveCamera(cameraTargets[0]);
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
        if (targetUnit != null)
        {
            SetCurrentTarget(new List<Unit>{targetUnit});
        }
        switch (stateStack.Peek())
        {
            case CombatState.Root:
                stateStack.Push(CombatState.TargetEnemy);
                BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(1f));
                SetActionUI(false);
                yield return BattleSystem.system.MoveCameraToIndexTransform(1);
                break;
            case CombatState.Skill:
                BattleSystem.system.SkillTabVisibility(false);
                if (selectedSkill.type == SkillTypes.Damage || selectedSkill.type == SkillTypes.Debuff)
                {
                    stateStack.Push(CombatState.TargetEnemy);
                    yield return BattleSystem.system.MoveCameraToIndexTransform(3);
                }
                else
                {
                    stateStack.Push(CombatState.TargetAlly);
                    yield return BattleSystem.system.MoveCameraToIndexTransform(4);
                }
                BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(selectedSkill.timeValue));
                break;
            case CombatState.Inspect:
                break;
            case CombatState.TargetEnemy:
                stateStack.Pop();
                switch (stateStack.Peek())
                {
                    case CombatState.Root:
                        stateStack.Clear();
                        yield return BasicAttack();
                        break;
                    case CombatState.Skill:
                        stateStack.Clear();
                        yield return SkillUsage();
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
                BattleSystem.system.SkillTabVisibility(false);
                selectedSkill = null;
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
        BattleSystem.system.SkillTabVisibility(true, skills, this);
        SetActionUI(false);
        yield return BattleSystem.system.MoveCamera(cameraTargets[1]);
    }

    public IEnumerator Inspect()
    {
        if (stateStack.Count > 1) yield break;
        stateStack.Push(CombatState.Inspect);
        stateStack.Push(CombatState.TargetEnemy);
        SetActionUI(false);
        yield return BattleSystem.system.MoveCameraToIndexTransform(1);
    }


    private void SetActionUI(bool active)
    {
        playerActionCanvas?.gameObject.SetActive(active);
    }

    #endregion
}