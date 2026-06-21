using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager system;

    
    private List<Actor> actors;
    
    /// <summary>
    /// This class is meant to be loaded when entering a story node.
    /// it will take the story part that would be next for the given actor and returns a JSON file that can
    /// be read
    /// </summary>
    /// <param name="actor"></param>
    public void GetNextStoryPart(Actor actor){}
}
