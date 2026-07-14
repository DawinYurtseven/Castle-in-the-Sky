using System;
using System.Collections.Generic;
using UnityEngine;

public class Actor : ScriptableObject
{
    public List<Dialogue> scenes, fillerScenes;
    public string actorName;

    public int currentProgress;
    public Sprite defaultSprite;

}
[Serializable]
public struct Dialogue : IEquatable<Dialogue>
{
    // Helpful for organizing in the Inspector
    public string dialogueName;
    public bool replacedByAnimation; // this is a Mouseketeer tool for later replacing the dialogue with an animation if needed.
    public List<DialogueCondition> conditions;
    public List<Sentence> sentences;

    public bool Equals(Dialogue other)
    {
        return dialogueName == other.dialogueName && replacedByAnimation == other.replacedByAnimation && Equals(conditions, other.conditions) && Equals(sentences, other.sentences);
    }

    public override bool Equals(object obj)
    {
        return obj is Dialogue other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(dialogueName, replacedByAnimation, conditions, sentences);
    }
}

public enum ConditionOperator
{
    [InspectorName("<")]  LessThan,
    [InspectorName("<=")] LessThanOrEqual,
    [InspectorName(">")]  GreaterThan,
    [InspectorName(">=")] GreaterThanOrEqual,
    [InspectorName("==")] Equal,
    [InspectorName("!=")] NotEqual
}

[Serializable]
public class DialogueCondition
{
    // The key/id of the variable you want to check (e.g., "player_gold", "story_stage")
    public Actor variableKey; 
    
    // The type of math check (e.g., >=, ==)
    public ConditionOperator op;
    
    // The target integer value to compare against
    public int targetValue;

    // The evaluation engine
    public bool IsMet(int currentVariableValue)
    {
        return op switch
        {
            ConditionOperator.LessThan           => currentVariableValue < targetValue,
            ConditionOperator.LessThanOrEqual    => currentVariableValue <= targetValue,
            ConditionOperator.GreaterThan        => currentVariableValue > targetValue,
            ConditionOperator.GreaterThanOrEqual => currentVariableValue >= targetValue,
            ConditionOperator.Equal              => currentVariableValue == targetValue,
            ConditionOperator.NotEqual           => currentVariableValue != targetValue,
            _ => false
        };
    }
}