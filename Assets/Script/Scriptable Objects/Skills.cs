using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum SkillType {Damage , Debuff, Buff , Heal}
[CreateAssetMenu(menuName = "RPG/Player/Create Skill")]
public class Skills : ScriptableObject
{
    [Header("Skill description")]
    [SerializeField] private string skillName;
    [SerializeField] private string description;
    [SerializeField] private SkillType type;
    [SerializeField] private Status statusType;

    [Header("Skill Effects")] 
    [SerializeField] private int effectAmount;
    [SerializeField] private int spUsage;

    [Header("Skill Owner")] 
    [SerializeField] private CharacterInformation owner;

    public SkillType GetSkillType()
    {
        return type;
    }

    public int GetSPUsage()
    {
        return spUsage;
    }

    public string GetName()
    {
        return skillName;
    }

    public void SetOwner(CharacterInformation newOwner)
    {
        owner = newOwner;
    }

    public void Execute(CharacterInformation nextTarget)
    {
        owner.UseSP(spUsage);
        switch (type)
        {
            case SkillType.Damage:
                //do some animations , you dipshit
                nextTarget.TakeDamage(effectAmount);
                Debug.Log("Get Rekt!");
                break;
            case SkillType.Heal:
                //heal the fucking target, you motherfucker
                nextTarget.HealHP(effectAmount);
                Debug.Log("Healing for yall!");
                break;
            case SkillType.Debuff:
                nextTarget.SetStatus(statusType);
                Debug.Log("Get Fucked!");
                break;
            case SkillType.Buff:
                nextTarget.SetStatus(statusType);
                Debug.Log("All rise motherfucker");
                break;
        }
    }
}
