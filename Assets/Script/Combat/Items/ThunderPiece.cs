using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class ThunderPiece : Items
{
    protected override void TriggeredEvent(Unit unit)
    {
        if (!getChance()) return;
        var list = new List<EnemyUnit>(BattleSystem.Manager.enemyUnits);
        if (unit.currentTargets.Count == 1 && list.Count > 1)
        {
            list.Remove((EnemyUnit)unit.currentTargets[0]);
        }

        var partialDamage = unit.CurrentTotalDamage / 5;

        for (int i = 0; i < stacks; i++)
        {
            var index = Random.Range(0, list.Count);
            list[index]?.TakeDamage(partialDamage);
        }
    }
}
