using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Actor : ScriptableObject
{
    public List<Sentence> scenes;
    public string actorName;

    public int currentProgress = 0;
    public Image defaultImage;

}
