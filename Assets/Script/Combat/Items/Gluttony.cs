using UnityEngine;

public class Gluttony : Items
{
    public Gluttony()
    {
        triggerPosition = ItemTriggerPosition.ReactionDone;
    }


    protected override void TriggeredEvent(Unit unit)
    {
        var randomChance = Random.Range(0, 100);
        //hyperbolik stacks
        var threshold = 100 / (1 + 0.01 * stacks);
        if (randomChance >= threshold)
        {
            unit.blocked = true;
            unit.damageAddition += unit.bufferedDamage + 1.5f * unit.Strength;
        }
    }
}
