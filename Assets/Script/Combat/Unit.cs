using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    /// -   Intelligence: this stat calculates the usage of skills. The higher the intelligence,
    ///     the more skills a unit can use until end of combat. the deeper idea is that intelligence increases
    ///     the amount of Skill Points a unit has and the regeneration of said Points
    /// -   Luck: there will be a lot of chance triggers and special item effects as well.
    ///     Critical hits, item rolls, skill effects and so on. Luck is the only stat that can influence anything.
    ///     But unlike the other ones, this one should be treated as a late-bloomer stat
    ///     and should show its effects later in a run.
    /// </summary>
    [Header("Stats")] 
    [SerializeField] protected int strength = 1;
    [SerializeField] protected int constitution = 1;
    [SerializeField] protected int speed = 1;
    [SerializeField] protected int intelligence = 1;
    [SerializeField] protected int luck = 1;

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
    public int MaxHP, CurrentHP, MaxSP, CurrentSP;

    public int HP => CurrentHP;
    public int SP => CurrentSP;

    [SerializeField] public float
        critAmount,
        critChance,
        damageAddition,
        damageMultiplier; //these are for the damage calculation and critical hits.

    internal float TimeValue; //this stat is the bread and butter of this combat system. 
    private const float ConstantReduction = 0.3f; //this stat is the bread and butter of this combat system. 

    public float QueueTimeValue => TimeValue;
    public List<Unit> currentTarget;

    #endregion

    #region Items

    [Header("Items")]
    //TODO: Look at after implementing items. For now, think of it as a dictionary of <items,int> where the int is the amount of that item the unit has. 

    public bool repeated;
    public bool blocked;
    public float bufferedDamage;

    #endregion

    #region Skills

    [Header("Skills")] 
    [SerializeField] protected List<SkillNames> Skills = new();
    internal Skill SelectedSkill;

    //TODO: Same for skills.

    #endregion

    #region Components

    /// <summary>
    /// 0- base target for other units to go to when performing a 1-1 action
    /// </summary>
    ///
    [Header("Components")]
    [SerializeField] protected Transform[] positionTargets;

    [SerializeField] internal Animator animator;
    [SerializeField] internal Canvas hudCanvas;

    public Button selected;

    private Vector3 startPosition;

    //image for Queue and player values
    public Sprite hudImage;

    #endregion

    #region Combat

    [Header("Combat")]
    public UnityAction<Unit> BasicAttackTrigger,
        BeginningOfCombatTrigger,
        BeginningOfTurnTrigger,
        EndOfTurnTrigger,
        EndOfCombatTrigger,
        ActionTakenTrigger,
        ReactionDoneTrigger,
        CriticalTrigger;

    public UnityAction<Unit,object> SkillUsageTrigger;


    protected void Awake()
    {
        CalculateStats();

        //Something to instantiate the mesh at the position.
        startPosition = transform.position;

        //calculate time value at the beginning of combat
        TimeValue = CalculateTimeValue(2f);
    }

    private void CalculateStats()
    {
        //TODO: Balance this shit after playing
        if (MaxHP == 0)
        {
            MaxHP = CurrentHP = constitution * 10;
        }
        else
        {
            var percentHP = (float)CurrentHP / MaxHP;
            MaxHP = constitution * 10;
            CurrentHP = Mathf.CeilToInt(percentHP * MaxHP);
        }

        if (MaxSP == 0)
        {
            MaxSP = CurrentSP = intelligence * 5;
        }
        else
        {
            var percentSP = (float)CurrentSP / MaxSP;
            MaxSP = intelligence * 5;
            CurrentSP = Mathf.CeilToInt(percentSP * MaxSP);
        }

        damageMultiplier = 1;
    }

    internal float CalculateTimeValue(float newTimeValue)
    {
        return newTimeValue - ConstantReduction * Mathf.Log(speed, 99);
    }

    public void PassTimeValue(float passedTime)
    {
        TimeValue -= passedTime;
    }

    //TODO: Think about hwhat can be done to affect the damage on unit

    protected virtual IEnumerator BasicAttack()
    {
        TimeValue += CalculateTimeValue(1f);
        BattleSystem.system.AcceptNewQueuePosition(this, TimeValue);

        yield return BattleSystem.system.MoveCameraToIndexTransform(0);
        
        do
        {
            yield return transform.DOMove(currentTarget[0].positionTargets[0].position, 0.2f).SetEase(Ease.InExpo)
                     .WaitForCompletion();
            
            ActionTakenTrigger?.Invoke(this);
            BasicAttackTrigger?.Invoke(this);

            var baseDamage = (strength + damageAddition) * damageMultiplier;
            var totalDamage = Random.Range(0, 100) < critChance ? baseDamage * critAmount / 100 : baseDamage;

            //move object towards target

            yield return transform.DOMove(currentTarget[0].positionTargets[0].position, 0.2f).SetEase(Ease.InExpo)
                .WaitForCompletion();
            //TODO: Do some anime shit 
            yield return new WaitForSeconds(0.3f);

            currentTarget[0].TakeDamage(totalDamage);
            yield return transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        } while (repeated && currentTarget[0].CurrentHP > 0);

        yield return EndTurn();
    }

    protected virtual IEnumerator SkillUsage() //change this later
    {
        TimeValue += CalculateTimeValue(SelectedSkill.timeValue);
        //TODO: make skill cost cut with a global function
        bool validAction;
        do
        {
            ActionTakenTrigger?.Invoke(this);
            SkillUsageTrigger?.Invoke(this,SelectedSkill.type);
            validAction = SelectedSkill.Execute(this);
            
            yield return new WaitForSeconds(0.2f);
        } while (repeated && validAction);
        
        yield return EndTurn();
    }

    public virtual void BeginningOfCombat()
    {
        //prep shit here, maybe take this out later when battle system can call these.
        BeginningOfCombatTrigger?.Invoke(this);
    }

    public virtual IEnumerator BeginningOfTurn()
    {
        TimeValue = 0;
        yield return null;
        BeginningOfTurnTrigger?.Invoke(this);
    }

    protected IEnumerator EndTurn()
    {
        yield return transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        EndOfTurnTrigger?.Invoke(this);
        BattleSystem.system.EndOfTurnTrigger.Invoke(this, TimeValue);
    }

    public virtual void TakeDamage(float damage)
    {
        bufferedDamage = damage;
        ReactionDoneTrigger?.Invoke(this);
        if (blocked)
        {
            blocked = false;
            bufferedDamage = 0;
            return;
        }
        CurrentHP -= (int)damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        //TODO: maybe an event here as well?
        if (CurrentHP <= 0)
        {
            BattleSystem.system.DeathOfUnit(this);
            gameObject.SetActive(false);
        }
    }

    public void SelectHUD(bool active, Transform toLookAt = null)
    {
        hudCanvas.gameObject.SetActive(active);
        hudCanvas.transform.LookAt(toLookAt);
    }


    public void SetCurrentTarget(List<Unit> units)
    {
        currentTarget = units;
    }

    #endregion
}