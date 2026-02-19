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

    public Transform inFrontOfEnemies, inFrontOfPlayers;
    [SerializeField] private Canvas winCanvas, loseCanvas;

    public UnityEvent<Unit, float> endOfTurnTrigger = new UnityEvent<Unit, float>();

    private Unit currentActiveUnit;

    private GameObject playerValueHorizontalGameObject, queueHorizontalGameObject;

    private void Awake()
    {
        if(system == null) system = this;
        else Destroy(gameObject);
        
        battleCamera ??= Camera.main;
        
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
        foreach (var unit in queue)
        {
            Debug.Log(unit.name);
        }
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
            winCanvas?.gameObject.SetActive(true);
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
        temporaryImageGameObject.GetComponent<RectTransform>().DOLocalMove(new (-435 + index*115,-60, 0), 0.2f).SetEase(Ease.OutExpo);
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
        for (int i = 0; i < queueHorizontalGameObject.transform.childCount; i++)
        {
            var child = queueHorizontalGameObject.transform.GetChild(i);
            if (child.localPosition.x >= -385 + index * 115)
            {
                child.DOLocalMove(child.localPosition + new Vector3(115, 0, 0), 0.2f).SetEase(Ease.OutExpo);
            }
        }

        if(temporaryImageGameObject != null)
            temporaryImageGameObject?.GetComponent<RectTransform>().DOLocalMove(new(-385 + index * 115, 0, 0), 0.2f)
            .SetEase(Ease.OutExpo).OnComplete(() => temporaryImageGameObject = null);
    }

    public void FreeNewQueuePosition()
    {
        if(temporaryImageGameObject != null) temporaryImageGameObject.transform.DOLocalMove(new(-1000,temporaryImageGameObject.transform.localPosition.y,0), 0.2f).SetEase(Ease.OutExpo).OnComplete(() => DestroyImmediate(temporaryImageGameObject));
    }

    private void PopQueue()
    {
        queue.RemoveAt(0);
        for (int i = 0; i <queueHorizontalGameObject.transform.childCount; i++)
        {
            if (i == 0)
            {
                var child = queueHorizontalGameObject.transform.GetChild(0);
                queueHorizontalGameObject.transform.GetChild(0).DOLocalMove(new(-5000,0,0), 0.2f).SetEase(Ease.OutExpo).OnComplete(()=> DestroyImmediate(child.gameObject));
            }
            else
            {
                queueHorizontalGameObject.transform.GetChild(i).GetComponent<RectTransform>().DOLocalMove( new (-385  + (i-1)*115, 0, 0), 0.2f).SetEase(Ease.OutExpo);
            }
        }
    }

    private void OrderQueue()
    {
        for (var i = queueHorizontalGameObject.transform.childCount -1; i >= 0; i--)
        {
            DestroyImmediate(queueHorizontalGameObject.transform.GetChild(i).gameObject);
        }
        queue.Sort((unit, unit1) => unit.QueueTimeValue <= unit1.QueueTimeValue ? -1 : 1);
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
                currentSelectButton.Select();
                targetUnit.SelectHUD(true, battleCamera.transform);
                var index = enemyUnits.IndexOf((EnemyUnit)targetUnit) + 1;
                Vector3 interpolatedPosition = Vector3.Lerp(enemyUnits[0].transform.position, enemyUnits[^1].transform.position, index/((float)enemyUnits.Count+1));
                cameraTargets[1].LookAt(interpolatedPosition);
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
                enemyUnits[0].selected.onClick.AddListener(() =>
                {
                    currentActiveUnit.SetCurrentTarget(new List<Unit>(enemyUnits));
                });
                middlePointEnemy /= enemyUnits.Count;
                cameraTargets[1].LookAt(middlePointEnemy);
                targetIndex = 1;
                break;
            case 4: // all players
                targetIndex = 2;
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
        var skillTab = gameGUI.transform.Find("Skill Tab");
        var queueTab = gameGUI.transform.Find("Queue");
        if (isVisible && skills is { Count: > 0 } && playerUnit != null)
        {
            skillTab.localPosition = new Vector3(1452, 0, 0);
            skillTab.gameObject.SetActive(true);
            skillTab.DOLocalMove(new(468,0,0), 0.2f).SetEase(Ease.OutExpo);
            for (int i = 0; i < skills.Count; i++)
            {
                var skillObj = Instantiate(skillTabPrefab, skillTab);
                
                skillObj.transform.Find("Skill Name").GetComponent<TextMeshProUGUI>().text = skills[i].name;
                skillObj.transform.Find("Skill Description").GetComponent<TextMeshProUGUI>().text =
                    skills[i].skillDescription;
                skillObj.transform.Find("Skill Cost").GetComponent<TextMeshProUGUI>().text = skills[i].skillCost.ToString();
                
                skillObj.GetComponent<RectTransform>().localPosition = new(0, -2000, 0);
                skillObj.GetComponent<RectTransform>().DOLocalMove(new(0, 0  - 100 * i, 0), 0.1f + 0.05f*i).SetEase(Ease.OutExpo);
            }

            for (int i = 0; i < skills.Count; i++)
            {
                var skillObj = skillTab.GetChild(i).GetComponent<Button>();
                if (skillObj == null) continue;

                Skill skill = skills[i];
                skillObj.onClick.AddListener(() => playerUnit.selectedSkill = skill);
                
                if (i != 0)
                {
                    var nav = skillObj.navigation;
                    nav.selectOnUp = skillTab.GetChild(i-1).GetComponent<Button>();
                    skillObj.navigation = nav;
                }

                if (i != skills.Count - 1)
                {
                    var nav = skillObj.navigation;
                    nav.selectOnDown = skillTab.GetChild(i+1).GetComponent<Button>();
                    skillObj.navigation = nav;
                }
            }

            currentSelectButton = skillTab.GetChild(0).GetComponent<Button>();
        }
        else
        {
            for (int i = skillTab.childCount -1 ; i >=0; i--)
            {
                DestroyImmediate(skillTab.GetChild(i).gameObject);
            }
            skillTab.DOLocalMove(new (1452, 0, 0), 0.2f).SetEase(Ease.OutExpo).OnComplete(() => skillTab.gameObject.SetActive(false));
        }
        
        //TODO: make actual animation for the queue tab to be enabled
        queueTab.gameObject.SetActive(!isVisible);
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