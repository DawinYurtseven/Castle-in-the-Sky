using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrandSlash : Skill
{
    public GrandSlash()
    {
        name = SkillNames.GrandSlash;
        skillName = "Grand Slash";
        skillDescription = "Slash through all available enemies with Physical damage"; //TODO: physical damage?
        skillCost = 3;
        timeValue = 1.5f;
        type = SkillTypes.Damage;
        targetOne = false;
        affectValue =1.5f;
        animationName = "GrandSlash_Animation";
        userTargetPoint = 0;
        turnEffect = 0;
        additionalCritChance = 10;
        additionalCritAddition = 4;
    }

    public override bool Execute(Unit unit)
    {
        bool validAction = false;
        var baseDamage = affectValue * (unit.Strength + unit.damageAddition) * unit.damageMultiplier;
        var totalDamage = Random.Range(0, 100) < unit.critChance + additionalCritChance
            ? baseDamage * ((unit.critAmount + additionalCritAddition) / 100)
            : baseDamage;

        //TODO: name your parameters ALL BETTER!
        List<Unit> targetList = new ();
        targetList.AddRange(unit.currentTarget);
                    
        foreach (var targetUnit in unit.currentTarget)
        {
            targetUnit.TakeDamage(totalDamage);
            if (targetUnit.currentHP > 0)
            {
                validAction = true;
            }
            else
            {
                targetList.Remove(unit);
            }
        }
        Debug.Log("GrandSlash and HEEEEEELP!!!!");
        
        return validAction;
    }
}
