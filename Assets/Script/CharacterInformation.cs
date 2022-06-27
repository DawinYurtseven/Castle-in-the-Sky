using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum Status {NONE ,AtkUP, AtkDOWN , DefUP, DefDOWN , AgiUP , AgiDOWN, ALLUP, ALLDOWN}
public class CharacterInformation : MonoBehaviour
{
    
    

    [Header("Character Info")] 
    public string characterName;
    public int characterLevel;
    [SerializeField] private int characterWill = 0;
    [SerializeField] private bool blockState;

    [Header("Character Stats")]
    [SerializeField] private int ATK, DEF, AGI,HP,SP;
    /*
     * Strength: Physical Damage, basically the factor for the neutral attack and any Physical Attacks.
     * Constitution: HP and ability to block. the higher the more HP and the more can be blocked.
     * Dexterity: Agility and Speed. factor in dodge probability and requirement for some special attacks.
     * Intelligence: MP/SP amount. factor in Special attacks and Magical Damage and resistance.
     * Will: Special stat for an ultimate Attack. the higher the will the faster the gauge for Ultimates are.
     * Charisma: stat needed to interact with certain NPC. this stat will not be included in the current
     *          version of the code because i'll first do an turn based combat only version that's only
     *          focused on the combat aspect. if i decide to turn this more open world like `Octopath Travellers`
     *          then this will be an essential stat like will for interaction with NPC's.
     */
    [Header("Character Fighting Stats")]
    [SerializeField] private int strength, constitution, dexterity, intelligence, will;

    [Header("Character State")] 
    [SerializeField] private Status currentStatus;
    [SerializeField] private int roundTimer;

    [Header("Character Skills")] [SerializeField]
    private List<Skills> skillList;

    [Header("Character Inventory")] [SerializeField]
    private List<Items> itemList;

    public void Awake()
    {
        ATK = strength * 7;
        DEF = constitution * 3;
        AGI = dexterity * 2;
        HP = constitution * 10;
        SP = intelligence * 4;
        /*
         * this next section is for each skill to have their own owner
         * it is meant so that I can use the same scriptible object without casting it each time
         */
        foreach (Skills skill in skillList)
        {
            skill.SetOwner(this);
        }
    }
    /*
     * now there will be a list of special attacks and one Ultimate attack depending on what was chosen at the menu
     * there will only be a maximum of 6 special and 1 ultimate for a fight selected because i dont want to fuck around
     * but because i dont want to hardcode my special attacks and ultimate attack for each fucking character i'll create
     * a class where there can be properties like animation, damage etc. inputted so they may be added into the list.
     * how i'll be able to add more and then make them selectable in an Menu will depend later on storage and all.
     * for now ill only add 3 and add them in the prefab of one character each for enemy and player
     *
     * besides that ill need a bool for block, a counter for HP and MP that will be generated per stat and
     * other instances for the other classes and logics like the BattleSystem Class.
     */

    public void NextTurn()
    {
        blockState = false;
        if (roundTimer > 0) roundTimer -= 1;
        else
        {
            currentStatus = Status.NONE;
            ResetStats(this);
            roundTimer = 0;
        }

    }

    public void SetStatus(Status newStatus)
    {
        currentStatus = newStatus;
        switch (currentStatus)
        {
            case Status.AtkUP:
                ATK += (ATK * 67) / 100;
                break;
            case Status.AtkDOWN:
                ATK -= (ATK * 67) / 100;
                break;
            case Status.AgiUP:
                AGI += (AGI * 45) / 100;
                break;
            case Status.AgiDOWN:
                AGI -= (AGI * 45) / 100;
                break;
            case Status.DefUP:
                DEF += DEF / 2;
                break;
            case Status.DefDOWN:
                DEF -= DEF / 2;
                break;
            case Status.ALLUP:
                ATK += (ATK * 67) / 100;
                AGI += (AGI * 45) / 100;
                DEF += DEF / 2;
                break;
            case Status.ALLDOWN:
                ATK -=(ATK * 67) / 100;
                AGI -= (AGI * 45) / 100;
                DEF -= DEF / 2;
                break;
            default:
                Debug.Log("Analysis can suck my dick!!!");
                break;
        }

        roundTimer = 3;
    }

    public void ResetStats(CharacterInformation target)
    {
        target.ATK = target.strength * 7;
        target.DEF = target.constitution * 3;
        target.AGI = target.dexterity * 2;
        target.HP = target.constitution * 10;
        target.SP = target.intelligence * 4;
    }
    
    public void Attack (CharacterInformation enemy)
    {
        
        //CheckStatus();
        if (!CalculateEvasion(enemy.AGI))
        {
            if (enemy.blockState)
            { 
                int blockedDamage = (2 * ATK) / 10; 
                enemy.TakeDamage(blockedDamage);
            }
            else
            { 
                enemy.TakeDamage(ATK);
            }
            Debug.Log("this motherfucker can take some shit");
        }
        /*
         * move animation towards the enemy and the return animation logic of the attack
         * should be that the opponent should have the possibility to dodge and a block will have a reduction
         * of the damage if not immunity depending on stat differences.
         */

    }
    
    
    //Block state may change depending on how i wanna implement it with stats and of course also public if needed
    public void Block()
    {
        blockState = true;
        /*
         * implement a block animation transition with animations a state change in blocking
         * this State will change when a round is over
         * Blocking may in the future also change depending on state change and i'll call it "Guard Breaking"
         */
    }
/*
    public void ShowStats()
    {
        /*
         * just a function to display stats to a text place or ,depending on how unity likes to be fucked,
         * a complete UI for the char display if i figure that out and wanna implement it to that
         
    }
*/
    public bool CalculateEvasion(int targetEnemy)
    {
        if (AGI >= targetEnemy) return false;
        int randomValue = Random.Range(0, 100);
        int agilityDifference = targetEnemy - AGI;
        switch (agilityDifference)
        {
            case int n when (n<= 10):
                return randomValue <= 10;
            case int n when (n <= 20):
                return randomValue <= 25;
            case int n when (n>= 100):
                return randomValue <= 75;
            default:
                return randomValue <= 45;
        }
    }

    /*public void UseUltimate()
    {
        if (characterWill == 100)
        {
            //will use will 
        }
    }*/


    public List<Skills> getSkillList()
    {
        return skillList;
    }

    public List<Items> getItemList()
    {
        return itemList;
    }

    public int GetHP()
    {
        return HP;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (0 >= HP) HP = 0;
    }

    public void HealHP(int healing)
    {
        HP += healing;
        if(HP > constitution * 10) HP = constitution * 10;
    }

    public void UseSP(int SPusage)
    {
        SP -= SPusage;
        if (SP < 0) SP = 0;
    }

    public void RegenSP(int regen)
    {
        SP += regen;
        if (SP > intelligence * 4) SP = intelligence * 4;
    }

    public void Revive()
    {
        ResetStats(this);
    }
    
}
