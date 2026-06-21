using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public class ApplyBurn : Skill
{

    public ApplyBurn()
    {
        skillName = "Apply Burn";
        skillDescription = "deal a small amount of damage that burns the target for 3 turns at the end of your end step";
        skillCost = 4;
        timeValue = 0.6f;
        target = SkillTarget.Enemy;
        affectValue = 0.1f;
        animationName = "applyBurn_Animation";
        turnEffect = 3;
        additionalCritChance = 20;
        additionalCritAddition = 10;
    }
    
    public override bool Execute(Unit unit)
    {
        var validAction = false;
        var baseDamage = (unit.Strength + unit.damageAddition) * unit.damageMultiplier;
        var totalDamage = Random.Range(0, 100) < unit.critChance + additionalCritChance
            ? baseDamage * ((unit.critAmount + additionalCritAddition) / 100)
            : baseDamage;
        
        Debug.Log("Never had a choice, never let the opps win" +
                  "\nCalm them nerves, got the whole world watching" +
                  "\nReady, set, go, and there ain't no stopping" +
                  "\nGot one option" +
                  "\nLight it up");
        
        foreach (var t in unit.currentTarget)
        {
            t.TakeDamage(totalDamage);
            if (t.currentHP <= 0) continue;
            validAction = true;
            var turnsRemaining = turnEffect;
            UnityAction<Unit> burn = null; 
                
            burn = targetUnit =>
            {
                Debug.Log("EVERYTHING BURNS!!!\nLight it up, let's go, light it up, let's go");
                baseDamage *= affectValue;
                totalDamage = Random.Range(0, 100) < unit.critChance + additionalCritChance
                    ? baseDamage * ((unit.critAmount + additionalCritAddition) / 100)
                    : baseDamage;
                targetUnit.TakeDamage(totalDamage);
                turnsRemaining--;
                if (turnsRemaining <= 0)
                {
                    t.EndOfTurnTrigger -= burn;
                }
            };
            t.EndOfTurnTrigger += burn;
        }
        
        
        return validAction;
    }
}
