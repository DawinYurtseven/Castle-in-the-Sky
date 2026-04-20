using System.Collections;
using UnityEngine;

public enum SkillTypes
{
    Damage,
    Buff,
    Debuff,
    Heal
}

public enum SkillNames
{
    GrandSlash,
    HealAll
}

//TODO: Make it class based

[System.Serializable]
public abstract class Skill
{
    public SkillNames name; 
    
    [Header("Skill Info")] public string skillName;
    public string skillDescription;
    public int skillCost;
    public float timeValue;

    [Header("Skill type and action")] public SkillTypes type = SkillTypes.Damage;
    public bool targetOne;
    public float affectValue;
    public string animationName;
    public int userTargetPoint; //-1 is to stand still, 0 is to go to target and 1 is infront of all
    public int turnEffect;
    public float additionalCritChance;
    public float additionalCritAddition;

    public static Skill GetSkill(SkillNames skillName)
    {
        switch (skillName)
        {
            case SkillNames.GrandSlash:
                return new GrandSlash();
            case SkillNames.HealAll:
                return new HealAll();
            default:
                return null;
        }
    }

    public abstract bool Execute(Unit unit);
}