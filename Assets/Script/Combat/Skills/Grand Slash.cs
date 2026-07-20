using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrandSlash : Skill
{

    public override bool Execute(Unit unit)
    {
        bool validAction = false;
        var baseDamage = (1+boost/10f) *affectValue * (unit.Strength + unit.damageAddition) * unit.damageMultiplier;
        var totalDamage = Random.Range(0, 100) < unit.critChance + additionalCritChance
            ? baseDamage * ((unit.critAmount + additionalCritAddition) / 100)
            : baseDamage;
                    
        foreach (var targetUnit in unit.currentTarget)
        {
            targetUnit.TakeDamage(totalDamage);
            if (targetUnit.currentHP > 0)
            {
                validAction = true;
            }
        }
        Debug.Log("GrandSlash and HEEEEEELP!!!!");
        
        return validAction;
    }
}
