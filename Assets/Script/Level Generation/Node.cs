using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Node : MonoBehaviour
{
    public enum NodeType
    {
        Battle,
        Merchant,
        Story,
    }
    
    public NodeType type;
    public int level;
    
    public void CreateNode()
    {
        var button = GetComponent<Button>();
        //Create assets based on the type of the node. 
        switch (type)
        {
            case NodeType.Battle:
                //I WANNA FIIIIIIIGHT~!!! WITH MY LIFE ON THE LIIIIINE!!!
                button.onClick.AddListener(() =>
                {
                    BattleSystem.system.enemyUnits.Clear();
                    BattleSystem.system.playerUnits.Clear();
                    List<EnemyUnit> range = new ();
                    int maxRange = Mathf.Min(level / 2 + 1, 5); // get a specific range of enemies based on the level. work on how to deal with it later
                    int numRange = Random.Range(1,maxRange);
                    for (int i = 0; i < numRange; i++)
                    {
                        //Get a better method for this depending on the level.
                        var random = Random.Range(0, Map.EnemyUnitAssetList.Count);
                        range.Add(Map.EnemyUnitAssetList[random]);
                    }
                    BattleSystem.system.playerUnits.AddRange(Map.system.currentPlayerUnits);
                    BattleSystem.system.enemyUnits.AddRange(range);
                    
                    BattleSystem.system.StartOfCombat();
                });
                break;
            case NodeType.Merchant:
                //bombs? you want them? 
                break;
            case NodeType.Story:
                //make a story manager that fatches story progression of each type depending on story progression.
                break;
        }
        
    }
}
