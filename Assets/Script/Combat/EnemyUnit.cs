using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{


    public TextMeshProUGUI HUDValues;
    


    private new void Awake()
    {
        base.Awake();
        HUDValues = GetComponentInChildren<TextMeshProUGUI>(true);
        selected = GetComponentInChildren<Button>(true);
    }

    public override IEnumerator BeginningOfTurn()
    {
        yield return null;
        yield return base.BeginningOfTurn();
        Debug.Log("Beginning of turn of enemy");
        yield return MakeDecision();
        yield return EndTurn();
    }
    
    private enum EnemyState
    {
        Prep,
        BasicAttack,
        Skill,
        Defend,
    }

    private IEnumerator MakeDecision()
    {
        //make actual decisions
        var random = Random.Range(0, BattleSystem.system.playerUnits.Length);
        yield return StartCoroutine(BasicAttack(BattleSystem.system.playerUnits[random]));
        CalculateTimeValue(1f);
    }

    public void CalculateHUDValues(Button left = null,Button right = null)
    {
        HUDValues.text = $"{name}\nHP: {CurrentHP}/{MaxHP}\nSP: {CurrentSP}/{MaxSP}";
        hudCanvas.gameObject.transform.LookAt(BattleSystem.system.battleCamera.gameObject.transform.position);
        var navigation = selected.navigation;
        if(left != null)
        {
            navigation.selectOnLeft = left;
        }
        if(right != null)
        {
            navigation.selectOnRight = right;
            
        }
        selected.navigation = navigation;
    }

    
}
