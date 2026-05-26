using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public static Map System;
    public List<EnemyUnit> enemyUnitAssetList = new ();
    public List<PlayerUnit> playerUnitAssetList;
    
    public List<PlayerUnit> currentPlayerUnits;
    
    public List<Node> nodes = new List<Node>();
    public Node currentNode;

    [SerializeField] private List<GameObject> systems = new ();
    [SerializeField] private GameObject mapImage, nodePrefab;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private float width;
    [SerializeField] private float depth;
    [SerializeField] private int levelDepth;
    
    private Camera mainCamera;
    private Vector3 cameraStartPos, cameraStartRot;

    private void Awake()
    {
        if(!System) System = this;
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

    public void GameStart(List<PlayerUnit> playerUnits)
    {
        currentPlayerUnits = playerUnits;
        nodes.Clear();

        // 1. Create a jagged list to hold nodes layer by layer
        List<List<Node>> layers = new List<List<Node>>();

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
                    float segmentWidth = width / (nodesInLayer +1);
                    spawnPos.x = startPos.x - (width / 2f) + (segmentWidth * (j+1));
                }

                Node newNode = Instantiate(nodePrefab, spawnPos, Quaternion.identity, transform).GetComponent<Node>();
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
                        for (int k = 0; k < layers[i1+1].Count; k++)
                        {
                            layers[i1+1][k].GetComponent<Button>().interactable = true;
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
                Node connectNode = currentLayer[j];

                // Enforce at least one connection forward
                connectNode.nextNodes.Add(nextLayer[nextLayerIndexPointer]);
                connectNode.ConnectLine(nextLayer[nextLayerIndexPointer].transform.position);
                
                // Decides if this node wants to branch out to the *next* adjacent node too
                // Enforcing that it doesn't overshoot the available nodes
                if (nextLayerIndexPointer + 1 < nextLayerCount && Random.value > 0.5f)
                {
                    nextLayerIndexPointer++;
                    connectNode.nextNodes.Add(nextLayer[nextLayerIndexPointer]);
                    connectNode.ConnectLine(nextLayer[nextLayerIndexPointer].transform.position);
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
                            connectNode.ConnectLine(nextLayer[nextLayerIndexPointer].transform.position);
                        }
                        nextLayerIndexPointer++;
                    }
                }
            }
        }
    }

    public void ReturnToMap()
    {
        mainCamera.transform.DOMove(cameraStartPos, 0.2f).SetEase(Ease.OutExpo);
        mainCamera.transform.DORotate(cameraStartRot, 0.2f).SetEase(Ease.OutExpo);
        foreach (GameObject go in systems)
        {
            go.SetActive(false);
        }
        gameObject.SetActive(true);
    }
}