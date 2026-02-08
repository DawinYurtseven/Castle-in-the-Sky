
using System.Collections;
using UnityEngine;

public class EnemyUnit : Unit
{
    public override IEnumerator BeginningOfTurn()
    {
        yield return null;
        yield return base.BeginningOfTurn();
        Debug.Log("Beginning of turn of enemy");
        CalculateTimeValue(1f);
        EndTurn();
    }
}
