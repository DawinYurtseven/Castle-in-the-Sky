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
        basicAttack,
        beginningOfCombat,
        beginningOfTrun,
        endofCombat,
        endOfTrun,
        actionTaken,
        reactionDone,
        criticalTrigger
    }
    
    protected ItemTriggerPosition triggerPosition;
    
    public int stacks;
    public void SubscribeToTeamEvents(List<Unit> teamUnits)
    {
        for (int i = 0; i < teamUnits.Count; i++)
        {
            var unit = teamUnits[i];
            switch (triggerPosition)
            {
                case ItemTriggerPosition.basicAttack:
                    unit.BasicAttackTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.beginningOfCombat:
                    unit.BeginningOfCombatTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.beginningOfTrun:
                    unit.BeginningOfTurnTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.endofCombat:
                    unit.EndOfCombatTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.endOfTrun:
                    unit.EndOfTurnTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.actionTaken:
                    unit.ActionTakenTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.reactionDone:
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

    internal abstract void TriggeredEvent(Unit unit);
    
}