using System.Collections.Generic;
using UnityEngine;

public class LuckPendant : Items
{
    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Luck += stack * 10;
        }
    }

    internal override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    }
}