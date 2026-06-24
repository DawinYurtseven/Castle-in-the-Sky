using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Actor : ScriptableObject
{
    public List<Dialogue> scenes;
    public string actorName;

    public int currentProgress = 0;
    public Sprite defaultSprite;

}
[System.Serializable]
public struct Dialogue
{
    public string dialogueName; // Helpful for organizing in the Inspector
    public List<Sentence> sentences;
}