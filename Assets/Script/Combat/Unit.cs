using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    #region Stats

    /// <summary>
    /// The idea of stats needs to be kept simple. Too many and leveling them would be difficult and hard to heep up with.
    /// Each stat starts at 1 allowing diminishing returns to calculate changes in stats, items and so on.
    /// 
    /// -   Strength: this stat determines damage. Any damage should have Strength involved.
    ///     Any skill that deals damage has its damage scaled with this stat
    /// -   Constitution: the HP, defence and resistance to status effects scale with this.
    ///     Healing effects are also scaled with this stat.
    /// -   Speed: dictate the Time value and dodge possibility. The higher the speed, the faster the unit can act and
    ///     the more likely it is to dodge an attack or have the enemy not dodge its attack.
    /// -   Intelligence: this stats calculates the usage of skills. The higher the intelligence,
    ///     the more skills a unit can use until end of combat. the deeper idea is that intelligence increases
    ///     the amount of Skill Points a unit has and the regeneration of said Points
    /// -   Luck: there will be a lot of chance triggers and special item effects as well.
    ///     Critical hits, item rolls, skill effects and so on. Luck is the only stat that can influence anything.
    ///     But unlike the other ones, this one should be treated as a late-bloomer stat
    ///     and should show its effects later in a run.
    /// </summary>
    [SerializeField] private int strength = 1, constitution = 1, speed = 1, intelligence = 1, luck = 1;

    public int Strength
    {
        get => strength;
        set => strength = value;
    }

    public int Constitution
    {
        get => constitution;
        set => constitution = value;
    }

    public int Speed
    {
        get => speed;
        set => speed = value;
    }

    public int Intelligence
    {
        get => intelligence;
        set => intelligence = value;
    }

    public int Luck
    {
        get => luck;
        set => luck = value;
    }

    /// <summary>
    /// These are the stats that are calculated from the base stats and can not be influence directly from outside.
    /// </summary>
    private int maxHP, currentHP, maxSP, currentSP;

    private float timeValue, constantReduction = 0.3f; //this stat is the bread and butter of this combat system. 

    public float TimeValue => timeValue;

    #endregion

    #region Items

    //TODO: Look at after implementing items. For now, think of it as a dictionary of <items,int> where the int is the amount of that item the unit has. 

    #endregion

    #region Skills

    public enum SkillTypes
    {
        damage,
        buff,
        debuff,
        heal
    }
    
    public Dictionary<string,SkillTypes> skills = new Dictionary<string,SkillTypes>();

    //TODO: Same for skills.

    #endregion

    #region Components

    /// <summary>
    /// 0- standard camera angle for when it is players turn
    /// 1- view towards the enemies
    /// maybe I'll need later more so this is an array now
    /// </summary>
    [SerializeField] private Transform[] cameraTargets; // this is for the camera to move to depending on the situation.

    /// <summary>
    /// 0- base target for other units to go to when performing a 1-1 action
    /// </summary>
    [SerializeField] private Transform[] positionTargets;

    [SerializeField] internal Animator animator;

    private Vector3 startPosition;

    #endregion

    #region Combat

    public UnityEvent BasicAttackTrigger,
        BeginningOfCombatTrigger,
        BeginningOfTurnTrigger,
        EndOfTurnTrigger,
        EndOfCombatTrigger,
        ActionTakenTrigger,
        ReactionDoneTrigger,
        CriticalTrigger;

    public UnityEvent<object> SkillUsagTrigger;

    private UnityEvent<Unit, float> EndOfTurnSystemEvent;

    private void OnEnable()
    {
        //TODO: change with when the need for the event is there to be created if null, otherwise keep empty
        BasicAttackTrigger ??= new UnityEvent();
        BeginningOfCombatTrigger ??= new UnityEvent();
        BeginningOfTurnTrigger ??= new UnityEvent();
        EndOfTurnTrigger ??= new UnityEvent();
        EndOfCombatTrigger ??= new UnityEvent();
        ActionTakenTrigger ??= new UnityEvent();
        ReactionDoneTrigger ??= new UnityEvent();
        CriticalTrigger ??= new UnityEvent();
    }

    private void Start()
    {
        CalculateStats();

        //Something to instantiate the mesh at the position.
        startPosition = transform.position;
        
        //calculate time value at the beginning of combat
        CalculateTimeValue(2f);
    }

    private void CalculateStats()
    {
        //TODO: Balance this shit after playing
        if (maxHP == 0)
        {
            maxHP = currentHP = constitution * 10;
        }
        else
        {
            float percentHP = currentHP / maxHP;
            maxHP = constitution * 10;
            currentHP = Mathf.CeilToInt(percentHP * maxHP);
        }

        if (maxSP == 0)
        {
            maxSP = currentSP = intelligence * 5;
        }
        else
        {
            float percentSP = currentSP / maxSP;
            maxSP = intelligence * 5;
            currentSP = Mathf.CeilToInt(percentSP * maxSP);
        }
    }

    internal void CalculateTimeValue(float newTimeValue)
    {
        timeValue += newTimeValue - constantReduction * Mathf.Log(speed, 99);
    }

    public void PassTimeValue(float passedTime)
    {
        timeValue -= passedTime;
    }

    public virtual void BasicAttack()
    {
        BasicAttackTrigger?.Invoke();
        EndTurn();
    }

    public virtual void SkillUsage(SkillTypes type) //change this later
    {
        SkillUsagTrigger?.Invoke(type);
    }

    public virtual void BeginningOfCombat(UnityEvent<Unit, float> e)
    {
        //prep shit here, maybe take this out later when battlesystem can call these.
        BeginningOfCombatTrigger?.Invoke();
        EndOfTurnSystemEvent = e;
    }

    public virtual IEnumerator BeginningOfTurn()
    {
        timeValue = 0;
        yield return null;
        BeginningOfTurnTrigger?.Invoke();
    }

    public void EndTurn()
    {
        EndOfTurnTrigger?.Invoke();
        EndOfTurnSystemEvent.Invoke(this, timeValue);
    }

    #endregion
}