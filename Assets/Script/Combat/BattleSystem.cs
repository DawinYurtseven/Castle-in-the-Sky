using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    
    public static BattleSystem system;
    
    private readonly List<Unit>
        queue = new(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    public PlayerUnit[] playerUnits;
    public EnemyUnit[] enemyUnits;

    private int playerDeaths, enemyDeaths;
    
    private Unit targetUnit;
    
    [SerializeField] public Camera battleCamera;
    /// <summary>
    /// 0- standard camera angle for when the camera should show all
    /// 1 - view towards the enemies
    /// 2 - view towards the player
    /// </summary>
    [SerializeField] private Transform[] cameraTargets;
    [SerializeField] private Canvas winCanvas, loseCanvas;
    [SerializeField] private GameObject gameGUI, playerValuePrefab;

    public UnityEvent<Unit, float> endOfTurnTrigger = new UnityEvent<Unit, float>();

    private Unit currentActiveUnit;

    private void Awake()
    {
        if(system == null) system = this;
        else Destroy(gameObject);
        //TODO: instantiate the GUI here
        gameGUI.SetActive(true);
    }

    void Start()
    {
        StartOfCombat();
    }


    void StartOfCombat()
    {
        endOfTurnTrigger?.AddListener(EndOfTurn);

        //first, order all the units based on their 'speed' stat

        queue.AddRange(playerUnits);
        queue.AddRange(enemyUnits);
        queue.Sort((unit, unit1) => (int)(unit.QueueTimeValue - unit1.QueueTimeValue));


        //trigger their Beginning Of Combat
        foreach (var unit in queue)
        {
            unit.BeginningOfCombat();
        }

        currentActiveUnit = queue[0];
        queue.RemoveAt(0);
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
    }

    private bool combatIsOver = false;
    
    void EndOfTurn(Unit currentUnit, float timeValue)
    {
        if(combatIsOver) return;
        MoveCameraToIndex(0);
        foreach (var unit in queue)
        {
            unit.PassTimeValue(timeValue);
        }
        queue.Add(currentUnit);
        queue.Sort((unit, unit1) => (int)(unit.QueueTimeValue - unit1.QueueTimeValue));
        //maybe animations or something.
        currentActiveUnit = queue[0];
        queue.RemoveAt(0);
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
        if (currentActiveUnit is PlayerUnit playerUnit)
        {
            var index = playerUnits.ToList().IndexOf(playerUnit);
            
        }
    }
    
    public void DeathOfUnit(Unit unit)
    {
        if (unit is PlayerUnit)
        {
            playerDeaths++;
            if (playerDeaths == playerUnits.Length)
            {
                EndOfCombat(false);
            }
        }
        else
        {
            enemyDeaths++;
            if (enemyDeaths == enemyUnits.Length)
            {
                EndOfCombat(true);
            }
        }
    }
    
    public void EndOfCombat(bool playerWon)
    {
        //return control to UI element type I guess...
        combatIsOver = true;
        if (playerWon)
        {
            winCanvas?.gameObject.SetActive(true);
        }
        else
        {
            loseCanvas?.gameObject.SetActive(true);
        }
    }


    #region Camera, UI and Input
    
    [SerializeField] private Button currentSelectButton;
    
    /// <summary>
    /// Outside the normal set positions,
    /// this is for when the camera needs to move to a specific position with set angles.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>

    public IEnumerator MoveCamera(Transform target)
    {
        battleCamera.transform.DOMove(target.position, 0.2f).SetEase(Ease.OutExpo);
        yield return battleCamera.transform.DORotate(target.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
    }

    
    /// <summary>
    /// this interpolates the camera to one of the predetermined positions and applies ui/input changes
    /// depending on the position
    ///
    /// 0 for standard
    /// 1 for enemy view
    /// 2 for player view
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <returns></returns>
    private IEnumerator MoveCameraToIndexTransform(int targetIndex)
    {
        switch (targetIndex)
        {
            case 0:
                currentSelectButton = null;
                targetUnit?.SelectHUD(false);
                targetUnit = null;
                break;
            case 1:
                for(int i = 0; i < enemyUnits.Length; i++)
                {
                    Button left = null, right = null;
                    if (i != 0)
                    {
                        left = enemyUnits[i - 1].selected;
                    }
                    else if (i != enemyUnits.Length - 1)
                    {
                        right = enemyUnits[i + 1].selected;
                    }
                    enemyUnits[i].CalculateHUDValues(left,right);
                }
                if(targetUnit == null || targetUnit is not EnemyUnit)
                    targetUnit = enemyUnits[0];
                currentSelectButton = targetUnit.selected;
                targetUnit.SelectHUD(true);
                var index = enemyUnits.ToList().IndexOf((EnemyUnit)targetUnit) + 1;
                Vector3 interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position, enemyUnits[^1].transform.position, index/((float)enemyUnits.Length+1));
                cameraTargets[1].LookAt(interpolatedPosition);
                break;
        }
        
        battleCamera.transform.DOMove(cameraTargets[targetIndex].position, 0.2f).SetEase(Ease.OutExpo);
        yield return battleCamera.transform.DORotate(cameraTargets[targetIndex].rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        
    }
    
    public void MoveCameraToIndex(int targetIndex)
    {
        StartCoroutine(MoveCameraToIndexTransform(targetIndex));
    }

    #endregion

    #region Input

    public void Submit()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            playerUnit.Submit(targetUnit);
        }
    }

    public void Cancel()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            playerUnit.Cancel();
        }
    }

    public void SkillTab()
    {
        //do on player something something instead next time
    }
    
    public void InspectTab()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            playerUnit.Inspect();
        }
    }

    public void clearSelection()
    {
        targetUnit?.SelectHUD(false);
        currentSelectButton = null;
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if(currentSelectButton == null) return;
        if (normalizedInput != Vector2.zero)
        {
            bool isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
            Selectable selectable;
            if (isVertical)
            {
                selectable = normalizedInput.y > 0 ? currentSelectButton.navigation.selectOnUp : currentSelectButton.navigation.selectOnDown;
                
            }
            else
            {
                selectable = normalizedInput.x > 0 ? currentSelectButton.navigation.selectOnRight : currentSelectButton.navigation.selectOnLeft;
            }
            if (selectable != null)
            {
                if (selectable.gameObject.transform.parent.parent.TryGetComponent(typeof(Unit), out var unitComponent))
                {
                    Unit unit = (Unit)unitComponent;
                    targetUnit?.SelectHUD(false);
                    targetUnit = unit;
                    targetUnit.SelectHUD(true);
                    int index = 1;
                    Vector3 interpolatedPosition = targetUnit.transform.position;
                    if(unit is EnemyUnit enemy)
                    {
                        index = enemyUnits.ToList().IndexOf(enemy) +1 ;
                        interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position, enemyUnits[^1].transform.position, index/((float)enemyUnits.Length+1));
                    }
                    else
                    {
                        index = playerUnits.ToList().IndexOf((PlayerUnit)unit) +1;
                        interpolatedPosition = Vector3.Lerp(playerUnits[0].transform.position, playerUnits[^1].transform.position, index/((float)playerUnits.Length+1));
                    }
                    cameraTargets[1].LookAt(interpolatedPosition);
                    StartCoroutine(MoveCamera(cameraTargets[1]));
                }
                currentSelectButton = (Button)selectable;
                if (selectable.transform.parent.parent.TryGetComponent(typeof(Unit), out var button))
                {
                    var unitButton = (Unit)button;
                    if (unitButton != null)
                    {
                        targetUnit = unitButton;
                    }
                }
            }
        }
    }
    
    #endregion
}