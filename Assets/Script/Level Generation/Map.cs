using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map system;
    public static List<EnemyUnit> EnemyUnitAssetList;
    public List<PlayerUnit> PlayerUnitAssetList;
    
    public List<PlayerUnit> currentPlayerUnits;
    
    public List<Node> nodes = new List<Node>();
    public Node currentNode;

    [SerializeField] private List<GameObject> systems = new ();
    [SerializeField] private GameObject mapImage, nodePrefab;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private float width;
    [SerializeField] private float depth;
    [SerializeField] private int levelDepth;
    
    
    

    private void Awake()
    {
        if(!system) system = this;
        else Destroy(this);

        ReturnToMap();
    }

    public void GameStart(List<PlayerUnit> playerUnits)
    {
        currentPlayerUnits = playerUnits;
        
        currentNode = Instantiate(nodePrefab, startPos, Quaternion.identity, transform).GetComponent<Node>();
        nodes.Add(currentNode);
        
        //Spawn new nodes or do it after completion
    }

    public void ReturnToMap()
    {
        foreach (GameObject go in systems)
        {
            go.SetActive(false);
        }
        gameObject.SetActive(true);
    }
}