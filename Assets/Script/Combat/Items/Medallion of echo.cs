using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Medallionofecho : Items
{

    protected override void TriggeredEvent(Unit unit)
    {
        if (getChance())
        {
            unit.repeated = true;
            //Some Banner with "AGAIN!!"
            Debug.Log($"AGAIN, {unit.name}!");
        }
        else unit.repeated = false;
    }
}
