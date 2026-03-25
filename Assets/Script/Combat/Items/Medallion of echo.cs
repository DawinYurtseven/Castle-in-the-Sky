using UnityEngine;

public class Medallionofecho : Items
{
    public Medallionofecho()
    {
        triggerPosition = ItemTriggerPosition.actionTaken;
    }
    
    internal override void TriggeredEvent(Unit unit)
    {
        var randomChance = Random.Range(0, 100);
        //hyperbolik stacks
        var threshold = 100 / (1 + 0.1 + 0.02 * (stacks - 1));
        if (randomChance >= threshold)
        {
            unit.repeated = true;
            //Some Banner with "AGAIN!!"
            Debug.Log($"AGAIN, {unit.name}!");
        }
        else unit.repeated = false;
    }
}
