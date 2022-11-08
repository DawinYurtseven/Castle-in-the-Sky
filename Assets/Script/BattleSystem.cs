using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    PlayerTurn,
    EnemyTurn,
    Win,
    Lose,
    Start
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private List<CharacterInformation> players;

    [SerializeField] private List<CharacterInformation> enemies;

    [SerializeField] private CharacterInformation currentPlayer;
    [SerializeField] private CharacterInformation currentEnemy;
    [SerializeField] private UIController ui_controller;

    #region private Attributes

    [SerializeField] private BattleState state = BattleState.Start;
    [SerializeField] private bool change_state;

    [SerializeField] private int currentPlayerIndex = 0;
    [SerializeField] private int currentEnemieIndex = 0;
    [SerializeField] private bool switch_side;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        currentPlayer = players[players.Count - 1];
        change_state = true;
        switch_side = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (change_state)
        {
            switch (state)
            {
                case BattleState.Start:
                    //Bullshitery
                    state = BattleState.PlayerTurn;
                    break;
                case BattleState.PlayerTurn:
                    switch_side = false;
                    change_state = false;
                    StartCoroutine(PlayersTurn());
                    break;
                case BattleState.EnemyTurn:
                    switch_side = false;
                    change_state = false;
                    
                    break;
                case BattleState.Win:
                    break;
                case BattleState.Lose:
                    break;
            }
            
        }
    }

    public void UpdateCurrentPlayer()
    {
        if (currentPlayerIndex == players.Count - 1)
            switch_side = true;
        else if (SideIsDead(enemies))
            state = BattleState.Win;
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        currentPlayer = players[currentPlayerIndex];

    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public IEnumerator PlayersTurn()
    {
        ui_controller.UpdateList();
        while (!switch_side)
        {
            yield return null;
        }

        state = BattleState.EnemyTurn;
        change_state = true;
    }
    
    

    public void UpdateCurrentEnemy()
    {
        /*currentEnemieIndex = (currentEnemieIndex + 1) % enemies.Count;
        currentEnemy = enemies[currentEnemieIndex];
        if (currentEnemieIndex == 0) state = BattleState.EnemyTurn;
        else if (SideIsDead(players)) state = BattleState.Lose;*/
    }

    public void ExecuteEnemyAction(CharacterController enemy)
    {
        
    }
    

    public List<CharacterInformation> getPlayers()
    {
        return players;
    }

    public List<CharacterInformation> getEnemies()
    {
        return enemies;
    }

    public CharacterInformation getCurrentPlayer()
    {
        return currentPlayer;
    }

    public BattleState getState()
    {
        return state;
    }

    public bool SideIsDead(List<CharacterInformation> side)
    {
        int totalHP = 0;
        foreach (CharacterInformation character in side)
        {
            totalHP += character.GetHP();
        }

        return totalHP == 0;
    }
}