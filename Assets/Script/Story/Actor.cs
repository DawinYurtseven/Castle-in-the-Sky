using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Actor : ScriptableObject
{
    public List<Dialogue> Scenes;
    public string actorName;

    public int currentProgress = 0;
    public Image defaultImage;

}
[System.Serializable]
public struct Dialogue
{
    public string dialogueName; // Helpful for organizing in the Inspector
    public List<Sentence> sentences;
}