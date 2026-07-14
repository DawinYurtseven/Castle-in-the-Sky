using UnityEngine;

public class Burn : StatusEffect
{
    protected override void Effect(Unit unit)
    {
        Debug.Log("EVERYTHING BURNS!!!\nLight it up, let's go, light it up, let's go");
        unit.TakeDamage(baseValue);
        turnsRemaining--;
        if (turnsRemaining <= 0 )
        {
            RemoveSelf();
        }
    }
}
