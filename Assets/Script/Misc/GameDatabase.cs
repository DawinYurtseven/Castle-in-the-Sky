using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Database/Game Database")]
public class GameDatabase : ScriptableObject
{
    [SerializeReference, SubclassSelector] public List<Items> allItems = new();
    [SerializeReference, SubclassSelector] public List<Skill> allSkills = new();
}
