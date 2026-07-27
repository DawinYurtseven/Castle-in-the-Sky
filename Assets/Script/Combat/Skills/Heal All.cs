using System.Collections;
using UnityEngine;

[System.Serializable]
public class HealAll : Skill
{

    public override bool Execute(Unit unit)
    {
        bool validAction = false;
        var baseHeal = (1+boost/10f) * affectValue * unit.Intelligence;
        var totalHeal = Random.Range(0, 100) < unit.critChance + additionalCritChance
            ? baseHeal * ((unit.critAmount + additionalCritAddition) / 100)
            : baseHeal;
        
        //TODO: maybe not take damage call for healing?
        foreach (var units in unit.currentTargets)
        {
            units.TakeDamage(-totalHeal);
            validAction = units.currentHP == units.maxHP || validAction;
        }
        
        return validAction;
    }
}
