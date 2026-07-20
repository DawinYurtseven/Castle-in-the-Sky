using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpeedPendant : Items
{

    public override void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.Speed += Mathf.CeilToInt(stack * stackingIncrease);
        }
    }

    protected override void TriggeredEvent(Unit unit)
    {
        Debug.Log("lmao what?");
    }     
}
