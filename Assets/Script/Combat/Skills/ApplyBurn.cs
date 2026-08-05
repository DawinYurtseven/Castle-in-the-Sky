using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ApplyBurn : Skill
{
    public override bool Execute(Unit unit)
    {
        var validAction = false;
        var baseDamage = (1 + boost / 10f) * (unit.Strength + unit.damageAddition) * unit.damageMultiplier;
        var totalDamage = Random.Range(0, 100) < unit.critChance + additionalCritChance
            ? baseDamage * (1 + (unit.critAmount + additionalCritAddition) / 100)
            : baseDamage;

        Debug.Log("Never had a choice, never let the opps win" +
                  "\nCalm them nerves, got the whole world watching" +
                  "\nReady, set, go, and there ain't no stopping" +
                  "\nGot one option" +
                  "\nLight it up");

        unit.currentTarget.TakeDamage(totalDamage);
        if (unit.currentTarget.currentHP <= 0) return validAction;
        validAction = true;
        totalDamage *= (1 + boost / 10f) * affectValue;
        var burn = new Burn
        {
            turnsRemaining = turnEffect,
            baseValue = totalDamage
        };
        burn.Subscribe(ref unit.currentTarget.EndOfTurnTrigger, unit.currentTarget);


        return validAction;
    }
}