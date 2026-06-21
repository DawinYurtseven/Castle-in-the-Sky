using System.Collections;
using UnityEngine;

[System.Serializable]
public class HealAll : Skill
{
    public HealAll()
    {
        skillName = "Heal All";
        skillDescription = "Heal all allies with a flat increase"; //TODO: physical damage?
        skillCost = 5;
        timeValue = 2f;
        target = SkillTarget.AllyAll;
        affectValue = 2f;
        animationName = "HealAll_Animation";
        turnEffect = 0;
        additionalCritChance = 10;
        additionalCritAddition = 4;
    }

    public override bool Execute(Unit unit)
    {
        bool validAction = false;
        var baseHeal = affectValue * unit.Intelligence;
        var totalHeal = Random.Range(0, 100) < unit.critChance + additionalCritChance
            ? baseHeal * ((unit.critAmount + additionalCritAddition) / 100)
            : baseHeal;
        
        //TODO: maybe not take damage call for healing?
        foreach (var units in unit.currentTarget)
        {
            units.TakeDamage(-totalHeal);
            validAction = units.currentHP == units.maxHP || validAction;
        }
        
        return validAction;
    }
}
