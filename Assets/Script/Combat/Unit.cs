using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
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

    [Header("Scaling Curves")] 
    [SerializeField] internal float strCurve;
    [SerializeField] internal float conCurve;
    [SerializeField] internal float spdCurve;
    [SerializeField] internal float intCurve;
    [SerializeField] internal float lckCurve;  
    

    /// <summary>
    /// These are the stats that are calculated from the base stats and can not be influence directly from outside.
    /// </summary>
    [Header("Temporary Stats")] 
    public int maxHP;
    public int currentHP;
    public int maxSP;
    public int currentSP;

    public int HP => currentHP;
    public int SP => currentSP;

    [SerializeField] public float
        critAmount,
        critChance,
        damageAddition,
        damageMultiplier = 1f; //these are for the damage calculation and critical hits.

    private const float ConstantReduction = 0.3f; //this stat is the bread and butter of this combat system. 

    public float QueueTimeValue { get; private set; }

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
    [SerializeReference, SubclassSelector] public List<Skill> skills= new();

    internal Skill SelectedSkill;
    public int SkillCount => skills.Count;
    public Skill GetSkill(int index) => skills[index];

    //TODO: Same for skills.

    #endregion

    #region Components

    public List<Unit> currentTargets;
    public Unit currentTarget;

    /// <summary>
    /// 0- base target for other units to go to when performing a 1-1 action
    /// </summary>
    ///
    [Header("Components")] 
    [SerializeField] internal Transform[] positionTargets;
    /// <summary>
    /// 0 - base 
    /// </summary>
    [SerializeField] public Transform[] cameraTargets;

    [SerializeField] internal Animator animator;
    [SerializeField] internal Canvas hudCanvas;

    public Button selected;

    private Vector3 startPosition;

    //image for Queue and player values
    public Sprite hudImage;
    public TextMeshProUGUI hudValues;
    
    //this dictionary is to count how many status effect and of what type they are so that skills/effects that count them can work
    public Dictionary<StatusEffect, int> StatusCounts = new();

    #endregion

    #region Combat

    [Header("Combat")] public UnityAction<Unit> BasicAttackTrigger,
        BeginningOfCombatTrigger,
        BeginningOfTurnTrigger,
        EndOfTurnTrigger,
        EndOfCombatTrigger,
        ActionTakenTrigger,
        ReactionDoneTrigger,
        CriticalTrigger,
        DamageDealtTrigger;

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
            BattleSystem.Manager.SetSelection(this);
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
        BattleSystem.Manager.SetSelection(this);
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

        if (reset)
        {
            damageMultiplier = 1f;
            damageAddition = 0f;
        }
    }

    internal float CalculateTimeValue(float newTimeValue)
    {
        return newTimeValue - ConstantReduction * Mathf.Log(speed, 99);
    }

    public void PassTimeValue(float passedTime)
    {
        QueueTimeValue -= passedTime;
    }

    //TODO: Think about hwhat can be done to affect the damage on unit

    protected virtual IEnumerator BasicAttack()
    {
        QueueTimeValue += CalculateTimeValue(1f);
        BattleSystem.Manager.AcceptNewQueuePosition(this, QueueTimeValue);
        
        do
        {
            yield return transform.DOMove(currentTargets[0].positionTargets[0].position, 0.2f).SetEase(Ease.InExpo)
                     .WaitForCompletion();
            
            ActionTakenTrigger?.Invoke(this);
            BasicAttackTrigger?.Invoke(this);

            //move object towards target
            yield return BattleSystem.Manager.MoveCamera(null, BattleSystem.CameraTargets.Empty);
            yield return transform.DOMove(currentTargets[0].positionTargets[0].position, 0.2f).SetEase(Ease.InExpo)
                .WaitForCompletion();
            //TODO: Do some anime shit 
            yield return new WaitForSeconds(0.3f);
            animator.Play("Attack");
            yield return null;
            while(!(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0)))
            {
                BattleSystem.Manager.battleCamera.transform.position = cameraTargets[0].position;
                BattleSystem.Manager.battleCamera.transform.rotation = cameraTargets[0].rotation;
                yield return null;
            }
            yield return BattleSystem.Manager.MoveCamera(null, BattleSystem.CameraTargets.Empty);
            yield return transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        } while (repeated && currentTargets[0].currentHP > 0);

        yield return EndTurn();
    }

    ///This is for the Animation trigger to enter a float between (0, 1] to split the damage to the attack if multiple hits exists. 
    ///The total value of the split should be 1,
    /// so if there are 2 hits, the first one can pass 0.6 and the second one can pass 0.4,
    /// if there are 3 hits, the split can be 0.5, 0.3 and 0.2 and so on as example.
    ///
    /// This function should also have the damage calculation for the attack, so crits are individually calculated and not from one action
    public float CurrentTotalDamage
    {
        get;
        set;
    }
    
    public void DealDamage(float split)
    {
        var baseDamage = (strength + damageAddition) * (1+damageMultiplier) * split;
        var totalDamage = Random.Range(0, 100) < critChance ? baseDamage * (1 + critAmount / 100) : baseDamage;
        CurrentTotalDamage = totalDamage;
        
        foreach (var t in currentTargets)
        {
            t?.TakeDamage(totalDamage);
            currentTarget = t;
            DamageDealtTrigger?.Invoke(this);
        }
    }

    protected virtual IEnumerator SkillUsage() //change this later
    {
        QueueTimeValue += CalculateTimeValue(SelectedSkill.timeValue);
        currentSP -= SelectedSkill.skillCost;
        //TODO: make skill cost cut with a global function
        bool validAction;
        do
        {
            ActionTakenTrigger?.Invoke(this);
            SkillUsageTrigger?.Invoke(this,SelectedSkill.target);
            validAction = SelectedSkill.Execute(this);
            
            yield return new WaitForSeconds(0.2f);
        } while (repeated && validAction);
        
        yield return EndTurn();
    }

    public virtual void BeginningOfCombat()
    {
        CalculateStats(true);

        //Something to instantiate the mesh at the position.
        startPosition = transform.position;

        //calculate time value at the beginning of combat
        QueueTimeValue = CalculateTimeValue(2f);
        //prep shit here, maybe take this out later when battle system can call these.
        BeginningOfCombatTrigger?.Invoke(this);
    }

    public virtual IEnumerator BeginningOfTurn()
    {
        QueueTimeValue = 0;
        yield return null;
        BeginningOfTurnTrigger?.Invoke(this);
    }

    protected virtual IEnumerator EndTurn()
    {
        yield return transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutExpo).WaitForCompletion();
        EndOfTurnTrigger?.Invoke(this);
        BattleSystem.Manager.EndOfTurnTrigger.Invoke(this, QueueTimeValue);
    }

    public virtual void TakeDamage(float damage, Unit unit)
    {
        if(currentHP<= 0) return;
        bufferedDamage = damage;
        ReactionDoneTrigger?.Invoke(this);
        if (blocked)
        {
            blocked = false;
            bufferedDamage = 0;
            return;
        }

        
        currentHP -= (int)Mathf.Ceil(damage);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        //TODO: maybe an event here as well?
        if (currentHP <= 0)
        {
            BattleSystem.Manager.DeathOfUnit(this);
        }
        else
        {
            StartCoroutine(BattleSystem.Manager.DisplayDamageNumber((int)Mathf.Ceil(damage)));
        }
    }

    public virtual void Death()
    {
        currentHP = 0;
        BattleSystem.Manager.DeathOfUnit(this);
        //TODO: Don't forget an event here
    }

    private IEnumerator DisplayDamageNumber()
    {
        
        var temp = TrySpawnDamageDisplay();
        if (!temp) yield break;
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

    public void SelectHUD(bool active, Transform toLookAt = null)
    {
        if (toLookAt)
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
        currentTargets = units;
        if (units.Count == 1) currentTarget = units[0];
    }
    
    public void CalculateHUDValues(Button left = null, Button right = null)
    {
        hudValues.text = $"{name}\nHP: {currentHP}/{maxHP}\nSP: {currentSP}/{maxSP}";
        hudCanvas.gameObject.transform.LookAt(BattleSystem.Manager.battleCamera.gameObject.transform.position);
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