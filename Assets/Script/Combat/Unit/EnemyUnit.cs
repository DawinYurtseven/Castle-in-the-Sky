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