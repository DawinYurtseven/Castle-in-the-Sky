using UnityEngine;

public class Gluttony : Items
{
    public Gluttony()
    {
        triggerPosition = ItemTriggerPosition.ReactionDone;
    }
    
    public override string ItemName => "Gluttony";

    //TODO: WORDING!
    public override string ItemDescription => "with a 1% chance (+1% per stack), incoming damage will be stored and applied to the next instance of damage from the unit.";

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
