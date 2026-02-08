using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleSystem : MonoBehaviour
{
    private List<Unit>
        queue = new List<Unit>(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    [SerializeField] private PlayerUnit[] playerUnits;
    [SerializeField] private EnemyUnit[] enemyUnits;

    public UnityEvent<Unit, float> EndOfTurnTrigger = new UnityEvent<Unit, float>();

    private Unit currentActiveUnit;

    void Start()
    {
        StartOfCombat();
    }


    void StartOfCombat()
    {
        EndOfTurnTrigger?.AddListener(EndOfTurn);

        //first, order all of the units based on their 'speed' stat

        queue.AddRange(playerUnits);
        queue.AddRange(enemyUnits);
        queue.Sort((unit, unit1) => (int)(unit.TimeValue - unit1.TimeValue));


        //trigger their Beginning Of Combat
        foreach (var unit in queue)
        {
            unit.BeginningOfCombat(EndOfTurnTrigger);
        }

        currentActiveUnit = queue[0];
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit player) seeSituationDEBUG(player);
        queue.RemoveAt(0);
        currentActiveUnit.BeginningOfTurn();
    }

    void EndOfTurn(Unit currentUnit, float timeValue)
    {
        foreach (var unit in queue)
        {
            unit.PassTimeValue(timeValue);
        }
        queue.Add(currentUnit);
        queue.Sort((unit, unit1) => (int)(unit.TimeValue - unit1.TimeValue));
        //maybe animations or something.
        currentActiveUnit = queue[0];
        queue.RemoveAt(0);
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit player) {seeSituationDEBUG(player);}
        currentActiveUnit.BeginningOfTurn();
    }


    #region Camera and UI

    public void Submit()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit player)
        {
            player.Submit();
            seeSituationDEBUG(player);
        }
    }
    
    public void seeSituationDEBUG(PlayerUnit player)
    {
        switch (player.currentState)
        {
            case PlayerUnit.combatState.root:
                Debug.Log($"options: \nX for attack\nSquare for skill\nR1 for inspect");
                break;
            case PlayerUnit.combatState.skill:
                string skills = "";
                foreach (var skill in player.skills )
                {
                    skills += $"{skill.Key} is of type {skill.Value.ToString()}\n";
                }
                Debug.Log(skills);
                break;
            case PlayerUnit.combatState.inspect:
                break;
            case PlayerUnit.combatState.targetEnemy:
                foreach (var enemy in enemyUnits)
                {
                    Debug.Log(enemy.name);
                }
                break;
            case PlayerUnit.combatState.targetAlly:
                foreach (var playerUnit in playerUnits)
                {
                    Debug.Log(playerUnit.name);
                }
                break;
        }
    }

    #endregion
}