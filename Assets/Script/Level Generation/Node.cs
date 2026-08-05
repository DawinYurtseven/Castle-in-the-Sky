using System.Collections.Generic;
using TMPro;
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
    
    public List<Node> previousNodes = new List<Node>();
    public List<Node> nextNodes = new List<Node>();
    public NodeType type;
    public int level;
    public int boost; // incrementally boost of bonuses that increases depending on the previous boosts and connectivity
    
    public GameObject lineRendererPrefab;
    public TMP_Text boostText;
    
    [SerializeField] private Sprite combatSprite, merchantSprite, storySprite;
    [SerializeField] private Image nodeIcon;

    private Actor actor;
    public void SetActor(Actor act)
    {
        actor = act;
    }
    
    public void CreateNode()
    {
        if(boost >1 )
            boostText.text = boost.ToString();
        else 
            boostText.gameObject.SetActive(false);
        var button = GetComponent<Button>();
        //Create assets based on the type of the node. 
        switch (type)
        {
            case NodeType.Battle:
                nodeIcon.sprite = combatSprite;
                nodeIcon.rectTransform.rotation = Quaternion.Euler(30, 0, -45);
                nodeIcon.rectTransform.sizeDelta = new Vector2(nodeIcon.rectTransform.sizeDelta.x, nodeIcon.rectTransform.sizeDelta.y * 2); 
                //I WANNA FIIIIIIIGHT~!!! WITH MY LIFE ON THE LIIIIINE!!!
                button.onClick.AddListener(() =>
                {
                    Map.Manager.gameObject.SetActive(false);
                    BattleSystem.Manager.gameObject.SetActive(true);
                    BattleSystem.Manager.enemyUnits.Clear();
                    BattleSystem.Manager.playerUnits.Clear();
                    List<EnemyUnit> range = new ();
                    int maxRange = Mathf.Min(level / 2 + 1, 5); // get a specific range of enemies based on the level. work on how to deal with it later
                    int numRange = Random.Range(1,maxRange);
                    for (int i = 0; i < numRange; i++)
                    {
                        //Get a better method for this depending on the level.
                        var random = Random.Range(0, Map.Manager.enemyUnitAssetList.Count);
                        var enemy = Instantiate(Map.Manager.enemyUnitAssetList[random]);
                        enemy.gameObject.SetActive(false);
                        enemy.GetComponent<EnemyUnit>().Level = level;
                        range.Add(enemy.GetComponent<EnemyUnit>());
                    }
                    BattleSystem.Manager.playerUnits.AddRange(Map.Manager.currentPlayerUnits);
                    BattleSystem.Manager.enemyUnits.AddRange(range);
                    InputSystemWrapper.Instance.SetState(InputSystemWrapper.State.Combat);
                    BattleSystem.Manager.StartOfCombat();
                });
                break;
            case NodeType.Merchant:
                button.onClick.AddListener(() =>
                {
                    Map.Manager.gameObject.SetActive(false);
                    MerchantSystem.Manager.gameObject.SetActive(true);
                    InputSystemWrapper.Instance.SetState(InputSystemWrapper.State.Merchant);
                    MerchantSystem.Manager.Reroll();
                });
                //bombs? you want them? 
                break;
            case NodeType.Story:
                //make a story manager that fatches story progression of each type depending on story progression.
                button.onClick.AddListener(() =>
                {
                    InputSystemWrapper.Instance.SetState(InputSystemWrapper.State.Dialogue);
                    Map.Manager.gameObject.SetActive(false);
                    StoryManager.Manager.gameObject.SetActive(true);
                    StoryManager.Manager.GetNextStoryPart(actor);;
                });
                break;
        }
        
    }

    public void ConnectLine(Vector3 position)
    {
        //make 90% line 
        var newPos = position.normalized * position.magnitude * 0.9f;
        
        var temp = Instantiate(lineRendererPrefab, transform);
        var linerend = temp.GetComponent<LineRenderer>();
        linerend.SetPositions(new []{Vector3.zero,newPos});
        linerend.startColor = Color.blue;
        linerend.endColor = Color.blue;
        linerend.useWorldSpace = false;
    }
}
