using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { PlayerTurn, EnemyTurn, Win, Lose,Start }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private List<CharacterInformation> players;

    [SerializeField] private List<CharacterInformation> enemies;

    [SerializeField] private CharacterInformation currentPlayer;
    [SerializeField] private CharacterInformation currentEnemy;
    
    #region private Attributes

    [SerializeField] private BattleState state = BattleState.Start;
    private bool change_state = false;
    
    private int currentPlayerIndex = 0;
    private int currentEnemieIndex = 0;
    
    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        currentPlayer = players[0];
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
                    UpdateCurrentPlayer();
                    break;
                case BattleState.EnemyTurn:
                    break;
                case BattleState.Win:
                    break;
                case BattleState.Lose:
                    break;
            }

            change_state = false;
        }
    }

    public void UpdateCurrentPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        currentPlayer = players[currentPlayerIndex];
        if (currentEnemieIndex == 0) state = BattleState.EnemyTurn;
        else if (SideIsDead(enemies)) state = BattleState.Win;
    }

    public void UpdateCurrentEnemy()
    {
        currentEnemieIndex = (currentEnemieIndex + 1) % enemies.Count;
        currentEnemy = enemies[currentEnemieIndex];
        if (currentEnemieIndex == 0) state = BattleState.EnemyTurn;
        else if (SideIsDead(players)) state = BattleState.Lose;
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
