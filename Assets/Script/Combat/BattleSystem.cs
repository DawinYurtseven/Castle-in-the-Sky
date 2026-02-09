using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleSystem : MonoBehaviour
{
    
    public static BattleSystem system;
    
    private readonly List<Unit>
        queue = new(); // I will be reordering this queue whenever an action has been done, so no actual queue 

    [SerializeField] private PlayerUnit[] playerUnits;
    [SerializeField] private EnemyUnit[] enemyUnits;
    
    
    [SerializeField] public Camera battleCamera;
    private Vector3 basePosition;
    private Quaternion baseRotation;

    public UnityEvent<Unit, float> endOfTurnTrigger = new UnityEvent<Unit, float>();

    private Unit currentActiveUnit;

    private void Awake()
    {
        if(system == null) system = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        basePosition = battleCamera.transform.position;
        baseRotation = battleCamera.transform.rotation;
        StartOfCombat();
    }


    void StartOfCombat()
    {
        endOfTurnTrigger?.AddListener(EndOfTurn);

        //first, order all the units based on their 'speed' stat

        queue.AddRange(playerUnits);
        queue.AddRange(enemyUnits);
        queue.Sort((unit, unit1) => (int)(unit.TimeValue - unit1.TimeValue));


        //trigger their Beginning Of Combat
        foreach (var unit in queue)
        {
            unit.BeginningOfCombat();
        }

        currentActiveUnit = queue[0];
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit player) SeeSituationDebug(player);
        queue.RemoveAt(0);
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
    }

    void EndOfTurn(Unit currentUnit, float timeValue)
    {
        foreach (var unit in queue)
        {
            unit.PassTimeValue(timeValue);
        }
        queue.Add(currentUnit);
        queue.Sort((unit, unit1) => (int)(unit.TimeValue - unit1.TimeValue));
        //maybe animations or something.
        currentActiveUnit = queue[0];
        queue.RemoveAt(0);
        StartCoroutine(currentActiveUnit.BeginningOfTurn());
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit player) {SeeSituationDebug(player);}
    }


    #region Camera, UI

    public IEnumerator MoveCamera(Transform target)
    {
        var startPos = battleCamera.gameObject.transform.position;
        var startRot = battleCamera.gameObject.transform.rotation;
        float timer = 0f;
        while (timer < 1.5f)
        {
            timer += Time.unscaledDeltaTime;
            battleCamera.gameObject.transform.position = Vector3.Lerp(startPos, target.position, timer);
            battleCamera.gameObject.transform.rotation = Quaternion.Lerp(startRot, target.rotation, timer);
            yield return null;
        }
        battleCamera.gameObject.transform.position = target.position;
        battleCamera.gameObject.transform.rotation = target.rotation;
    }

    public IEnumerator MoveCameraBack()
    {
        var startPos = battleCamera.gameObject.transform.position;
        var startRot = battleCamera.gameObject.transform.rotation;
        float timer = 0f;
        while (timer < 1.5f)
        {
            timer += Time.unscaledDeltaTime;
            battleCamera.gameObject.transform.position = Vector3.Lerp(startPos, basePosition, timer);
            battleCamera.gameObject.transform.rotation = Quaternion.Lerp(startRot, baseRotation, timer);
            yield return null;
        }
        battleCamera.gameObject.transform.position = basePosition;
        battleCamera.gameObject.transform.rotation = baseRotation;
    }

    private void SeeSituationDebug(PlayerUnit player)
    {
        switch (player.currentState)
        {
            case PlayerUnit.CombatState.Root:
                Debug.Log($"options: \nX for attack\nSquare for skill\nR1 for inspect");
                break;
            case PlayerUnit.CombatState.Skill:
                string skills = "";
                foreach (var skill in player.skills )
                {
                    skills += $"{skill.Key} is of type {skill.Value.ToString()}\n";
                }
                Debug.Log(skills);
                break;
            case PlayerUnit.CombatState.Inspect:
                break;
            case PlayerUnit.CombatState.TargetEnemy:
                foreach (var enemy in enemyUnits)
                {
                    Debug.Log(enemy.name);
                }
                break;
            case PlayerUnit.CombatState.TargetAlly:
                foreach (var playerUnit in playerUnits)
                {
                    Debug.Log(playerUnit.name);
                }
                break;
        }
    }

    #endregion

    #region Input

    public void Submit()
    {
        if (currentActiveUnit != null && currentActiveUnit is PlayerUnit player)
        {
            player.Submit();
            if(player.currentState != PlayerUnit.CombatState.Root)SeeSituationDebug(player);
        }
    }

    public void Cancel()
    {
        
    }

    public void SkillTab()
    {
        
    }
    
    public void InspectTab()
    {
        
    }
    
    

    #endregion
}