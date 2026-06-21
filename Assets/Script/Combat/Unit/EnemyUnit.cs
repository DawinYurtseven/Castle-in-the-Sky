using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
    internal override void Awake()
    {
        base.Awake();
        hudValues = GetComponentInChildren<TextMeshProUGUI>(true);
        selected = GetComponentInChildren<Button>(true);
    }

    public override void BeginningOfCombat()
    {
        ScaleStats();
        base.BeginningOfCombat();
    }
    
    public int Level { get; set; }

    internal void ScaleStats()
    {
        Strength = Mathf.CeilToInt(strCurve * Level);
        Constitution = Mathf.CeilToInt(conCurve * Level);
        Speed = Mathf.CeilToInt(spdCurve * Level);
        Intelligence = Mathf.CeilToInt(intCurve * Level);
        Luck = Mathf.CeilToInt(lckCurve * Level);
    }

    /// <summary>
    /// we want to make something like a scaling. So that when they are instantiated, given with a level,
    /// scale to the appropriate state. This can be either with skills they have to unlock at a certain level and/or
    /// stats that scale appropriately.
    ///
    /// I am not sure if I want to make it depending on the depth of the node it is on, or on the strength of the current party.
    ///
    /// for now, I will define something called a scaling curve for each stat. It will be multiplied with the depth of the node
    /// and ceil to get an int for the stat. there can be additional bonuses on these stats depending on the style of node
    /// they are spawned from. 
    /// </summary>
    
    
    
    
    public override IEnumerator BeginningOfTurn()
    {
        yield return base.BeginningOfTurn();
        yield return new WaitForSeconds(0.3f);
        yield return MakeDecision();
    }

    private enum EnemyActions
    {
        Prep,
        BasicAttack,
        Skill,
        Defend,
    }

    //TODO: make a buffer that can change on later steps but not the immediate next step
    //and make a proper make decision once you think you can implement behaviour trees
    private IEnumerator MakeDecision()
    {
        //make actual decisions
        var random = Random.Range(0, BattleSystem.system.playerUnits.Count);
        SetCurrentTarget(new List<Unit> { BattleSystem.system.playerUnits[random] });
        yield return BasicAttack();
    }

    protected override IEnumerator BasicAttack()
    {
        BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(1f));
        yield return new WaitForSeconds(0.2f);
        yield return base.BasicAttack();
        BattleSystem.system.AcceptNewQueuePosition(this, CalculateTimeValue(1f));
    }
}