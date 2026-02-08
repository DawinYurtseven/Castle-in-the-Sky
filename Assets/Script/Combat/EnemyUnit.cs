
using UnityEngine;

public class EnemyUnit : Unit
{
    public override void BeginningOfTurn()
    {
        base.BeginningOfTurn();
        Debug.Log("Beginning of turn of enemy");
        CalculateTimeValue(1f);
        EndTurn();
    }
}
