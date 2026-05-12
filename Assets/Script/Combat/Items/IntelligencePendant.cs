using System.Collections.Generic;
using UnityEngine;

public class IntelligencePendant : Items
{
    public override string ItemName => "Intelligence Pendant";

    public override string ItemDescription => "Increases the Intelligence stat of each Unit by 10";

    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Intelligence += stack * 10;
        }
    }

    protected override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    } 
}
