using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public static Map Manager;
    public List<EnemyUnit> enemyUnitAssetList = new();
    public List<PlayerUnit> playerUnitAssetList;

    public List<PlayerUnit> currentPlayerUnits;

    public List<Node> nodes = new(); 
    private readonly List<List<Node>> layers = new();
    public Node currentNode;

    [SerializeField] private List<GameObject> systems = new();
    [SerializeField] private GameObject mapGameObject, nodePrefab;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private float width;
    [SerializeField] private float depth;
    [SerializeField] private int levelDepth;

    private Camera mainCamera;
    private Vector3 cameraStartPos, cameraStartRot;
    
    //TODO: add currency to the game so a merchant can work. maybe also make this added to a total outside the run?
    public int money;

    private void Awake()
    {
        if (!Manager) Manager = this;
        else Destroy(this);
        mainCamera = Camera.main;
        if (!mainCamera) return;
        cameraStartPos = mainCamera.transform.position;
        cameraStartRot = mainCamera.transform.rotation.eulerAngles;
    }

    private void Start()
    {
        ReturnToMap();
        GameStart(currentPlayerUnits);
    }


    //TODO: make 4 in a row less prominent
    private void GameStart(List<PlayerUnit> playerUnits)
    {
        for (var i = 0; i < playerUnits.Count; i++)
        {
            var temp = Instantiate(playerUnits[i]);
            temp.gameObject.SetActive(false);
            currentPlayerUnits[i] = temp;
        }

        nodes.Clear();

        for (var i = 0; i < levelDepth; i++)
        {
            layers.Add(new List<Node>());

            // Determine how many nodes are in this specific layer
            var nodesInLayer = 1;
            if (i > 0 && i < levelDepth - 1)
            {
                nodesInLayer = Random.Range(2, 5); // 2 to 3 nodes wide for middle layers
            }

            for (var j = 0; j < nodesInLayer; j++)
            {
                var spawnPos = startPos;
                spawnPos.z += depth * i;

                // Handle horizontal spacing dynamically based on the node count
                if (nodesInLayer > 1)
                {
                    var segmentWidth = width * 2 / (nodesInLayer + 1);
                    spawnPos.x = startPos.x - width + (segmentWidth * (j + 1));
                }

                var newNode = Instantiate(nodePrefab, spawnPos, Quaternion.identity, mapGameObject.transform)
                    .GetComponent<Node>();
                layers[i].Add(newNode);
                newNode.level = i + 1;
                nodes.Add(newNode); // Keep your global tracking list happy
                var i1 = i;
                newNode.GetComponent<Button>().interactable = false;
                //TODO: move this to the connection part and activate the connection when done with the node
                newNode.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (i1 < levelDepth - 1)
                    {
                        for (int k = 0; k < layers[i1 + 1].Count; k++)
                        {
                            if(newNode.nextNodes.Contains(layers[i1 + 1][k]))
                                layers[i1 + 1][k].GetComponent<Button>().interactable = true;
                        }
                    }

                    currentNode = newNode;
                });
                newNode.CreateNode();
            }
        }
        
        var targetNodes = Random.Range(3, 5); // Decides on exactly 3 or 4 nodes
        var placedNodes = 0;


        var candidateNodes = new List<Node>();
        for (var i = 1; i < levelDepth - 1; i++) // Skip index 0 (start) and index levelDepth - 1 (boss)
        {
            candidateNodes.AddRange(layers[i]);
        }

        var actor = StoryManager.Manager.GetPossibleActor();
        
        while (placedNodes < targetNodes && candidateNodes.Count > 0)
        {
            var randomIndex = Random.Range(0, candidateNodes.Count);
            var potentialStoryNode = candidateNodes[randomIndex];
            candidateNodes.RemoveAt(randomIndex); // Remove so we don't pick it twice

            // CONDITION CHECK: Ensure no immediately adjacent node in the *same* layer is already a story node
            var isAdjacentToStoryNode = false;
            var currentLayerIndex = potentialStoryNode.level - 1; 
            var currentLayer = layers[currentLayerIndex];

            var nodeIndexInLayer = currentLayer.IndexOf(potentialStoryNode);

            // Check left neighbor
            if (nodeIndexInLayer > 0 && currentLayer[nodeIndexInLayer - 1].type == Node.NodeType.Story)
                isAdjacentToStoryNode = true;

            // Check right neighbor
            if (nodeIndexInLayer < currentLayer.Count - 1 && currentLayer[nodeIndexInLayer + 1].type == Node.NodeType.Story)
                isAdjacentToStoryNode = true;

            // Optional Check: If your layers are small, you might also want to prevent story nodes in back-to-back layers
            // Check previous layer
            if (currentLayerIndex > 1) 
            {
                if (layers[currentLayerIndex - 1].Exists(n => n.type == Node.NodeType.Story)) 
                    isAdjacentToStoryNode = true;
            }

            // If all rules pass, successfully convert it!
            if (isAdjacentToStoryNode) continue;
            potentialStoryNode.type = Node.NodeType.Story;
            placedNodes++;
        
            // Visual cue debugging (Change its button color or text so you can visually see it worked)
            potentialStoryNode.GetComponentInChildren<Image>().color = Color.red;
            potentialStoryNode.SetActor(actor);
            potentialStoryNode.CreateNode();
        }
        
        candidateNodes.Clear();
        for (var i = 2; i < levelDepth - 1; i++) // Skip index 0 and 1 (start) and index levelDepth - 1 (boss)
        {
            var excludedList = from node in layers[i] where node.type != Node.NodeType.Story select node;
            candidateNodes.AddRange(excludedList);
        }
        placedNodes = 0;
        targetNodes = Random.Range(1, 3); // Decides on exactly 3 or 4 nodes
        
        while (placedNodes < targetNodes && candidateNodes.Count > 0)
        {
            var randomIndex = Random.Range(0, candidateNodes.Count);
            var potentialStoryNode = candidateNodes[randomIndex];
            candidateNodes.RemoveAt(randomIndex); // Remove so we don't pick it twice

            // CONDITION CHECK: Ensure no immediately adjacent node in the *same* layer is already a story node
            var isAdjacentToStoryNode = false;
            var currentLayerIndex = potentialStoryNode.level - 1; 
            var currentLayer = layers[currentLayerIndex];

            var nodeIndexInLayer = currentLayer.IndexOf(potentialStoryNode);

            // Check left neighbor
            if (nodeIndexInLayer > 0 && currentLayer[nodeIndexInLayer - 1].type == Node.NodeType.Merchant)
                isAdjacentToStoryNode = true;

            // Check right neighbor
            if (nodeIndexInLayer < currentLayer.Count - 1 && currentLayer[nodeIndexInLayer + 1].type == Node.NodeType.Merchant)
                isAdjacentToStoryNode = true;

            // Optional Check: If your layers are small, you might also want to prevent story nodes in back-to-back layers
            // Check previous layer
            if (currentLayerIndex > 1) 
            {
                if (layers[currentLayerIndex - 1].Exists(n => n.type == Node.NodeType.Merchant)) 
                    isAdjacentToStoryNode = true;
            }

            // If all rules pass, successfully convert it!
            if (isAdjacentToStoryNode) continue;
            potentialStoryNode.type = Node.NodeType.Merchant;
            placedNodes++;
        
            // Visual cue debugging (Change its button color or text so you can visually see it worked)
            potentialStoryNode.GetComponentInChildren<Image>().color = Color.burlywood;
            potentialStoryNode.CreateNode();
        }

        // 2. Connect the layers together
        ConnectLayers();

        currentNode = layers[0][0]; // Start node
        currentNode.GetComponent<Button>().interactable = true;
        currentSelectButton = currentNode.GetComponent<Button>();
    }


    //Todo: check conditions and maybe adjust them for better preference/better performance
    private void ConnectLayers()
    {
        for (var i = 0; i < layers.Count - 1; i++)
        {
            var currentLayer = layers[i];
            var nextLayer = layers[i + 1];

            var nextLayerCount = nextLayer.Count;
            var currentLayerCount = currentLayer.Count;

            // Track the furthest left index in the next layer we can connect to
            var nextLayerIndexPointer = 0;

            for (var j = 0; j < currentLayerCount; j++)
            { 
                //prevent overshooting the index
                if (nextLayerIndexPointer == nextLayerCount) nextLayerIndexPointer--;
                //prevent undershooting the index
                else if (nextLayer[nextLayerIndexPointer].previousNodes.Count != 0
                         &&
                    //when the next connection already has 2 connections and it is not the last one, increase
                    ((
                        nextLayer[nextLayerIndexPointer].previousNodes.Count == 2
                        && nextLayerIndexPointer < nextLayerCount - 1
                        && currentLayerCount != 1
                    )
                    ||
                    //This logic prevents that the last node on the right gets a connection too far left
                    (
                        i != 0 
                        && nextLayerIndexPointer + 1 != nextLayerCount
                        && j == currentLayerCount - 1
                        && nextLayerIndexPointer == 0
                        && nextLayerCount > 2
                        )
                    ||
                    //This logic prevents the last node in a layer to have more than 2 connections
                    (
                        i != 0 
                        && j == currentLayerCount - 1 
                        && nextLayerCount - 1 - nextLayerIndexPointer >=2 
                        )
                    ) ) 
                    nextLayerIndexPointer++;
                
                
                var initialIndex = nextLayerIndexPointer;
                var connectNode = currentLayer[j];
                
 
                // Enforce at least one connection forward
                connectNode.nextNodes.Add(nextLayer[nextLayerIndexPointer]);
                nextLayer[nextLayerIndexPointer].previousNodes.Add(connectNode);
                var localPosition = nextLayer[nextLayerIndexPointer].transform.position -
                                    connectNode.transform.position;
                connectNode.ConnectLine(localPosition);

                // Decides if this node wants to branch out to the *next* adjacent node too
                // Enforcing that it doesn't overshoot the available nodes
                if (nextLayerIndexPointer + 1 < nextLayerCount && Random.value > 0.7f &&
                    !(j == 0 && nextLayerIndexPointer + 1 == nextLayerCount - 1))
                {
                    nextLayerIndexPointer++;
                    connectNode.nextNodes.Add(nextLayer[nextLayerIndexPointer]);
                    nextLayer[nextLayerIndexPointer].previousNodes.Add(connectNode);
                    localPosition = nextLayer[nextLayerIndexPointer].transform.position -
                                    connectNode.transform.position;
                    connectNode.ConnectLine(localPosition);
                }

                // If it's the last node in the current layer, force it to bridge 
                // all the way to the end of the next layer so no nodes are left orphaned
                if (j == currentLayerCount - 1)
                {
                    while (nextLayerIndexPointer < nextLayerCount)
                    {
                        if (!connectNode.nextNodes.Contains(nextLayer[nextLayerIndexPointer]))
                        {
                            connectNode.nextNodes.Add(nextLayer[nextLayerIndexPointer]);
                            nextLayer[nextLayerIndexPointer].previousNodes.Add(connectNode);
                            localPosition = nextLayer[nextLayerIndexPointer].transform.position -
                                            connectNode.transform.position;
                            connectNode.ConnectLine(localPosition);
                        }

                        nextLayerIndexPointer++;
                    }
                }
                else if (Random.value > 0.5f && nextLayerIndexPointer == initialIndex)
                {
                    nextLayerIndexPointer++;
                }
            }
        }

        var lastNodeNav = new Navigation
        {
            selectOnDown = layers[levelDepth - 2][layers[levelDepth - 2].Count / 2].GetComponent<Button>()
        };
        layers[^1][0].GetComponent<Button>().navigation = lastNodeNav;
    }

    public void ReturnToMap()
    {
        if (currentNode)
        {
            foreach (var node in layers[currentNode.level - 1])
            {
                node.GetComponent<Button>().interactable = false;
            }
        }
        
        InputSystemWrapper.Instance.SetState(InputSystemWrapper.State.Map);
        foreach (var unit in currentPlayerUnits)
        {
            unit.gameObject.SetActive(false);
        }

        mainCamera.transform.DOMove(cameraStartPos, 0.2f).SetEase(Ease.OutExpo);
        mainCamera.transform.DORotate(cameraStartRot, 0.2f).SetEase(Ease.OutExpo);
        foreach (var go in systems)
        {
            go.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    #region Input

    private Button currentSelectButton;

    public void Submit()
    {
        if (currentSelectButton?.interactable == true)
            currentSelectButton?.onClick.Invoke();
    }

    public void Cancel()
    {
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if (currentSelectButton == null) return;
        if (normalizedInput != Vector2.zero)
        {
            Selectable selectable;
            var node = currentSelectButton.GetComponent<Node>().nextNodes;
            node.AddRange(currentSelectButton.GetComponent<Node>().previousNodes);
            var direction = new Vector3(normalizedInput.x, 0, normalizedInput.y);
            var closest = node[0];
            Vector3 closestNormalized;
            Vector3 nextNodeNormalized;
            for (var i = 1; i < node.Count; i++)
            {
                closestNormalized = (closest.transform.position - currentSelectButton.transform.position).normalized;
                nextNodeNormalized = (node[i].transform.position - currentSelectButton.transform.position).normalized;
                if (Vector3.Distance(direction, closestNormalized) >
                    Vector3.Distance(direction, nextNodeNormalized))
                {
                    closest = node[i];
                }
            }
            closestNormalized = (closest.transform.position - currentSelectButton.transform.position).normalized;
            if (Vector3.Distance(closestNormalized, direction) > Math.Sqrt(2)) return;
            
            selectable = closest.GetComponent<Button>();

            if (selectable != null)
            {
                currentSelectButton = (Button)selectable;
                currentSelectButton?.Select();
                if (currentSelectButton != null)
                {
                    mapGameObject.transform.DOKill();
                    mapGameObject.transform.DOMove(-currentSelectButton.transform.localPosition, 0.3f)
                        .SetEase(Ease.OutExpo);
                }
            }
        }
    }

    #endregion
}