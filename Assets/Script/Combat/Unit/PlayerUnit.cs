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
    /// 0- Animation handle, not for playerUnit, but for Unit to use
    /// 1- standard camera angle for when it is players turn
    /// 2- skill view
    /// 3- skill view right
    /// 4- Enemy View
    /// 5- Inspect
    /// maybe I'll need later more so this is an array now
    /// </summary>
    /// // this is for the camera to move to depending on the situation.

    private int cameraPosition;

    [SerializeField] private PlayerCombatUiController playerCombatUiController;


    public List<Tuple<string, int>> GetStats()
    {
        List<Tuple<string, int>> stats = new List<Tuple<string, int>>
        {
            new ("Strength", strength),
            new ("Constitution", constitution),
            new ("Speed", speed),
            new ("Intelligence", intelligence),
            new ("Luck", luck)
        };
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

    public void AddSkill(Skill skill, int i = -1)
    {
        if (i != -1 && i < Skills.Count)
        {
            Skills.RemoveAt(i);
            Skills.Insert(i, skill);
        }
        var foundName = Skills.Find((e) => e.name.Equals(skill.name));
        if (foundName.name == SkillNames.none && skill.name != SkillNames.none)
            Skills.Add(skill);
    }

    #endregion

    public override void BeginningOfCombat()
    {
        base.BeginningOfCombat();
        playerCombatUiController.SetButtonInfos(Skills, this);
    }

    protected override IEnumerator BasicAttack()
    {
        inAnim = true;
        yield return base.BasicAttack();
        inAnim = false;
    }

    protected override IEnumerator SkillUsage()
    {
        yield return BattleSystem.system.MoveCamera(null, BattleSystem.CameraTargets.Empty);
        BattleSystem.system.ClearSelection(true);
        if (SelectedSkill.type == SkillTypes.Damage || SelectedSkill.type == SkillTypes.Debuff)
        {
            yield return transform.DOMove(BattleSystem.system.inFrontOfEnemies.position, 0.2f).SetEase(Ease.OutExpo)
                .WaitForCompletion();
        }

        //call animation with selectedSkill.animationName here but IDK
        yield return new WaitForSeconds(0.3f);
        yield return base.SkillUsage();
        BattleSystem.system.UpdatePlayerValues(this);
    }

    protected override IEnumerator EndTurn()
    {
        isTurn = false;
        return base.EndTurn();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        BattleSystem.system.UpdatePlayerValues(this);
    }


    public override IEnumerator BeginningOfTurn()
    {
        yield return base.BeginningOfTurn();
        BattleSystem.system.UpdatePlayerValues(this);
        stateStack.Push(CombatState.Root);
        yield return BattleSystem.system.MoveCamera(cameraTargets[1], BattleSystem.CameraTargets.Base);
        cameraPosition = 1;
        playerCombatUiController.SetVisibility(true);
        isTurn = true;
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
    /// too late, I wanna punch you for the fuckery you done.
    /// I HAVE TO MAKE SURE WHAT GOES TO WHAT NOW, YOU STUPID FUCK!!!
    /// 
    /// </summary>
    private readonly Stack<CombatState> stateStack = new();

    private bool inAnim, isTurn;

    public IEnumerator Submit(Unit targetUnit)
    {
        //this will simulate the ui for now until I have actually implemented ui
        if ( inAnim || !isTurn || stateStack.Count == 0) yield break;
        if (targetUnit)
        {
            SetCurrentTarget(new List<Unit> { targetUnit });
        }

        inAnim = true;
        switch (stateStack.Peek())
        {
            case CombatState.Root:
                stateStack.Push(CombatState.TargetEnemy);
                BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(1f));
                playerCombatUiController.SetVisibility(false);
                yield return BattleSystem.system.MoveCamera(cameraTargets[4], BattleSystem.CameraTargets.EnemyView);
                cameraPosition = 4;
                break;
            case CombatState.Skill:
                if (SelectedSkill.skillCost > currentSP) yield break;
                playerCombatUiController.SkillTabVisibility(false);
                if (SelectedSkill.type is SkillTypes.Damage or SkillTypes.Debuff)
                {
                    stateStack.Push(CombatState.TargetEnemy);
                    if (!SelectedSkill.targetOne)
                    {
                        var list = new List<Unit>();
                        list.AddRange(BattleSystem.system.enemyUnits);
                        SetCurrentTarget(list);
                        BattleSystem.system.ClearSelection();
                    }
                    yield return BattleSystem.system.MoveCamera(cameraTargets[4], BattleSystem.CameraTargets.EnemyView, true);
                    cameraPosition = 4;
                }
                else
                {
                    stateStack.Push(CombatState.TargetAlly);
                    if (!SelectedSkill.targetOne)
                    {
                        var list = new List<Unit>();
                        list.AddRange(BattleSystem.system.playerUnits);
                        SetCurrentTarget(list);
                        yield return BattleSystem.system.MoveCamera(null, BattleSystem.CameraTargets.PlayerView, true);
                        BattleSystem.system.ClearSelection();
                    }
                    else
                    {
                        yield return BattleSystem.system.MoveCamera(cameraTargets[4], BattleSystem.CameraTargets.PlayerView);
                        cameraPosition = 4;
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
                stateStack.Pop();
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
        inAnim = false;
    }

    public IEnumerator Cancel()
    {
        if (inAnim|| !isTurn || stateStack.Count < 2 ) yield break;
        stateStack.Pop();
        switch (stateStack.Peek())
        {
            case CombatState.Root:
                BattleSystem.system.ClearSelection();
                BattleSystem.system.FreeNewQueuePosition();
                playerCombatUiController.SkillTabVisibility(false);
                SelectedSkill = null;
                yield return BattleSystem.system.MoveCamera(cameraTargets[1], BattleSystem.CameraTargets.Base);
                cameraPosition = 1;
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
                yield return BattleSystem.system.MoveCamera(cameraTargets[1], BattleSystem.CameraTargets.Base);
                cameraPosition = 1;
                playerCombatUiController.SetVisibility(true);
                stateStack.Pop();
                break;
            case CombatState.TargetEnemy:
                break;
            case CombatState.TargetAlly:
                break;
        }
        inAnim = false;
    }

    public IEnumerator SkillTab()
    {
        if ( inAnim|| !isTurn || stateStack.Peek() != CombatState.Root) yield break;
        inAnim = true;
        playerCombatUiController.SetVisibility(false);
        yield return BattleSystem.system.MoveCamera(cameraTargets[2], BattleSystem.CameraTargets.Base);
        cameraPosition = 2;
        playerCombatUiController.SkillTabVisibility(true, cameraTargets[2], this);
        playerCombatUiController.SetVisibility(true);
        BattleSystem.system.SetCurrentSelectButton(playerCombatUiController.PeekFirstButton());
        stateStack.Push(CombatState.Skill);
        inAnim = false;
    }

    public IEnumerator Inspect()
    {
        if(inAnim|| !isTurn) yield break;
        inAnim = true;
        var state = stateStack.Peek();
        if (state == CombatState.Root)
        {
            stateStack.Push(CombatState.Inspect);
            stateStack.Push(CombatState.TargetEnemy);
            playerCombatUiController.SetVisibility(false);
            yield return BattleSystem.system.MoveCamera(cameraTargets[5], BattleSystem.CameraTargets.FullView);
            cameraPosition = 5;
        }
        else if (state == CombatState.Skill)
        {
            BattleSystem.system.TriggerSpecificButtonAction();
        }
        inAnim = false;
    }

    
    public IEnumerator TabFunctionality()
    {
        if (inAnim|| !isTurn) yield break;
        inAnim = true;
        var state = stateStack.Peek();
        if (state == CombatState.Skill && Skills.Count > 3)
        {
            // do some change camera position and enable other skill tab.
            bool left = cameraPosition == 2;
            yield return BattleSystem.system.MoveCamera(cameraTargets[left ? 3 : 2], BattleSystem.CameraTargets.Base);
            playerCombatUiController.SwitchSkillSide(left, cameraTargets[left ? 3 : 2], this);
            cameraPosition = left ? 3 : 2;
        }
        yield return null;
        inAnim = false;
    }

    #endregion
}