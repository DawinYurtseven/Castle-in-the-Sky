using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public static Map System;
    public List<EnemyUnit> enemyUnitAssetList = new();
    public List<PlayerUnit> playerUnitAssetList;

    public List<PlayerUnit> currentPlayerUnits;

    public List<Node> nodes = new List<Node>(); 
    private List<List<Node>> layers = new List<List<Node>>();
    public Node currentNode;

    [SerializeField] private List<GameObject> systems = new();
    [SerializeField] private GameObject mapGameObject, nodePrefab;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private float width;
    [SerializeField] private float depth;
    [SerializeField] private int levelDepth;

    private Camera mainCamera;
    private Vector3 cameraStartPos, cameraStartRot;

    private void Awake()
    {
        if (!System) System = this;
        else Destroy(this);
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraStartPos = mainCamera.transform.position;
            cameraStartRot = mainCamera.transform.rotation.eulerAngles;
        }
    }

    private void Start()
    {
        ReturnToMap();
        GameStart(currentPlayerUnits);
    }


    //TODO: make 4 in a row less prominent
    public void GameStart(List<PlayerUnit> playerUnits)
    {
        for (int i = 0; i < playerUnits.Count; i++)
        {
            var temp = Instantiate(playerUnits[i]);
            temp.gameObject.SetActive(false);
            currentPlayerUnits[i] = temp;
        }

        nodes.Clear();

        for (int i = 0; i < levelDepth; i++)
        {
            layers.Add(new List<Node>());

            // Determine how many nodes are in this specific layer
            int nodesInLayer = 1;
            if (i > 0 && i < levelDepth - 1)
            {
                nodesInLayer = Random.Range(2, 5); // 2 to 4 nodes wide for middle layers
            }

            for (int j = 0; j < nodesInLayer; j++)
            {
                Vector3 spawnPos = startPos;
                spawnPos.z += depth * i;

                // Handle horizontal spacing dynamically based on the node count
                if (nodesInLayer > 1)
                {
                    float segmentWidth = width * 2 / (nodesInLayer + 1);
                    spawnPos.x = startPos.x - (width) + (segmentWidth * (j + 1));
                }

                Node newNode = Instantiate(nodePrefab, spawnPos, Quaternion.identity, mapGameObject.transform)
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

        // 2. Connect the layers together
        ConnectLayers(layers);

        currentNode = layers[0][0]; // Start node
        currentNode.GetComponent<Button>().interactable = true;
        currentSelectButton = currentNode.GetComponent<Button>();
    }


    private void ConnectLayers(List<List<Node>> layers)
    {
        for (int i = 0; i < layers.Count - 1; i++)
        {
            List<Node> currentLayer = layers[i];
            List<Node> nextLayer = layers[i + 1];

            int nextLayerCount = nextLayer.Count;
            int currentLayerCount = currentLayer.Count;

            // Track the furthest left index in the next layer we can connect to
            int nextLayerIndexPointer = 0;

            for (int j = 0; j < currentLayerCount; j++)
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
                Node connectNode = currentLayer[j];

                var nav = new Navigation();
                if (j != 0)
                {
                    nav.selectOnLeft = currentLayer[j - 1].GetComponent<Button>();
                }

                if (j != currentLayerCount - 1)
                {
                    nav.selectOnRight = currentLayer[j + 1].GetComponent<Button>();
                }

                if (i != 0)
                {
                    nav.selectOnDown = connectNode.previousNodes[0].GetComponent<Button>();
                }
 
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

                nav.selectOnUp = connectNode.nextNodes[(connectNode.nextNodes.Count / 2)].GetComponent<Button>();
                connectNode.GetComponent<Button>().navigation = nav;
            }
        }

        var lastNodeNav = new Navigation
        {
            selectOnDown = layers[levelDepth - 2][(layers[levelDepth - 2].Count / 2)].GetComponent<Button>()
        };
        layers[^1][0].GetComponent<Button>().navigation = lastNodeNav;
    }

    public void ReturnToMap()
    {
        if (currentNode != null)
        {
            foreach (var node in layers[currentNode.level - 1])
            {
                node.GetComponent<Button>().interactable = false;
            }
        }
        
        InputSystemWrapper.instance.SetState(InputSystemWrapper.State.Map);
        foreach (var unit in currentPlayerUnits)
        {
            unit.gameObject.SetActive(false);
        }

        mainCamera.transform.DOMove(cameraStartPos, 0.2f).SetEase(Ease.OutExpo);
        mainCamera.transform.DORotate(cameraStartRot, 0.2f).SetEase(Ease.OutExpo);
        foreach (GameObject go in systems)
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

    //TODO: This is what I need for the battleSystem as well
    public void Menu()
    {
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if (currentSelectButton == null) return;
        if (normalizedInput != Vector2.zero)
        {
            bool isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
            Selectable selectable;
            if (isVertical)
            {
                selectable = normalizedInput.y > 0
                    ? currentSelectButton.navigation.selectOnUp
                    : currentSelectButton.navigation.selectOnDown;
            }
            else
            {
                selectable = normalizedInput.x > 0
                    ? currentSelectButton.navigation.selectOnRight
                    : currentSelectButton.navigation.selectOnLeft;
            }

            if (selectable != null)
            {
                //TODO: maybe make it so it depends on the next connection?
                var previous = currentSelectButton;
                currentSelectButton = (Button)selectable;
                currentSelectButton?.Select();
                if (currentSelectButton != null)
                {
                    var targetPos = previous.transform.position - currentSelectButton.transform.position;
                    mapGameObject.transform.DOMove(mapGameObject.transform.position + targetPos, 0.3f)
                        .SetEase(Ease.OutExpo);
                }
            }
        }
    }

    #endregion
}