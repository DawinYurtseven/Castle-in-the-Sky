using System.Collections.Generic;
using UnityEngine;

public class LuckPendant : Items
{
    public override string ItemName => "Luck Pendant";

    public override string ItemDescription => "Increases the Luck stat of each Unit by 10";

    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Luck += stack * 10;
        }
    }

    protected override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    }
}