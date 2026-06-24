using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrandSlash : Skill
{
    public GrandSlash()
    {
        skillName = "Grand Slash";
        skillDescription = "Slash through all available enemies with Physical damage"; //TODO: physical damage?
        skillCost = 3;
        timeValue = 1.5f;
        target = SkillTarget.EnemyAll;
        affectValue =2.5f;
        animationName = "GrandSlash_Animation";
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
