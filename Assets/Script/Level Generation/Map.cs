using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map system;
    public static List<EnemyUnit> EnemyUnitAssetList;
    public List<PlayerUnit> PlayerUnitAssetList;
    
    public List<PlayerUnit> currentPlayerUnits;
    
    

    private void Awake()
    {
        if(!system) system = this;
        else Destroy(this);
    }

    public void GameStart(List<PlayerUnit> playerUnits)
    {
        currentPlayerUnits = playerUnits;
    }

    public void ReturnToMap()
    {
        
    }
}