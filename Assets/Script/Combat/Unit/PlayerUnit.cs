using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerUnit : Unit
{
    #region Components

    /// for the cameraTargets from the unit base class
    /// <summary>
    /// 0- standard camera angle for when it is players turn
    /// 1- skill view
    /// 2- Enemy View
    /// maybe I'll need later more so this is an array now
    /// </summary>
    /// // this is for the camera to move to depending on the situation.

    [SerializeField] private PlayerCombatUiController playerCombatUiController;


    public List<Tuple<string, int>> GetStats()
    {
        List<Tuple<string, int>> stats = new List<Tuple<string, int>>();
        stats.Add(new Tuple<string, int>("Strength", strength));
        stats.Add(new Tuple<string, int>("Constitution", constitution));
        stats.Add(new Tuple<string, int>("Speed", speed));
        stats.Add(new Tuple<string, int>("Intelligence", intelligence));
        stats.Add(new Tuple<string, int>("Luck", luck));
        return stats;
    }

    public void IncreaseStat(string stat, int amount)
    {
        switch (stat)
        {
            case "Strength":
                strength += amount;
                break;
            case "Constitution":
                constitution += amount;
                break;
            case "Speed":
                speed += amount;
                break;
            case "Intelligence":
                intelligence += amount;
                break;
            case "Luck":
                luck += amount;
                break;
            default:
                Debug.Log("What?");
                break;
        }
    }

    public int GetStat(string stat)
    {
        switch (stat)
        {
            case "Strength":
                return strength;
            case "Constitution":
                return constitution;
            case "Speed":
                return speed;
            case "Intelligence":
                return intelligence;
            case "Luck":
                return luck;
            default:
                Debug.Log("What?");
                return 0;
        }
    }

    public void AddSkill(Skill skill)
    {
        var foundName = Skills.Find((e) => e.Equals(skill.name));
        if (foundName == SkillNames.none && skill.name != SkillNames.none)
            Skills.Add(skill.name);
    }

    #endregion

    protected override IEnumerator BasicAttack()
    {
        yield return base.BasicAttack();
    }

    protected override IEnumerator SkillUsage()
    {
        yield return BattleSystem.system.MoveCameraToIndexTransform(0);
        BattleSystem.system.ClearSelection(true);
        if (SelectedSkill.type == SkillTypes.Damage || SelectedSkill.type == SkillTypes.Debuff)
        {
            yield return transform.DOMove(BattleSystem.system.inFrontOfEnemies.position, 0.2f).SetEase(Ease.OutExpo)
                .WaitForCompletion();
        }

        //call animation with selectedSkill.animationName here but idk
        yield return new WaitForSeconds(0.3f);
        yield return base.SkillUsage();
        BattleSystem.system.UpdatePlayerValues(this);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        BattleSystem.system.UpdatePlayerValues(this);
    }


    public override IEnumerator BeginningOfTurn()
    {
        yield return base.BeginningOfTurn();
        stateStack.Push(CombatState.Root);
        yield return BattleSystem.system.MoveCamera(cameraTargets[0], BattleSystem.CameraTargets.Base);
        playerCombatUiController.SetVisibility(true);
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
    private readonly Stack<CombatState> stateStack = new();

    public IEnumerator Submit(Unit targetUnit)
    {
        //this will simulate the ui for now until I have actually implemented ui
        if (stateStack.Count == 0) yield break;
        if (targetUnit != null)
        {
            SetCurrentTarget(new List<Unit> { targetUnit });
        }

        switch (stateStack.Peek())
        {
            case CombatState.Root:
                stateStack.Push(CombatState.TargetEnemy);
                BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(1f));
                playerCombatUiController.SetVisibility(false);
                yield return BattleSystem.system.MoveCamera(cameraTargets[2], BattleSystem.CameraTargets.EnemyView);
                break;
            case CombatState.Skill:
                if (SelectedSkill.skillCost > CurrentSP) yield break;
                playerCombatUiController.SkillTabVisibility(false);
                if (SelectedSkill.type == SkillTypes.Damage || SelectedSkill.type == SkillTypes.Debuff)
                {
                    stateStack.Push(CombatState.TargetEnemy);
                    if (!SelectedSkill.targetOne)
                    {
                        var list = new List<Unit>();
                        list.AddRange(BattleSystem.system.enemyUnits);
                        SetCurrentTarget(list);
                    }
                    yield return BattleSystem.system.MoveCamera(cameraTargets[2], BattleSystem.CameraTargets.EnemyView);
                }
                else
                {
                    stateStack.Push(CombatState.TargetAlly);
                    if (!SelectedSkill.targetOne)
                    {
                        var list = new List<Unit>();
                        list.AddRange(BattleSystem.system.playerUnits);
                        SetCurrentTarget(list);
                        yield return BattleSystem.system.MoveCameraToIndexTransform(4);
                    }
                    else
                    {
                        yield return BattleSystem.system.MoveCamera(cameraTargets[3], BattleSystem.CameraTargets.PlayerView);
                    }

                }

                playerCombatUiController.SetVisibility(false);

                BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(SelectedSkill.timeValue));
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
                switch (stateStack.Peek())
                {
                    case CombatState.Root:
                        Debug.Log("oke oke");
                        break;
                    case CombatState.Skill:
                        stateStack.Clear();
                        yield return SkillUsage();
                        break;
                    case CombatState.Inspect:
                        break;
                    default:
                        Debug.Log("How?");
                        break;
                }

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
                playerCombatUiController.SkillTabVisibility(false);
                SelectedSkill = null;
                yield return BattleSystem.system.MoveCamera(cameraTargets[0], BattleSystem.CameraTargets.Base);
                playerCombatUiController.SetVisibility(true);
                break;
            case CombatState.Skill:
                BattleSystem.system.ClearSelection(!SelectedSkill.targetOne);
                BattleSystem.system.FreeNewQueuePosition();
                stateStack.Pop();
                yield return SkillTab();
                break;
            case CombatState.Inspect:
                BattleSystem.system.ClearSelection(true);
                yield return BattleSystem.system.MoveCamera(cameraTargets[0], BattleSystem.CameraTargets.Base);
                playerCombatUiController.SetVisibility(true);
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
        playerCombatUiController.SetVisibility(false);
        stateStack.Push(CombatState.Skill);
        yield return BattleSystem.system.MoveCamera(cameraTargets[1], BattleSystem.CameraTargets.Base);
        playerCombatUiController.SkillTabVisibility(true, cameraTargets[1], Skills, this);
        playerCombatUiController.SetVisibility(true);
        BattleSystem.system.SetCurrentSelectButton(playerCombatUiController.PeekFirstButton());
    }

    public IEnumerator Inspect()
    {
        var state = stateStack.Peek();
        if (state == CombatState.Root)
        {
            stateStack.Push(CombatState.Inspect);
            stateStack.Push(CombatState.TargetEnemy);
            playerCombatUiController.SetVisibility(false);
            yield return BattleSystem.system.MoveCamera(cameraTargets[2], BattleSystem.CameraTargets.FullView);
        }
        else if (state == CombatState.Skill)
        {
            BattleSystem.system.TriggerSpecificButtonAction();
        }
    }

    #endregion
}