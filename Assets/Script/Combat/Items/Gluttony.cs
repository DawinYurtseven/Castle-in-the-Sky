using System;
using Random = UnityEngine.Random;

[Serializable]
public class Gluttony : Items
{
    
    protected override void TriggeredEvent(Unit unit)
    {
        if (!getChance()) return;
        unit.blocked = true;
        unit.damageAddition += unit.bufferedDamage + 1.5f * unit.Strength;
    }
}
