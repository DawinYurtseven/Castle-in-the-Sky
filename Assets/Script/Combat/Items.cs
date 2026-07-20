using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public abstract class Items
{
    protected enum ItemTypes
    {
        specialType,
        StatBoost,
    }

    protected enum ItemTriggerPosition
    {
        None,
        BasicAttack,
        BeginningOfCombat,
        BeginningOfTurn,
        EndOfCombat,
        EndOfTurn,
        ActionTaken,
        ReactionDone,
        CriticalTrigger
    }

    public string ItemName;
    public string ItemDescription;
    public Sprite ItemImage;
    [SerializeField] protected ItemTriggerPosition triggerPosition;
    [SerializeField] protected float startingChance, stackChance, baseValue, stackingIncrease;
    

    protected int stacks;
    public void SubscribeToTeamEvents(Unit unit)
    {
        
            unit.Items.Add(this);
            switch (triggerPosition)
            {
                case ItemTriggerPosition.BasicAttack:
                    unit.BasicAttackTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.BeginningOfCombat:
                    unit.BeginningOfCombatTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.BeginningOfTurn:
                    unit.BeginningOfTurnTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.EndOfCombat:
                    unit.EndOfCombatTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.EndOfTurn:
                    unit.EndOfTurnTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.ActionTaken:
                    unit.ActionTakenTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.ReactionDone:
                    unit.ReactionDoneTrigger += TriggeredEvent;
                    break;
                case ItemTriggerPosition.CriticalTrigger:
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
        var data = FindDatabaseAsset();
        var items = new List<Items>(data.allItems);
        if (exclude != null)
        {
            foreach (var item in exclude)
            {
                var i = items.Find((x) => x.ItemName == item.ItemName);
                items.Remove(i);
            }
        }
        var index = Random.Range(0, items.Count);
        return items[index];
    }
    
    private static GameDatabase FindDatabaseAsset()
    {
        var database = Resources.Load<GameDatabase>("Values");
        if (database != null)
        {
            Debug.Log($"Successfully loaded database! Found {database.allItems.Count} items.");
        }
        else
        {
            Debug.LogError("Could not find GameDatabase asset in any Resources folder!");
        }

        return database;
    }

    protected bool getChance()
    {
        var randomChance = Random.Range(0, 100);
        //hyperbolik stacks
        var threshold = 100 / (1 + startingChance + stackChance * stacks);
        return randomChance >= threshold;
    }
    
}