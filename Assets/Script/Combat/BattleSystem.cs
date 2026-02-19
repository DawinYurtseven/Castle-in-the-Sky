using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    
    public static BattleSystem system;
    
    private readonly List<Unit>
        queue = new(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    public List<PlayerUnit> playerUnits;
    public List<EnemyUnit> enemyUnits;
    
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
    [SerializeField] private Canvas winCanvas, loseCanvas;

    public UnityEvent<Unit, float> endOfTurnTrigger = new UnityEvent<Unit, float>();

    private Unit currentActiveUnit;

    private GameObject playerValueHorizontalGameObject, queueHorizontalGameObject;

    private void Awake()
    {
        if(system == null) system = this;
        else Destroy(gameObject);
        //TODO: instantiate the GUI here
        gameGUI.SetActive(true);
        playerValueHorizontalGameObject = gameGUI.transform.Find("Player value horizontal").gameObject;
        queueHorizontalGameObject = gameGUI.transform.Find("Queue").gameObject;
    }

    void Start()
    {
        StartOfCombat();
        SetAllPlayerValues();
    }


    void StartOfCombat()
    {
        endOfTurnTrigger?.AddListener(EndOfTurn);

        //first, order all the units based on their 'speed' stat
        queue.Clear();
        queue.AddRange(playerUnits);
        queue.AddRange(enemyUnits);
        OrderQueue();

        //trigger their Beginning Of Combat
        foreach (var unit in queue)
        {
            unit.BeginningOfCombat();
        }

        currentActiveUnit = queue[0];
        PopQueue();
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
        
    }


    private bool combatIsOver;

    void EndOfTurn(Unit currentUnit, float timeValue)
    {
        if(combatIsOver) return;
        MoveCameraToIndex(0);
        foreach (var unit in queue)
        {
            unit.PassTimeValue(timeValue);
        }
        queue.Add(currentUnit);
        OrderQueue();
        //maybe animations or something.
        currentActiveUnit = queue[0];
        PopQueue();
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
        /*if (currentActiveUnit is PlayerUnit playerUnit)
        {
            var index = playerUnits.IndexOf(playerUnit);
            //TODO: I forgo
        }*/
    }

    public void DeathOfUnit(Unit unit)
    {
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
            winCanvas?.gameObject.SetActive(true);
        }
        else
        {
            loseCanvas?.gameObject.SetActive(true);
        }
    }


    #region Camera, UI and Input

    [SerializeField] private Button currentSelectButton;

    [SerializeField] private GameObject gameGUI, playerValuePrefab,queueImagePrefab, temporaryImageGameObject;

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

    public void UpdatePlayerValues(PlayerUnit playerUnit)
    {
        var index = playerUnits.IndexOf(playerUnit);
        playerValues[index].GetComponentInChildren<TextMeshProUGUI>().text = $"HP:{playerUnit.HP}\nSP:{playerUnit.SP}";
    }

    public void ShowNewQueuePosition(Unit unit, float timeValue)
    {
        var index = 0;
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue[i].QueueTimeValue - timeValue > timeValue)
            {
                index = i;
                break;
            }
        }
        if (index == 0) index = queue.Count - 1;
        
        temporaryImageGameObject = Instantiate(queueImagePrefab, queueHorizontalGameObject.transform);
        temporaryImageGameObject.GetComponent<RectTransform>().localPosition = new(-335, -60, 0);
        temporaryImageGameObject.GetComponent<RectTransform>().DOLocalMove(new (-335 + index*115,-60, 0), 0.2f).SetEase(Ease.OutExpo);
        temporaryImageGameObject.GetComponent<Image>().sprite = unit.hudImage;
    }

    public void FreeNewQueuePosition()
    {
        if(temporaryImageGameObject != null) DestroyImmediate(temporaryImageGameObject);
    }

    private void PopQueue()
    {
        queue.RemoveAt(0);
        DestroyImmediate(queueHorizontalGameObject.transform.GetChild(0).gameObject);
        for (int i = queueHorizontalGameObject.transform.childCount -1; i >= 0; i--)
        {
            queueHorizontalGameObject.transform.GetChild(i).GetComponent<RectTransform>().localPosition = new (-385  + i*115, 0, 0);
        }
    }

    private void OrderQueue()
    {
        for (var i = queueHorizontalGameObject.transform.childCount -1; i >= 0; i--)
        {
            DestroyImmediate(queueHorizontalGameObject.transform.GetChild(i).gameObject);
        }
        queue.Sort((unit, unit1) => (int)(unit.QueueTimeValue - unit1.QueueTimeValue));
        for (int i = 0; i < queue.Count; i++)
        {
            var temp = Instantiate(queueImagePrefab, queueHorizontalGameObject.transform);
            temp.GetComponent<RectTransform>().localPosition = new (-385  + i*115, 0, 0);
            temp.GetComponent<Image>().sprite = queue[i].hudImage;
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
                for(int i = 0; i < enemyUnits.Count; i++)
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
                    enemyUnits[i].CalculateHUDValues(left,right);
                }
                if(targetUnit == null || targetUnit is not EnemyUnit)
                    targetUnit = enemyUnits[0];
                currentSelectButton = targetUnit.selected;
                targetUnit.SelectHUD(true, battleCamera.transform);
                var index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
                Vector3 interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position, enemyUnits[^1].transform.position, index/((float)enemyUnits.Count+1));
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

    public void InspectTab()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit playerUnit)
        {
            StartCoroutine(playerUnit.Inspect());
        }
    }

    public void ClearSelection()
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
                    int index;
                    Vector3 interpolatedPosition;
                    if(unit is EnemyUnit enemy)
                    {
                        index = enemyUnits.IndexOf(enemy) +1 ;
                        interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position, enemyUnits[^1].transform.position, index/((float)enemyUnits.Count+1));
                    }
                    else
                    {
                        index = playerUnits.IndexOf((PlayerUnit)unit) +1;
                        interpolatedPosition = Vector3.Lerp(playerUnits[0].transform.position, playerUnits[^1].transform.position, index/((float)playerUnits.Count+1));
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