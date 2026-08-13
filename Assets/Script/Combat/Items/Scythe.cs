public class Scythe : Items
{
    protected override void TriggeredEvent(Unit unit)
    {
        if(unit.currentTarget.HP > unit.currentTarget.maxHP * (100 / (1 + baseValue + stackingIncrease * stacks))) return;
        unit.currentTarget.Death();
    }
}