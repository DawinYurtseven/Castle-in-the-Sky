using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{
    public TextMeshProUGUI hudValues;

    private new void Awake()
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

    private IEnumerator MakeDecision()
    {
        //make actual decisions
        var random = Random.Range(0, BattleSystem.system.playerUnits.Count);
        SetCurrentTarget(new List<Unit> { BattleSystem.system.playerUnits[random] });
        yield return BasicAttack();
    }

    public void CalculateHUDValues(Button left = null, Button right = null)
    {
        hudValues.text = $"{name}\nHP: {CurrentHP}/{MaxHP}\nSP: {CurrentSP}/{MaxSP}";
        hudCanvas.gameObject.transform.LookAt(BattleSystem.system.battleCamera.gameObject.transform.position);
        var navigation = new Navigation();
        if (left != null)
        {
            navigation.selectOnLeft = left;
        }

        if (right != null)
        {
            navigation.selectOnRight = right;
        }

        selected.navigation = navigation;
    }

    protected override IEnumerator BasicAttack()
    {
        BattleSystem.system.ShowNewQueuePosition(this, CalculateTimeValue(1f));
        yield return new WaitForSeconds(0.2f);
        yield return base.BasicAttack();
        BattleSystem.system.AcceptNewQueuePosition(this, CalculateTimeValue(1f));
    }
}