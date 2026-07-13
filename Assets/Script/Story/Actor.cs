using System;
using System.Collections.Generic;
using UnityEngine;

public class Actor : ScriptableObject
{
    public List<Dialogue> scenes, fillerScenes;
    public string actorName;

    public int currentProgress = 0;
    public Sprite defaultSprite;

}
[Serializable]
public struct Dialogue
{
    // Helpful for organizing in the Inspector
    public string dialogueName;
    public bool replacedByAnimation; // this is a Mouseketeer tool for later replacing the dialogue with an animation if needed.
    public List<DialogueCondition> conditions;
    public List<Sentence> sentences;
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