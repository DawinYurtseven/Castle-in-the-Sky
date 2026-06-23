using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    public static StoryManager System;

    [SerializeField] private Image actorOne, actorTwo, continueButton;
    [SerializeField] private TMP_Text bubbleText, actorNameText;
    [SerializeField] private List<Button> multipleChoiceButtons;
    [SerializeField] private GameObject timer;
    
    private Button currentSelectButton;
    
    private List<Actor> actors;
    
    /// <summary>
    /// This class is meant to be loaded when entering a story node.
    /// it will take the story part that would be next for the given actor and returns a JSON file that can
    /// be read
    /// </summary>
    /// <param name="actor"></param>
    public void GetNextStoryPart(Actor actor){}


    public void GoToNextLine()
    {
        
    }


    #region Input

    public void Submit()
    {
        currentSelectButton?.onClick?.Invoke();
    }
    
    public void Navigate(Vector2 normalizedInput)
    {
        
        if (!currentSelectButton || normalizedInput == Vector2.zero) return;
        var isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
        Selectable selectable;
        if (isVertical)
        {
            selectable = normalizedInput.y > 0
                ? currentSelectButton.navigation.selectOnUp
                : currentSelectButton.navigation.selectOnDown;
        }
        else
        {
            selectable = normalizedInput.x > 0
                ? currentSelectButton.navigation.selectOnRight
                : currentSelectButton.navigation.selectOnLeft;
        }

        if (!selectable) return;
        SetCurrentSelectButton((Button)selectable);
    }

    private void SetCurrentSelectButton(Button button)
    {
        if (currentSelectButton && currentSelectButton.TryGetComponent(typeof(GameButton), out var component))
        {
            ( component as GameButton)?.OnDeselectEvent?.Invoke();
        }
        currentSelectButton = button;
        currentSelectButton?.Select();
        if (!currentSelectButton || !currentSelectButton.TryGetComponent(typeof(GameButton), out component)) return;
        {
            (component as GameButton)?.OnSelectEvent?.Invoke();
        }
    }

    #endregion
}

public struct Sentence
{
    public AudioClip Audio;
    public string Text;
    public Image ActorImage;
    public List<Sentence> multipleChoiceSentences;
}
