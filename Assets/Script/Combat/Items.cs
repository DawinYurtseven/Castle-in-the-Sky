using System.Collections.Generic;
using UnityEngine;

public abstract class Items
{
    protected enum ItemTypes
    {
        specialType,
        StatBoost,
    }

    protected enum ItemTriggerPosition
    {
        BasicAttack,
        BeginningOfCombat,
        BeginningOfTrun,
        EndofCombat,
        EndOfTrun,
        ActionTaken,
        ReactionDone,
        criticalTrigger
    }
    
    protected ItemTriggerPosition triggerPosition;

    protected int stacks;
    public void SubscribeToTeamEvents(List<Unit> teamUnits)
    {
        for (int i = 0; i < teamUnits.Count; i++)
        {
            var unit = teamUnits[i];
            switch (triggerPosition)
            {
                case ItemTriggerPosition.BasicAttack:
                    unit.BasicAttackTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.BeginningOfCombat:
                    unit.BeginningOfCombatTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.BeginningOfTrun:
                    unit.BeginningOfTurnTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.EndofCombat:
                    unit.EndOfCombatTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.EndOfTrun:
                    unit.EndOfTurnTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.ActionTaken:
                    unit.ActionTakenTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.ReactionDone:
                    unit.ReactionDoneTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.criticalTrigger:
                    unit.CriticalTrigger += TriggeredEvent;
                    break;
            }
        }
    }

    public virtual void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        if (stacks == 0)
        {
            SubscribeToTeamEvents(teamUnits);
        }
        stacks += stack;
    }

    protected abstract void TriggeredEvent(Unit unit);
    
}