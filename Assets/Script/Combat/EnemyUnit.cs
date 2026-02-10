using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : Unit
{


    public TextMeshProUGUI HUDvalues;
    public Button selected;


    private new void Awake()
    {
        base.Awake();
        HUDvalues = GetComponentInChildren<TextMeshProUGUI>(true);
        selected = GetComponentInChildren<Button>(true);
    }

    public override IEnumerator BeginningOfTurn()
    {
        yield return null;
        yield return base.BeginningOfTurn();
        Debug.Log("Beginning of turn of enemy");
        CalculateTimeValue(1f);
        EndTurn();
    }

    public void CalculateHUDValues(Button left = null,Button right = null)
    {
        HUDvalues.text = $"{name}\nHP: {CurrentHP}/{MaxHP}\nSP: {CurrentSP}/{MaxSP}";
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
