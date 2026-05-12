using System.Collections.Generic;
using UnityEngine;

public class SpeedPendant : Items
{
    public override string ItemName => "Speed Pendant";

    public override string ItemDescription => "Increases the Speed stat of each Unit by 10";

    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Speed += stack * 10;
        }
    }

    protected override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    }     
}
