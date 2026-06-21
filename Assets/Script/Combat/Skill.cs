using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum SkillTarget
{
    Enemy,
    Ally,
    EnemyAll,
    AllyAll,
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

    [FormerlySerializedAs("type")] [Header("Skill type and action")] public SkillTarget target = SkillTarget.Enemy;
    public float affectValue;
    public string animationName;
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

    //TODO: add animations to the units for the skill and try that out. but for now, not needed
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