using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem system;

    private readonly List<Unit>
        queue = new(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;

    //have dicts for the items for enemies and players

    public List<GameObject> playerValues;

    private int playerDeaths, enemyDeaths;
    private Unit targetUnit;

    [SerializeField] public Camera battleCamera;

    /// <summary>
    /// 0- standard camera angle for when the camera should show all
    /// 1 - view towards the enemies
    /// 2 - view towards the player
    /// </summary>
    [SerializeField] private Transform[] cameraTargets;
    
    [SerializeField] private SplineContainer playerPositionTargets, enemyPositionTargets;

    public Transform inFrontOfEnemies, inFrontOfPlayers;
    [SerializeField] private GameObject winCanvas, loseCanvas;

    public UnityAction<Unit, float> EndOfTurnTrigger;

    private Unit currentActiveUnit;

    private GameObject playerValueHorizontalGameObject, queueHorizontalGameObject;

    private void Awake()
    {
        if (system == null) system = this;
        else Destroy(gameObject);


        battleCamera ??= Camera.main;

        //TODO: make a smarter way to enable gui. animations on enable would work
        gameGUI.SetActive(true);
        //TODO: better work than this search part. maybe custom script? 
        playerValueHorizontalGameObject = gameGUI.transform.Find("Player value horizontal").gameObject;
        queueHorizontalGameObject = gameGUI.transform.Find("Queue").gameObject;
    }

    private void ResetCombatState()
    {
        combatIsOver = false;
        EndOfTurnTrigger = null;
        queue.Clear();
        playerDeaths = 0;
        enemyDeaths = 0;
    }

    public void StartOfCombat()
    {
        InputSystemWrapper.instance.SetState(InputSystemWrapper.State.Combat);
        ResetCombatState();
        
        for(int i = 0; i < playerUnits.Count; i++)
        {
            var vec = playerPositionTargets.EvaluatePosition(1f / (playerUnits.Count + 1) * (i + 1));
            Vector3 startPosition = new Vector3(vec.x, vec.y, vec.z) + Vector3.up * 0.5f;
            var temp = playerUnits[i].gameObject;
            temp.transform.position = startPosition;
            //TODO: Take the spline and cut it up in the amount of units per side. then assign the positions and rotations based on the new cut up splines.
            temp.transform.rotation = Quaternion.Euler
            (
                0, 
                Quaternion.LookRotation(
                    playerPositionTargets.transform.parent.transform.position - temp.transform.position, 
                    Vector3.forward).eulerAngles.y, 
                0);
            temp.SetActive(true);
        }

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            var vec = enemyPositionTargets.EvaluatePosition(1f/(enemyUnits.Count + 1) * (i+1));
            Vector3 startPosition = new Vector3(vec.x, vec.y, vec.z) + Vector3.up * 0.5f;
            var temp = Instantiate(enemyUnits[i], startPosition, Quaternion.identity);
            temp.transform.LookAt(enemyPositionTargets.transform.parent.transform);
            enemyUnits[i] = temp;
        }

        SetAllPlayerValues();

        EndOfTurnTrigger += EndOfTurn;
        
        //first, order all the units based on their 'speed' stat
        queue.AddRange(playerUnits);
        queue.AddRange(enemyUnits);
        //trigger their Beginning Of Combat
        foreach (var unit in queue)
        {
            unit.BeginningOfCombat();
        }
        //then order the queue 
        OrderQueue();

        
        currentActiveUnit = queue[0];
        PopQueue();
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
    }


    private bool combatIsOver;

    void EndOfTurn(Unit currentUnit, float timeValue)
    {
        if (combatIsOver) return;
        MoveCameraToIndex(0);
        foreach (var unit in queue)
        {
            unit.PassTimeValue(timeValue);
        }

        //TODO: Maybe more animation handling instead of hard code?
        queue.Add(currentUnit);
        OrderQueue();
        //maybe animations or something.
        currentActiveUnit = queue[0];
        PopQueue();
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
    }

    public void DeathOfUnit(Unit unit)
    {
        RemoveUnitFromQueue(unit);
        if (unit is PlayerUnit)
        {
            playerDeaths++;
            if (playerDeaths == playerUnits.Count)
            {
                EndOfCombat(false);
            }
        }
        else
        {
            queue.Remove(unit);
            OrderQueue();
            enemyDeaths++;
            if (enemyDeaths == enemyUnits.Count)
            {
                EndOfCombat(true);
            }
        }
    }

    
    
    private void EndOfCombat(bool playerWon)
    {
        //return control to UI element type I guess...
        combatIsOver = true;
        if (playerWon)
        {
            //main character always the first object
            winCanvas.GetComponent<WinScreenController>().mainCharacter = playerUnits[0];
            winCanvas?.gameObject.SetActive(true);
            for (int i = enemyUnits.Count - 1; i >= 0; i--)
            {
                Destroy(enemyUnits[i].gameObject);
            }
            enemyUnits.Clear();
        }
        else
        {
            loseCanvas?.gameObject.SetActive(true);
        }
    }


    #region Camera, UI and Input

    [SerializeField] private Button currentSelectButton;

    [SerializeField] private GameObject gameGUI, playerValuePrefab, queueImagePrefab, skillTabPrefab;
    private GameObject temporaryImageGameObject;

    public void SetCurrentSelectButton(Button button)
    {
        currentSelectButton = button;
        currentSelectButton?.Select();
    }

    public void DeselectButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public enum CameraTargets
    {
        EnemyView,
        PlayerView,
        FullView,
        Base,
    }

    /// <summary>
    /// Outside the normal set positions,
    /// this is for when the camera needs to move to a specific position with set angles.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cameraTarget"></param>
    /// <returns></returns>
    /// 
    /// 
    public IEnumerator MoveCamera(Transform target, CameraTargets cameraTarget )
    {
        float wantedFOV = 60f;

        int index = 0;
        float zAxis = 0.0f;
        Vector3 interpolatedPosition;
        switch (cameraTarget)
        {
            case CameraTargets.Base:
                //nothing
                break;
            case CameraTargets.PlayerView:
                //idk yet
                break;
            case CameraTargets.EnemyView:
                wantedFOV = 70f;
                for (int i = 0; i < enemyUnits.Count; i++)
                {
                    Button left = null, right = null;
                    if (i != 0)
                    {
                        left = enemyUnits[i - 1].selected;
                    }
                    if (i != enemyUnits.Count - 1)
                    {
                        right = enemyUnits[i + 1].selected;
                    }

                    enemyUnits[i].CalculateHUDValues(left, right);
                }

                if (targetUnit == null || targetUnit is not EnemyUnit)
                {
                    for (int i = 0; i < enemyUnits.Count; i++)
                    {
                        if (enemyUnits[i].HP > 0)
                        {
                            targetUnit = enemyUnits[i];
                            break;
                        }
                    }
                }
                
                //TODO: make this player unit specific for enemy view and not rotate the z axis

                currentSelectButton = targetUnit.selected;
                currentSelectButton.Select();
                targetUnit.SelectHUD(true, battleCamera.transform);
                index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
                zAxis = target.eulerAngles.z;
                interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position,
                    enemyUnits[^1].transform.position, index / ((float)enemyUnits.Count + 1));
                target.LookAt(interpolatedPosition);
                target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);
                break;
            case CameraTargets.FullView:
                wantedFOV = 70;
                for (int i = 0; i < enemyUnits.Count; i++)
                {
                    Button left = null, right = null;
                    enemyUnits[i].selected.GetComponent<GameButton>().OnSelectEvent += () =>
                    {
                        MoveValuesForEnemyView(target);
                    };
                    if (i != 0)
                    {
                        left = enemyUnits[i - 1].selected;
                    }
                    else
                    {
                        left = playerUnits[i].selected;
                        playerUnits[i].selected.GetComponent<GameButton>().OnSelectEvent += () =>
                        {
                            MoveValuesForPlayerView(target);
                        };
                    }

                    if (i != enemyUnits.Count - 1)
                    {
                        right = enemyUnits[i + 1].selected;
                    }
                    else
                    {
                        right = playerUnits[^1].selected;
                        playerUnits[^1].selected.GetComponent<GameButton>().OnSelectEvent += () =>
                        {
                            MoveValuesForPlayerView(target);
                        };
                    }

                    enemyUnits[i].CalculateHUDValues(left, right);
                }
                for (int i = 0; i < playerUnits.Count; i++)
                {
                    Button left = null, right = null;
                    playerUnits[i].selected.GetComponent<GameButton>().OnSelectEvent += () =>
                    {
                        MoveValuesForPlayerView(target);
                    };
                    if (i != 0)
                    {
                        left = playerUnits[i - 1].selected;
                    }
                    else
                    {
                        left = enemyUnits[i].selected;
                        enemyUnits[i].selected.GetComponent<GameButton>().OnSelectEvent += () =>
                        {
                            MoveValuesForEnemyView(target);
                        };
                    }

                    if (i != playerUnits.Count - 1)
                    {
                        right = playerUnits[i + 1].selected;
                    }
                    else
                    {
                        right = enemyUnits[^1].selected;
                        enemyUnits[^1].selected.GetComponent<GameButton>().OnSelectEvent += () =>
                        {
                            MoveValuesForEnemyView(target);
                        };
                    }
                    
                    playerUnits[i].CalculateHUDValues(left, right);
                }
                
                if (targetUnit == null || targetUnit is not EnemyUnit)
                {
                    for (int i = 0; i < enemyUnits.Count; i++)
                    {
                        if (enemyUnits[i].HP > 0)
                        {
                            targetUnit = enemyUnits[i];
                            break;
                        }
                    }
                }
                
                //TODO: make this player unit specific for enemy view and not rotate the z axis

                currentSelectButton = targetUnit.selected;
                currentSelectButton.Select();
                targetUnit.SelectHUD(true, battleCamera.transform);
                index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
                zAxis = target.eulerAngles.z;
                interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position,
                    enemyUnits[^1].transform.position, index / ((float)enemyUnits.Count + 1));
                target.LookAt(interpolatedPosition);
                target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);
                
                break;
        }
        
        battleCamera.DOFieldOfView(wantedFOV, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DOMove(target.position, 0.2f).SetEase(Ease.OutExpo);
        yield return battleCamera.transform.DORotate(target.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo)
            .WaitForCompletion();

        
        
    }

    private void MoveValuesForEnemyView(Transform target)
    {
        float wantedFOV = 70f;
        var index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
        var zAxis = target.eulerAngles.z;
        Vector3 interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position,
            enemyUnits[^1].transform.position, index / ((float)enemyUnits.Count + 1));
        target.LookAt(interpolatedPosition);
        target.eulerAngles = new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);
        battleCamera.DOFieldOfView(wantedFOV, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DOMove(target.position, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DORotate(target.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            targetUnit.RotateSelected(target);
        });

    }

    private void MoveValuesForPlayerView(Transform target)
    {
        float wantedFOV = 60f;
        var nextIndex = playerUnits.IndexOf((PlayerUnit)targetUnit) + 1;
        var nextTarget = cameraTargets[2];
        var nextZAxis = nextTarget.eulerAngles.z;
        Vector3 nextInterpolatedPosition = Vector3.Lerp(playerUnits[0].transform.position,
            playerUnits[^1].transform.position, nextIndex / ((float)playerUnits.Count + 1));
        nextTarget.LookAt(nextInterpolatedPosition);
        nextTarget.eulerAngles =  new Vector3(nextTarget.eulerAngles.x, nextTarget.eulerAngles.y, nextZAxis);
        battleCamera.DOFieldOfView(wantedFOV, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DOMove(nextTarget.position, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DORotate(nextTarget.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            targetUnit.RotateSelected(nextTarget);
        });
    }

    public void UpdatePlayerValues(PlayerUnit playerUnit)
    {
        var index = playerUnits.IndexOf(playerUnit);
        playerValues[index].GetComponentInChildren<TextMeshProUGUI>().text = $"HP:{playerUnit.HP}\nSP:{playerUnit.SP}";
    }

    public void ShowNewQueuePosition(Unit unit, float timeValue)
    {
        var index = -1;
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].QueueTimeValue - timeValue > timeValue)
            {
                index = i;
                break;
            }
        }

        if (index == -1) index = queue.Count;

        temporaryImageGameObject = Instantiate(queueImagePrefab, queueHorizontalGameObject.transform);
        temporaryImageGameObject.GetComponent<RectTransform>().localPosition = new(-1000, -60, 0);
        temporaryImageGameObject.GetComponent<RectTransform>()?.DOLocalMove(new(-435 + index * 115, -60, 0), 0.2f)
            .SetEase(Ease.OutExpo);
        temporaryImageGameObject.GetComponent<Image>().sprite = unit.hudImage;
    }

    public void AcceptNewQueuePosition(Unit unit, float timeValue)
    {
        var index = -1;
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].QueueTimeValue - timeValue > timeValue)
            {
                index = i;
                break;
            }
        }

        if (index == -1) index = queue.Count;
        else
        {
            for (int i = 0; i < queueHorizontalGameObject.transform.childCount; i++)
            {
                var child = queueHorizontalGameObject.transform.GetChild(i);
                if (child.localPosition.x >= -385 + index * 115)
                {
                    child.DOLocalMove(child.localPosition + new Vector3(115, 0, 0), 0.2f).SetEase(Ease.OutExpo);
                }
            }
        }

        if (temporaryImageGameObject != null)
            temporaryImageGameObject?.GetComponent<RectTransform>()?.DOLocalMove(new(-385 + index * 115, 0, 0), 0.2f)
                .SetEase(Ease.OutExpo).OnComplete(() => temporaryImageGameObject = null);
    }

    public void FreeNewQueuePosition()
    {
        if (temporaryImageGameObject != null)
            temporaryImageGameObject.transform
                .DOLocalMove(new(-1000, temporaryImageGameObject.transform.localPosition.y, 0), 0.2f)
                .SetEase(Ease.OutExpo).OnComplete(() => DestroyImmediate(temporaryImageGameObject));
    }

    private void PopQueue()
    {
        queue.RemoveAt(0);
        for (int i = 0; i < queueHorizontalGameObject.transform.childCount; i++)
        {
            if (i == 0)
            {
                var child = queueHorizontalGameObject.transform.GetChild(0);
                queueHorizontalGameObject.transform.GetChild(0)?.DOLocalMove(new(-5000, 0, 0), 0.2f)
                    .SetEase(Ease.OutExpo).OnComplete(() => DestroyImmediate(child.gameObject));
            }
            else
            {
                queueHorizontalGameObject.transform.GetChild(i).GetComponent<RectTransform>()
                    ?.DOLocalMove(new(-385 + (i - 1) * 115, 0, 0), 0.2f).SetEase(Ease.OutExpo);
            }
        }
    }

    private void OrderQueue()
    {
        for (var i = queueHorizontalGameObject.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(queueHorizontalGameObject.transform.GetChild(i).gameObject);
        }

        queue.Sort((unit, unit1) => unit.QueueTimeValue <= unit1.QueueTimeValue ? -1 : 1);
        for (int i = 0; i < queue.Count; i++)
        {
            var temp = Instantiate(queueImagePrefab, queueHorizontalGameObject.transform);
            temp.GetComponent<RectTransform>().localPosition = new(-385 + i * 115, 0, 0);
            temp.GetComponent<Image>().sprite = queue[i].hudImage;
        }
    }

    private void RemoveUnitFromQueue(Unit unit)
    {
        if (queue.Contains(unit))
        {
            queue.Remove(unit);
            OrderQueue();
        }
    }

    private void SetAllPlayerValues()
    {
        for (int i = playerValueHorizontalGameObject.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(playerValueHorizontalGameObject.transform.GetChild(i).gameObject);
        }

        playerValues.Clear();
        foreach (var t in playerUnits)
        {
            var temp = Instantiate(playerValuePrefab, playerValueHorizontalGameObject.transform);
            temp.transform.Find("Image").GetComponent<Image>().sprite = t.hudImage;
            temp.GetComponentInChildren<TextMeshProUGUI>().text = $"HP:{t.HP}\nSP:{t.SP}";
            playerValues.Add(temp.gameObject);
        }
    }


    /// <summary>
    /// this interpolates the camera to one of the predetermined positions and applies ui/input changes
    /// depending on the position
    ///
    /// 0 for standard
    /// 1 for enemy view
    /// 2 for player view
    /// 3 for all enemies
    /// 4 for all players
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <returns></returns>
    public IEnumerator MoveCameraToIndexTransform(int targetIndex)
    {
        switch (targetIndex)
        {
            case 0:
                currentSelectButton = null;
                targetUnit?.SelectHUD(false);
                targetUnit = null;
                break;
            case 1: //individual enemies
                for (int i = 0; i < enemyUnits.Count; i++)
                {
                    Button left = null, right = null;
                    if (i != 0)
                    {
                        left = enemyUnits[i - 1].selected;
                    }
                    else if (i != enemyUnits.Count - 1)
                    {
                        right = enemyUnits[i + 1].selected;
                    }

                    enemyUnits[i].CalculateHUDValues(left, right);
                }

                if (targetUnit == null || targetUnit is not EnemyUnit)
                {
                    for (int i = 0; i < enemyUnits.Count; i++)
                    {
                        if (enemyUnits[i].HP > 0)
                        {
                            targetUnit = enemyUnits[i];
                            break;
                        }
                    }
                }
                
                //TODO: make this player unit specific for enemy view and not rotate the z axis

                currentSelectButton = targetUnit.selected;
                currentSelectButton.Select();
                targetUnit.SelectHUD(true, battleCamera.transform);
                var index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
                var zAxis = cameraTargets[1].eulerAngles.z;
                Vector3 interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position,
                    enemyUnits[^1].transform.position, index / ((float)enemyUnits.Count + 1));
                cameraTargets[1].LookAt(interpolatedPosition);
                cameraTargets[1].eulerAngles =  new Vector3(cameraTargets[1].eulerAngles.x, cameraTargets[1].eulerAngles.y, zAxis);
                break;
            case 2: //individual players
                break;
            case 3: // all enemies;
                Vector3 middlePointEnemy = Vector3.zero;
                foreach (var t in enemyUnits)
                {
                    middlePointEnemy += t.transform.position;
                    t.CalculateHUDValues();
                    t.SelectHUD(true, battleCamera.transform);
                }
                middlePointEnemy /= enemyUnits.Count;
                cameraTargets[1].LookAt(middlePointEnemy);
                targetIndex = 1;
                break;
            case 4: // all players
                targetIndex = 2;
                break;
        }

        battleCamera.transform.DOMove(cameraTargets[targetIndex].position, 0.2f).SetEase(Ease.OutExpo);
        yield return battleCamera.transform.DORotate(cameraTargets[targetIndex].rotation.eulerAngles, 0.2f)
            .SetEase(Ease.OutExpo).WaitForCompletion();
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
            currentSelectButton?.onClick.Invoke();
            StartCoroutine(playerUnit.Submit(targetUnit));
        }
    }

    public void Cancel()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.Cancel());
        }
    }

    public void SkillTab()
    {
        //do on player something-something instead next time
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.SkillTab());
        }
    }

    public void SkillTabVisibility(bool isVisible, List<Skill> skills = null, PlayerUnit playerUnit = null)
    {
        //TODO: make actual animation for the queue tab to be enabled
        var queueTab = gameGUI.transform.Find("Queue");
        queueTab.gameObject.SetActive(!isVisible);
        
        
    }

    public void InspectTab()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.Inspect());
        }
    }

    public void SetSelection(Unit selectedUnit)
    {
        targetUnit?.SelectHUD(false);
        targetUnit = selectedUnit;
        targetUnit.SelectHUD(true);
    }

    public void ClearSelection(bool all = false)
    {
        if (!all)
        {
            targetUnit?.SelectHUD(false);
            targetUnit?.ResetSelected();
        }
        else
        {
            List<Unit> temp = new List<Unit>();
            temp.AddRange(enemyUnits);
            temp.AddRange(playerUnits);
            for (int i = 0; i < temp.Count; i++)
            {
                temp[i]?.SelectHUD(false);
                temp[i]?.ResetSelected();
            }
        }

        currentSelectButton = null;
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
                SetCurrentSelectButton((Button)selectable);
                //frendly unit
                if (selectable.transform.parent.transform.parent.TryGetComponent(typeof(Unit), out var unitComponent ))
                {
                    var unit = (Unit)unitComponent.GetComponent(typeof(Unit));
                    if (unit == null)
                    {
                        Debug.Log("Tough luck");
                        return;
                    }
                    if (selectable.gameObject.TryGetComponent(typeof(GameButton), out var go))
                    {
                        var button = go.GetComponent<GameButton>();
                        button.OnSelectEvent.Invoke();
                    }
                }
            }
        }
    }

    public void TriggerSpecificButtonAction()
    {
        if (currentSelectButton.TryGetComponent(typeof(GameButton), out var go))
        {
            var button = go.GetComponent<GameButton>();
            button.OnSpecificAction.Invoke();
        }
    }

    #endregion
}