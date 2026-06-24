using UnityEngine;

public class Medallionofecho : Items
{
    
    public override string ItemName => "Medallion of echo";

    //TODO: WORDING
    public override string ItemDescription => "With a 30% chance (+2% per additional stack), repeat any action a unit takes without a cost of resources";
    public Medallionofecho()
    {
        triggerPosition = ItemTriggerPosition.ActionTaken;
    }

    protected override void TriggeredEvent(Unit unit)
    {
        var randomChance = Random.Range(0, 100);
        //hyperbolic stacks
        var threshold = 100 / (1 + 0.3 + 0.02 * (stacks - 1));
        if (randomChance >= threshold)
        {
            unit.repeated = true;
            //Some Banner with "AGAIN!!"
            Debug.Log($"AGAIN, {unit.name}!");
        }
        else unit.repeated = false;
    }
}
