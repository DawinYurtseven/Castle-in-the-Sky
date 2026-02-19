using UnityEngine;

public enum SkillTypes
{
    Damage,
    Buff,
    Debuff,
    Heal
}

[CreateAssetMenu(fileName = "Skill", menuName = "CreateSO/Skill", order = 1)]
public class Skill : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public string skillDescription;
    public int skillCost;
    public float timeValue;
    
    [Header("Skill type and action")]
    public SkillTypes type = SkillTypes.Damage;
    public bool targetOne;
    public float affectValue;
    public string animationName;
    public int userTargetPoint;//-1 is to stand still, 0 is to go to target and 1 is infront of all
    public int turnEffect;
    public float additionalCritChance;
    public float additionalCritAddition;

}
