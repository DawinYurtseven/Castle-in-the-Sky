using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
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
    
    [SerializeField] private ItemTriggerPosition triggerPosition = ItemTriggerPosition.beginningOfCombat;

    public void SubscribeToTeamEvents(List<Unit> teamUnits)
    {
        for (int i = 0; i < teamUnits.Count; i++)
        {
            //TODO: fill this out and maybe start inhereted items for special events
            var unit = teamUnits[i];
            switch (triggerPosition)
            {
                case ItemTriggerPosition.beginningOfCombat:
                    unit.beginningOfCombatTrigger.AddListener(TriggeredEvent);
                    break;
            }
        }
        
    }

    internal extern void TriggeredEvent(Unit unit);
}
