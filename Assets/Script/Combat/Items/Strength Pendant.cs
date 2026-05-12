using System.Collections.Generic;
using UnityEngine;

public class StrengthPendant : Items
{
    
    public override string ItemName => "Strength Pendant";

    public override string ItemDescription => "Increases the Strength stat of each Unit by 10";
    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Strength += stack * 10;
        }
    }

    protected override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    }
    
}