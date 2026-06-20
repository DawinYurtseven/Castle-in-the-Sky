using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum SkillTypes
{
    Damage,
    Buff,
    Debuff,
    Heal
}

public enum SkillNames
{
    none,
    GrandSlash,
    HealAll
}

//TODO: Make it class based

[System.Serializable]
public abstract class Skill
{
    
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
        return skillName switch
        {
            SkillNames.GrandSlash => new GrandSlash(),
            SkillNames.HealAll => new HealAll(),
            _ => null
        };
    }

    public abstract bool Execute(Unit unit);
    
    public static Skill GetRandomSkill(List<Skill> exclude = null)
    {
        var skills = new List<Skill>()
        {
            new GrandSlash(),
            new HealAll(),
        };
        if (exclude != null)
        {
            foreach (var skill in from skill in exclude let i = skills.Find((x) => x.GetType() == skill.GetType()) select skill)
            {
                skills.Remove(skill);
            }
        }
        var index = Random.Range(0, skills.Count);
        return skills[index] != null ? skills[index] : new GrandSlash();
    }
}

// Simple attribute marker
public class SubclassSelectorAttribute : PropertyAttribute { }