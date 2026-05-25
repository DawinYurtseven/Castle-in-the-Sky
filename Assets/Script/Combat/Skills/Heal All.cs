using System.Collections;
using UnityEngine;

public class HealAll : Skill
{
    public HealAll()
    {
        name = SkillNames.HealAll;
        skillName = "Heal All";
        skillDescription = "Heal all allies with a flat increase"; //TODO: physical damage?
        skillCost = 5;
        timeValue = 2f;
        type = SkillTypes.Heal;
        targetOne = false;
        affectValue = 2f;
        animationName = "HealAll_Animation";
        userTargetPoint = 0;
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
            validAction = units.CurrentHP == units.MaxHP || validAction;
        }
        
        return validAction;
    }
}
