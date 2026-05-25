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

    public abstract string ItemName
    {
        get;
    }

    public abstract string ItemDescription
    {
        get;
    }
    
    
    protected ItemTriggerPosition triggerPosition;

    protected int stacks;
    public void SubscribeToTeamEvents(Unit unit)
    {
        
            unit.items.Add(this);
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

    public virtual void Acquire(List<Unit> teamUnits, int stack = 1)
    {
        
        if (stacks == 0)
        {
            for (int i = 0; i < teamUnits.Count; i++)
            {
                SubscribeToTeamEvents(teamUnits[i]);
            }
            
        }
        stacks += stack;
    }

    protected abstract void TriggeredEvent(Unit unit);

    public static Items GetRandomItem(List<Items> exclude = null)
    {
        List<Items> items = new List<Items>()
        {
            new ConstitutionPendant(),
            new Gluttony(),
            new IntelligencePendant(),
            new LuckPendant(),
            new Medallionofecho(),
            new SpeedPendant(),
            new StrengthPendant()
        };
        if (exclude != null)
        {
            foreach (var item in exclude)
            {
                items.Remove(item);
            }
        }
        var index = Random.Range(0, items.Count);
        return items[index];
    }
    
}