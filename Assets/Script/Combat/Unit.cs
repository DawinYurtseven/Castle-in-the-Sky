using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
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
    public int maxHP,currentHP,maxSP, currentSP;

    public int HP => currentHP;
    public int SP => currentSP;

    [SerializeField] public float
        critAmount,
        critChance,
        damageAddition,
        damageMultiplier; //these are for the damage calculation and critical hits.

    private float timeValue; //this stat is the bread and butter of this combat system. 
    private const float ConstantReduction = 0.3f; //this stat is the bread and butter of this combat system. 

    public float QueueTimeValue => timeValue;
    public List<Unit> currentTarget;

    #endregion

    #region Items

    [Header("Items")]
    //TODO: Look at after implementing items. For now, think of it as a dictionary of <items,int> where the int is the amount of that item the unit has. 

    public bool repeated;
    public bool blocked;
    public float bufferedDamage;

    public readonly List<Items> Items = new();

    #endregion

    #region Skills
    
    [Header("Skills")] 
    [SerializeField] protected List<SkillNames> skills = new();
    internal Skill SelectedSkill;
    public int SkillCount => skills.Count;
    public Skill GetSkill(int index) => Skill.GetSkill(skills[index]);

    //TODO: Same for skills.

    #endregion

    #region Components
    
    /// <summary>
    /// 0- base target for other units to go to when performing a 1-1 action
    /// </summary>
    ///
    [Header("Components")] 
    [SerializeField] protected Transform[] positionTargets;
    [SerializeField] public Transform[] cameraTargets;

    [SerializeField] internal Animator animator;
    [SerializeField] internal Canvas hudCanvas;

    public Button selected;

    private Vector3 startPosition;

    //image for Queue and player values
    public Sprite hudImage;
    public TextMeshProUGUI hudValues;

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

    internal virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ResetSelected();
    }

    public void ResetSelected()
    {
        selected.GetComponent<GameButton>().OnSelectEvent = () =>
        {
            BattleSystem.system.SetSelection(this);
        };
    }

    public void RotateSelected(Transform target)
    {
        hudCanvas.transform.rotation = Quaternion.Euler
        (
            0,
            target.transform.rotation.eulerAngles.y +180f,
            0
        );
        BattleSystem.system.SetSelection(this);
    }

    private void CalculateStats(bool reset = false)
    {
        //TODO: Balance this shit after playing
        if (maxHP == 0 || reset)
        {
            maxHP = currentHP = constitution * 10;
        }
        else
        {
            var percentHP = (float)currentHP / maxHP;
            maxHP = constitution * 10;
            currentHP = Mathf.CeilToInt(percentHP * maxHP);
        }

        if (maxSP == 0 || reset)
        {
            maxSP = currentSP = intelligence * 5;
        }
        else
        {
            var percentSP = (float)currentSP / maxSP;
            maxSP = intelligence * 5;
            currentSP = Mathf.CeilToInt(percentSP * maxSP);
        }

        damageMultiplier = 1;
    }

    internal float CalculateTimeValue(float newTimeValue)
    {
        return newTimeValue - ConstantReduction * Mathf.Log(speed, 99);
    }

    public void PassTimeValue(float passedTime)
    {
        timeValue -= passedTime;
    }

    //TODO: Think about hwhat can be done to affect the damage on unit

    protected virtual IEnumerator BasicAttack()
    {
        timeValue += CalculateTimeValue(1f);
        BattleSystem.system.AcceptNewQueuePosition(this, timeValue);
        
        do
        {
            yield return transform.DOMove(currentTarget[0].positionTargets[0].position, 0.2f).SetEase(Ease.InExpo)
                     .WaitForCompletion();
            
            ActionTakenTrigger?.Invoke(this);
            BasicAttackTrigger?.Invoke(this);

            var baseDamage = (strength + damageAddition) * damageMultiplier;
            var totalDamage = Random.Range(0, 100) < critChance ? baseDamage * critAmount / 100 : baseDamage;

            //move object towards target
            yield return BattleSystem.system.MoveCamera(null, BattleSystem.CameraTargets.Empty);
            yield return transform.DOMove(currentTarget[0].positionTargets[0].position, 0.2f).SetEase(Ease.InExpo)
                .WaitForCompletion();
            //TODO: Do some anime shit 
            yield return new WaitForSeconds(0.3f);
            animator.Play("Attack");
            yield return null;
            while(!(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0)))
            {
                BattleSystem.system.battleCamera.transform.position = cameraTargets[0].position;
                BattleSystem.system.battleCamera.transform.rotation = cameraTargets[0].rotation;
                yield return null;
            }

            currentTarget[0].TakeDamage(totalDamage);// change with animation events
            yield return BattleSystem.system.MoveCamera(null, BattleSystem.CameraTargets.Empty);
            yield return transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        } while (repeated && currentTarget[0].currentHP > 0);

        yield return EndTurn();
    }

    protected virtual IEnumerator SkillUsage() //change this later
    {
        timeValue += CalculateTimeValue(SelectedSkill.timeValue);
        currentSP -= SelectedSkill.skillCost;
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

    public void BeginningOfCombat()
    {
        CalculateStats(true);

        //Something to instantiate the mesh at the position.
        startPosition = transform.position;

        //calculate time value at the beginning of combat
        timeValue = CalculateTimeValue(2f);
        //prep shit here, maybe take this out later when battle system can call these.
        BeginningOfCombatTrigger?.Invoke(this);
    }

    public virtual IEnumerator BeginningOfTurn()
    {
        timeValue = 0;
        yield return null;
        BeginningOfTurnTrigger?.Invoke(this);
    }

    protected virtual IEnumerator EndTurn()
    {
        yield return transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        EndOfTurnTrigger?.Invoke(this);
        BattleSystem.system.EndOfTurnTrigger.Invoke(this, timeValue);
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
        currentHP -= (int)damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        //TODO: maybe an event here as well?
        if (currentHP <= 0)
        {
            BattleSystem.system.DeathOfUnit(this);
            gameObject.SetActive(false);
        }
    }

    public void SelectHUD(bool active, Transform toLookAt = null)
    {
        if (toLookAt != null)
            hudCanvas.transform.rotation = Quaternion.Euler
            (
                0,
                toLookAt.transform.rotation.eulerAngles.y +180f,
                0
            );
        hudCanvas.gameObject.SetActive(active);
    }


    protected void SetCurrentTarget(List<Unit> units)
    {
        currentTarget = units;
    }
    
    public void CalculateHUDValues(Button left = null, Button right = null)
    {
        hudValues.text = $"{name}\nHP: {currentHP}/{maxHP}\nSP: {currentSP}/{maxSP}";
        hudCanvas.gameObject.transform.LookAt(BattleSystem.system.battleCamera.gameObject.transform.position);
        var navigation = new Navigation();
        if (left != null)
        {
            navigation.selectOnLeft = left;
        }

        if (right != null)
        {
            navigation.selectOnRight = right;
        }

        selected.navigation = navigation;
    }

    #endregion
}