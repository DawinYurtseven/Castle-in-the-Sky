using System.Collections.Generic;
using UnityEngine;

public class ConstitutionPendant : Items
{
    public override string ItemName => "Constitution Pendant";

    public override string ItemDescription => "Increases the Constitution stat of each Unit by 10";

    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Constitution += stack * 10;
        }
    }

    protected override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    }
}