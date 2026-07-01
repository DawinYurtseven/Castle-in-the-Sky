using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem system;
    private static readonly int Exit = Animator.StringToHash("Exit");

    private readonly List<Unit>
        queue = new(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;

    //have dictionary for the items for enemies and players

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
        if (!system) system = this;
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
        queue.Clear();
        playerDeaths = 0;
        enemyDeaths = 0;
        gameGUI.SetActive(true);
        foreach (var area in damageDisplayAreas)
        {
            for (var i = area.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(area.transform.GetChild(i).gameObject);
            }
        }
        activeDisplayCount = 0;
    }

    public void StartOfCombat()
    {
        InputSystemWrapper.Instance.SetState(InputSystemWrapper.State.Combat);
        ResetCombatState();
        
        for(int i = 0; i < playerUnits.Count; i++)
        {
            var vec = playerPositionTargets.EvaluatePosition(1f / (playerUnits.Count + 1) * (i + 1));
            Vector3 startPosition = new Vector3(vec.x, vec.y, vec.z) + Vector3.up * 0.5f;
            var temp = playerUnits[i].gameObject;
            temp.transform.position = startPosition;
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
            var temp = enemyUnits[i].gameObject;
            temp.transform.position = startPosition;
            temp.transform.rotation = Quaternion.Euler
            (
                0, 
                Quaternion.LookRotation(
                    playerPositionTargets.transform.parent.transform.position - temp.transform.position, 
                    Vector3.forward).eulerAngles.y, 
                0);
            temp.SetActive(true);
        }

        SetAllPlayerValues();

        EndOfTurnTrigger = (t, f) =>
        {
            StartCoroutine(EndOfTurn(t, f));
        };
        
        //first, order all the units based on their 'speed' stat
        queue.AddRange(playerUnits);
        queue.AddRange(enemyUnits);
        //trigger their Beginning Of Combat
        foreach (var unit in queue)
        {
            unit.BeginningOfCombat();
        }
        //then order the queue 
        OrderQueue(true);

        
        PopQueue();
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
    }


    private bool combatIsOver;

    IEnumerator EndOfTurn(Unit currentUnit, float timeValue)
    {
        //TODO: think about other things you should wait for before starting with the next turn
        yield return new WaitUntil(() => activeDisplayCount == 0);
        if (combatIsOver) yield break;
        foreach (var unit in queue)
        {
            unit.PassTimeValue(timeValue);
        }

        //TODO: Maybe more animation handling instead of hard code?
        queue.Add(currentUnit);
        OrderQueue();
        //maybe animations or something.
        PopQueue();
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
    }

    public void DeathOfUnit(Unit unit)
    {
        unit.gameObject.SetActive(false);
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
        ResetQueue();
        combatIsOver = true;
        gameGUI.SetActive(false);
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
            playerUnits.Clear();
        }
        else
        {
            loseCanvas?.gameObject.SetActive(true);
        }
    }


    #region Camera and UI

    [Header("UI")]
    [SerializeField] private GameObject gameGUI, playerValuePrefab, queueImagePrefab;
    private GameObject temporaryImageGameObject;
    [SerializeField] private GameObject damageDisplay;
    [SerializeField] private List<GameObject> damageDisplayAreas;

    #region Camera

    public enum CameraTargets
    {
        EnemyView,
        PlayerView,
        FullView,
        Base,
        Empty
    }

    /// <summary>
    /// Outside the normal set positions,
    /// this is for when the camera needs to move to a specific position with set angles.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cameraTarget"></param>
    /// <param name="median"></param>
    /// <returns></returns>
    /// 
    /// 
    public IEnumerator MoveCamera(Transform target, CameraTargets cameraTarget, bool median = false)
    {
        var wantedFOV = 60f;
        float zAxis;
        Action method = () => {};
        switch (cameraTarget)
        {
            case CameraTargets.Empty:
                currentSelectButton = null;
                targetUnit?.SelectHUD(false);
                targetUnit = null;
                target = cameraTargets[0];
                break;
            case CameraTargets.Base:
                //nothing
                break;
            case CameraTargets.PlayerView:
                //IDK yet
                wantedFOV = 70f;
                if (median)
                {
                    target = cameraTargets[2];
                    Vector3 middlePointEnemy = Vector3.zero;
                    foreach (var t in playerUnits)
                    {
                        middlePointEnemy += t.transform.position;
                        t.CalculateHUDValues();
                    }
                    middlePointEnemy /= playerUnits.Count;
                    
                    zAxis = target.eulerAngles.z;
                    target.LookAt(middlePointEnemy);
                    target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);
                    method = () =>
                    {
                        foreach (var t in playerUnits)
                        {
                            t.SelectHUD(true, battleCamera.transform);
                        }
                    };
                }
                else
                {
                    for (int i = 0; i < playerUnits.Count; i++)
                    {
                        Button left = null, right = null;
                        if (i != 0)
                        {
                            left = playerUnits[i - 1].selected;
                        }

                        if (i != playerUnits.Count - 1)
                        {
                            right = playerUnits[i + 1].selected;
                        }

                        playerUnits[i].CalculateHUDValues(left, right);
                    }
                    if (!targetUnit || targetUnit is  EnemyUnit)
                    {
                        foreach (var t in playerUnits.Where(t => t.HP > 0))
                        {
                            targetUnit = t;
                            break;
                        }
                    }
                
                    //TODO: make this player unit specific for enemy view and not rotate the z axis

                    SetCurrentSelectButton(targetUnit.selected);
                }
                break;
            case CameraTargets.EnemyView:
                wantedFOV = 70f;
                if (median)
                {
                    Vector3 middlePointEnemy = Vector3.zero;
                    foreach (var t in enemyUnits)
                    {
                        middlePointEnemy += t.transform.position;
                        t.CalculateHUDValues();
                    }
                    middlePointEnemy /= enemyUnits.Count;
                    
                    zAxis = target.eulerAngles.z;
                    target.LookAt(middlePointEnemy);
                    target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);

                    method = () =>
                    {
                        foreach (var t in enemyUnits)
                        {
                            t.SelectHUD(true, battleCamera.transform);
                        }
                    };
                }
                else
                {
                    for (var i = 0; i < enemyUnits.Count; i++)
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
                        var i1 = i;
                        enemyUnits[i].selected.GetComponent<GameButton>().OnSelectEvent = () =>
                        {
                            SetSelection(enemyUnits[i1]);
                            zAxis = target.eulerAngles.z;
                            var interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position,
                                enemyUnits[^1].transform.position, i1+ 1 / ((float)enemyUnits.Count + 1));
                            target.LookAt(interpolatedPosition);
                            target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);
                            battleCamera.transform.DOKill();
                            battleCamera.transform.DORotate(target.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo);
                        };
                    }
                    
                    var index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
                    zAxis = target.eulerAngles.z;
                    var interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position,
                        enemyUnits[^1].transform.position, index / ((float)enemyUnits.Count + 1));
                    target.LookAt(interpolatedPosition);
                    target.eulerAngles =  new Vector3(target.eulerAngles.x, target.eulerAngles.y, zAxis);
                    if (!targetUnit || targetUnit is not EnemyUnit)
                    {
                        foreach (var t in enemyUnits.Where(t => t.HP > 0))
                        {
                            targetUnit = t;
                            break;
                        }
                    }
                
                    //TODO: make this player unit specific for enemy view and not rotate the z axis

                    SetCurrentSelectButton(targetUnit.selected);
                }
                break;
            case CameraTargets.FullView:
                for (int i = 0; i < enemyUnits.Count; i++)
                {
                    var ie = i;
                    enemyUnits[i].selected.GetComponent<GameButton>().OnSelectEvent = () =>
                    {
                        MoveValuesForInspectView(enemyUnits[ie].cameraTargets[1],enemyUnits[ie]);
                    };
                    var left = i != 0 ? enemyUnits[i - 1].selected : playerUnits[i].selected;

                    var right = i != enemyUnits.Count - 1 ? enemyUnits[i + 1].selected : playerUnits[^1].selected;

                    enemyUnits[i].CalculateHUDValues(left, right);
                }
                for (int i = 0; i < playerUnits.Count; i++)
                {
                    var ip = i;
                    playerUnits[i].selected.GetComponent<GameButton>().OnSelectEvent = () =>
                    {
                        MoveValuesForInspectView(playerUnits[ip].cameraTargets[5], playerUnits[ip]);
                    };
                    var right = i != 0 ? playerUnits[i - 1].selected : enemyUnits[i].selected;

                    var left = i != playerUnits.Count - 1 ? playerUnits[i + 1].selected : enemyUnits[^1].selected;
                    
                    playerUnits[i].CalculateHUDValues(left, right);
                }
                
                if (!targetUnit || targetUnit is not EnemyUnit)
                {
                    foreach (var t in enemyUnits.Where(t => t.HP > 0))
                    {
                        targetUnit = t;
                        break;
                    }
                }
                
                //TODO: make this player unit specific for enemy view and not rotate the z axis
                
                SetCurrentSelectButton(targetUnit.selected);
                yield break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cameraTarget), cameraTarget, null);
        }
        
        battleCamera.DOFieldOfView(wantedFOV, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DOMove(target.position, 0.2f).SetEase(Ease.OutExpo);
        yield return battleCamera.transform.DORotate(target.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo).OnComplete(() => method.Invoke())
            .WaitForCompletion();
        
    }

    private void MoveValuesForInspectView(Transform target, Unit unit)
    {
        float wantedFOV = 70f;
        battleCamera.DOFieldOfView(wantedFOV, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DOMove(target.position, 0.2f).SetEase(Ease.OutExpo);
        battleCamera.transform.DORotate(target.rotation.eulerAngles, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            unit.RotateSelected(target);
        });

    }

    #endregion

    #region Player Values

    public void UpdatePlayerValues(PlayerUnit playerUnit)
    {
        if (!combatIsOver)
        {
            var index = playerUnits.IndexOf(playerUnit);
            playerValues[index].GetComponentInChildren<TextMeshProUGUI>().text = $"HP:{playerUnit.HP}\nSP:{playerUnit.SP}";
        }
    }

    private void SetAllPlayerValues()
    {
        for (int i = playerValueHorizontalGameObject.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(playerValueHorizontalGameObject.transform.GetChild(i).gameObject);
        }

        playerValues.Clear();
        //TODO: replace with already existing UI element and make it reset at end of combat.
        foreach (var t in playerUnits)
        {
            var temp = Instantiate(playerValuePrefab, playerValueHorizontalGameObject.transform);
            temp.transform.Find("Image").GetComponent<Image>().sprite = t.hudImage;
            temp.GetComponentInChildren<TextMeshProUGUI>().text = $"HP:{t.HP}\nSP:{t.SP}";
            playerValues.Add(temp.gameObject);
        }
    }

    #endregion

    #region Queue
    
    //the first 4 are player images, the last 4 are enemy images.
    //I will be reusing the same objects and just changing the sprite and position instead of instantiating and destroying them all the time
    [SerializeField] private List<GameObject> queueImages = new ();
    private readonly List<Tuple<Unit,GameObject>> queueImageInUse = new ();
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

        temporaryImageGameObject = queueImageInUse.Find((e) => e.Item1 == unit ).Item2;
        temporaryImageGameObject.SetActive(true);
        temporaryImageGameObject.transform.SetSiblingIndex(queue.Count );
        temporaryImageGameObject.GetComponent<RectTransform>().localPosition = new(-1000, -60, -1);
        temporaryImageGameObject.GetComponent<RectTransform>()?.DOLocalMove(new(-435 + index * 115, -60, -1), 0.2f)
            .SetEase(Ease.OutExpo);
        temporaryImageGameObject.GetComponent<Image>().sprite = unit.hudImage;
    }

    public void AcceptNewQueuePosition(Unit unit, float timeValue)
    {
        var index = -1;
        for (var i = 0; i < queue.Count; i++)
        {
            if (!(queue[i].QueueTimeValue - timeValue > timeValue)) continue;
            index = i;
            break;
        }

        if (index == -1) index = queue.Count;
        else
        {
            for (var i = 0; i < queueHorizontalGameObject.transform.childCount; i++)
            {
                var child = queueHorizontalGameObject.transform.GetChild(i);
                if (child.localPosition.x >= -385 + index * 115)
                {
                    child.DOLocalMove(child.localPosition + new Vector3(115, 0, 0), 0.2f).SetEase(Ease.OutExpo);
                }
            }
        }

        if (temporaryImageGameObject)
            temporaryImageGameObject?.GetComponent<RectTransform>()?.DOLocalMove(new(-385 + index * 115, 0, 0), 0.2f)
                .SetEase(Ease.OutExpo).OnComplete(() => temporaryImageGameObject = null);
    }

    public void FreeNewQueuePosition()
    {
        if (temporaryImageGameObject)
            temporaryImageGameObject.transform
                .DOLocalMove(new(-1000, temporaryImageGameObject.transform.localPosition.y, 0), 0.2f)
                .SetEase(Ease.OutExpo).OnComplete(() =>
                {
                    temporaryImageGameObject.SetActive(false);
                    temporaryImageGameObject = null;
                });
    }

    private void PopQueue()
    {
        currentActiveUnit = queue[0];
        for (var i = 0; i < queue.Count; i++)
        {
            var unit = queue[i];
            var obj = queueImageInUse.Find(tuple => tuple.Item1 == unit).Item2;
            if (i == 0)
            {
                obj.transform.DOLocalMove(new Vector3(-5000, 0, 0), 0.2f)
                    .SetEase(Ease.OutExpo).OnComplete(() => obj.SetActive(false));
            }
            else
            {
                obj.transform.DOLocalMove(new Vector3(-385 + (i - 1) * 115, 0, 0), 0.2f).SetEase(Ease.OutExpo);
            }
        }
        queue.RemoveAt(0);
    }

    private void OrderQueue(bool initialize = false)
    {
        if (initialize)
        {
            for (var i = queueImageInUse.Count - 1; i >= 0; i--)
            {
                queueImageInUse.RemoveAt(i);
            }

            foreach (var t in queueImages)
            {
                t.SetActive(false);
            }

            for (var i = 0; i < queue.Count; i++)
            {
                var temp = queueImages[i];
                queueImageInUse.Add(new Tuple<Unit, GameObject>(queue[i], temp));
                temp.SetActive(true);
                temp.GetComponent<Image>().sprite = queue[i].hudImage;
            }
        }
        

        queue.Sort((unit, unit1) => unit.QueueTimeValue <= unit1.QueueTimeValue ? -1 : 1);
        for (int i = 0; i < queue.Count; i++)
        {
            var temp = queueImageInUse.Find((e) => e.Item1 == queue[i]).Item2;
            temp.SetActive(true);
            temp.GetComponent<RectTransform>().localPosition = new Vector3(-385 + i * 115, 0, 0);
        }
    }

    private void RemoveUnitFromQueue(Unit unit)
    {
        if (queue.Contains(unit))
        {
            queueImageInUse.Find( e => e.Item1 == unit).Item2.SetActive(false);
            queue.Remove(unit);
            OrderQueue(true);
        }
    }

    private void ResetQueue()
    {
        foreach (var t in queueImageInUse)
        {
            t.Item2.SetActive(false);
        }

        queueImageInUse.Clear();
    }

    #endregion

    #region Damage Ui

    private int displayIndex;
    private int activeDisplayCount;
    public IEnumerator DisplayDamageNumber(int damage)
    {
        
        var temp = TrySpawnDamageDisplay();
        if (!temp) yield break;
        activeDisplayCount++;
        temp.GetComponentInChildren<TMP_Text>().text = damage.ToString();
        
        var animator = temp.GetComponent<Animator>();
        yield return null;

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
        yield return new WaitForSeconds(0.2f);
        animator.SetTrigger(Exit);
        yield return null;
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && animator.IsInTransition(0));
        Destroy(temp);
        activeDisplayCount--;
    }
    
    private GameObject TrySpawnDamageDisplay()
    {
        var spawnArea = damageDisplayAreas[displayIndex].GetComponent<RectTransform>();
        displayIndex = (displayIndex + 1) % damageDisplayAreas.Count;

        // 1. Get the local boundaries of the spawn area
        var corners = new Vector3[4];
        spawnArea.GetLocalCorners(corners);
        
        // corners[0] is bottom-left, corners[2] is top-right
        var minX = corners[0].x;
        var maxX = corners[2].x;
        var minY = corners[0].y;
        var maxY = corners[2].y;

        // Get the size of the prefab's RectTransform to handle edge padding and overlap logic
        RectTransform prefabRect = damageDisplay.GetComponent<RectTransform>();

        var prefabWidth = prefabRect.rect.width;
        var prefabHeight = prefabRect.rect.height;

        // Pad the boundaries so the spawned object doesn't bleed past the edges of the panel
        minX += prefabWidth / 2f;
        maxX -= prefabWidth / 2f;
        minY += prefabHeight / 2f;
        maxY -= prefabHeight / 2f;
       
        var randomX = Random.Range(minX, maxX);
        var randomY = Random.Range(minY, maxY);

        var newSpawn = Instantiate(damageDisplay, spawnArea);
        var spawnRect = newSpawn.GetComponent<RectTransform>();
                
        // Force anchors to center to make localPosition placement predictable
        spawnRect.anchorMin = new Vector2(0.5f, 0.5f);
        spawnRect.anchorMax = new Vector2(0.5f, 0.5f);
        spawnRect.anchoredPosition = new Vector3(randomX, randomY, 0f);
        
        return newSpawn; 
    }

    #endregion

    #endregion

    #region Input

    #region Input calls

    public void Submit()
    {
        if(combatIsOver) winCanvas.GetComponent<WinScreenController>().Confirm();
        else if (currentActiveUnit && currentActiveUnit is PlayerUnit playerUnit)
        {
            currentSelectButton?.onClick.Invoke();
            StartCoroutine(playerUnit.Submit(targetUnit));
        }
    }

    public void Cancel()
    {
        if (currentActiveUnit && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.Cancel());
        }
    }

    public void SkillTab()
    {
        //do on player something-something instead next time
        if (currentActiveUnit && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.SkillTab());
        }
    }

    public void InspectTab()
    {
        if (currentActiveUnit && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.Inspect());
        }
    }

    public void SwitchTab()
    {
        if (currentActiveUnit && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.TabFunctionality());
        }
    }

    public void Navigate(Vector2 normalizedInput)
    {
        if(combatIsOver) winCanvas.GetComponent<WinScreenController>().Navigate(normalizedInput);
        
        if (!currentSelectButton || normalizedInput == Vector2.zero) return;
        var isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
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

        if (!selectable) return;
        SetCurrentSelectButton((Button)selectable);
        //friendly unit
        if (selectable.transform.parent.transform.parent.TryGetComponent(typeof(Unit), out var unitComponent ))
        {
            targetUnit = unitComponent as Unit;
        }
    }

    public void TriggerSpecificButtonAction()
    {
        if (currentSelectButton && currentSelectButton.TryGetComponent(typeof(GameButton), out var go))
        {
            (go as GameButton)?.OnSpecificAction.Invoke();
        }
    }

    #endregion

    #region Button Selects

    [SerializeField] private Button currentSelectButton;

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
            targetUnit = null;
        }
        else
        {
            List<Unit> temp = new List<Unit>();
            temp.AddRange(enemyUnits);
            temp.AddRange(playerUnits);
            foreach (var t in temp)
            {
                t?.SelectHUD(false);
                t?.ResetSelected();
            }
        }

        currentSelectButton = null;
    }

    public void SetCurrentSelectButton(Button button)
    {
        if (currentSelectButton && currentSelectButton.TryGetComponent(typeof(GameButton), out var component))
        {
            ( component as GameButton)?.OnDeselectEvent?.Invoke();
        }
        currentSelectButton = button;
        currentSelectButton?.Select();
        if (!currentSelectButton || !currentSelectButton.TryGetComponent(typeof(GameButton), out component)) return;
        {
            (component as GameButton)?.OnSelectEvent?.Invoke();
        }
    }

    public void DeselectButton()
    {
        SetCurrentSelectButton(null);
    }

    #endregion

    #endregion
}