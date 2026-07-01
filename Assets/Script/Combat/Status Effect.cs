using UnityEngine;
using UnityEngine.Events;

public abstract class StatusEffect
{
    protected UnityAction<Unit> subscribedAction;
    public float baseValue;
    public int turnsRemaining;

    public void Subscribe(ref UnityAction<Unit> action, Unit unit)
    {
        action += Effect;
        subscribedAction = action;
        if(!unit.StatusCounts.ContainsKey(this))
            unit.StatusCounts.Add(this, 0);
        unit.StatusCounts[this]++;
    }

    protected abstract void Effect(Unit unit);

    public void RemoveSelf()
    {
        subscribedAction -= Effect;
    }
}
