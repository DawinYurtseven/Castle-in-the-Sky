using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{
    
    public static BattleSystem system;
    
    private readonly List<Unit>
        queue = new(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    [SerializeField] private PlayerUnit[] playerUnits;
    [SerializeField] private EnemyUnit[] enemyUnits;
    
    public Unit targetUnit;
    
    [SerializeField] public Camera battleCamera;
    /// <summary>
    /// 0- standard camera angle for when the camera should show all
    /// 1 - view towards the enemies
    /// 2 - view towards the player
    /// </summary>
    [SerializeField] private Transform[] cameraTargets;

    public UnityEvent<Unit, float> endOfTurnTrigger = new UnityEvent<Unit, float>();

    private Unit currentActiveUnit;

    private void Awake()
    {
        if(system == null) system = this;
        else Destroy(gameObject);
            
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

    void EndOfTurn(Unit currentUnit, float timeValue)
    {
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
        var startPos = battleCamera.gameObject.transform.position;
        var startRot = battleCamera.gameObject.transform.rotation;
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * 5f;
            battleCamera.gameObject.transform.position = Vector3.Lerp(startPos, target.position, timer);
            battleCamera.gameObject.transform.rotation = Quaternion.Lerp(startRot, target.rotation, timer);
            yield return null;
        }
        battleCamera.gameObject.transform.position = target.position;
        battleCamera.gameObject.transform.rotation = target.rotation;
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
        var startPos = battleCamera.gameObject.transform.position;
        var startRot = battleCamera.gameObject.transform.rotation;
        
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
                targetUnit = enemyUnits[0];
                currentSelectButton = enemyUnits[0].selected;
                targetUnit.SelectHUD(true);
                cameraTargets[1].LookAt(targetUnit.transform.position);
                break;
        }
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime * 5f;
            battleCamera.gameObject.transform.position = Vector3.Lerp(startPos, cameraTargets[targetIndex].position, timer);
            battleCamera.gameObject.transform.rotation = Quaternion.Lerp(startRot, cameraTargets[targetIndex].rotation, timer);
            yield return null;
        }
        battleCamera.gameObject.transform.position = cameraTargets[targetIndex].position;
        battleCamera.gameObject.transform.rotation = cameraTargets[targetIndex].rotation;
        
        
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
    }

    public void SkillTab()
    {
        //do on player something something instead next time
    }
    
    public void InspectTab()
    {
        
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
                    cameraTargets[1].LookAt(targetUnit.transform.position);
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